using System;

namespace IT.Player.Status
{
    // Health fields removed in Story 4.1 — Health component owns health data.
    // Lives fields kept; Lives-component extraction is a future story (OQ-4.1-B locked).
    public class PlayerStatus
    {
        public PlayerStatus()
        {
            Lives = 3;
        }

        int Lives;

        public event Action<int> LivesChange;

        public void UpdateLives(int life)
        {
            Lives += life;
            LivesChange?.Invoke(life);
        }
    }
}
