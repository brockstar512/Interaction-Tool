using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Animator anim;
    const string WalkRight= "WalkRight";
    const string WalkUp = "WalkUp";
    const string WalkDown = "WalkDown";
    const string WalkLeft = "WalkLeft";

    const string StandRight = "StandRight";
    const string StandLeft = "StandLeft";
    const string StandUp = "StandUp";
    const string StandDown = "StandDown";






    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Walk(Vector2 movement, Vector2 lookDirection)
    {
        if(movement.x != 0 || movement.y != 0)
        {
            if (lookDirection == Vector2.down)
            {
                anim.Play(WalkDown);
            }
            if (lookDirection == Vector2.up)
            {
                anim.Play(WalkUp);
            }
            if (lookDirection == Vector2.right)
            {
                anim.Play(WalkRight);

            }
            if (lookDirection == Vector2.left)
            {
                anim.Play(WalkLeft);
            }

        }
        else
        {
            if (lookDirection == Vector2.down)
            {
                anim.Play(StandDown);
            }
            if (lookDirection == Vector2.up)
            {
                anim.Play(StandUp);
            }
            if (lookDirection == Vector2.right)
            {
                anim.Play(StandRight);

            }
            if (lookDirection == Vector2.left)
            {
                anim.Play(StandLeft);
            }
        }
        
    }

    public void Hurt()
    {
        //anim.Play();
    }

}
