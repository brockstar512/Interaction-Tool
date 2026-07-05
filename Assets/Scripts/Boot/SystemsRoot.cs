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
