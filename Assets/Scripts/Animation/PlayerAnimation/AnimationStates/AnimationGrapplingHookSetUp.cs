using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Animation.PlayerAnimation.AnimationStates
{
    public class AnimationGrapplingHookSetUp : AnimationState
    {
        readonly int _setUpGrapplingRight = Animator.StringToHash("GrapplingHookSetUpRight");
        readonly int _setUpGrapplingUp = Animator.StringToHash("GrapplingHookSetUpUp");
        readonly int _setUpGrapplingDown = Animator.StringToHash("GrapplingHookSetUpDown");
        readonly int _setUpGrapplingLeft = Animator.StringToHash("GrapplingHookSetUpLeft");

        readonly Dictionary<int, float> TimeSheet;

        public AnimationGrapplingHookSetUp()
        {
            TimeSheet = new()
            {
                { _setUpGrapplingRight, 0.042f },
                { _setUpGrapplingUp, 0.042f },
                { _setUpGrapplingDown, 0.042f },
                { _setUpGrapplingLeft, 0.042f }
            };

        }

        public async Task Play(PlayerStateMachineManager state)
        {
            if (state.currentState.LookDirection == Vector2.down)
            {
                state.animator.Play(_setUpGrapplingDown);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingDown]);
                return;
            }

            if (state.currentState.LookDirection == Vector2.right)
            {
                state.animator.Play(_setUpGrapplingRight);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingRight]);
                return;
            }

            if (state.currentState.LookDirection == Vector2.left)
            {
                state.animator.Play(_setUpGrapplingLeft);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingLeft]);
                return;
            }

            if (state.currentState.LookDirection == Vector2.up)
            {
                state.animator.Play(_setUpGrapplingUp);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingUp]);
                return;
            }
        }
    }
}
