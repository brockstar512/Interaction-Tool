using IT.Core.WorldState;
using IT.Player.Control;
using IT.Segments;
using UnityEngine;

namespace IT.Boot
{
    using IT.Core.Config;

    // Persistent (DontDestroyOnLoad) host for true-global systems (C-C). Story 2.1 holds only the
    // GameConfig; PlayerRoster (Epic 3), WorldState (2.5), CameraController (Epic 6), SegmentManager
    // (Epic 5) and the reworked HUDManager (7.2) register here as they're built.
    public class SystemsRoot : MonoBehaviour
    {
        public static SystemsRoot Instance { get; private set; }

        public GameConfig Config { get; private set; }
        public WorldState WorldState { get; private set; }

        // Story PB.4 (DD2) — the death-respawn orchestrator, hosted here (true-global, C-C) rather
        // than scene-placed so it exists in every scene automatically (L9's join-race environment).
        // A plain component, reached via this accessor — NOT a new Singleton<T>. Null-guard reads
        // as SystemsRoot.Instance?.Spawn (the Instance?.Config posture).
        public SpawnManager Spawn { get; private set; }

        // 4.6.1 (DD1, DQ-1(c)/DQ-2(c) ruled): the presentation coordinator — same
        // hosting shape as Spawn. Scene-local canvases live on PresentationRoot
        // (ensured per scene by code); this holds only the typed handles.
        public IT.Presentation.Presentation Presentation { get; private set; }

        // Created only by BootInit.EnsureSystems() — not placed in a scene by hand.
        public static SystemsRoot Create(GameConfig config)
        {
            if (Instance != null) return Instance;

            var go = new GameObject("SystemsRoot");
            DontDestroyOnLoad(go);
            var root = go.AddComponent<SystemsRoot>();   // Awake sets Instance
            root.Config = config;
            root.WorldState = new WorldState();
            go.AddComponent<PlayerRoster>();
            // Story PB.4 (DD2): the respawn orchestrator. AFTER PlayerRoster (mirrors the SegmentManager
            // ordering) so it can reach a live roster; hosted here so it exists in every scene without
            // Inspector wiring — L9's join-race environment is then automatic.
            root.Spawn = go.AddComponent<SpawnManager>();
            // 4.6.1 (DD1): the presentation coordinator — AFTER SpawnManager (its
            // game-over branch will reach Presentation via Instance?, R4).
            root.Presentation = go.AddComponent<IT.Presentation.Presentation>();
            // 4.6.1 R6.1 (Session A defect, ordering half): arm the per-scene ensure
            // hook AFTER the property assignment above — arming from Presentation.Awake
            // ran mid-AddComponent, when Instance.Presentation was still null, so the
            // immediately-ensured root's module could never register.
            IT.Presentation.PresentationRoot.ArmEnsurePerScene();
            // Story 5.1: true-global segment membership (C-C), the SystemsRoot slot reserved above.
            // AFTER PlayerRoster so SegmentManager.Awake finds a live roster to subscribe PlayerLeft on.
            go.AddComponent<SegmentManager>();
            // Story 5.2: routes SegmentManager crossings into behaviour-typed request events (C-G, not a
            // parallel bus). AFTER SegmentManager so SegmentRouter.Awake finds a live manager to subscribe to.
            go.AddComponent<SegmentRouter>();
            return root;
        }

        void Awake()
        {
            // Guard against a stray duplicate (e.g. one accidentally dropped into a scene).
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
    }
}
