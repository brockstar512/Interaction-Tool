using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AnimationEquipItem : AnimationStateAsync
{
    const string PickUpRight = "PickUpRight";
    const string PickUpUp = "PickUpUp";
    const string PickUpDown = "PickUpDown";
    const string PickUpLeft = "PickUpLeft";

    public override Task Play(PlayerStateMachineManager playerstate)
    {
        throw new System.NotImplementedException();
    }
}
