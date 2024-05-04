using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovementManager : MonoBehaviour
{
    //put this in abstract class?

    const float WALK_SPEED = 5f;
    const float SLOW_SPEED = 2f;
    Rigidbody2D rb;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 movement)
    {

        rb.MovePosition(rb.position + movement * GetSpeed() * Time.deltaTime);
        
    }

    float GetSpeed()
    {
        //if()
        //{

        //}

        return WALK_SPEED;
    }


}
