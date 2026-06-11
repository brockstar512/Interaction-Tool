using UnityEngine;
using System;
using DG.Tweening;


namespace IT.Items.GrapplingHook
{
    using IT.Animation.Player.States;
    using IT.Enemies;
    using IT.Interactables;
    using IT.Interactables.Throwable;
    using IT.Player.Movement;

    public class GrapplingHook : ItemBase, IButtonUp
    {
        //prefab for projectile
        [SerializeField] private GrappleProjectile projectilePrefab;
        //this is the projectile we are actually shooting
        GrappleProjectile _projectile;
        //this is the animation set to set up the animation
        private readonly GrappleSetupAnimState _animationGrapplingHookSetUp = new GrappleSetupAnimState();
        //this is the animation set to shoot the animation
        private readonly GrappleAnimState _animationGrapplingHookFire = new GrappleAnimState();
        //this is the tween that actually shoots the animation
        private Tweener _projectileAnimation;
        //this is the max distance the grappling hook will go
        private const float MaxDistance = 5f;
        //this is where we no where the grappling hook should return to
        private Vector3 _originPoint = Vector3.zero;
        //this is so we know where the grappling hook always is
        private Vector3 _currentLocation = Vector3.zero;
        //this is the max location where the grappling hook can go
        private Vector3 _maxLocation = Vector3.zero;
        //this is so for if we hit an item we can interact with it...
        //right now we only are allowed to interact with throwable items
        private Interactable item { get; set; }
        //this is the callback for when we can dispose
        //of the item if we hit a hook connector
        Action _disposeOfItem;
        
        public override void Use(IInteractionContext context)
        {
            if (_projectile != null)
            {
                return;
            }

            _disposeOfItem = context.Items.DisposeOfCurrentItem;
            ItemFinishedCallback = context.EndInteraction;
            Action(context);
        }

        async void Action(IInteractionContext context)
        {
            try
            {
                await _animationGrapplingHookSetUp.Play(context);
                _originPoint = context.Transform.GetComponentInChildren<ItemAnchorPoint>().transform.position;
                _currentLocation = _originPoint;
                _maxLocation = (context.LookDirection * MaxDistance) + (Vector2)_originPoint;
                _projectile = Instantiate(projectilePrefab, _originPoint, Quaternion.identity).Init(_originPoint, HitSomething, context.Transform.position);
                _projectile.SetHookSprite(context.LookDirection);
                SendGrapplingHook();
                await _animationGrapplingHookFire.Play(context);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GrapplingHookItem.Action failed: {ex}");
                PutAway();
            }
        }
        void SendGrapplingHook()
        {
            //get the location of the grappling hook so we can gage the time
            _currentLocation = _projectile.transform.position;
            //get the time for a consistent speed
            float time = MeasureTime(GetDistance(_currentLocation,_maxLocation));
            //animation it
            _projectileAnimation = _projectile.transform.DOMove(_maxLocation, time).SetEase(Ease.Linear);
            //when the animation is done retract the grappling hook.
            _projectileAnimation.onComplete = RetractGrapplingHook;
        }
        
        void RetractGrapplingHook()
        {
            //if there is no grappling hook reset so you are not stuck in this state...
            //I don't think this is ever suppose to run but it's here to prevent null
            //references if something is wrong
            if (_projectile == null)
            {
                PutAway();
                return;
            }
            //get the location to calculate the time... this is more important than
            //the send grappling hook because if you press the button up the distance is not constant
            _currentLocation = _projectile.transform.position;
            //calcuate the time
            float time = MeasureTime(GetDistance(_currentLocation,_originPoint));
            //animate the grappling hook
            _projectileAnimation = _projectile.transform.DOMove(_originPoint, time).SetEase(Ease.Linear);
            //when the animation is done. go to default state
            _projectileAnimation.onComplete = PutAway;
        }

        void HitSomething(IGrappleTarget somethingHit)
        {

            //the projectile tells the gun you hit something with the interface IGrappleTarget
            switch (somethingHit)
            {
                //if it was a hook connector
                case GrappleSocket:
                    //kill the current animation
                    _projectileAnimation.Kill();
                    //destroy the start and end of the connectors script
                    //so you can't connect connect with them again
                    Destroy(_projectile.hookConnectorStartPin);
                    Destroy(_projectile.hookConnectorEndPin);
                    //destroy the grappling hook
                    Destroy(_projectile.gameObject);
                    //dispose of the item from your inventory
                    _disposeOfItem?.Invoke();
                break;
                //you hit a throwable item
                case ThrowableBase throwable:
                    //cache the item that you hit
                    item = throwable;
                    //stop the current animation
                    _projectileAnimation.Kill();
                    //retract the grappling hook
                    RetractGrapplingHook();
                    break;
                case EnemyDummy:
                    _projectileAnimation.Kill();
                    //this should retract grappling hook
                    RetractGrapplingHook();
                    break;
            }
            
        }
        
        float GetDistance(Vector3 start, Vector3 finish)
        {
            //this will always be 0 in one coord
            //so no matter if it's vertical or horizontal it will return the distance
            Vector3 dist = finish - start;
            return dist.x + dist.y;
        }
        
        float MeasureTime(float distance)
        {
            //this is the constant speed
            const float speed = 8.5f;
            //time should always be positive
            return Mathf.Abs(distance / speed);
        }
        
        public void ButtonUp()
        {
            //when you the button goes up
            //which this is invoked by the state
            //it will retract the grappling hook
            _projectileAnimation.Kill();
            RetractGrapplingHook();
        }

        public override void PutAway()
        {
            //if there is remnants of an animation kill it
            _projectileAnimation.Kill();
            //destroy the projectile
            if (_projectile != null)
            {
                Destroy(_projectile.gameObject);
            }


            //if this is null it will go to default...
            //if we switched item to throwable
            //we will switched to that state
            ItemFinishedCallback?.Invoke(item);
            
        }
    }
}
