using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AnimationPickUp : AnimationState
{
    readonly int PickUpRight = Animator.StringToHash("PickUpRight");
    readonly int PickUpUp = Animator.StringToHash("PickUpUp");
    readonly int PickUpDown = Animator.StringToHash("PickUpDown");
    readonly int PickUpLeft = Animator.StringToHash("PickUpLeft");

    readonly Dictionary<int, float> TimeSheet;


    public AnimationPickUp()
    {
        TimeSheet = new()
        {
            { PickUpRight, 0.250f },
            { PickUpUp,0.250f  },
            { PickUpDown, 0.250f },
            { PickUpLeft, 0.250f }
        };

    }


    public async Task Play(PlayerStateMachineManager playerstate)
    {
        if (playerstate.LookDirection == Vector2.down)
        {
            playerstate.animator.Play(PickUpDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpDown]);
            return;
        }
        if (playerstate.LookDirection == Vector2.right)
        {
            playerstate.animator.Play(PickUpRight);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpRight]);
            return;
        }
        if (playerstate.LookDirection == Vector2.left)
        {
            playerstate.animator.Play(PickUpLeft);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpLeft]);
            return;
        }
        if (playerstate.LookDirection == Vector2.up)
        {
            playerstate.animator.Play(PickUpUp);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpUp]);
            return;
        }
    }
}

