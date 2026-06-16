using UnityEngine;

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

            GameConfig config = GameConfigLoader.GetSource().Load();   // platform-selected source
            SystemsRoot.Create(config);

            // NFR-1 no-recompile proof: edit game-settings.json and press Play — this line changes
            // without recompiling. Remove or gate behind a flag if log noise is an issue post-v1.
            Debug.Log($"[Config] gameType={config.GameType} maxPlayers={config.MaxPlayers} " +
                      $"backpack={config.BackpackEnabled} itemsUpgradable={config.ItemsUpgradable} " +
                      $"save={config.SaveEnabled} crafting={config.CraftingEnabled} " +
                      $"dialogue={config.DialogueEnabled}");
        }
    }
}
