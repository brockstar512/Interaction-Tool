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
    // Story 3.2 (architecture D2): the wrapper owns its PlayerInputActions instance,
    // pairs it to its device(s) via the manual InputUser API (no PlayerInput /
    // PlayerInputManager — C-A), and polls those actions each Update() to build
    // PlayerInputState.
    //
    // Story 3.3: SetUpInput(InputDevice) pairs exactly one device (the specific device
    // that triggered a join, or the auto-detected primary device for the scene-placed P1
    // wrapper). PlayerRoster coordinates join/suspend/re-pair via the internal API below.
    public class PlayerWrapper : MonoBehaviour
    {
        IPlayerController _activeController;
        OnFootController _onFoot;

        public IPlayerController ActiveController => _activeController;
        public WrapperState State { get; private set; } = WrapperState.Active;

#pragma warning disable 67 // raised on possession swap starting Story 3.4; declared now for a stable public API
        public event System.Action ActiveControllerChanged;
#pragma warning restore 67

        // Fires on every WrapperState transition. Story 7.3 (HUD reconnect overlay) subscribes here.
        public event System.Action<PlayerWrapper> StateChanged;

        public InputUser User => _user;
        public bool OwnsDevice(InputDevice device)
            => _user.valid && _user.pairedDevices.ContainsReference(device);

        // --- input ownership (architecture D2) ---
        PlayerInputActions _actions;
        InputUser _user;

        void Awake()
        {
            _onFoot = new OnFootController();
            _activeController = _onFoot;
            _onFoot.OnPossess(this);

            // PendingJoinDevice is set by PlayerRoster.TryJoin() before Instantiate and
            // cleared here immediately on read. Null means this is the scene-placed P1
            // wrapper: auto-pair to keyboard, fall back to the first available gamepad.
            var device = PlayerRoster.PendingJoinDevice;
            PlayerRoster.PendingJoinDevice = null;
            device = device ?? (InputDevice)Keyboard.current ?? Gamepad.current;
            if (device != null)
                SetUpInput(device);

            PlayerRoster.Instance.Register(this);
        }

        // Pairs exactly one device (architecture D2 — per-player routing).
        // Called from Awake with the auto-detected or roster-supplied device.
        void SetUpInput(InputDevice pairDevice)
        {
            _actions = new PlayerInputActions();
            _user = InputUser.PerformPairingWithDevice(pairDevice, _user);
            if (_user.valid)
                _user.AssociateActionsWithUser(_actions);
            if (_user.valid)            // D-1 fix: only enable when a device is actually paired
                _actions.Player.Enable();
        }

        internal void Suspend()
        {
            State = WrapperState.Suspended;
            if (_actions != null)
                _actions.Player.Disable();  // D-3 fix: don't consume edges while frozen
            StateChanged?.Invoke(this);
        }

        internal void Resume()
        {
            State = WrapperState.Active;
            if (_user.valid && _actions != null)
                _actions.Player.Enable();
            StateChanged?.Invoke(this);
        }

        internal void RePair(InputDevice newDevice)
        {
            _user = InputUser.PerformPairingWithDevice(newDevice, _user);
            if (_user.valid)
                _user.AssociateActionsWithUser(_actions);
            Resume();
        }

        void Update()
        {
            if (_actions == null) return;

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
