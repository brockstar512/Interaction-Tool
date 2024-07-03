using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMove : AnimationState
{
    const string WalkRight = "WalkRight";
    const string WalkUp = "WalkUp";
    const string WalkDown = "WalkDown";
    const string WalkLeft = "WalkLeft";

    const string StandRight = "StandRight";
    const string StandLeft = "StandLeft";
    const string StandUp = "StandUp";
    const string StandDown = "StandDown";

    public void Play(PlayerStateMachineManager state)
    {
        if (state.Movement.x != 0 || state.Movement.y != 0)
        {
            //Debug.Log("walking");
            if (state.currentState.LookDirection == Vector2.down)
            {
                state.animator.Play(WalkDown);
            }
            if (state.currentState.LookDirection == Vector2.up)
            {
                state.animator.Play(WalkUp);
            }
            if (state.currentState.LookDirection == Vector2.right)
            {
                state.animator.Play(WalkRight);

            }
            if (state.currentState.LookDirection == Vector2.left)
            {
                state.animator.Play(WalkLeft);
            }

        }
        else
        {
            if (state.currentState.LookDirection == Vector2.down)
            {
                state.animator.Play(StandDown);
            }
            if (state.currentState.LookDirection == Vector2.up)
            {
                state.animator.Play(StandUp);
            }
            if (state.currentState.LookDirection == Vector2.right)
            {
                state.animator.Play(StandRight);

            }
            if (state.currentState.LookDirection == Vector2.left)
            {
                state.animator.Play(StandLeft);
            }
        }

    }

}
