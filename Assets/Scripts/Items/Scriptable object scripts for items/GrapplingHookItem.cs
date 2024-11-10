using UnityEngine;
using Items.SubItems;
using System;
using Interactable;
using DG.Tweening;
using Animation.PlayerAnimation.AnimationStates;
using Interface;


namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "GrapplingHookItemObject", menuName = "ScriptableObjects/Grappling Hook")]
    public class GrapplingHookItem : Item, IButtonUp
    {
        [SerializeField] private HookProjectile projectilePrefab;
        HookProjectile _projectile;
        private readonly AnimationGrapplingHookSetUp _animationGrapplingHookSetUp = new AnimationGrapplingHookSetUp();
        private readonly AnimationGrapplingHook _animationGrapplingHookFire = new AnimationGrapplingHook();
        private Tweener _projectileAnimation;
        private const float MaxDistance = 5f;
        private Vector3 _originPoint = Vector3.zero;
        private Vector3 _currentLocation = Vector3.zero;
        private Vector3 _maxLocation = Vector3.zero;
        private InteractableBase item { get; set; } = null;
        Action _disposeOfItem;




        public override void Use(PlayerStateMachineManager stateManager)
        {
            if (_projectile != null)
            {
                Debug.Log("Grappling hook is active");
                return;
                //send out again?
            }

            _disposeOfItem = stateManager.itemManager.DisposeOfCurrentItem;
            ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;

            Action(stateManager);
        }


        
        async void Action(PlayerStateMachineManager stateManager)
        {
            await _animationGrapplingHookSetUp.Play(stateManager);
            _originPoint = stateManager.GetComponentInChildren<OriginPoint>().transform.position;
            _currentLocation = _originPoint;
            _maxLocation = (stateManager.currentState.LookDirection * MaxDistance) + (Vector2)_originPoint;
            _projectile = Instantiate(projectilePrefab, _originPoint,Quaternion.identity).Init(_originPoint,HitSomething,stateManager.transform.position);
            _projectile.SetHookSprite(stateManager.currentState.LookDirection);
            SendGrapplingHook();
            await _animationGrapplingHookFire.Play(stateManager);
        }
       
        void SendGrapplingHook()
        {
            _currentLocation = _projectile.transform.position;
            float time = MeasureTime(GetDistance(_currentLocation,_maxLocation));
            _projectileAnimation = _projectile.transform.DOMove(_maxLocation, time).SetEase(Ease.Linear);
            _projectileAnimation.onComplete = RetractGrapplingHook;
        }
        
        void RetractGrapplingHook()
        {
            if (_projectile == null)
            {
                PutAway();
                return;
            }

            _currentLocation = _projectile.transform.position;
            float time = MeasureTime(GetDistance(_currentLocation,_originPoint));
            _projectileAnimation = _projectile.transform.DOMove(_originPoint, time).SetEase(Ease.Linear);
            _projectileAnimation.onComplete = PutAway;
        }

        void HitSomething(IInteractWithHookProjectile somethingHit)
        {
            Debug.Log($"Here is item 1 {item==null}");

            //notifies whatever it hit that it was hit as well as 
            //does what the character needs to do as a result of the type of hit
            Debug.Log($"We hit {somethingHit.GetType()}");
            switch (somethingHit)
            {
                //fine
                case HookConnector:
                    _projectileAnimation.Kill();
                    Destroy(_projectile.hookConnectorStartPin);
                    Destroy(_projectile.hookConnectorEndPin);
                    Destroy(_projectile.gameObject);
                    _disposeOfItem?.Invoke();
                break;
                case Throwable throwable:
                    item = throwable;
                    _projectileAnimation.Kill();
                    //this should retract grappling hook
                    RetractGrapplingHook();
                    break;
                case EnemyPlaceholder:
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
            const float speed = 8.5f;
            //time should always be positive
            return Mathf.Abs(distance / speed);
        }
        
        public void ButtonUp()
        {
            _projectileAnimation.Kill();
            RetractGrapplingHook();
        }

        public override void PutAway()
        {
            Debug.Log("Shooting grappling hook");
            _projectileAnimation.Kill();
            if (_projectile != null)
            {
                Destroy(_projectile.gameObject);
            }
            Debug.Log($"Here is item 2 {item==null}");


            //if this is null it will go to default...
            //depending on the item it will figure it out in the callback
            ItemFinishedCallback?.Invoke(item);
            
        }
    }
}
