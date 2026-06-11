using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation;
    using IT.Animation.Player.States;

    public class PlayerThrowState : PlayerStateBase
    {
        protected override float Speed => 4;

        readonly PickUpAnimState _pickUpAnimation;
        readonly ThrowAnimState _throwAnimation;
        readonly CarryAnimState _carryAnimation;
        AnimStateBase _currentAnimation = null;
    
        public PlayerThrowState()
        {
            _pickUpAnimation = new PickUpAnimState();
            _throwAnimation = new ThrowAnimState(); 
            _carryAnimation = new CarryAnimState();
        }
    
        public override async void EnterState(PlayerStateMachine stateManager)
        {
            try
            {
                _currentAnimation = _pickUpAnimation;
                await _pickUpAnimation.Play(stateManager);
                stateManager.item.Interact(stateManager);
                _currentAnimation = _carryAnimation;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ThrowItemState.EnterState failed: {ex}");
                stateManager.SwitchState(stateManager.defaultState);   // recover instead of hanging
            }
        }
    
        public override void UpdateState(PlayerStateMachine stateManager)
        {
            UpdateLookDirection(stateManager.movement);
        }
        public override void OnCollisionEnter(PlayerStateMachine stateManager, Collision collision)
        {

        }

        public override void ExitState(PlayerStateMachine stateManager)
        {
        
        }

        public override void FixedUpdateState(PlayerStateMachine stateManager)
        {
            if (_currentAnimation is not CarryAnimState)
                return;
        
            base.Move(stateManager);
            _carryAnimation.Play(stateManager);
        }

        public override async void Action(PlayerStateMachine stateManager)
        {
            try
            {
                _currentAnimation = _throwAnimation;
                stateManager.item.Release(stateManager);
                await _throwAnimation.Play(stateManager);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ThrowItemState.Action failed: {ex}");
            }
            finally
            {
                _currentAnimation = null;
                stateManager.SwitchState(stateManager.defaultState);
            }
        }
    }
}
