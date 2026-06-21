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

            // TODO: wrap in an ITransition fade (Story 5.4) instead of a hard cut.
            SceneManager.LoadScene(firstScene);
        }
    }
}
