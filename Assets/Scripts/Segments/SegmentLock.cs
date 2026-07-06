using System;
using UnityEngine;
using IT.Core.Dependency;

namespace IT.Segments
{
    /// <summary>
    /// Story 5.3: per-edge lock gating (Rulings A–F). Consumes ONE <see cref="IDependencySource{T}"/>
    /// condition (source true = satisfied = OPEN) and IS itself an <c>IDependencySource&lt;bool&gt;</c>
    /// (Ruling F), so doors, quest systems, and Epic 6's FixedLockedCamera can depend on lock state
    /// through the existing machinery — the lock consumes sources and is a source, no new bus (C-G).
    ///
    /// <see cref="SegmentRouter"/> consults <see cref="Gates"/> / <see cref="IsOpen"/> at crossing time
    /// and refuses dispatch on a closed gated edge — locks gate the ROUTER, never movement (Ruling A);
    /// physical containment is level geometry's job (a door prefab can be a Dependent on the SAME
    /// source that drives this lock: one condition, two effects). Seamless edges ignore locks —
    /// gating one is a dead flag, warned at OnValidate.
    ///
    /// Composition (spec DD1): SegmentBounds = shape, SegmentConfig = routing, SegmentLock = gating.
    /// FAIL-CLOSED: <see cref="IsOpen"/> defaults false, and Dependent's OnEnable never syncs when no
    /// source is assigned — an unwired lock is permanently locked (a gate that fails open would be a
    /// silent progression skip; failing closed is a loud, debuggable authoring error).
    ///
    /// Runtime state (<c>_isOpen</c>/<c>_latched</c>) is TRANSIENT (C-H): resets every session and
    /// re-derives from the source's current value via the inherited initial sync. A key door that must
    /// stay open across save/load pairs <see cref="_latchOpen"/> with a Permanent-scope WorldState flag
    /// — the flag is what persists, and its restored true re-springs the latch on load.
    /// </summary>
    [RequireComponent(typeof(SegmentBounds))]
    [DisallowMultipleComponent]
    public class SegmentLock : Dependent<bool>, IDependencySource<bool>
    {
        [Header("Edges this lock gates (Ruling B — per-edge)")]
        [SerializeField] bool _gatesLeft;
        [SerializeField] bool _gatesRight;
        [SerializeField] bool _gatesTop;
        [SerializeField] bool _gatesBottom;

        [Header("Latch (Ruling D)")]
        [Tooltip("OFF = live-mirror: re-locks when the source goes false again (switch-gates). " +
                 "ON = one-shot: the first true opens the lock for the rest of the session (key doors). " +
                 "Latch state is TRANSIENT (C-H) — pair with a Permanent WorldState flag to persist.")]
        [SerializeField] bool _latchOpen;

        // Runtime state — transient by design (C-H): resets every session; re-derives from the source
        // via Dependent's OnEnable initial sync. Default false = fail-closed.
        bool _isOpen;
        bool _latched;

        /// <summary>True = condition satisfied (or latched) — the router may dispatch gated edges.</summary>
        public bool IsOpen => _isOpen;

        /// <summary>Does this lock gate the given edge? <see cref="SegmentEdge.None"/> is never gated (not a real edge).</summary>
        public bool Gates(SegmentEdge edge) => edge switch
        {
            SegmentEdge.Left   => _gatesLeft,
            SegmentEdge.Right  => _gatesRight,
            SegmentEdge.Top    => _gatesTop,
            SegmentEdge.Bottom => _gatesBottom,
            _                  => false,
        };

        // --- IDependencySource<bool> (Ruling F): the lock is a source ---

        public bool Value => _isOpen;
        public event Action<bool> Changed;

        // --- Dependent<bool>: the lock consumes a source ---

        protected override void OnSourceChanged(bool satisfied)
        {
            if (_latched) return;                        // sprung latch ignores the source for the rest of the session
            if (_latchOpen && satisfied) _latched = true;

            if (satisfied == _isOpen) return;            // transitions only — no redundant Changed
            _isOpen = satisfied;
            Changed?.Invoke(_isOpen);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Source == null)
                Debug.LogWarning(
                    $"[SegmentLock] '{name}' has no condition source — permanently locked (fail-closed). " +
                    $"Wire an IDependencySource<bool> (e.g. WorldStateFlagSource).", this);

            var config = GetComponent<SegmentConfig>();
            WarnIfDeadFlag(config, _gatesLeft,   SegmentEdge.Left);
            WarnIfDeadFlag(config, _gatesRight,  SegmentEdge.Right);
            WarnIfDeadFlag(config, _gatesTop,    SegmentEdge.Top);
            WarnIfDeadFlag(config, _gatesBottom, SegmentEdge.Bottom);
        }

        // A gated edge that resolves Seamless — or a segment with no SegmentConfig at all (all-Seamless,
        // 5.2 DD1) — gates nothing: Seamless edges ignore locks (Ruling A). The flag is dead, not an error.
        void WarnIfDeadFlag(SegmentConfig config, bool gated, SegmentEdge edge)
        {
            if (!gated) return;
            if (config != null && config.GetEdgeConfig(edge)?.behavior != EdgeBehavior.Seamless) return;

            Debug.LogWarning(
                $"[SegmentLock] '{name}' gates {edge}, but that edge is Seamless" +
                $"{(config == null ? " (no SegmentConfig — all edges Seamless)" : "")} — " +
                $"Seamless edges ignore locks (Ruling A); the flag has no effect.", this);
        }
#endif
    }
}
