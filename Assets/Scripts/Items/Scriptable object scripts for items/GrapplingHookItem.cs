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


        public override void Use(PlayerStateMachineManager stateManager, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {
            if (_projectile != null)
            {
                Debug.Log("Grappling hook is active");
                return;
                //send out again?
            }
                
            ItemFinishedCallback = callbackAction;
            DefaultState = defaultStateArg;
            Action(stateManager);
        }
        
        async void Action(PlayerStateMachineManager stateManager)
        {
            await _animationGrapplingHookSetUp.Play(stateManager);
            //maybe break this animation down?
            _originPoint = stateManager.GetComponentInChildren<OriginPoint>().transform.position;
            _currentLocation = _originPoint;
            _maxLocation = (stateManager.currentState.LookDirection * MaxDistance) + (Vector2)_originPoint;
            _projectile = Instantiate(projectilePrefab, _originPoint,Quaternion.identity).Init(_originPoint,HitSomething);
            _projectile.SetHookSprite(stateManager.currentState.LookDirection);
            //get the distance so we can caculate the time.
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

        void HitSomething(Collider2D col)
        {
            IInteractWithHookProjectile somethingHit = col.GetComponent<IInteractWithHookProjectile>();
            if (somethingHit is null)
                return;
            somethingHit.InteractWithHookProjectile(_projectile);
            switch (somethingHit)
            {
                case HookConnector hookConnector:
                    Hit(hookConnector);
                    //we dont want it to retract
                    return;
                    break;
                case Throwable throwable:
                    Hit(throwable);
                    break;
                case EnemyPlaceholder enemy:
                    Hit(enemy);
                    break;
                default:
                    break;
            }
            
            _projectileAnimation.Kill();
            //this should retract grappling hook
            RetractGrapplingHook();
        }

        void Hit(Throwable throwable)
        {
            
        }
        void Hit(EnemyPlaceholder throwable)
        {
            
        }
        void Hit(HookConnector hookConnector)
        {
            Debug.Log("Connect line");
        }
        
        
        public void ButtonUp()
        {
            _projectileAnimation.Kill();
            RetractGrapplingHook();
        }

        public override void PutAway()
        {
            // Debug.Log("item is done");
            _projectileAnimation.Kill();
            if (_projectile != null)
            {
                Destroy(_projectile.gameObject);
            }

            //this could be a different state depending what we latch onto
            ItemFinishedCallback?.Invoke(DefaultState);
        }
        

        float GetDistance(Vector3 start, Vector3 finish)
        {
            //this will always be 0 in one coord so no matter if it's vertical or horizontal it will return the distance
            Vector3 dist = finish - start;
            return dist.x + dist.y;
        }
        
        float MeasureTime(float distance)
        {
            const float speed = 8.5f;
            //time should always be positive
            return Mathf.Abs(distance / speed);
        }


    }
}
