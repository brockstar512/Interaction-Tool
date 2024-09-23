using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class AnimationGrapplingHook : AnimationState
{
    readonly int ShootRight = Animator.StringToHash("GrapplingHookShootRight");
    readonly int ShootUp = Animator.StringToHash("GrapplingHookShootUp");
    readonly int ShootDown = Animator.StringToHash("GrapplingHookShootDown");
    readonly int ShootLeft = Animator.StringToHash("GrapplingHookShootLeft");

    readonly Dictionary<int, float> TimeSheet;
    
    public AnimationGrapplingHook()
    {
        TimeSheet = new()
        {
            { ShootRight, 0.292f },
            { ShootUp, 0.292f },
            { ShootDown,  0.292f },
            { ShootLeft,  0.292f }
        };

    }
    
    public async Task Play(PlayerStateMachineManager state)
    {
        if (state.currentState.LookDirection == Vector2.down)
        {
            state.animator.Play(ShootDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ShootDown]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.right)
        {
            state.animator.Play(ShootRight);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ShootRight]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.left)
        {
            state.animator.Play(ShootLeft);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ShootLeft]);
            return;
        }
        if (state.currentState.LookDirection == Vector2.up)
        {
            state.animator.Play(ShootUp);
            await Awaitable.WaitForSecondsAsync(TimeSheet[ShootUp]);
            return;
        }
    }
}
