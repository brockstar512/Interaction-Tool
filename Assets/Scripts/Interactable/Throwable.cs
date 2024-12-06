using System.Collections;
using System.Collections.Generic;
using Interface;
using Items.SubItems;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Throwable : InteractableBase, IInteractWithHookProjectile
{
    //if it needs to bounces add to the list
    [SerializeField] protected List<AnimationCurve> bounceSequence;
    //keep track of the index of the arches in the animation curve
    protected int currentBounceIndex = 0;
    //how fast it will be thrown
    protected float Speed = 12f;
    //how far it will go...this is set by the last key in the animation set key frames
    protected float DistanceLimit;
    //cached starting point so the distance
    protected Vector3 _startingPoint;
    //so we can keep track of how far it is going
    protected bool _isThrown;
    //which direction to make it thrown
    protected Vector2 _throwDirection;
    //what we are throwing
    [SerializeField] protected Transform throwable;
    //the shadow to give it the illusion of height
    [SerializeField] protected Transform shadow;
    //we throw before we animation, so we should be able to calculate the correct distance to the ground...I don't know if it would or wouldnt work in the animation cycle
    protected Bounds PlayerSpriteBounds;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //update the layer so we can interact with it
        UpdateLayerName();
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        //pick up the item and set the parent as the player
        PickUp(player.transform);
        PlayerSpriteBounds = player.gameObject.GetComponent<SpriteRenderer>().bounds;
        return true;
    }

    private void FixedUpdate()
    {
        //if it's thrown run the in air logic
        if (_isThrown)
        {
            InAir();
        }
        
        if (_isThrown && Vector2.Distance(_startingPoint, transform.position) >= DistanceLimit)
        {
            _isThrown = false;
            //wait for the break animation to play
            Destroy(this.gameObject);
            //if it does not have more distance to go

        }
    }

    protected Throwable PickUp(Transform parent)
    {
        //when we pick it up set the trigger to true so it doesn't interact with anything else
        //todo consider updating the layer as well if there is a problem with anyone else picking it
        //up while it's picked up
        this.transform.GetComponent<BoxCollider2D>().isTrigger = true;
        //set the parent as the player
        //this.transform.SetParent(parent);
        //move it so it looks good...
        this.transform.localPosition = new Vector3(0, 0, 0);

        return this;
    }

    protected void Toss(Vector3 direction)
    {
        //turn on the shadow when it is thrown
        shadow.gameObject.SetActive(true);
        //move the shadow
        shadow.position = new Vector3(shadow.position.x, shadow.position.y , shadow.position.z);//- .25f
        //remove it as the child of player
        this.transform.SetParent(null);
        //cache where it starts so we can keep track of the distance...and where the ground is so it doesn't start where the origin point is when its over it's head
        _startingPoint = transform.position;
        //cache the direction
        _throwDirection = direction;
        //set the boolean to true that it is thrown
        _isThrown = true;
    }

    protected void InAir()
    {
        Vector3 travelPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //x is height
        float yPos = bounceSequence[currentBounceIndex].Evaluate(Vector2.Distance(_startingPoint, travelPos));
  
        throwable.localPosition = new Vector3(0, yPos, 0);
        rb.MovePosition(rb.position + _throwDirection * Speed * Time.deltaTime);
    }

    public override void Release(PlayerStateMachineManager state)
    {
        
        
        // Keyframe keys = new()[2];
        //current location + start keyframe
        //current location + end keyframe
        //bounceSequence[currentBounceIndex].keys[0].value = transform.position.y;
        // Debug.Log(bounceSequence[currentBounceIndex].keys[0].value);
        // Debug.Log(transform.position.y);
        Vector3 direction = state.currentState.LookDirection;
        //get the distance based off the keyframe end
        DistanceLimit = bounceSequence[currentBounceIndex].keys[1].time;
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

    float GetYPose(Vector2 travelPos)
    {
        return bounceSequence[currentBounceIndex].Evaluate(Vector2.Distance(_startingPoint, travelPos));
    }

    Vector3 GetGroundPosition()
    {
        
        return Vector3.zero;
    }
}
//locallocation - sprite
/*
 *
 *  _distanceFromGround = startArchPoint;
   //set the animation tween values
   Keyframe[] keyframes = curve.keys;
   keyframes[0].value = _distanceFromGround;
   keyframes[1].value = 0;
   curve.keys = keyframes;
 */