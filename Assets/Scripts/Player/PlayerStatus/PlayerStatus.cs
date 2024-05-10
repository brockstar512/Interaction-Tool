using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus
{

    int _health;
    int Lives;

    public event Action<int> HealthChange;
    public event Action<int> LivesChange;






    public void UpdateHealth(int HP)
    {

    }

    public void UpdateLives()
    {

    }

}
