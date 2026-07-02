using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using IT.Core.Utilities;
using IT.Player.Control;

namespace IT.Segments
{
    /// <summary>Which AABB boundary a segment crossing passed through (Story 5.1, V9).</summary>
    /// <remarks>
    /// Best-effort: derived from the player's prev→current position vs the segment's world AABB.
    /// <see cref="None"/> is the honest answer for indeterminate cases — no movement (spawn / first
    /// frame), a corner crossing that straddles two edges at once, or a future teleport (Story 5.4).
    /// 5.1 DETECTS and EXPOSES this on the enter/exit event so Story 5.2 can route EdgeBehavior off
    /// it; 5.1 never acts on it.
    /// </remarks>
    public enum SegmentEdge { None, Left, Right, Top, Bottom }

    /// <summary>
    /// One enter/exit event payload. Carries everything Story 5.2 needs to route EdgeBehavior:
    /// who crossed (<see cref="Player"/>), which segment (<see cref="Segment"/>), and which edge
    /// (<see cref="Edge"/>). All three are declared fields on this readonly struct.
    /// </summary>
    public readonly struct SegmentCrossing
    {
        public readonly PlayerWrapper Player;
        public readonly SegmentBounds Segment;
        public readonly SegmentEdge   Edge;

        public SegmentCrossing(PlayerWrapper player, SegmentBounds segment, SegmentEdge edge)
        {
            Player  = player;
            Segment = segment;
            Edge    = edge;
        }
    }

    /// <summary>
    /// True-global singleton (C-C) that tracks per-player segment membership by pure point-in-rect
    /// math — ZERO trigger colliders, no Physics2D anywhere in the path (FR-18 / NFR-3 / C-E). Hosted
    /// on <c>SystemsRoot</c> beside <c>PlayerRoster</c> (see SystemsRoot.Create), the same C-C home
    /// its Story-2.1 comment reserved.
    ///
    /// Segments push themselves in via <see cref="Register"/> / <see cref="Unregister"/> from
    /// <c>SegmentBounds.OnEnable/OnDisable</c> — the same registration idiom PlayerWrapper uses with
    /// PlayerRoster (not a scan), so the persistent manager self-heals across the scene loads/unloads
    /// that arrive in Story 5.4.
    /// </summary>
    public class SegmentManager : Singleton<SegmentManager>
    {
        // Registered scene segments. Populated by push from SegmentBounds.OnEnable (Register).
        readonly List<SegmentBounds> _segments = new();

        // Per-player membership. A SET, not a single segment: two rects overlapping by an AUTHORING
        // MISTAKE legitimately contain the same point, and a set shows that honestly (the M dump
        // prints "{ A, B }"), making the bug visible. A single-segment field would force a silent
        // arbitrary tiebreak and hide it. The half-open rule (R4) only guarantees exactly-one for a
        // SHARED EDGE, never for an overlapping area — so the set is the correct default.
        readonly Dictionary<PlayerWrapper, HashSet<SegmentBounds>> _membership = new();

        // Last frame's effective position per player — the other input to edge-identity (EdgeFor).
        readonly Dictionary<PlayerWrapper, Vector2> _prevPos = new();

        // Reused each tick so the per-player current-membership scan allocates nothing per frame.
        readonly HashSet<SegmentBounds> _scratch = new();

        /// <summary>Registered scene segments (read surface for 5.2/5.3 authoring tools).</summary>
        public IReadOnlyList<SegmentBounds> Segments => _segments;

        /// <summary>Fires when a player enters a segment (frame-diff). Story 5.2 subscribes.</summary>
        public event System.Action<SegmentCrossing> SegmentEntered;

        /// <summary>Fires when a player leaves a segment (frame-diff). Story 5.2 subscribes.</summary>
        public event System.Action<SegmentCrossing> SegmentExited;

        protected override void Awake()
        {
            base.Awake();   // Singleton: sets instance (play mode only)

            // Prune stale keys when a player leaves the roster. PlayerRoster.PlayerLeft is currently
            // dead code (never fires — see planning §1); Epic 4.5 PB.4 makes it fire on wrapper
            // teardown. Subscribing now costs nothing and behaves correctly the moment PB.4 lands,
            // so a torn-down wrapper can't leak a _membership / _prevPos entry across a long session.
            PlayerRoster.Instance.PlayerLeft += OnPlayerLeft;
        }

