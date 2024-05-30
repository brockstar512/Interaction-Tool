using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Throwable : InteractableBase
{
    Vector3 startingPoint;
    float DISTANCE_LIMIT = 7.5f;
    bool isThrown = false;
    Vector2 throwDirection;
    const float SPEED = 10f;
    [SerializeField] Transform throwable;
    [SerializeField] Transform shadow;
    [SerializeField] AnimationCurve curve;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        PickUp(player.transform);
        //if it's too heavy or there is a status on it I can add more complex logic of it I can or can't pick it up
        return true;
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
        this.transform.localPosition = new Vector3(0, 0 + 2.32f, 0);

        return this;
    }

    private void Toss(Vector3 direction)
    {
        shadow.gameObject.SetActive(true);
        shadow.position = new Vector3(shadow.position.x, shadow.position.y - .25f, shadow.position.z);
        this.transform.SetParent(null);
        startingPoint = transform.position;
        throwDirection = direction;
        isThrown = true;

    }

    private void InAir()
    {
        Vector3 finishPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //x is height
        float yPos = curve.Evaluate(Vector2.Distance(startingPoint, finishPos));
        throwable.localPosition = new Vector3(0, yPos, 0);
        rb.MovePosition(rb.position + throwDirection * SPEED * Time.deltaTime);
    }

    public override void Release(PlayerStateMachineManager player)
    {
        Toss(player.LookDirection);
    }
}
