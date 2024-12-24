using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleDoorAnimation : AnimationState
{
    readonly int _doubleDoorDefault = Animator.StringToHash("DoubleDoorClosed");
    readonly int _doubleDoorOpen = Animator.StringToHash("DoubleDoorsOpening");
    readonly Dictionary<int, float> TimeSheet;
    private Animator animator { get; set; }
    public DoubleDoorAnimation(Animator animator)
    {
        this.animator = animator;
        TimeSheet = new()
        {
            { _doubleDoorDefault, 1f },
            { _doubleDoorOpen, 0.292f },
        };
    }
    public void Play()
    {
        animator.Play(_doubleDoorOpen);
    }
}
