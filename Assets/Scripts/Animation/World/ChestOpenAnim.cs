using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IT.Animation.World
{
    public class ChestOpenAnim : AnimStateBase
    {
        readonly int ChestDefault = Animator.StringToHash("ChestDefault");
        readonly int ChestOpen = Animator.StringToHash("ChestOpen");
        readonly Dictionary<int, float> TimeSheet;
        private Animator animator { get; set; }
        public ChestOpenAnim(Animator Animator)
        {
            animator = Animator;
            TimeSheet = new()
            {
                { ChestDefault, 1f },
                { ChestOpen, 0.125f },
            };
        }
        public void Play()
        {
            animator.Play(ChestOpen);
        }
    }
}
