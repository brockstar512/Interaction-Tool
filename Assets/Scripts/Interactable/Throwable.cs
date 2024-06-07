using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Throwable : InteractableBase
{
    Vector3 startingPoint;
    Vector3 playersFeet;
    float distanceFromGround;
    float DISTANCE_LIMIT = 7.5f;//anymore will go in a straight line when it reaches point
    bool isThrown = false;
    Vector2 throwDirection;
    const float SPEED = 12f;
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
        this.transform.localPosition = new Vector3(0, 0, 0);//+ 2.32f
        throwable.localPosition = new Vector3(0, 2.32f, 0); 

        return this;
    }

    private void Toss(Vector3 direction)
    {
        shadow.gameObject.SetActive(true);
        shadow.position = new Vector3(shadow.position.x, shadow.position.y , shadow.position.z);//- .25f
        this.transform.SetParent(null);
        startingPoint = transform.position;
        throwDirection = direction;
        isThrown = true;

    }
    //the tranform of the parent is straight. the throwable follows a curve
    private void InAir()
    {
        Vector3 travelPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //x is height
        float yPos = curve.Evaluate(Vector2.Distance(startingPoint, travelPos));
        //change the size of the shadow based on height
        //scaledValue = (rawValue - min) / (max - min);
        //float scale = Mathf.Clamp(yPos,.1f,1f);
        //float scale = yPos / 3.5f;
        //float scale = Mathf.Clamp(yPos,.25f,.75f);

        //shadow.transform.localScale = new Vector3(1-scale, 1-scale, 1);
            //(yPos - .2f) / (1 - .2f);
        //Debug.Log(scale);

        //Debug.Log(yPos);
        throwable.localPosition = new Vector3(0, yPos, 0);
        rb.MovePosition(rb.position + throwDirection * SPEED * Time.deltaTime);
    }

    public override void Release(PlayerStateMachineManager player)
    {
        distanceFromGround = player.transform.GetComponent<SpriteRenderer>().bounds.size.y;
        Keyframe[] keyframes = curve.keys;
        keyframes[0].value = distanceFromGround;
        keyframes[1].value = 0;

        curve.keys = keyframes;

        Toss(player.LookDirection);
    }
}
