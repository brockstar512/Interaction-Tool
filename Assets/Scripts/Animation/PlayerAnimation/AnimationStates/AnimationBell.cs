using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class AnimationBell : MonoBehaviour
{
    readonly int BellRight = Animator.StringToHash("BellRight");
    readonly int BellUp = Animator.StringToHash("BellUp");
    readonly int BellDown = Animator.StringToHash("BellDown");
    readonly int BellLeft = Animator.StringToHash("BellLeft");

    readonly Dictionary<int, float> TimeSheet;
    
    public AnimationBell()
    {
        TimeSheet = new()
        {
            { BellRight, 0.797f },
            { BellUp, 0.797f },
            { BellDown,  0.797f },
            { BellLeft,  0.797f }
        };

    }
    
    public async Task Play(PlayerStateMachineManager state)
    {
        if (state.currentState.LookDirection == Vector2.down)
        {
            state.animator.Play(BellDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[BellDown]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.right)
        {
            state.animator.Play(BellRight);
            await Awaitable.WaitForSecondsAsync(TimeSheet[BellRight]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.left)
        {
            state.animator.Play(BellLeft);
            await Awaitable.WaitForSecondsAsync(TimeSheet[BellLeft]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.up)
        {
            state.animator.Play(BellUp);
            await Awaitable.WaitForSecondsAsync(TimeSheet[BellUp]);
            return;
        }
    }
}
