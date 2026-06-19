using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerMover : MonoBehaviour
    {
        //put this in abstract class?

        const float WALK_SPEED = 5f;
        const float SLOW_SPEED = 2f;
        Rigidbody2D _rb;


        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 movement)
        {
            _rb.MovePosition(_rb.position + movement * GetSpeed() * Time.deltaTime);
        }

        float GetSpeed()
        {
            return WALK_SPEED;
        }


    }
}
