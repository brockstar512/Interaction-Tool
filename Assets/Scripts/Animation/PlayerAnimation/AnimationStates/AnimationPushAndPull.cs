using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPushAndPull : AnimationState
{
    const string PushDown = "PushDown";
    const string PushUp = "PushUp";
    const string PushRight = "PushRight";
    const string PushLeft = "PushLeft";

    const string PushHoldLeft = "PushHoldLeft";
    const string PushHoldRight = "PushHoldRight";
    const string PushHoldUp = "PushHoldUp";
    const string PushHoldDown = "PushHoldDown";

    public override void Play(PlayerStateMachineManager playerstate)
    {
        throw new System.NotImplementedException();
    }
}