        void OnDestroy()
        {
            // TryGetInstance (not Instance) so tearing this down at quit doesn't resurrect the roster.
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null)
                roster.PlayerLeft -= OnPlayerLeft;
        }

        // --- registration surface (push, mirrors PlayerRoster.Register) ---

        public void Register(SegmentBounds segment)
        {
            if (segment == null || _segments.Contains(segment)) return;
            _segments.Add(segment);
        }

        public void Unregister(SegmentBounds segment)
        {
            if (segment == null || !_segments.Remove(segment)) return;

            // Silently drop the leaving segment from every player's membership — NO exit event. A
            // segment disappearing (disable / scene-unload) is not a player crossing, and pruning it
            // here means the next tick's diff never dereferences a now-destroyed SegmentBounds (Unity
            // fake-null → the log's seg.SegmentId would throw MissingReferenceException).
            foreach (var kvp in _membership)
                kvp.Value.Remove(segment);
        }

        /// <summary>Current segment set for a player (empty if none / unknown). Read-only.</summary>
        public IReadOnlyCollection<SegmentBounds> MembershipOf(PlayerWrapper player)
            => _membership.TryGetValue(player, out var set) ? set : System.Array.Empty<SegmentBounds>();

        // --- membership tick ---

        // LateUpdate, NOT Update/FixedUpdate: (1) positions are post-FixedUpdate settled here, so a
        // possessed vehicle's kinematic body has already moved this frame; (2) Epic 6's camera also
        // runs in LateUpdate and consumes a consistent membership snapshot. Crucially there is NO
        // Active-gate: membership is position-based, not agency-based (owner ruling R3), so a
        // Suspended wrapper still ticks — unlike PlayerWrapper.Update/FixedUpdate, which early-return
        // on State != Active and would stop tracking exactly when a player is Suspended.
        void LateUpdate()
        {
            if (_segments.Count == 0) return;
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null) return;

