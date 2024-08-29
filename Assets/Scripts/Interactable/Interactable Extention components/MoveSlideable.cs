using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MoveSlideable : MonoBehaviour
{
    private bool _canMove = false;
    private Rigidbody2D _rb;
    private Vector3 _dir = Vector3.zero;
    public float _speed = 8f;
    //consider an animation curve

    public MoveSlideable Init(Vector3 direction)
    {
        _dir = direction;
        _canMove = true;
        return this;
    }
    
    void FixedUpdate()
    {
        if (_canMove)
        {
            transform.Translate(_dir * _speed * Time.deltaTime);
        }
    }
}
