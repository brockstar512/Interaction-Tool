using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator anim { get; private set; }
    //public AnimationMoveState animationMoveState { get; private set; }


    void Awake()
    {
        anim = GetComponent<Animator>();
    }


   

}
