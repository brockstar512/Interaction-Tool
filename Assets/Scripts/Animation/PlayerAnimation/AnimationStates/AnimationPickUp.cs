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


    public async Task Play(PlayerStateMachineManager state)
    {
        Transform originHolder = state.GetComponentInChildren<OriginPoint>().transform;
        if (state.item is Throwable throwItem)
        {
            throwItem.transform.SetParent(originHolder);
        }
        
        if (state.currentState.LookDirection == Vector2.down)
        {
            state.animator.Play(PickUpDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpDown]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.right)
        {
            state.animator.Play(PickUpRight);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpRight]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.left)
        {
            state.animator.Play(PickUpLeft);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpLeft]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.up)
        {
            state.animator.Play(PickUpUp);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpUp]);
            return;
        }
    }
}

