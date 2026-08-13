using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.Core.Config
{
    // PB.5 (DD2/DD3): per-field carry-over policy for a Location-entry boundary.
    //   Fresh     = ignore incoming DTO field, seed from the config chain.
    //   CarryOver = DTO field if present + valid, else chain.
    //   Strict    = DTO required; missing/invalid -> LOUD structured log + chain (fail-alive).
    // Death-respawn does NOT consult these (DD5-b, owner-ruled): FRESH stays immutable
    // there; extension is a designed future step, trigger = Epic 4.6 continue-flow.
    public enum CarryOverMode { Fresh, CarryOver, Strict }

    // PB.5 (DD2): first-spawn control mode. InVehicle is DATA-complete but THIN-WIRED in
    // v1 (OQ-C, owner-ruled): resolution logs the named gap and spawns OnFoot; full
    // wiring lands with the next story that touches the possession spine (5.4 / Epic 7).
    public enum StartMode { OnFoot, InVehicle }

    // PB.5 (DD1, owner-ruled): scene-placed serialized component, ONE per scene — the
    // C-B §12 row-2 placement ("lives next to the object it configures"; SpawnMarker
    // precedent). NOT per-scene JSON, NOT a ScriptableObject. A scene WITHOUT one runs
    // on all-global defaults with exactly one warn (OQ-D, fail-alive).
    // Naming: rename to LocationConfig is DEFERRED to the v1 refactor pass (Directive 3).
    // NO spawnPoints field (OQ-A, owner-ruled): respawn points are SpawnMarker's
    // (PB.4); named Transport-edge targets are the Directive-3 Location/Zone tier's,
    // landing with Epic 5 Transport (5.4). The AC's spawnPoints entry is re-homed, not
    // implemented here — duplicating a registry as config data is the two-sources-of-
    // truth defect the OQ-PB45-G ruling killed.
    public class LevelConfig : MonoBehaviour
    {
        [Header("Seeds (first-launch / fresh-join route, DD6)")]
        [Tooltip("<= 0 means no override — GameConfig.DefaultLivesCount applies (A-1 chain).")]
        [SerializeField] int livesOverride = 0;
        [SerializeField] StartMode startMode = StartMode.OnFoot;
        [Tooltip("DURABLE item keys (ItemStateRegistry space: \"candle\", \"key\", \"masterkey\").")]
        [SerializeField] List<string> startingInventory = new List<string>();

        [Header("Per-field carry-over at Location entry (DD2/DD3)")]
        // statuses default = CarryOver is OWNER-RULED (OQ-B / Directive 2's persist
        // default). The other three default CarryOver for consistency with it —
        // an unauthored field behaves like the directive's global default.
        [SerializeField] CarryOverMode health = CarryOverMode.CarryOver;
        [SerializeField] CarryOverMode lives = CarryOverMode.CarryOver;
        [SerializeField] CarryOverMode inventory = CarryOverMode.CarryOver;
        [SerializeField] CarryOverMode statuses = CarryOverMode.CarryOver;

        public bool HasLivesOverride => livesOverride > 0;
        public int LivesOverride => livesOverride;
        public StartMode StartMode => startMode;
        public IReadOnlyList<string> StartingInventory => startingInventory;
        public CarryOverMode HealthCarry => health;
        public CarryOverMode LivesCarry => lives;
        public CarryOverMode InventoryCarry => inventory;
        public CarryOverMode StatusesCarry => statuses;

        // DD1 discovery + OQ-D fail-alive: null is a LEGAL result (all-global defaults);
        // the warn fires once per scene, not per caller.
        static string _warnedScene;
        public static LevelConfig Resolve()
        {
            var found = FindFirstObjectByType<LevelConfig>();
            if (found == null)
            {
                var scene = SceneManager.GetActiveScene().name;
                if (_warnedScene != scene)
                {
                    _warnedScene = scene;
                    Debug.LogWarning($"[LevelConfig] no LevelConfig in scene '{scene}' — all-global defaults apply (OQ-D fail-alive).");
                }
            }
            return found;
        }

        // DD2: presets are AUTHORING SUGAR — they expand to the four per-field values,
        // which are the runtime truth (kickoff ruling). Edit-mode ContextMenu.
        [ContextMenu("Preset: Fresh (all four fields)")]
        void PresetFresh() => ApplyPreset(CarryOverMode.Fresh);
        [ContextMenu("Preset: CarryOver (all four fields)")]
        void PresetCarryOver() => ApplyPreset(CarryOverMode.CarryOver);
        [ContextMenu("Preset: Strict (all four fields)")]
        void PresetStrict() => ApplyPreset(CarryOverMode.Strict);

        void ApplyPreset(CarryOverMode mode)
        {
            // PB.5 R6.2 (sweep findings): the ContextMenu is reachable in Play mode, where
            // the write dies with the session — refuse loudly rather than silently not
            // persisting (this bit the R6 sweep: a Play-mode preset read as authored).
            if (Application.isPlaying)
            {
                Debug.LogWarning($"[LevelConfig] preset '{mode}' REFUSED in Play mode — presets are edit-mode authoring; the write would not persist.");
                return;
            }
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, $"LevelConfig preset: {mode}");   // R6.2: preset application is undoable
#endif
            health = lives = inventory = statuses = mode;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);   // persist the edit-mode change
#endif
            Debug.Log($"[LevelConfig] preset '{mode}' applied to all four fields ({gameObject.scene.name}).");
        }
    }
}
