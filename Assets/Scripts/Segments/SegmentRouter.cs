using System;
using UnityEngine;
using IT.Core.Utilities;
using IT.Player.Control;

namespace IT.Segments
{
    /// <summary>
    /// Transport edge crossing — an in-scene relocation to <see cref="TargetSegment"/> (a SegmentBounds
    /// in the SAME scene, Ruling 1). Story 5.4 executes; 5.2 only requests. Value payload (mirrors how
    /// <c>SegmentCrossing</c> lives beside its manager).
    /// </summary>
    public readonly struct TransportRequest
    {
        public readonly SegmentBounds FromSegment;
        public readonly SegmentBounds TargetSegment;
        public readonly string        SpawnPointId;
        public readonly PlayerWrapper Player;
        public readonly SegmentEdge   Edge;

        public TransportRequest(SegmentBounds from, SegmentBounds target, string spawnPointId,
                                PlayerWrapper player, SegmentEdge edge)
        {
            FromSegment  = from;
            TargetSegment = target;
            SpawnPointId = spawnPointId;
            Player       = player;
            Edge         = edge;
        }
    }

    /// <summary>
    /// Scene edge crossing — a cross-scene relocation. Carries the runtime-safe <see cref="TargetScenePath"/>
    /// (not the editor-only SceneAsset). Story 5.4 loads the scene + spawns; carry-over is governed by
    /// Epic 4.5 CarryOverMode, not here.
    /// </summary>
    public readonly struct ScenerRequest
    {
        public readonly SegmentBounds FromSegment;
        public readonly string        TargetScenePath;
        public readonly string        SpawnPointId;
        public readonly PlayerWrapper Player;
        public readonly SegmentEdge   Edge;

        public ScenerRequest(SegmentBounds from, string targetScenePath, string spawnPointId,
                             PlayerWrapper player, SegmentEdge edge)
        {
            FromSegment     = from;
            TargetScenePath = targetScenePath;
            SpawnPointId    = spawnPointId;
            Player          = player;
            Edge            = edge;
        }
    }

    /// <summary>
    /// Story 5.2 router: the TRANSLATION LAYER (DD2) between <see cref="SegmentManager"/>'s raw enter/exit
    /// crossings and behaviour-typed request events that Story 5.4 / 5.5 / Epic 6 consume. It reuses the
    /// existing C# event mechanism (C-G) — it is NOT a parallel bus. A true-global (C-C), hosted on
    /// SystemsRoot beside SegmentManager; added AFTER it in <c>SystemsRoot.Create</c> so
    /// <c>SegmentManager.Instance</c> is live in <see cref="Awake"/>.
    ///
    /// ROUTING (spec DD3):
    ///   • Edge behaviours (Transport / Scene / Slide) route off <c>SegmentExited</c> — the EXITED
    ///     segment's <see cref="SegmentConfig"/> decides, keyed by the crossed edge. <see cref="SegmentEdge.None"/>
    ///     (first-frame / corner / teleport) is filtered here (D2): no edge crossed ⇒ no edge behaviour.
    ///   • Camera mode routes off <c>SegmentEntered</c>, firing only on a DELTA from the current tracked
    ///     mode. The router boots with no mode set (Option A), so the first entered configured segment
    ///     sets the initial camera — and camera is deliberately NOT filtered on Edge==None, so a
    ///     first-frame spawn still establishes the mode.
    ///
    /// A segment with no <see cref="SegmentConfig"/>, or an unconfigured edge, resolves to
    /// <see cref="EdgeBehavior.Seamless"/> — nothing fires (DD1). 5.2 REQUESTS; it never executes.
    /// </summary>
    public class SegmentRouter : Singleton<SegmentRouter>
    {
        /// <summary>Entered a segment whose <see cref="CameraMode"/> differs from the current — Epic 6 executes (Q5).</summary>
        public event Action<SegmentBounds, CameraMode, PlayerWrapper> OnCameraModeChangeRequested;

        /// <summary>
        /// Crossed a Slide edge (DD7): (from, to, edge, player). <c>to</c> is best-effort — the segment the
        /// player is now inside, or <c>null</c> for an edge into the void / an unloaded neighbour. Epic 6
        /// pans; it can fall back to <c>edge</c> for direction when <c>to</c> is null.
        /// </summary>
        public event Action<SegmentBounds, SegmentBounds, SegmentEdge, PlayerWrapper> SlideRequested;

        /// <summary>Crossed a Transport edge — Story 5.4 relocates in-scene.</summary>
        public event Action<TransportRequest> OnTransportRequested;

        /// <summary>Crossed a Scene edge — Story 5.4 loads the scene + spawns.</summary>
        public event Action<ScenerRequest> OnScenerRequested;

        /// <summary>
        /// A closed <see cref="SegmentLock"/> refused a gated edge crossing (Story 5.3, Ruling E):
        /// (fromSegment, edge, player, lock). Fired INSTEAD of the behaviour's request — never alongside it.
        /// Only the throwaway logger consumes this in 5.3; game-feel consumers (sound/UI) come later.
        /// </summary>
        public event Action<SegmentBounds, SegmentEdge, PlayerWrapper, SegmentLock> OnLockRefused;

