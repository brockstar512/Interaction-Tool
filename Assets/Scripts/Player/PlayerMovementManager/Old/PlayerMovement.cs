using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 movement;
    Rigidbody2D rb;
    Vector2 lookDirection;

    Throw obj;
    bool HasItem { get { return obj != null; } }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveCharacter(movement);
    }

    private void Update()
    {


        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        movement = new Vector2(inputX, inputY).normalized;


        if (movement == Vector2.up)
        {
            lookDirection = movement;
        }
        if (movement == Vector2.down)
        {
            lookDirection = movement;
        }
        if (movement == Vector2.right)
        {
            lookDirection = movement;
        }
        if (movement == Vector2.left)
        {
            lookDirection = movement;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            //Slide();
            if(HasItem)
            {
                Toss();
            }
            else
            {
                PickUp();
            }
        }
    }

    void moveCharacter(Vector2 direction)
    {

        //return; if you want it to stay in place ignore the following
        rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
    }


    void Slide()
    {
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, lookDirection, .75f);

        if (hit.collider != null)
        {
            Slide obj = hit.collider.transform.GetComponent<Slide>();
            obj.Move(lookDirection);
        }
    }

    void PickUp()
    {
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, lookDirection, .75f);

        if (hit.collider != null)
        {
            obj = hit.collider.transform.GetComponent<Throw>().PickUp(this.transform);
        }
    }

    void Toss()
    {
        obj.Toss(lookDirection);
        obj = null;
    }

    void Push()
    {

    }

    void Pull()
    {

    }

    //height throw
    //https://www.youtube.com/watch?v=6iS0qbSbKuw
}

