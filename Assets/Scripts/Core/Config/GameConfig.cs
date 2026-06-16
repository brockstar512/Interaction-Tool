namespace IT.Core.Config
{
    // Immutable runtime accessor over the parsed settings (C-B: plain C#, not a ScriptableObject).
    // Story 2.1 holds the minimal v1 schema; Story 2.2 extends; Story 2.3 adds field-level validation.
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
            MaxPlayers      = s.maxPlayers;
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
            // v1: only Topdown ships. Unknown/blank → Topdown (proper field validation is Story 2.3).
            return System.Enum.TryParse(raw, ignoreCase: true, out GameType g) ? g : GameType.Topdown;
        }
    }
}
