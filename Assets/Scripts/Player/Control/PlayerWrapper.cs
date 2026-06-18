using UnityEngine;

namespace IT.Player.Control
{
    // Owns the player's per-frame tick order and routes input to the active
    // IPlayerController. Lives on the Player GameObject alongside PlayerStateMachine,
    // PlayerInputHandler and PlayerStatusManager (OQ-3.1-B). Story 3.1 only ever has
    // one controller (OnFootController); possession swaps arrive in Story 3.4.
    public class PlayerWrapper : MonoBehaviour
    {
        IPlayerController _activeController;
        OnFootController _onFoot;

        public IPlayerController ActiveController => _activeController;
        public WrapperState State { get; private set; } = WrapperState.Active;

#pragma warning disable 67 // raised on possession swap starting Story 3.4; declared now for a stable public API
        public event System.Action ActiveControllerChanged;
#pragma warning restore 67

        // --- input bridge (Story 3.1) ---
        // PlayerInputHandler sets these edge-triggered flags; Update() drains them into
        // a PlayerInputState each frame and clears them. Move is polled (FixedUpdate),
        // so it is not cleared here.
        // TODO (Story 3.2): entire bridge replaced by InputUser polling in PlayerWrapper.
        internal bool _interactPressed;
        internal bool _interactReleased;
        internal bool _usePressed;
        internal bool _useReleased;
        internal bool _switchItemPressed;
        internal Vector2 _moveInput;

        internal void SetMoveInput(Vector2 v) => _moveInput = v;

        void Awake()
        {
            _onFoot = new OnFootController();
            _activeController = _onFoot;
            _onFoot.OnPossess(this);
        }

        void Update()
        {
            var input = new PlayerInputState
            {
                Move              = _moveInput,
                InteractPressed   = _interactPressed,
                InteractReleased  = _interactReleased,
                UsePressed        = _usePressed,
                UseReleased       = _useReleased,
                SwitchItemPressed = _switchItemPressed,
                PausePressed      = false, // wired in Story 3.2
                EjectPressed      = false, // wired in Story 3.4
            };

            // Clear edge-triggered flags (Move is polled in FixedUpdate, not cleared).
            _interactPressed = _interactReleased = _usePressed = _useReleased = _switchItemPressed = false;

            if (State == WrapperState.Active)
                _activeController.Tick(input);
        }

        void FixedUpdate()
        {
            if (State == WrapperState.Active)
                _activeController.FixedTick();
        }
    }
}
