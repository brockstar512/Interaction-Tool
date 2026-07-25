using System;
using UnityEngine;

namespace IT.Player.Status
{
    // Health fields removed in Story 4.1 — Health component owns health data.
    // Lives fields kept; Lives-component extraction is a future story (OQ-4.1-B locked).
    // Story PB.1: seed arrives via the ctor (GameConfig.DefaultLivesCount chain) — no hardcoded value.
    public class PlayerStatus
    {
        public PlayerStatus(int startingLives)
        {
            Lives = startingLives;
        }

        int Lives;

        public int CurrentLives => Lives;

        // Carries the TOTAL (Story PB.1 sub-step R5.1, review R-02 ruling — was the delta;
        // the delta shape made the HUD render "X0"/"X-2" on restore).
        public event Action<int> LivesChange;

        public void UpdateLives(int life)
        {
            Lives += life;
            LivesChange?.Invoke(Lives);
        }

        // Story PB.1 (PlayerStateBuilder restore path) + PB.4 R4.1. Fail-alive posture (review R-01,
        // mirroring Health.RestoreCurrent / OQ-PB1-B). Floor lowered 1 → 0 under OQ-PB4-B (2026-07-24):
        // "lives" = REMAINING RESPAWNS, so X0 (last life) is a LEGAL live state, not a dead player — the
        // respawn path writes 0 legally and must not be clamped up. Only a NEGATIVE (a corrupt DTO) is
        // clamped now — visible warning, alive player at X0. The respawn path never passes < 0
        // (SpawnManager's game-over branch diverts it first), so this floor guards only the scene-restore
        // path against a bad DTO.
        public void RestoreLives(int value)
        {
            if (value < 0)
            {
                Debug.LogWarning($"[PlayerStatus] RestoreLives clamped {value} → 0 (fail-alive floor)");
                value = 0;
            }
            Lives = value;
            LivesChange?.Invoke(Lives);
        }
    }
}
