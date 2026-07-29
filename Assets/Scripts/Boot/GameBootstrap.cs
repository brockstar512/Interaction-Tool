using IT.Player.Control;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.Boot
{
    // Lives on a GameObject in Boot.unity (build index 0). Ensures the systems exist, then loads the
    // first gameplay scene. NOTE: BootGuard already runs EnsureSystems() before this scene loads, so
    // the call here is belt-and-suspenders — it's idempotent (a no-op if already initialized).
    //
    // Inspector-wired references live here (architecture D1) — the intended authoring surface for
    // anything that can't be set from JSON or discovered at runtime.
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Scene name to load after boot. Must be in Build Settings.")]
        [SerializeField] private string firstScene = "Level 1 Test";

        [Tooltip("Player prefab instantiated for P2+ join (Story 3.3). " +
                 "Null in direct-play (BootGuard) path — P2 join requires Boot.unity.")]
        [SerializeField] private GameObject playerPrefab;

        void Start()
        {
            BootInit.EnsureSystems();

            // TryGetInstance (not Instance) so we never silently auto-create a throwaway,
            // non-persistent PlayerRoster if EnsureSystems() failed to add the real one —
            // that would swallow the prefab and break P2+ join with no obvious cause.
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null)
                Debug.LogError("[GameBootstrap] PlayerRoster missing after EnsureSystems() — " +
                               "PlayerPrefab not wired; P2+ join will fail.");
            else if (playerPrefab != null)
                roster.PlayerPrefab = playerPrefab;

            // PB.4 (R3-Q4): feed the same player prefab to the SpawnManager for death-respawn (arch D1 —
            // one wiring line here). Null-guarded like the Instance?.Config posture.
            var spawn = SystemsRoot.Instance?.Spawn;
            if (spawn != null && playerPrefab != null)
                spawn.PlayerPrefab = playerPrefab;

            // TODO: wrap in an ITransition fade (Story 5.4) instead of a hard cut.
            // PB.4.5 R3.1: lock joins through the boot load (the only runtime LoadScene site
            // today); the roster reopens itself on activeSceneChanged. Epic 5's Transport
            // loading inherits this same pair of calls when it is built.
            var joinRoster = PlayerRoster.TryGetInstance();
            if (joinRoster != null) joinRoster.JoinPolicy = LockedJoinPolicy.Instance;
            SceneManager.LoadScene(firstScene);
        }
    }
}
