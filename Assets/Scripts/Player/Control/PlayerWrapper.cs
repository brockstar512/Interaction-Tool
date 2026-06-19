using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using IT.Player.Input;

namespace IT.Player.Control
{
    // Owns the player's per-frame tick order and routes input to the active
    // IPlayerController. Lives on the Player GameObject alongside PlayerStateMachine
    // and PlayerStatusManager (OQ-3.1-B). Story 3.1 only ever has one controller
    // (OnFootController); possession swaps arrive in Story 3.4.
    //
    // Story 3.2 (architecture D2): the wrapper now OWNS its own PlayerInputActions
    // instance, pairs it to its device(s) through the manual InputUser API
    // (no PlayerInput / PlayerInputManager — C-A), and POLLS those actions each
    // Update() to build PlayerInputState. The Story 3.1 PlayerInputHandler bridge is gone.
    public class PlayerWrapper : MonoBehaviour
    {
        IPlayerController _activeController;
        OnFootController _onFoot;

        public IPlayerController ActiveController => _activeController;
        public WrapperState State { get; private set; } = WrapperState.Active;

#pragma warning disable 67 // raised on possession swap starting Story 3.4; declared now for a stable public API
        public event System.Action ActiveControllerChanged;
#pragma warning restore 67

        // --- input ownership (Story 3.2, architecture D2) ---
        // The wrapper owns its actions and the InputUser they are paired to. Single-wrapper
        // for now; press-any-button join, hot-unplug suspend and re-pair (Story 3.3) hang
        // off this same _user.
        PlayerInputActions _actions;
        InputUser _user;

        void Awake()
        {
            _onFoot = new OnFootController();
            _activeController = _onFoot;
            _onFoot.OnPossess(this);

            SetUpInput();
        }

        // Auto-pair every currently-present compatible device (OQ-3.2-A): keyboard + any
        // gamepad. On a keyboard-only machine the gamepad branch is simply skipped (not an
        // error). AssociateActionsWithUser restricts the action set to the paired devices,
        // so this wrapper only ever reads its own input (the per-player routing FR-9 needs).
        void SetUpInput()
        {
            _actions = new PlayerInputActions();

            if (Keyboard.current != null)
                _user = InputUser.PerformPairingWithDevice(Keyboard.current, _user);
            if (Gamepad.current != null)
                _user = InputUser.PerformPairingWithDevice(Gamepad.current, _user);

            if (_user.valid)
                _user.AssociateActionsWithUser(_actions);

            _actions.Player.Enable();
        }

        void Update()
        {
            // Poll the paired actions directly. WasPressedThisFrame / WasReleasedThisFrame
            // give the same per-frame edges the 3.1 bridge captured via .performed/.canceled.
            var p = _actions.Player;
            var input = new PlayerInputState
            {
                Move              = p.Movement.ReadValue<Vector2>(),
                InteractPressed   = p.Interact.WasPressedThisFrame(),
                InteractReleased  = p.Interact.WasReleasedThisFrame(),
                UsePressed        = p.UseItem.WasPressedThisFrame(),
                UseReleased       = p.UseItem.WasReleasedThisFrame(),
                SwitchItemPressed = p.SwitchItem.WasPressedThisFrame(),
                PausePressed      = false, // no Pause action in PlayerControl.inputactions (OQ-3.2-D)
                EjectPressed      = false, // wired in Story 3.4
            };

            if (State == WrapperState.Active)
                _activeController.Tick(input);
        }

        void FixedUpdate()
        {
            if (State == WrapperState.Active)
                _activeController.FixedTick();
        }

        void OnDestroy()
        {
            if (_user.valid)
                _user.UnpairDevicesAndRemoveUser();
            if (_actions != null)
            {
                _actions.Player.Disable();
                _actions.Dispose();
            }
        }
    }
}
