using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    Vector3 startingPoint;
    float distance = 7.5f;
    bool isThrown = false;
    Vector2 throwDirection;
    Rigidbody2D rb;
    float speed = 10f;
    [SerializeField] Transform throwable;
    [SerializeField] Transform shadow;
    //public float gravity = -5;
    //public float verticalVelocity = 1;
    [SerializeField] AnimationCurve curve;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isThrown)
        {
            InAir();
        }

        if(Vector2.Distance(startingPoint,transform.position) >= distance)
        {
            isThrown = false;
            Destroy(this.gameObject);

        }
    }

    public Throw PickUp(Transform parent)
    {
        this.transform.GetComponent<BoxCollider2D>().isTrigger = true;
        this.transform.SetParent(parent);
        this.transform.localPosition = new Vector3(0,0,0);

        return this;
    }

    public void Toss(Vector3 direction)
    {
        //shadow.position = new Vector3(shadow.position.x, shadow.position.y - .25f, shadow.position.z);
        this.transform.SetParent(null);
        startingPoint = transform.position;
        throwDirection = direction;
        isThrown = true;
        
    }

    private void InAir()
    {
        float yPos = curve.Evaluate(Vector2.Distance(startingPoint, transform.position));
        throwable.localPosition = new Vector3(0, yPos, 0);
        rb.MovePosition(rb.position + throwDirection * speed * Time.deltaTime);
    }
}
