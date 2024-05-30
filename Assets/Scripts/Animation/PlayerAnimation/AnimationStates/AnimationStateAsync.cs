using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public abstract class AnimationStateAsync
{
    public abstract Task Play(PlayerStateMachineManager playerstate);
}
