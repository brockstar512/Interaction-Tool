using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AnimationThrow : AnimationState
{
    readonly int ThrowRight = Animator.StringToHash("ThrowRight");
    readonly int ThrowUp = Animator.StringToHash("ThrowUp");
    readonly int ThrowDown = Animator.StringToHash("ThrowDown");
    readonly int ThrowLeft = Animator.StringToHash("ThrowLeft");


    readonly Dictionary<int, float> TimeSheet;


    public AnimationThrow()
    {
        TimeSheet = new()
        {
            { ThrowRight, 0.500f },
            { ThrowUp,0.500f},
            { ThrowDown, 0.500f},
            { ThrowLeft,0.500f },
        };

    }

    public async Task Play(PlayerStateMachineManager state)
    {
        if (state.currentState.LookDirection == Vector2.down)
        {
            state.animator.Play(ThrowDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowDown]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.right)
        {
            state.animator.Play(ThrowRight);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowRight]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.left)
        {
            state.animator.Play(ThrowLeft);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowLeft]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.up)
        {
            state.animator.Play(ThrowUp);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowUp]);
            return;
        }
    }
}
