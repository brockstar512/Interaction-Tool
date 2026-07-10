using System;
using System.Collections.Generic;
using IT.Player.Control;

namespace IT.Player.Persistence
{
    // Story PB.1 (C-H machinery). THE one shape player state crosses boundaries in —
    // scene transitions and save/load alike. Schema-complete (spec DD1): every v1 field
    // is declared now; later stories FILL fields, they never reshape the struct.
    // Deliberately non-spatial — position/facing are OUT (Q1 amendment + OQ-PB1-A:
    // spawn points own spatial state) — and scene-blind: currentSceneId lives on
    // SAVE.1's SaveGameDTO envelope ALONGSIDE this, never on it (DD8). Transients
    // (i-frames, FSM state, status effects, carried throwables, async timers) never
    // cross (C-H).
    [Serializable]
    public struct PlayerStateDTO
    {
        public string playerId;     // schema only — populated by PB.4 (identity/lifecycle)
        public string deviceId;     // schema only — populated by PB.4 (deviceId spike = PB.4 Step 1)

        public WrapperState wrapperState;   // captured on EVERY path; restore POLICY differs by RestoreMode (DD5)

        public int currentHealth;   // Health.Current — Max stays prefab-authored, never serialized
        public int lives;           // PlayerStatus lives (seeded via the GameConfig.DefaultLivesCount chain)

        public List<ItemStateDTO> items;   // schema only — capture/restore is PB.3 (ISerializableItem + registry)
        public int currentItemIndex;       // schema only — PB.3
    }

    // Per-item runtime state envelope (planning §4 shape). PB.1 ships the shape;
    // PB.3's registry produces and consumes it.
    [Serializable]
    public struct ItemStateDTO
    {
        public string itemType;
        public string instanceState;
    }
}