        // Option A: no mode set on boot. The first configured segment entered fires the initial camera
        // request (delta from "unset"); thereafter only genuine mode changes fire.
        CameraMode? _currentMode;

        protected override void Awake()
        {
            base.Awake();   // Singleton: sets instance (play mode only)

            // SegmentManager is added before this component in SystemsRoot.Create, so its Instance is live.
            var mgr = SegmentManager.Instance;
            mgr.SegmentExited  += HandleExited;
            mgr.SegmentEntered += HandleEntered;
        }

        void OnDestroy()
        {
            // TryGetInstance (not Instance) so tearing this down at quit doesn't resurrect the manager.
            var mgr = SegmentManager.TryGetInstance();
            if (mgr != null)
            {
                mgr.SegmentExited  -= HandleExited;
                mgr.SegmentEntered -= HandleEntered;
            }
        }

        // --- EDGE routing: Transport / Scene / Slide, keyed off the EXIT (DD3) ---
        void HandleExited(SegmentCrossing c)
        {
            if (c.Edge == SegmentEdge.None) return;   // D2: no determinate edge ⇒ no edge behaviour
            if (c.Segment == null) return;

            var config = c.Segment.GetComponent<SegmentConfig>();
            if (config == null) return;               // config-less segment ⇒ all-Seamless (DD1)

            var maybe = config.GetEdgeConfig(c.Edge);
            if (maybe == null) return;                 // not a real edge (None) — defensive
            var edge = maybe.Value;

            // Story 5.3 lock gate (spec DD3): ONE guard ahead of the dispatch switch, so every
            // non-Seamless behaviour — including future ones — is gated; Seamless never reaches it
            // (nothing to gate, Ruling A). Refusal must precede dispatch: an event can't be un-fired.
            if (edge.behavior != EdgeBehavior.Seamless)
            {
                var lockComp = c.Segment.GetComponent<SegmentLock>();
                if (lockComp != null && lockComp.Gates(c.Edge) && !lockComp.IsOpen)
                {
                    Debug.Log(
                        $"[SegmentLock] {edge.behavior} on '{c.Segment.SegmentId}' {c.Edge} refused — " +
                        $"locked (source not satisfied). No request dispatched.");
                    OnLockRefused?.Invoke(c.Segment, c.Edge, c.Player, lockComp);
                    return;
                }
            }

            switch (edge.behavior)
            {
                case EdgeBehavior.Seamless:
                    return;                            // free crossing — nothing fires

                case EdgeBehavior.Slide:
                    SlideRequested?.Invoke(c.Segment, NeighbourAt(c.Player, c.Segment), c.Edge, c.Player);
                    return;

                case EdgeBehavior.Transport:
                    // DD4.1 runtime guard: a null or cross-scene target is a config error, not a crossing.
                    // Log LOUD (error) and skip dispatch — never route a player into a nulled reference.
                    if (edge.targetSegment == null ||
                        edge.targetSegment.gameObject.scene != c.Segment.gameObject.scene)
                    {
                        Debug.LogError(
                            $"[SegmentRouter] Transport on '{c.Segment.SegmentId}' {c.Edge} has a null or " +
                            $"cross-scene targetSegment — rejected (Ruling 1: Transport is same-scene; use a " +
                            $"Scene edge for cross-scene). No request dispatched.");
                        return;
                    }
                    OnTransportRequested?.Invoke(new TransportRequest(
                        c.Segment, edge.targetSegment, edge.spawnPointId, c.Player, c.Edge));
                    return;

                case EdgeBehavior.Scene:
                    if (string.IsNullOrEmpty(edge.targetScenePath))
                    {
                        Debug.LogError(
                            $"[SegmentRouter] Scene edge on '{c.Segment.SegmentId}' {c.Edge} has no target " +
                            $"scene path — rejected. No request dispatched.");
                        return;
                    }
                    OnScenerRequested?.Invoke(new ScenerRequest(
                        c.Segment, edge.targetScenePath, edge.spawnPointId, c.Player, c.Edge));
                    return;
            }
        }

        // --- CAMERA routing: keyed off the ENTER, delta-only (DD3). No Edge==None filter (Option A). ---
        void HandleEntered(SegmentCrossing c)
        {
            if (c.Segment == null) return;

            var config = c.Segment.GetComponent<SegmentConfig>();
            if (config == null) return;               // config-less ⇒ no camera opinion (leaves current mode)

            var mode = config.CameraMode;
            if (_currentMode == mode) return;          // same mode ⇒ suppress (no redundant request)
            _currentMode = mode;
            OnCameraModeChangeRequested?.Invoke(c.Segment, mode, c.Player);
        }

        // Best-effort Slide "to" (DD7): the segment the player is now inside, other than the one they left.
        // Resolved by point-in-rect at the player's CURRENT position — independent of SegmentManager's
        // end-of-tick membership commit (which is still stale when SegmentExited fires). Null if the player
        // is over no other segment (edge into the void / unloaded neighbour).
        static SegmentBounds NeighbourAt(PlayerWrapper player, SegmentBounds from)
        {
            var mgr = SegmentManager.TryGetInstance();
            if (mgr == null || player == null) return null;

            Vector2 pos = player.EffectivePosition;
            var segments = mgr.Segments;
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                if (seg != null && seg != from && seg.Contains(pos)) return seg;
            }
            return null;
        }
    }
}
