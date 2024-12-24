using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChestOpenAnimation : AnimationState
{
    readonly int ChestDefault = Animator.StringToHash("ChestDefault");
    readonly int ChestOpen = Animator.StringToHash("ChestOpen");
    readonly Dictionary<int, float> TimeSheet;
    private Animator animator { get; set; }
    public ChestOpenAnimation(Animator Animator)
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
