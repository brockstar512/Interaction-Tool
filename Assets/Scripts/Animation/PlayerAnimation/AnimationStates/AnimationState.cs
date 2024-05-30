using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public abstract class AnimationState
{
    public abstract void Play(PlayerStateMachineManager playerstate);
}
