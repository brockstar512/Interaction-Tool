using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MoveSlideable : MonoBehaviour
{
    private bool _canMove = false;
    private Rigidbody2D _rb;
    private Vector3 _dir = Vector3.zero;
    private float _speed = 10f;


    public void Init(Vector3 direction)
    {
        _rb = GetComponent<Rigidbody2D>();
        _dir = direction;


        _canMove = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_canMove)
        {
            _rb.MovePosition(transform.position + _dir * _speed * Time.deltaTime);
        }
    }
}
