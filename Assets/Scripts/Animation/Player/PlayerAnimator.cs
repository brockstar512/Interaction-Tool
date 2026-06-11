using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace IT.Animation.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        public Animator anim { get; private set; }


        //list of animation controllers

        void Awake()
        {
            anim = GetComponent<Animator>();
        }


   

    }
}
