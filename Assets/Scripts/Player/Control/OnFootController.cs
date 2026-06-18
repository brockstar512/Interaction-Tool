using IT.Interactables;
using IT.Player.StateMachine;
using IT.Player.StateMachine.States;

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

        // Stateless stub — one shared instance, no per-access allocation.
        static readonly NullHealthSource s_health = new NullHealthSource();
        public IHealthSource HealthSource => s_health;

        public void OnPossess(PlayerWrapper wrapper)
        {
            _wrapper = wrapper;
            _sm = wrapper.GetComponent<PlayerStateMachine>();
        }

        public void OnRelease()
        {
            // Trip the stale-continuation guard exactly as SwitchState does, so any
            // in-flight async state action sees IsStale(token) and aborts before
            // acting on a state machine this controller no longer drives.
            _sm?.IncrementTransitionCount();
            _sm = null;
            _wrapper = null;
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

        // Stub health source until the real Health component lands in Story 4.1.
        sealed class NullHealthSource : IHealthSource
        {
            public int Current => 10;
            public int Max => 10;
            public event System.Action<int, int> HealthChanged { add { } remove { } }
        }
    }
}
