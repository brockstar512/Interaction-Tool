using IT.Core.Combat;
using IT.Interactables;
using IT.Player.StateMachine;
using IT.Player.StateMachine.States;
using UnityEngine;

namespace IT.Player.Control
{
    // The existing on-foot player expressed as an IPlayerController.
    //
    // A plain C# adapter over PlayerStateMachine: it forwards the per-frame input
    // snapshot to the state machine's existing entry points and drives the ticks
    // that Unity used to call directly (Story 3.1 moved Update/FixedUpdate onto the
    // wrapper). The input dispatch below is byte-for-byte equivalent to the pre-3.1
    // PlayerInputHandler — this story must not change how the game plays.
    public sealed class OnFootController : IPlayerController
    {
        PlayerStateMachine _sm;
        PlayerWrapper _wrapper;
        Health _health;

        public IHealthSource HealthSource => _health;

        public void OnPossess(PlayerWrapper wrapper)
        {
            _wrapper = wrapper;
            _sm = wrapper.GetComponent<PlayerStateMachine>();
            _health = wrapper.GetComponent<Health>();
            if (_health == null)
                Debug.LogWarning("[OnFootController] No Health component on Player — HealthSource will be null");
        }

        public void OnRelease()
        {
            // Story 4.4 follow-up (latent Story 3.1 bug): force a CLEAN exit to idle before
            // dropping our references. SwitchState(defaultState) runs the full battle-tested
            // cleanup — bumps the transition token (so any in-flight async action sees
            // IsStale and aborts, FR-3), runs currentState.ExitState (e.g. PlayerThrowState
            // unsubscribes the held item's Destroyed handler — the leaked-subscription bug),
            // and clears the held item on entry into PlayerIdleState. Supersedes the old
            // IncrementTransitionCount() call, which only bumped the token (no ExitState, no
            // item clear) and so left non-idle states half-exited. Releases from idle (Story
            // 3.4 vehicle possession) are a near-no-op here — unchanged behavior.
            _sm?.SwitchState(_sm.defaultState);
            _sm = null;
            _wrapper = null;
            _health = null;
        }

        public void Tick(in PlayerInputState input)
        {
            _sm.UpdateMove(input.Move);

            // --- input dispatch: mirrors the old PlayerInputHandler exactly ---
            if (input.InteractPressed)
                _sm.Interact();

            if (input.SwitchItemPressed)
                _sm.itemManager.SwitchItem();

            if (input.UsePressed)
            {
                // UseItem.performed was a no-op while a hold-to-use (IButtonUp) state was active.
                if (_sm.currentState is IButtonUp)
                {
                    // no-op — the hold state owns the button until release
                }
                else
                {
                    _sm.UseItem();
                }
            }

            if (input.UseReleased)
            {
                if (_sm.currentState is IButtonUp buttonUp)
                    buttonUp.ButtonUp();
            }

            if (input.InteractReleased)
            {
                if (_sm.currentState is IButtonUp buttonUp)
                    buttonUp.ButtonUp();
                else if (_sm.currentState is PlayerMoveItemState)
                    _sm.Release();
            }
            // ------------------------------------------------------------------

            _sm.UpdateTick(); // was PlayerStateMachine.Update()
        }

        public void FixedTick()
        {
            _sm.FixedTick(); // was PlayerStateMachine.FixedUpdate()
        }
    }
}
