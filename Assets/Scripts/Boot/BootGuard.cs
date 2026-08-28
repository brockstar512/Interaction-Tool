using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.Boot
{
    // Lazy-init guard: runs BEFORE the first scene loads, in builds AND editor play — including when
    // you press Play directly in a test scene (the dev loop). Guarantees SystemsRoot + GameConfig
    // exist (JSON-or-defaults) so nothing NREs. It's static, so it holds no Inspector references.
    public static class BootGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            BootInit.EnsureSystems();
            // 4.6.2 R5 (quality-audit #32 / SAVE.3 F-d, note-only nicety LANDED):
            // one line disambiguating direct-play from Bootstrap-play in the
            // Console. Fires once for the FIRST scene only; the Boot scene
            // (buildIndex 0) stays silent — its own boot-branch logs disambiguate.
            SceneManager.sceneLoaded += LogBootPathOnce;
        }

        static void LogBootPathOnce(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= LogBootPathOnce;
            if (scene.buildIndex != 0)
                Debug.Log($"[Boot] direct-play path — scene '{scene.name}' entered without Boot.unity (no save read, no auto-continue).");
        }
    }
}
