using UnityEngine;
using Items.SubItems;
using System;
using Interactable;
using DG.Tweening;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "GrapplingHookItemObject", menuName = "ScriptableObjects/Grappling Hook")]
    public class GrapplingHookItem : Item, IButtonUp
    {
        [SerializeField] private HookProjectile projectilePrefab;
        HookProjectile _projectile;
        private readonly AnimationGrapplingHook _animationGrapplingHook = new AnimationGrapplingHook();
        private Tweener _projectileAnimation;
        private const float MaxDistance = 7.5f;
        private Vector3 _originPoint = Vector3.zero;

        public override void Use(PlayerStateMachineManager stateManager, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {
            ItemFinishedCallback = callbackAction;
            DefaultState = defaultStateArg;
            Action(stateManager);
        }
        
        async void Action(PlayerStateMachineManager stateManager)
        {
            await _animationGrapplingHook.Play(stateManager);
            _originPoint = stateManager.GetComponentInChildren<OriginPoint>().transform.position;
            Vector3 destination = (stateManager.currentState.LookDirection * MaxDistance) + (Vector2)_originPoint;
            _projectile = Instantiate(projectilePrefab, _originPoint,Quaternion.identity).Init(_originPoint);
            _projectile.SetHookSprite(stateManager.currentState.LookDirection);
            //get the distance so we can caculate the time.
            float time = MeasureTime(GetDistance(_originPoint,destination));
            SendGrapplingHook(destination,time);
        }
        void SendGrapplingHook(Vector3 destination,float time)
        {
            _projectileAnimation = _projectile.transform.DOMove(destination, time).SetEase(Ease.Linear);
            _projectileAnimation.onComplete = RetractGrapplingHook;
            Debug.Log("send grappling hook");
        }
        void RetractGrapplingHook()
        {
            Vector3 currentPos = _projectile.transform.position;
            float time = MeasureTime(GetDistance(currentPos,_originPoint));
            _projectileAnimation = _projectile.transform.DOMove(_originPoint, time).SetEase(Ease.Linear);
            //PutAway();
        }

        void HitSomething()
        {
            _projectileAnimation.Kill();
            RetractGrapplingHook();
        }
        
        public void ButtonUp()
        {
            //todo we want to ake sure the edge cases work consistantly...
            //1) when to goes all the way and starts to retract and button goes up
            //2)when the item is halfway there and the button goes up
            //3)when we are fully done with the state and we transition and button goes up
            //todo now we need to run this when we hit a wall, or we hit an interactable.. if
            Debug.Log("Button up");
            _projectileAnimation.Kill();
            RetractGrapplingHook();
        }

        public override void PutAway()
        {
            Debug.Log("item is done");
            Destroy(projectilePrefab.gameObject);
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
    /*
     



    float MeasureTime(float distance)
       {
           const float speed = 15;
           return (distance / speed);
       }
               _moverCheck.SetEmergencyStop(EmergencyStopTween);


        slideAnimation = transform.DOMove(destination, time);
        slideAnimation.onComplete = CleanUp;
        
        
            void EmergencyStopTween()
       {
           slideAnimation.Kill();
           CleanUp();
       }
        
     * 
     */
}
