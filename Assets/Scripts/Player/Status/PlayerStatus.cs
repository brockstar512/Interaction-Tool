using System;

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

        public event Action<int> LivesChange;

        public void UpdateLives(int life)
        {
            Lives += life;
            LivesChange?.Invoke(life);
        }

        // Story PB.1 (PlayerStateBuilder restore path). LivesChange carries the DELTA — the
        // event's existing semantics, deliberately unchanged; HUD listeners hear the same shape.
        public void RestoreLives(int value)
        {
            var delta = value - Lives;
            Lives = value;
            LivesChange?.Invoke(delta);
        }
    }
}
