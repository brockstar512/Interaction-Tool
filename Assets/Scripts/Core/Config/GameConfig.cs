using UnityEngine;

namespace IT.Core.Config
{
    // Immutable runtime accessor over the parsed settings (C-B: plain C#, not a ScriptableObject).
    public class GameConfig
    {
        public GameType GameType { get; }
        public int MaxPlayers { get; }
        public bool BackpackEnabled { get; }
        public bool ItemsUpgradable { get; }
        public bool SaveEnabled { get; }
        public bool CraftingEnabled { get; }
        public bool DialogueEnabled { get; }

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
