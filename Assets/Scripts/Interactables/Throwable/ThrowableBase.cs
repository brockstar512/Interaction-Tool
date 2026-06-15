using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interactables.Throwable
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ThrowableBase : Interactable, IDestructible
    {
        public override InteractionType Kind => InteractionType.Throw;

        //fired when this throwable's GameObject is destroyed for any reason
        //(bomb explosion, scene unload, thrown-and-broken, etc.)
        //subscribers should clean up any references they hold to this object.
        public event Action Destroyed;

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

        protected virtual void OnDestroy()
        {
            //let anyone holding a reference know we're gone,
            //so they don't try to use us
            Destroyed?.Invoke();
        }

        public override bool Interact(IInteractionContext context)
        {
            PickUp(context.Transform);
            PlayerSpriteBounds = context.Transform.gameObject.GetComponent<SpriteRenderer>().bounds;
            return true;
        }

        public override void Release(IInteractionContext context)
        {
            this.transform.position = GetGroundPosition();
            Vector3 direction = context.LookDirection;
            DistanceLimit = bounceSequence[currentBounceIndex].keys[1].time;
            Toss(direction);
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

        protected ThrowableBase PickUp(Transform parent)
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
            shadow.position = new Vector3(shadow.position.x, shadow.position.y, shadow.position.z);//- .25f
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
            // Debug.Break();
        }

        Vector2 GetGroundPosition()
        {
            //distance from the ground so the animation curve starts at the correct height and not the height + overhead origin point
            return new Vector2(transform.position.x, (this.transform.position - PlayerSpriteBounds.size).y);
        }
    }
}