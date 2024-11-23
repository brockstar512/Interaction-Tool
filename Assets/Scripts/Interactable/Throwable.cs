using System.Collections;
using System.Collections.Generic;
using Interface;
using Items.SubItems;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Throwable : InteractableBase, IInteractWithHookProjectile
{

    //how fast it will be thrown
    const float Speed = 12f;
    //how far it will go...
    //anymore will go in a straight line when it reaches point
    float DistanceLimit = 7.5f;
    //cached starting point so the distance
    Vector3 _startingPoint;
    //how high it will go
    float _distanceFromGround;
    //so we can keep track of how far it is going
    bool _isThrown = false;
    //which direction to make it thrown
    Vector2 _throwDirection;
    //what we are throwing
    [SerializeField] private Transform throwable;
    //the shadow to give it the illusion of height
    [SerializeField] private Transform shadow;
    //the tween arch for it's path
    [SerializeField] private AnimationCurve curve;

    protected void Awake()
    {
        // for (int i = 0; i < curve.keys.Length;i++)
        // {
        //     Debug.Log($"Here is ker {i} is {curve.keys[i].}");
        //     //y coord...is value ... time is x coord
        // }
        DistanceLimit = curve.keys[1].time;
        rb = GetComponent<Rigidbody2D>();
        //update the layer so we can interact with it
        UpdateLayerName();
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        //pick up the item and set the parent as the player
        PickUp(player.transform);
        return true;
    }

    protected void FixedUpdate()
    {
        //if it's thrown run the in air logic
        if (_isThrown)
        {
            InAir();
        }
        //if it's thrown and we have reached the distance limit
        //destroy it
        if (_isThrown && Vector2.Distance(_startingPoint, transform.position) >= DistanceLimit)
        {
            _isThrown = false;
            Destroy(this.gameObject);

        }
    }

    protected Throwable PickUp(Transform parent)
    {
        //when we pick it up set the trigger to true so it doesn't interact with anything else
        //todo consider updating the layer as well if there is a problem with anyone else picking it
        //up while it's picked up
        this.transform.GetComponent<BoxCollider2D>().isTrigger = true;
        //set the parent as the player
        this.transform.SetParent(parent);
        //move it so it looks good...this will change if I just get a sprite of him carrying it
        this.transform.localPosition = new Vector3(0, 0, 0);//+ 2.32f
        throwable.localPosition = new Vector3(0, 2.32f, 0); 
        //this is not being cached/used elsewahere but we are still returning it
        return this;
    }

    private void Toss(Vector3 direction)
    {
        //turn on the shadow when it is thrown
        shadow.gameObject.SetActive(true);
        //move the shadow
        shadow.position = new Vector3(shadow.position.x, shadow.position.y , shadow.position.z);//- .25f
        //remove it as the child of player
        this.transform.SetParent(null);
        //cache where it starts so we can keep track of the distance
        _startingPoint = transform.position;
        //cache the direction
        _throwDirection = direction;
        //set the boolean to true that it is thrown
        _isThrown = true;
    }

    private void InAir()
    {
        Vector3 travelPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //x is height
        float yPos = curve.Evaluate(Vector2.Distance(_startingPoint, travelPos));
  
        throwable.localPosition = new Vector3(0, yPos, 0);
        rb.MovePosition(rb.position + _throwDirection * Speed * Time.deltaTime);
    }

    public override void Release(PlayerStateMachineManager state)
    {
        float startPos = state.transform.GetComponent<SpriteRenderer>().bounds.size.y;
        Vector3 direction = state.currentState.LookDirection;
        
        SetUpArch(startPos,direction);
    }

    private void SetUpArch(float startArchPoint, Vector3 direction)
    {
        //when we leave the state release will be called and it will be thrown and we immediately go to 
        //default state
        _distanceFromGround = startArchPoint;
        //set the animation tween values
        Keyframe[] keyframes = curve.keys;
        keyframes[0].value = _distanceFromGround;
        keyframes[1].value = 0;
        curve.keys = keyframes;
        //throw the item 
        Toss(direction);
    }
    
    public void InteractWithHookProjectile(HookProjectile projectile)
    {
        //todo if this is a bomb set the item to null for when it explodes...or in the switch statement check if the bomb has exploded
        //se the parent to the grappling hook
        this.transform.parent = projectile.transform;
        //set it as a trigger so it can be retracted without any interuption on the way back
        this.GetComponent<Collider2D>().isTrigger = true;
    }
}
