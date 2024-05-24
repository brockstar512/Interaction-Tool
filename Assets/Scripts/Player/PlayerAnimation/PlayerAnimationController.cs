using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator anim { get; private set; }
    //public AnimationMoveState animationMoveState { get; private set; }
    [SerializeField] AnimatorController EquipAnimController;
    [SerializeField] AnimatorController DefaultAnimController;//probably just use the classes that seperate the animations by strings


    //list of animation controllers

    void Awake()
    {
        anim = GetComponent<Animator>();
    }


   

}
