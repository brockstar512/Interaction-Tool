using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation;
    using IT.Animation.Player.States;
    using IT.Interactables;

    public class PlayerThrowState : PlayerStateBase
    {
        protected override float Speed => 4;

        readonly PickUpAnimState _pickUpAnimation;
        readonly ThrowAnimState _throwAnimation;
        readonly CarryAnimState _carryAnimation;
        AnimStateBase _currentAnimation = null;

        //tracks the destructible we subscribed to so we can unsubscribe cleanly.
        //null if the held item didn't implement IDestructible (a fixture that can't die in hand).
        IDestructible _subscribedItem;
        PlayerStateMachine _heldStateManager;

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

                //the held item might have been destroyed during the pickup animation await
                //(e.g. bomb fuse expired). bail to default instead of crashing on Interact.
                if (stateManager.item == null)
                {
                    stateManager.SwitchState(stateManager.defaultState);
                    return;
                }

                stateManager.item.Interact(stateManager);

                //if the held item can announce its destruction, subscribe so we can react.
                //non-destructibles just skip this — they won't die in our hand.
                if (stateManager.item is IDestructible destructible)
                {
                    _heldStateManager = stateManager;
                    _subscribedItem = destructible;
                    _subscribedItem.Destroyed += OnHeldItemDestroyed;
                }

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

        public override void ExitState(PlayerStateMachine stateManager)
        {
            //always unsubscribe on exit so we don't leak handlers or double-fire on the next throw
            if (_subscribedItem != null)
            {
                _subscribedItem.Destroyed -= OnHeldItemDestroyed;
                _subscribedItem = null;
            }
            _heldStateManager = null;
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
            //safety net: if the item died between subscribing and the player pressing throw, bail safely
            if (stateManager.item == null)
            {
                stateManager.SwitchState(stateManager.defaultState);
                return;
            }

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

        //fires when a destructible we were holding gets destroyed (bomb explosion in hand, etc.)
        private async void OnHeldItemDestroyed()
        {
            //unsubscribe immediately so this can't fire twice
            if (_subscribedItem != null)
            {
                _subscribedItem.Destroyed -= OnHeldItemDestroyed;
                _subscribedItem = null;
            }

            var sm = _heldStateManager;
            if (sm == null) return;

            try
            {
                // ════════════════════════════════════════════════════════════
                //  PLAY HURT / FLINCH / KNOCKBACK ANIMATION HERE
                //  e.g.  await _hurtAnimation.Play(sm);
                //  create a new AnimStateBase subclass following the pattern
                //  of HurtToeAnimState, instantiate it in the constructor.
                // ════════════════════════════════════════════════════════════
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ThrowItemState.OnHeldItemDestroyed failed: {ex}");
            }
            finally
            {
                sm.SwitchState(sm.defaultState);
            }
        }
    }
}