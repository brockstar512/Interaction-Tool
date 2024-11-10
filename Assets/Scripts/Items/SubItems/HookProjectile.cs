using UnityEngine;
using Player.ItemOverlap;
using System;
using Interface;

namespace Items.SubItems{
    
    public class HookProjectile : MonoBehaviour, IDamage
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
        [SerializeField] private OverlapHookSocketCheck hookOverlapPrefab;
        //cache hook overlaps for the start if we hit one
        private OverlapHookSocketCheck _hookStartOverlap;
        //cache hook overlaps for the end if we hit one
        private OverlapHookSocketCheck _hookEndOverlap;
        //this will be our cache for the hookOverlapPrefab
        private OverlapHookCheck _overlapHookCheck;
        //this will determine if we run logic for check for the end pin
        private bool _hasStartPin;
        //cache hook connector for the start pin
        public HookConnector hookConnectorStartPin { get; private set; }
        //cache hook connector for the end pin
        public HookConnector hookConnectorEndPin { get; private set; }
        //delegate to tell the gun that we hit something
        Action<IInteractWithHookProjectile> _hitSomethingCallback;
        
        private void Awake()
        {
            //get the overlap check for the start
            _overlapHookCheck = GetComponentInChildren<OverlapHookCheck>();
        }

        public HookProjectile Init(Vector3 gunBarrel, Action<IInteractWithHookProjectile> hitSomethingCallback,Vector3 playerLocation)
        {
            //get the location of the player.
            this.playerPos = playerLocation;
            //get the origin point of where we are shooting
            this.origin = gunBarrel;
            //get a callback so we can tell the gun we hit something
            _hitSomethingCallback = hitSomethingCallback;
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
            //check if you are starting in proximity of a hook
            hookConnectorStartPin = await _hookStartOverlap.GetMostOverlappedHookStartCol(playerPos);
            //if we are not on a start pin don't do anything
            if (hookConnectorStartPin is null)
                return;
            //otherwise change the layer of the layer of the socket so we won't hit it while we check with the end pin
            Utilities.PutObjectOnLayer(Utilities.SocketUsedLayer,hookConnectorStartPin.gameObject);
            //set the bool to true so update will run
            _hasStartPin = true;
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
            
            //if there is not any colliders we hit or if we hit something on the way back
            //don't run any logic
            if (col is null && _hitSomethingCallback is not null)
                return;

            //see if what we hit was something we can interact with
            IInteractWithHookProjectile somethingHit = col?.GetComponent<IInteractWithHookProjectile>();
            //if we hit a hook connector ignore it because this section does not handle that
            //if we what we hit is not soemthing we can interact with ignore it.
            if (somethingHit is null || somethingHit is HookConnector)
                return;
            //probably should tell the thing I hit that this interacted with it
            //somethingHit.InteractWithHookProjectile(this);
            
            //notify the gun we hit something
            _hitSomethingCallback?.Invoke(somethingHit);
            //i think i added this cause I was nervous it would be called twice, see if I can delete it later 
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
                Utilities.PutObjectOnLayer(Utilities.SocketUsedLayer,hookConnectorStartPin.gameObject);
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
                Utilities.PutObjectOnLayer(Utilities.SocketUsedLayer,hookConnectorEndPin.gameObject);
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
                Utilities.PutObjectOnLayer(Utilities.SocketUnusedLayer, hookConnectorStartPin.gameObject);
            }
            //if we have an end connector reset its layer
            if (hookConnectorEndPin != null)
            {
                Utilities.PutObjectOnLayer(Utilities.SocketUnusedLayer, hookConnectorEndPin.gameObject);
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
