using UnityEngine;

namespace IT.Core.Config
{
    // Immutable runtime accessor over the parsed settings (C-B: plain C#, not a ScriptableObject).
    public class GameConfig
    {
        public GameType GameType { get; }
        public int MaxPlayers { get; }
        public int DefaultLivesCount { get; }
        public bool BackpackEnabled { get; }
        public bool ItemsUpgradable { get; }
        public bool SaveEnabled { get; }
        public bool CraftingEnabled { get; }
        public bool DialogueEnabled { get; }

        // Single source for the lives fallback (review R-13): the GameSettings field default,
        // the invalid-value fallback below, and PlayerStatusManager's defensive seed all read
        // this — the three literals can no longer diverge. (MaxPlayers' 4 has the same two-site
        // shape — left as-is per the R-13 lives-only scope; quality-audit #15 covers the config
        // bounds audit.)
        public const int FallbackDefaultLives = 3;

        // SAVE.4 (DD1, R-13 one-source shape): save-slot cap. Valid range 1-5 (owner
        // re-ruling 2026-08-19); invalid/missing falls back HERE, not to a literal.
        public const int FallbackSaveSlotLimit = 1;
        public int SaveSlotLimit { get; }

        public GameConfig(GameSettings s)
        {
            GameType        = ParseGameType(s.gameType);

            if (s.maxPlayers <= 0)
            {
                Debug.LogWarning($"[GameConfig] Field 'maxPlayers': value {s.maxPlayers} is invalid (must be > 0) — defaulting to 4.");
                MaxPlayers = 4;
            }
            else
            {
                MaxPlayers = s.maxPlayers;
            }

            if (s.defaultLivesCount <= 0)
            {
                Debug.LogWarning($"[GameConfig] Field 'defaultLivesCount': value {s.defaultLivesCount} is invalid (must be > 0) — defaulting to {FallbackDefaultLives}.");
                DefaultLivesCount = FallbackDefaultLives;
            }
            else
            {
                DefaultLivesCount = s.defaultLivesCount;
            }

            if (s.saveSlotLimit < 1 || s.saveSlotLimit > 5)
            {
                Debug.LogWarning($"[GameConfig] Field 'saveSlotLimit': value {s.saveSlotLimit} is invalid (must be 1-5) — defaulting to {FallbackSaveSlotLimit}.");
                SaveSlotLimit = FallbackSaveSlotLimit;
            }
            else
            {
                SaveSlotLimit = s.saveSlotLimit;
            }

            BackpackEnabled = s.backpackEnabled;
            ItemsUpgradable = s.itemsUpgradable;
            SaveEnabled     = s.saveEnabled;
            CraftingEnabled = s.craftingEnabled;
            DialogueEnabled = s.dialogueEnabled;
        }

        // Hardcoded fallback (Topdown / 4 / all flags false) — used when the JSON is missing/unreadable.
        public static GameConfig Default => new GameConfig(new GameSettings());

        static GameType ParseGameType(string raw)
        {
            if (System.Enum.TryParse(raw, ignoreCase: true, out GameType g)) return g;
            Debug.LogWarning($"[GameConfig] Field 'gameType': unrecognised value '{raw}' — defaulting to Topdown.");
            return GameType.Topdown;
        }
    }
}
