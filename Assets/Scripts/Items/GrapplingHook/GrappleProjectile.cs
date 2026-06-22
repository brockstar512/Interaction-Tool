using UnityEngine;
using System;

namespace IT.Items.GrapplingHook
{
    using IT.Core;
    using IT.Core.Utilities;
    using IT.Overlap;
    using IT.Interactables.Throwable;

    public class GrappleProjectile : MonoBehaviour
    {
        //render for the rope bridge
        [SerializeField] private LineRenderer line;
        //origin point for the rope bridge
        [SerializeField] private Vector3 origin;
        //get the player position so I can use it for the player overlap
        [SerializeField] private Vector3 playerPos;
        //sprites to set the look of the direction
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite downSprite;
        [SerializeField] private Sprite upSprite;
        //this is the prefab for the overlaps
        [SerializeField] private GrappleSocketOverlap hookOverlapPrefab;
        //cache hook overlaps for the start if we hit one
        private GrappleSocketOverlap _hookStartOverlap;
        //cache hook overlaps for the end if we hit one
        private GrappleSocketOverlap _hookEndOverlap;
        //this will be our cache for the hookOverlapPrefab
        private GrappleTargetOverlap _overlapHookCheck;
        //this will determine if we run logic for check for the end pin
        private bool _hasStartPin;
        //cache hook connector for the start pin
        public GrappleSocket hookConnectorStartPin { get; private set; }
        //cache hook connector for the end pin
        public GrappleSocket hookConnectorEndPin { get; private set; }
        //delegate to tell the gun that we hit something
        Action<IGrappleTarget> _hitSomethingCallback;
        //delegate to tell the gun that a carried target died mid-flight (e.g. bomb exploded)
        Action _carriedTargetLostCallback;

        private void Awake()
        {
            //get the overlap check for the start
            _overlapHookCheck = GetComponentInChildren<GrappleTargetOverlap>();
        }

        public GrappleProjectile Init(
            Vector3 gunBarrel,
            Action<IGrappleTarget> hitSomethingCallback,
            Action carriedTargetLostCallback,
            Vector3 playerLocation)
        {
            //get the location of the player.
            this.playerPos = playerLocation;
            //get the origin point of where we are shooting
            this.origin = gunBarrel;
            //get a callback so we can tell the gun we hit something
            _hitSomethingCallback = hitSomethingCallback;
            //get a callback so we can tell the gun the carried thing died (bomb exploded, etc.)
            _carriedTargetLostCallback = carriedTargetLostCallback;
            //instantiate the overlap to see if we have a beginning pin to connect to if we have an end
            _hookStartOverlap = Instantiate(hookOverlapPrefab, playerLocation, Quaternion.identity);
            //instantiate the overlap check to see if we have an end connector
            _hookEndOverlap = Instantiate(hookOverlapPrefab, playerLocation, Quaternion.identity);
            //check for the start pin
            CheckForStartPin();
            //return this to the gun so it can destroy this when it's done and so it can set the direction of the sprite
            return this;
        }

        public void SetHookSprite(Vector3 spriteDirection)
        {
            //this set the direction of the sprite of the grappling hook
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (spriteDirection == Vector3.right)
            {
                sr.sprite = rightSprite;
            }
            if (spriteDirection == Vector3.left)
            {
                sr.sprite = leftSprite;
            }
            if (spriteDirection == Vector3.up)
            {
                sr.sprite = upSprite;
            }
            if (spriteDirection == Vector3.down)
            {
                sr.sprite = downSprite;
            }
        }

        async void CheckForStartPin()
        {
            try
            {
                hookConnectorStartPin = await _hookStartOverlap.GetMostOverlappedHookStartCol(playerPos);
                if (hookConnectorStartPin is null)
                    return;
                GameUtilities.PutObjectOnLayer(GameUtilities.SocketUsedLayer, hookConnectorStartPin.gameObject);
                _hasStartPin = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"HookProjectile.CheckForStartPin failed: {ex}");
            }
        }


