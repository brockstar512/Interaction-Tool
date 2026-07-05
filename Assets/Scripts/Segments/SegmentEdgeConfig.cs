using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IT.Segments
{
    /// <summary>
    /// Per-edge routing data for one segment edge (Story 5.2). Four of these are authored inside
    /// <c>SegmentConfig</c> — one per cardinal edge (Rung 3). Plain serialized value data (C-B):
    /// no ScriptableObjects, no colliders, no Physics2D.
    ///
    /// A [Serializable] STRUCT for value semantics + cheap pass-by-value into the router's request
    /// payloads (Rung 5). NOTE: it is NOT readonly-immutable — Unity Inspector serialization requires
    /// settable fields, so the fields are public and mutable; treat an authored instance as a value.
    ///
    /// Destination fields are behaviour-specific and only meaningful for their <see cref="behavior"/>:
    ///   • <see cref="EdgeBehavior.Transport"/> → <see cref="targetSegment"/> (a SegmentBounds in the
    ///     SAME scene — Ruling 1 invariant).
    ///   • <see cref="EdgeBehavior.Scene"/>     → <see cref="targetScenePath"/> (runtime scene ref),
    ///     authored via the editor-only <c>targetSceneAsset</c> drag-drop that
    ///     <c>SegmentConfig.OnValidate</c> syncs into the string (Rung 3). Story 5.4's
    ///     LocationTransporter feeds that path to <c>SceneManager.LoadScene</c>.
    /// </summary>
    [Serializable]
    public struct SegmentEdgeConfig
    {
        /// <summary>What crossing OUT through this edge triggers. Default <see cref="EdgeBehavior.Seamless"/> (fires nothing).</summary>
        public EdgeBehavior behavior;

        /// <summary>
        /// Transport destination — a <c>SegmentBounds</c> in the SAME scene (Ruling 1). A normal Unity
        /// object reference, so it serializes at runtime. Null (incl. a nulled cross-scene ref) ⇒ the
        /// router logs-and-skips (DD4.1).
        /// </summary>
        public SegmentBounds targetSegment;

#if UNITY_EDITOR
        /// <summary>
        /// Scene destination — AUTHOR-TIME ONLY. <c>UnityEditor.SceneAsset</c> is editor-only, so this
        /// field is stripped from builds; <c>SegmentConfig.OnValidate</c> (Rung 3) syncs it into
        /// <see cref="targetScenePath"/> for the runtime value Story 5.4 consumes. Guarded by
        /// <c>#if UNITY_EDITOR</c> so runtime code never references it (C-B, build-safe).
        /// </summary>
        public SceneAsset targetSceneAsset;
#endif

        /// <summary>
        /// Scene destination — the runtime-safe value (scene path from the SceneAsset). This is what
        /// Story 5.4 reads (fed to <c>SceneManager.LoadScene</c>); it survives into builds where
        /// <c>targetSceneAsset</c> does not.
        /// </summary>
        public string targetScenePath;

        /// <summary>
        /// Spawn point ID as an opaque string. Resolved to target-side <c>SpawnPoint</c> data at
        /// SegmentRouter dispatch time (Rung 4); Story 5.4 (LocationTransporter) executes the actual
        /// player positioning. Kept a plain string here (sufficient for authoring); a stronger
        /// dispatch-time type contract is deferred to Rung 4+ per the Gap 3 forward-compat note.
        /// </summary>
        public string spawnPointId;
    }
}
