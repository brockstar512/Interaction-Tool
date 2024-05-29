using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCarry
{
    readonly int HoldStillRight = Animator.StringToHash("HoldStillRight");
    readonly int HoldStillLeft = Animator.StringToHash("HoldStillLeft");
    readonly int HoldStillUp = Animator.StringToHash("HoldStillUp");
    readonly int HoldStillDown = Animator.StringToHash("HoldStillDown");


    readonly int HoldWalkRight = Animator.StringToHash("HoldWalkRight");
    readonly int HoldWalkLeft = Animator.StringToHash("HoldWalkLeft");
    readonly int HoldWalkUp = Animator.StringToHash("HoldWalkUp");
    readonly int HoldWalkDown = Animator.StringToHash("HoldWalkDown");
    readonly Dictionary<int, float> TimeSheet;

    public AnimationCarry()
    {
        TimeSheet = new()
        {
            { HoldStillRight,1f},
            { HoldStillLeft,1f},
            { HoldStillUp, 1f},
            { HoldStillDown,1f },
            //walking
            { HoldWalkRight,.333f},
            { HoldWalkLeft,.333f},
            { HoldWalkUp, .333f},
            { HoldWalkDown,.333f }
        };
    }

    public void Play(PlayerStateMachineManager playerstate)
    {
        if (playerstate.Movement.x != 0 || playerstate.Movement.y != 0)
        {
            if (playerstate.LookDirection == Vector2.down)
            {
                playerstate.animator.Play(HoldWalkDown);
            }
            if (playerstate.LookDirection == Vector2.up)
            {
                playerstate.animator.Play(HoldWalkUp);
            }
            if (playerstate.LookDirection == Vector2.right)
            {
                playerstate.animator.Play(HoldWalkRight);

            }
            if (playerstate.LookDirection == Vector2.left)
            {
                playerstate.animator.Play(HoldWalkLeft);
            }

        }
        else
        {
            if (playerstate.LookDirection == Vector2.down)
            {
                playerstate.animator.Play(HoldStillDown);
            }
            if (playerstate.LookDirection == Vector2.up)
            {
                playerstate.animator.Play(HoldStillUp);
            }
            if (playerstate.LookDirection == Vector2.right)
            {
                playerstate.animator.Play(HoldStillRight);

            }
            if (playerstate.LookDirection == Vector2.left)
            {
                playerstate.animator.Play(HoldStillLeft);
            }
        }

    }

}
