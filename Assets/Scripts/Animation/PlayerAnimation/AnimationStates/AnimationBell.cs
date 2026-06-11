using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


namespace IT.Animation.Player.States
{
    using IT.Interactables;

    public class AnimationBell : AnimationState
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
    
        public async Task Play(IInteractionContext ctx)
        {
            if (ctx.LookDirection == Vector2.down)
            {
                ctx.Animator.Play(BellDown);
                await Awaitable.WaitForSecondsAsync(TimeSheet[BellDown]);
                return;
            }
            if (ctx.LookDirection == Vector2.right)
            {
                ctx.Animator.Play(BellRight);
                await Awaitable.WaitForSecondsAsync(TimeSheet[BellRight]);
                return;
            }
            if (ctx.LookDirection == Vector2.left)
            {
                ctx.Animator.Play(BellLeft);
                await Awaitable.WaitForSecondsAsync(TimeSheet[BellLeft]);
                return;
            }
            if (ctx.LookDirection == Vector2.up)
            {
                ctx.Animator.Play(BellUp);
                await Awaitable.WaitForSecondsAsync(TimeSheet[BellUp]);
                return;
            }
        }
    }
}
