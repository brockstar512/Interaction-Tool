using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMoveState 
{
    const string WalkRight = "WalkRight";
    const string WalkUp = "WalkUp";
    const string WalkDown = "WalkDown";
    const string WalkLeft = "WalkLeft";

    const string StandRight = "StandRight";
    const string StandLeft = "StandLeft";
    const string StandUp = "StandUp";
    const string StandDown = "StandDown";

    public void Play(PlayerStateMachineManager playerstate)
    {
        if (playerstate.Movement.x != 0 || playerstate.Movement.y != 0)
        {
            if (playerstate.LookDirection == Vector2.down)
            {
                playerstate.animator.Play(WalkDown);
            }
            if (playerstate.LookDirection == Vector2.up)
            {
                playerstate.animator.Play(WalkUp);
            }
            if (playerstate.LookDirection == Vector2.right)
            {
                playerstate.animator.Play(WalkRight);

            }
            if (playerstate.LookDirection == Vector2.left)
            {
                playerstate.animator.Play(WalkLeft);
            }

        }
        else
        {
            if (playerstate.LookDirection == Vector2.down)
            {
                playerstate.animator.Play(StandDown);
            }
            if (playerstate.LookDirection == Vector2.up)
            {
                playerstate.animator.Play(StandUp);
            }
            if (playerstate.LookDirection == Vector2.right)
            {
                playerstate.animator.Play(StandRight);

            }
            if (playerstate.LookDirection == Vector2.left)
            {
                playerstate.animator.Play(StandLeft);
            }
        }

    }

}
