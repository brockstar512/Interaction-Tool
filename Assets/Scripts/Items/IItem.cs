using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
    public Sprite Sprite { get; }
    public void Use(PlayerStateMachineManager stateManager);
}
