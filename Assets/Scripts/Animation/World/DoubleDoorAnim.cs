using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Animation.World
{
    public class DoubleDoorAnim : AnimStateBase
    {
        readonly int _doubleDoorDefault = Animator.StringToHash("DoubleDoorClosed");
        readonly int _doubleDoorOpen = Animator.StringToHash("DoubleDoorsOpening");
        readonly Dictionary<int, float> TimeSheet;
        private Animator animator { get; set; }

        public DoubleDoorAnim(Animator animator)
        {
            this.animator = animator;
            TimeSheet = new()
            {
                { _doubleDoorDefault, 1f },
                { _doubleDoorOpen, 0.292f },
            };
        }

        // full-speed open, start to finish
        public void Play()
        {
            animator.speed = 1f;
            animator.Play(_doubleDoorOpen);
        }
    
    }
}
