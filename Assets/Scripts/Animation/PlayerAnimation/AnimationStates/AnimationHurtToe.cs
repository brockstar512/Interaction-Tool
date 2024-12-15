using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class AnimationHurtToe : AnimationState
{
    readonly int HurtToeRightHash = Animator.StringToHash("HurtToeRight");
    readonly int HurtToeLeftHash = Animator.StringToHash("HurtToeLeft");
    readonly int HurtToeUpHash = Animator.StringToHash("HurtToeUp");
    readonly int HurtToeDownHash = Animator.StringToHash("HurtToeDown");

    readonly Dictionary<int, float> TimeSheet;

    public AnimationHurtToe()
    {
        TimeSheet = new()
        {
            { HurtToeRightHash, 0.750f },
            { HurtToeLeftHash,0.750f},
            { HurtToeUpHash, 0.750f},
            { HurtToeDownHash,0.750f },
        };

    }
    public async Task Play(PlayerStateMachineManager state)
    {

        if (state.currentState.LookDirection == Vector2.down)
        {
            state.animator.Play(HurtToeDownHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[HurtToeDownHash]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.up)
        {
            state.animator.Play(HurtToeUpHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[HurtToeUpHash]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.right)
        {
            state.animator.Play(HurtToeRightHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[HurtToeRightHash]);
            return;

        }
        if (state.currentState.LookDirection == Vector2.left)
        {
            state.animator.Play(HurtToeLeftHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[HurtToeLeftHash]);
            return;
        }

    }
}
