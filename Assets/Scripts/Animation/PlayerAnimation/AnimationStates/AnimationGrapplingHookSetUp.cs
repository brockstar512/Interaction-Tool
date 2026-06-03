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

        public async Task Play(IInteractionContext ctx)
        {
            if (ctx.LookDirection == Vector2.down)
            {
                ctx.Animator.Play(_setUpGrapplingDown);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingDown]);
                return;
            }
            if (ctx.LookDirection == Vector2.right)
            {
                ctx.Animator.Play(_setUpGrapplingRight);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingRight]);
                return;
            }
            if (ctx.LookDirection == Vector2.left)
            {
                ctx.Animator.Play(_setUpGrapplingLeft);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingLeft]);
                return;
            }
            if (ctx.LookDirection == Vector2.up)
            {
                ctx.Animator.Play(_setUpGrapplingUp);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_setUpGrapplingUp]);
                return;
            }
        }
    }
}
