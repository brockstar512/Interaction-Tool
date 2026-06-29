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
            int token = stateManager.TransitionCount;
            try
            {
                _currentAnimation = _pickUpAnimation;
                await _pickUpAnimation.Play(stateManager);

                //stale-guard (Story 1.4): a SwitchState happened during the pickup await — abort silently.
                if (stateManager.IsStale(token)) return;

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

        // Story 4.4 Step 3.6: a controller swap interrupted the carry. Throw the held item via the
        // SAME path as a normal throw (item.Release → Toss → SetParent(null) + force), so the bomb
        // unparents from the player's ItemAnchorPoint and flies away instead of riding along.
        // Reuses the existing throw API; the normal Action throw path is untouched. The subsequent
        // SwitchState → ExitState unsubscribes Destroyed, exactly as after a normal throw.
        public override void OnInterrupt(PlayerStateMachine stateManager)
        {
            if (stateManager.item != null)
                stateManager.item.Release(stateManager);
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
            // Spam guard (Story 3.1.5): only throw once carrying is established. Spamming Interact
            // during the pickup animation must not re-enter Action and Release an item still being
            // picked up — ignore the press until _currentAnimation is the carry pose.
            if (_currentAnimation is not CarryAnimState)
                return;

            //safety net: if the item died between subscribing and the player pressing throw, bail safely
            if (stateManager.item == null)
            {
                stateManager.SwitchState(stateManager.defaultState);
                return;
            }

            int token = stateManager.TransitionCount;
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

            //stale-guard (Story 1.4): NOT in a finally — a SwitchState during the throw await
            //must not be re-overwritten by this continuation. (A genuine exception above still
            //falls through to the recovery SwitchState below.)
            if (stateManager.IsStale(token)) return;

            _currentAnimation = null;
            stateManager.SwitchState(stateManager.defaultState);
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

            int token = sm.TransitionCount;
            try
            {
                // ════════════════════════════════════════════════════════════
                //  PLAY HURT / FLINCH / KNOCKBACK ANIMATION HERE
                //  e.g.  await _hurtAnimation.Play(sm);
                //  create a new AnimStateBase subclass following the pattern
                //  of HurtToeAnimState, instantiate it in the constructor.
                //  NOTE: the stale-guard below already covers any await added here.
                // ════════════════════════════════════════════════════════════
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ThrowItemState.OnHeldItemDestroyed failed: {ex}");
            }

            //stale-guard (Story 1.4): not in a finally — if a future hurt-anim await lets the
            //state change, don't re-switch over it.
            if (sm.IsStale(token)) return;
            sm.SwitchState(sm.defaultState);
        }
    }
}