using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Animation.World
{
    public class SingleDoorAnim : AnimStateBase
    {
        readonly int _singleDoorDefault = Animator.StringToHash("DoorClosed");
        readonly int _singleDoorOpen = Animator.StringToHash("DoorOpening");
        readonly Dictionary<int, float> TimeSheet;
        private Animator animator { get; set; }
        public SingleDoorAnim(Animator animator)
        {
            this.animator = animator;
            TimeSheet = new()
            {
                { _singleDoorDefault, 1f },
                { _singleDoorOpen, 0.333f },
            };
        }
        public void Play()
        {
            animator.Play(_singleDoorOpen);
        }
    }
}