        private void FixedUpdate()
        {
            //if we have a start pin continually check for an end pin
            if (_hasStartPin)
            {
                CheckForEndPin();
            }

            //as the hook is moving check if we have an overlap of a collider that we can interact with
            Collider2D col = _overlapHookCheck.GetMostOverlappedCol();

            //bail if there's nothing to hit, or if we already hit something on the way out
            //(callback is nulled after the first hit, so further overlaps during retract are ignored)
            if (col is null || _hitSomethingCallback is null)
                return;

            //see if what we hit was something we can interact with
            IGrappleTarget somethingHit = col?.GetComponent<IGrappleTarget>();
            //if we hit a hook connector ignore it because this section does not handle that
            //if we what we hit is not soemthing we can interact with ignore it.
            if (somethingHit is null || somethingHit is GrappleSocket)
                return;

            //if it's a retractable target, subscribe to its Detached event BEFORE attaching
            //so we can't miss a same-frame destruction (e.g. bomb explodes the instant it's hooked)
            if (somethingHit is GrappleHookRetractableTarget retractable)
            {
                Action onDetach = null;
                onDetach = () =>
                {
                    //Unity fake-null guard: this projectile may already be destroyed
                    if (this == null) return;
                    retractable.Detached -= onDetach;
                    _carriedTargetLostCallback?.Invoke();
                };
                retractable.Detached += onDetach;
            }

            //tell the target it's been hooked so it can attach itself, set trigger, etc.
            somethingHit.InteractWithHookProjectile(this);

            //notify the gun we hit something
            _hitSomethingCallback?.Invoke(somethingHit);
            //null the callback so we don't reprocess on subsequent FixedUpdates while retracting
            _hitSomethingCallback = null;
        }

        private void Update()
        {
            //draw the rope as the projectile moves
            DrawLineConnector();
        }

        void CheckForEndPin()
        {
            //check if we are over a hook connector
            hookConnectorEndPin = _hookEndOverlap.GetMostOverlappedHookEndCol(this.transform.position);

            //if we have a hook connector
            if (hookConnectorEndPin != null)
            {
                //put it on the layer that says its being used so other overlap checkers dont deal withit
                GameUtilities.PutObjectOnLayer(GameUtilities.SocketUsedLayer, hookConnectorStartPin.gameObject);
                //connect the pins
                ConnectPin();
            }
        }

        void ConnectPin()
        {
            //if we have the script for both connecting pins
            if (hookConnectorEndPin != null && hookConnectorStartPin != null)
            {
                //put the end connector on the layer so no other grappling hook messes with it
                GameUtilities.PutObjectOnLayer(GameUtilities.SocketUsedLayer, hookConnectorEndPin.gameObject);
                //use the start pin to hand the interaction logic to create the bridge
                hookConnectorStartPin.InteractWithHookProjectile(this);
                //tell the gun we hit something
                _hitSomethingCallback?.Invoke(hookConnectorEndPin);
            }
        }

        void OnDestroy()
        {
            //if we have a start connector reset its layer
            if (hookConnectorStartPin != null)
            {
                GameUtilities.PutObjectOnLayer(GameUtilities.SocketUnusedLayer, hookConnectorStartPin.gameObject);
            }
            //if we have an end connector reset its layer
            if (hookConnectorEndPin != null)
            {
                GameUtilities.PutObjectOnLayer(GameUtilities.SocketUnusedLayer, hookConnectorEndPin.gameObject);
            }
            //destroy the overlaps that we created 
            Destroy(_hookStartOverlap.gameObject);
            Destroy(_hookEndOverlap.gameObject);
        }

        private void DrawLineConnector()
        {
            //this continually draws the rope.
            line.positionCount = 2;
            line.SetPosition(0, origin);
            line.SetPosition(1, this.transform.position);
        }
    }
}