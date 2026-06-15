using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interactables.Throwable
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ThrowableBase : Interactable
    {
        public override InteractionType Kind => InteractionType.Throw;

        [SerializeField] protected List<AnimationCurve> bounceSequence;
        protected int currentBounceIndex = 0;
        protected float Speed = 12f;
        protected float DistanceLimit;
        protected Vector3 _startingPoint;
        protected bool _isThrown;
        protected Vector2 _throwDirection;
        [SerializeField] protected Transform throwable;
        [SerializeField] protected Transform shadow;
        protected Bounds PlayerSpriteBounds;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            UpdateLayerName();
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
            if (_isThrown)
            {
                InAir();
            }

            if (_isThrown && Vector2.Distance(_startingPoint, transform.position) >= DistanceLimit)
            {
                _isThrown = false;
                Destroy(this.gameObject);
            }
        }

        protected ThrowableBase PickUp(Transform parent)
        {
            this.transform.GetComponent<BoxCollider2D>().isTrigger = true;
            this.transform.localPosition = new Vector3(0, 0, 0);
            return this;
        }

        protected void Toss(Vector3 direction)
        {
            shadow.gameObject.SetActive(true);
            shadow.position = new Vector3(shadow.position.x, shadow.position.y, shadow.position.z);
            this.transform.SetParent(null);
            _startingPoint = transform.position;
            _throwDirection = direction;
            _isThrown = true;
        }

        protected void InAir()
        {
            Vector3 travelPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            float yPos = bounceSequence[currentBounceIndex].Evaluate(Vector2.Distance(_startingPoint, travelPos));

            throwable.localPosition = new Vector3(0, yPos, 0);
            rb.MovePosition(rb.position + _throwDirection * Speed * Time.deltaTime);
        }

        Vector2 GetGroundPosition()
        {
            return new Vector2(transform.position.x, (this.transform.position - PlayerSpriteBounds.size).y);
        }
    }
}