using System;

namespace IT.Core.Config
{
    // Plain [Serializable] mirror of game-settings.json (minimal v1 schema; Story 2.2 extends it
    // with input options etc.). Field names MUST match the JSON keys exactly — JsonUtility is
    // case-sensitive and ignores unknown/missing keys (missing → these defaults).
    [Serializable]
    public class GameSettings
    {
        public string gameType = "Topdown";
        public int maxPlayers = 4;
        public int defaultLivesCount = GameConfig.FallbackDefaultLives;   // PB.1: global lives seed; LevelConfig per-scenario override lands in PB.5
        public bool backpackEnabled = false;
        public bool itemsUpgradable = false;
        public bool saveEnabled = false;
        public bool craftingEnabled = false;
        public bool dialogueEnabled = false;
    }
}
