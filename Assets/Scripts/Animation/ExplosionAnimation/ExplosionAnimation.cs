using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Animation.PlayerAnimation.AnimationStates
{
    public class ExplosionAnimation : AnimationState
    {
        readonly int _bombExplosion = Animator.StringToHash("BombExplosion");


        readonly Dictionary<int, float> TimeSheet;

        public ExplosionAnimation()
        {
            TimeSheet = new()
            {
                { _bombExplosion, 0.417f },

            };
        }
        
        public async Task Play()
        {
            await Awaitable.WaitForSecondsAsync(TimeSheet[_bombExplosion]);
            await Task.CompletedTask;
        }
    }
}
