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

        // Story PB.1 (PlayerStateBuilder restore path). Clamps to a floor of 1 — the
        // fail-alive posture (review R-01 ruling, mirroring Health.RestoreCurrent /
        // OQ-PB1-B): a restore must never materialize a zero-or-negative-lives player
        // from a bad DTO — visible warning, alive player.
        public void RestoreLives(int value)
        {
            if (value < 1)
            {
                Debug.LogWarning($"[PlayerStatus] RestoreLives clamped {value} → 1 (fail-alive floor)");
                value = 1;
            }
            Lives = value;
            LivesChange?.Invoke(Lives);
        }
    }
}
