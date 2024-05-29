using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AnimationThrow
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

    public async Task Play(PlayerStateMachineManager playerstate)
    {
        if (playerstate.LookDirection == Vector2.down)
        {
            playerstate.animator.Play(ThrowDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowDown]);
            return;
        }
        if (playerstate.LookDirection == Vector2.right)
        {
            playerstate.animator.Play(ThrowRight);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowRight]);
            return;
        }
        if (playerstate.LookDirection == Vector2.left)
        {
            playerstate.animator.Play(ThrowLeft);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowLeft]);
            return;
        }
        if (playerstate.LookDirection == Vector2.up)
        {
            playerstate.animator.Play(ThrowUp);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ThrowUp]);
            return;
        }
    }
}
