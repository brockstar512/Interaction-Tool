using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Animation.PlayerAnimation.AnimationStates
{
    public class AnimationGrapplingHook : AnimationState
    {
        readonly int _shootRight = Animator.StringToHash("GrapplingHookShootRight");
        readonly int _shootUp = Animator.StringToHash("GrapplingHookShootUp");
        readonly int _shootDown = Animator.StringToHash("GrapplingHookShootDown");
        readonly int _shootLeft = Animator.StringToHash("GrapplingHookShootLeft");

        readonly Dictionary<int, float> TimeSheet;

        public AnimationGrapplingHook()
        {
            TimeSheet = new()
            {
                { _shootRight, 0.292f },
                { _shootUp, 0.292f },
                { _shootDown, 0.292f },
                { _shootLeft, 0.292f }
            };

        }

        public async Task Play(PlayerStateMachineManager state)
        {
            if (state.currentState.LookDirection == Vector2.down)
            {
                state.animator.Play(_shootDown);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootDown]);
                return;
            }

            if (state.currentState.LookDirection == Vector2.right)
            {
                state.animator.Play(_shootRight);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootRight]);
                return;
            }

            if (state.currentState.LookDirection == Vector2.left)
            {
                state.animator.Play(_shootLeft);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootLeft]);
                return;
            }

            if (state.currentState.LookDirection == Vector2.up)
            {
                state.animator.Play(_shootUp);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootUp]);
                return;
            }
        }
    }
}
