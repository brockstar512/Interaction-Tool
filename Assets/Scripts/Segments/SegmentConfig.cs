using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IT.Segments
{
    /// <summary>
    /// Per-segment authoring for Story 5.2 routing: what each of the four edges does (an
    /// <see cref="EdgeBehavior"/> + its destination) and which <see cref="CameraMode"/> the segment
    /// requests on entry. An OPTIONAL component that sits alongside <see cref="SegmentBounds"/> on the
    /// same GameObject (DD1). A segment with only <see cref="SegmentBounds"/> and no SegmentConfig is a
    /// valid minimum-viable segment whose edges are all <see cref="EdgeBehavior.Seamless"/> (spec DD1);
    /// the SegmentRouter (Rung 5) reads this via <c>GetComponent</c> and treats an absent component as
    /// all-Seamless.
    ///
    /// Plain serialized component data — no ScriptableObjects, no colliders, no Physics2D (C-B / C-E).
    /// <see cref="SegmentBounds"/> is NOT modified by this component (Story 5.1 seal preserved).
    /// </summary>
    [RequireComponent(typeof(SegmentBounds))]
    public class SegmentConfig : MonoBehaviour
    {
        [SerializeField] CameraMode _cameraMode = CameraMode.Follow;

        // Four NAMED edge configs — one per cardinal SegmentEdge. Named fields (not an array/dict) are
        // the serialized source of truth: each renders as a labelled foldout in the Inspector, always
        // present, never missing or duplicated, and safely mutable by OnValidate (a List<struct> would
        // hand back copies).
        [SerializeField] SegmentEdgeConfig _leftEdge;
        [SerializeField] SegmentEdgeConfig _rightEdge;
        [SerializeField] SegmentEdgeConfig _topEdge;
        [SerializeField] SegmentEdgeConfig _bottomEdge;

        // Runtime read-cache keyed by edge, built once at Awake for O(1) router lookup (Rung 5). Not
        // serialized (Unity ignores a plain Dictionary field); the named fields above remain canonical.
        Dictionary<SegmentEdge, SegmentEdgeConfig> _byEdge;

        /// <summary>The camera mode this segment requests on entry (Q5). 5.2 requests; Epic 6 executes.</summary>
        public CameraMode CameraMode => _cameraMode;

        void Awake() => BuildLookup();

        void BuildLookup()
        {
            _byEdge = new Dictionary<SegmentEdge, SegmentEdgeConfig>(4)
            {
                { SegmentEdge.Left,   _leftEdge   },
                { SegmentEdge.Right,  _rightEdge  },
                { SegmentEdge.Top,    _topEdge    },
                { SegmentEdge.Bottom, _bottomEdge },
            };
        }

        /// <summary>
        /// The config for one edge, for the SegmentRouter to dispatch on (Rung 5). Returns
        /// <c>null</c> for <see cref="SegmentEdge.None"/> (not a real edge) — a real edge always yields
        /// a value (<see cref="EdgeBehavior.Seamless"/> by default). The genuine "no config at all" case
        /// is this whole component being absent, which the router handles by treating a missing
        /// SegmentConfig as all-Seamless (DD1); a caller may likewise read <c>null</c> here as Seamless.
        /// </summary>
        public SegmentEdgeConfig? GetEdgeConfig(SegmentEdge edge)
        {
            if (_byEdge == null) BuildLookup();   // defensive: edit-mode / pre-Awake queries
            return _byEdge.TryGetValue(edge, out var cfg) ? cfg : (SegmentEdgeConfig?)null;
        }

#if UNITY_EDITOR
        // Author-time only (editor): keep each edge's runtime targetScenePath in sync with its
        // editor-only SceneAsset, and surface misconfigurations early — cheaper than at runtime. The
        // authoritative runtime guard still lives in the router (DD4.1); this is LAYERED defence, not a
        // replacement, and intentionally goes beyond the spec letter (documented for the Rung 8 as-built).
        void OnValidate()
        {
            SyncAndWarn(ref _leftEdge,   SegmentEdge.Left);
            SyncAndWarn(ref _rightEdge,  SegmentEdge.Right);
            SyncAndWarn(ref _topEdge,    SegmentEdge.Top);
            SyncAndWarn(ref _bottomEdge, SegmentEdge.Bottom);
        }

        void SyncAndWarn(ref SegmentEdgeConfig cfg, SegmentEdge edge)
        {
            // SceneAsset (editor drag-drop) → runtime path string (the build-safe value Story 5.4 reads).
            cfg.targetScenePath = cfg.targetSceneAsset != null
                ? AssetDatabase.GetAssetPath(cfg.targetSceneAsset)
                : string.Empty;

            switch (cfg.behavior)
            {
                case EdgeBehavior.Transport when cfg.targetSegment == null:
                    Debug.LogWarning($"[SegmentConfig] '{name}' {edge} edge is Transport but has no targetSegment.", this);
                    break;
                case EdgeBehavior.Transport when cfg.targetSegment.gameObject.scene != gameObject.scene:
                    Debug.LogWarning($"[SegmentConfig] '{name}' {edge} edge Transport targetSegment is in a DIFFERENT scene — Transport must be same-scene (Ruling 1). Use a Scene edge for cross-scene destinations.", this);
                    break;
                case EdgeBehavior.Scene when string.IsNullOrEmpty(cfg.targetScenePath):
                    Debug.LogWarning($"[SegmentConfig] '{name}' {edge} edge is Scene but has no targetSceneAsset assigned.", this);
                    break;
            }
        }
#endif
    }
}
