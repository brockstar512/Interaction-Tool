using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        HealthChange.Invoke(HP);
    }

    public void UpdateLives(int Life)
    {
        Lives += Life;
        LivesChange.Invoke(Life);
    }

}
