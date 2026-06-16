using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.Boot
{
    // Lives on a GameObject in Boot.unity (build index 0). Ensures the systems exist, then loads the
    // first gameplay scene. NOTE: BootGuard already runs EnsureSystems() before this scene loads, so
    // the call here is belt-and-suspenders — it's idempotent (a no-op if already initialized).
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Scene name to load after boot. Must be in Build Settings.")]
        [SerializeField] private string firstScene = "Level 1 Test";

        void Start()
        {
            BootInit.EnsureSystems();

            // TODO: wrap in an ITransition fade (Story 5.4) instead of a hard cut.
            SceneManager.LoadScene(firstScene);
        }
    }
}