            var wrappers = roster.Wrappers;
            for (int i = 0; i < wrappers.Count; i++)
            {
                var wrapper = wrappers[i];
                if (wrapper == null) continue;   // defensive: destroyed wrapper still in roster (pre-5.4)

                Vector2 cur  = wrapper.EffectivePosition;                              // possession-aware, Suspend-agnostic
                Vector2 prev = _prevPos.TryGetValue(wrapper, out var pp) ? pp : cur;   // first sight → prev==cur → edge None

                if (!_membership.TryGetValue(wrapper, out var member))
                {
                    member = new HashSet<SegmentBounds>();
                    _membership[wrapper] = member;
                }

                // Current membership — pure point-in-rect, zero colliders.
                _scratch.Clear();
                for (int s = 0; s < _segments.Count; s++)
                {
                    var seg = _segments[s];
                    if (seg != null && seg.Contains(cur))
                        _scratch.Add(seg);
                }

                // Frame-diff → enter/exit. Exits: was a member, no longer. Enters: newly a member.
                foreach (var seg in member)
                    if (!_scratch.Contains(seg))
                        RaiseExit(i, wrapper, seg, EdgeFor(prev, cur, seg.WorldBounds));

                foreach (var seg in _scratch)
                    if (!member.Contains(seg))
                        RaiseEnter(i, wrapper, seg, EdgeFor(prev, cur, seg.WorldBounds));

                // Commit current → stored (reuse the existing set; no alloc).
                member.Clear();
                foreach (var seg in _scratch)
                    member.Add(seg);

                _prevPos[wrapper] = cur;
            }
        }

        void RaiseEnter(int playerIndex, PlayerWrapper w, SegmentBounds seg, SegmentEdge edge)
        {
            Debug.Log($"[Segment] ENTER {seg.SegmentId} — Player {playerIndex} (edge {edge})");
            SegmentEntered?.Invoke(new SegmentCrossing(w, seg, edge));
        }

        void RaiseExit(int playerIndex, PlayerWrapper w, SegmentBounds seg, SegmentEdge edge)
        {
            Debug.Log($"[Segment] EXIT  {seg.SegmentId} — Player {playerIndex} (edge {edge})");
            SegmentExited?.Invoke(new SegmentCrossing(w, seg, edge));
        }

        void OnPlayerLeft(PlayerWrapper wrapper)
        {
            _membership.Remove(wrapper);
            _prevPos.Remove(wrapper);
        }

        // --- edge identity (V9, best-effort) ---

        // Which single AABB edge the straight move prev→cur passed through. Returns exactly-one edge
        // only when the move cleanly crosses one boundary within that edge's span; otherwise None:
        //   • prev == cur (spawn / first frame / no movement)     → None
        //   • the move straddles two edges at once (corner)       → None (indeterminate)
        //   • prev inside, cur outside                            → the edge cur left through
        //   • prev outside, cur inside                            → the edge prev entered through
        static SegmentEdge EdgeFor(Vector2 prev, Vector2 cur, Bounds aabb)
        {
            if (prev == cur) return SegmentEdge.None;

            Vector2 min = aabb.min;
            Vector2 max = aabb.max;
            SegmentEdge found = SegmentEdge.None;
            int count = 0;

            // Vertical edges (constant x) — only if there is horizontal travel to cross them.
            if (!Mathf.Approximately(prev.x, cur.x))
            {
                if (CrossesVertical(prev, cur, min.x, min.y, max.y)) { found = SegmentEdge.Left;  count++; }
                if (CrossesVertical(prev, cur, max.x, min.y, max.y)) { found = SegmentEdge.Right; count++; }
            }
            // Horizontal edges (constant y) — only if there is vertical travel to cross them.
            if (!Mathf.Approximately(prev.y, cur.y))
            {
                if (CrossesHorizontal(prev, cur, min.y, min.x, max.x)) { found = SegmentEdge.Bottom; count++; }
                if (CrossesHorizontal(prev, cur, max.y, min.x, max.x)) { found = SegmentEdge.Top;    count++; }
            }

            return count == 1 ? found : SegmentEdge.None;
        }

        // Does segment a→b cross the vertical line x==xLine within [yMin, yMax]? Caller guarantees a.x != b.x.
        static bool CrossesVertical(Vector2 a, Vector2 b, float xLine, float yMin, float yMax)
        {
            if ((a.x < xLine) == (b.x < xLine)) return false;   // same side → no crossing
            float t = (xLine - a.x) / (b.x - a.x);
            float y = a.y + t * (b.y - a.y);
            return y >= yMin && y <= yMax;
        }

        // Does segment a→b cross the horizontal line y==yLine within [xMin, xMax]? Caller guarantees a.y != b.y.
        static bool CrossesHorizontal(Vector2 a, Vector2 b, float yLine, float xMin, float xMax)
        {
            if ((a.y < yLine) == (b.y < yLine)) return false;   // same side → no crossing
            float t = (yLine - a.y) / (b.y - a.y);
            float x = a.x + t * (b.x - a.x);
            return x >= xMin && x <= xMax;
        }

        // --- debug (throwaway; M joins the pre-ship removal list H/K/O/N/F/G/J/P/V) ---

        // Direct Keyboard.current poll, matching PlayerStatusManager's debug keys. Reads the keyboard
        // straight (a D2 shortcut allowed for throwaway scaffolding), so it answers even while a
        // wrapper is Suspended. Removed pre-ship with the [Segment] logs.
        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.mKey.wasPressedThisFrame) return;
            DumpMembership();
        }

        void DumpMembership()
        {
            var sb = new StringBuilder();
            sb.Append("[Segment] === Membership dump (M) ===");

            var roster = PlayerRoster.TryGetInstance();
            var wrappers = roster != null ? roster.Wrappers : null;
            if (wrappers == null || wrappers.Count == 0)
            {
                sb.Append("\n[Segment]   (no roster players)");
            }
            else
            {
                for (int i = 0; i < wrappers.Count; i++)
                {
                    var w = wrappers[i];
                    sb.Append($"\n[Segment]   Player {i} → {DescribeMembership(w)}");
                }
            }
            Debug.Log(sb.ToString());
        }

        string DescribeMembership(PlayerWrapper w)
        {
            if (w == null || !_membership.TryGetValue(w, out var set) || set.Count == 0) return "∅";

            var ids = new List<string>(set.Count);
            foreach (var seg in set)
                ids.Add(seg != null ? seg.SegmentId : "<destroyed>");

            return ids.Count == 1 ? ids[0] : "{ " + string.Join(", ", ids) + " }";
        }
    }
}
