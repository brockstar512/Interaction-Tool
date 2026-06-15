using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.Status
{
    public class PlayerStatus
    {
        public PlayerStatus()
        {
            Health = 10;
            Lives = 3;
        }

        int Health;
        int Lives;

        public event Action<int> HealthChange;
        public event Action<int> LivesChange;


        public void UpdateHealth(int HP)
        {
            Health += HP;
            // NOTE: totals/normalization fix lands in Health (Story 4.1) — HP here is the delta,
            // and PlayerStatusHUD.UpdateHealth writes it straight into Image.fillAmount (expects 0-1).
            // Story 1.2 scope is null-safety only; do NOT change the payload here.
            HealthChange?.Invoke(HP);
        }

        public void UpdateLives(int Life)
        {
            Lives += Life;
            LivesChange?.Invoke(Life);
        }

    }
}
