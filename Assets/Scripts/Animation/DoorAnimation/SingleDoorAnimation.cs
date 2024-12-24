using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleDoorAnimation : AnimationState
{
    readonly int _singleDoorDefault = Animator.StringToHash("DoorClosed");
    readonly int _singleDoorOpen = Animator.StringToHash("DoorOpening");
    readonly Dictionary<int, float> TimeSheet;
    private Animator animator { get; set; }
    public SingleDoorAnimation(Animator animator)
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
