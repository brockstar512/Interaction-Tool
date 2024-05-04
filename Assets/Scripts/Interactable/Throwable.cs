using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Throwable : InteractableBase
{
    Vector3 startingPoint;
    float DISTANCE_LIMIT = 7.5f;
    bool isThrown = false;
    Vector2 throwDirection;
    Rigidbody2D rb;
    const float SPEED = 10f;
    [SerializeField] Transform throwable;
    [SerializeField] Transform shadow;
    [SerializeField] AnimationCurve curve;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

   

    public override void Interact(PlayerStateMachineManager player)
    {
        PickUp(player.transform);
    }

    private void FixedUpdate()
    {
        if (isThrown)
        {
            InAir();
        }

        if (Vector2.Distance(startingPoint, transform.position) >= DISTANCE_LIMIT)
        {
            isThrown = false;
            Destroy(this.gameObject);

        }
    }

    Throwable PickUp(Transform parent)
    {
        this.transform.GetComponent<BoxCollider2D>().isTrigger = true;
        this.transform.SetParent(parent);
        this.transform.localPosition = new Vector3(0, 0, 0);

        return this;
    }

    private void Toss(Vector3 direction)
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
        rb.MovePosition(rb.position + throwDirection * SPEED * Time.deltaTime);
    }

    public override void Release(PlayerStateMachineManager player)
    {
        Toss(player.LookDirection);
    }
}
