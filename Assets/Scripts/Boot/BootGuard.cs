using UnityEngine;

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
        }
    }
}
