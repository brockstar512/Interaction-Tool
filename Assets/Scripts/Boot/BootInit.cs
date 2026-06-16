namespace IT.Boot
{
    using IT.Core.Config;

    // The ONE idempotent init routine. Called by both BootGuard (direct-play of any scene) and
    // GameBootstrap (Boot scene) so behavior is identical no matter how the game starts.
    public static class BootInit
    {
        public static void EnsureSystems()
        {
            if (SystemsRoot.Instance != null) return;        // already initialized — no-op

            GameConfig config = new JsonGameConfigSource().Load();   // JSON-or-defaults
            SystemsRoot.Create(config);
        }
    }
}
