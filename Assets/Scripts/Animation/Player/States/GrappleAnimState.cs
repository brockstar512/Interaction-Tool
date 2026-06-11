using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace IT.Animation.Player.States
{
    using IT.Interactables;

    public class GrappleAnimState : AnimStateBase
    {
        readonly int _shootRight = Animator.StringToHash("GrapplingHookShootRight");
        readonly int _shootUp = Animator.StringToHash("GrapplingHookShootUp");
        readonly int _shootDown = Animator.StringToHash("GrapplingHookShootDown");
        readonly int _shootLeft = Animator.StringToHash("GrapplingHookShootLeft");

        readonly Dictionary<int, float> TimeSheet;

        public GrappleAnimState()
        {
            TimeSheet = new()
            {
                { _shootRight, 0.292f },
                { _shootUp, 0.292f },
                { _shootDown, 0.292f },
                { _shootLeft, 0.292f }
            };

            
        }

        public async Task Play(IInteractionContext ctx)
        {
            if (ctx.LookDirection == Vector2.down)
            {
                ctx.Animator.Play(_shootDown);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootDown]);
                return;
            }
            if (ctx.LookDirection == Vector2.right)
            {
                ctx.Animator.Play(_shootRight);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootRight]);
                return;
            }
            if (ctx.LookDirection == Vector2.left)
            {
                ctx.Animator.Play(_shootLeft);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootLeft]);
                return;
            }
            if (ctx.LookDirection == Vector2.up)
            {
                ctx.Animator.Play(_shootUp);
                await Awaitable.WaitForSecondsAsync(TimeSheet[_shootUp]);
                return;
            }
        }
    }
}
