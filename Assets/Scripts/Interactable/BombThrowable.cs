using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

//https://yal.cc/top-down-bouncing-loot-effects/
class BombThrowable : InteractableBase
{
    [SerializeField]private List<AnimationCurve> bounceSequence;
    private int currentBounceIndex { get; set; } = 0;
    protected Vector3 direction = Vector3.zero;
    
    //how fast it will be thrown
    float Speed = 15f;
    //how far it will go...
    //anymore will go in a straight line when it reaches point
    private float DistanceLimit = 0;
    //cached starting point so the distance
    Vector3 _startingPoint;
 
    //so we can keep track of how far it is going
    bool _isThrown = false;
    //which direction to make it thrown
    Vector2 _throwDirection;
    [SerializeField] Transform shadow;
    [SerializeField] Transform throwable;

    protected void Awake()
    {
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
    protected BombThrowable PickUp(Transform parent)
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
    protected void FixedUpdate()
    {
        //if it's thrown run the in air logic
        if (_isThrown)
        {
            InAir();
        }
        
        //determine if it needs to go to the next bounce
        if (_isThrown && Vector2.Distance(_startingPoint, transform.position) >= DistanceLimit)
        {
            _isThrown = false;
            //if it does not have more distance to go
            if ( currentBounceIndex < bounceSequence.Count-1)
            {
                //increase tp the next curve
                currentBounceIndex++;
                Speed *= .75f;
                DistanceLimit = bounceSequence[currentBounceIndex].keys[1].time;
                SetUpArch(transform.position.y, direction);
            }
            
        }
    }
    
    protected void InAir()
    {
        //where is currently is
        Vector3 travelPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //this is height
        float yPos = bounceSequence[currentBounceIndex].Evaluate(Vector2.Distance(_startingPoint, travelPos));
  
        throwable.localPosition = new Vector3(0, yPos, 0);
        rb.MovePosition(rb.position + _throwDirection * Speed * Time.deltaTime);
    }
    
    protected void Toss(Vector3 direction)
    {
        // float time = bounceSequence[0].keys[1].time / Speed;
        // throwSpeedTween = DOTween.To(() => Speed, x => Speed = x, 5f, time)
        //     .SetEase(Ease.InOutQuad);
        //turn on the shadow when it is thrown
        // shadow.gameObject.SetActive(true);
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
    
    public override void Release(PlayerStateMachineManager state)
    {
        float startPos = state.transform.GetComponent<SpriteRenderer>().bounds.size.y;
        direction = state.currentState.LookDirection;
        
        SetUpArch(startPos,direction);
    }
    
    protected void SetUpArch(float startArchPoint, Vector3 dir)
    {
        //when we leave the state release will be called and it will be thrown and we immediately go to 
        DistanceLimit = bounceSequence[currentBounceIndex].keys[1].time;

        //throw the item 
        Toss(dir);
    }
    
    float GetTotalDistance()
    {
        float sum = bounceSequence.Sum(x => x.keys[1].time);
        return sum;
    }

  
    
}
