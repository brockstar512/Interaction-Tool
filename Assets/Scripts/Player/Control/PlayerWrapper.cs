using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using IT.Player.Input;
using IT.Player.Status;
using IT.Interactables.Vehicle;
using UnityEngine.InputSystem.Utilities;

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

        // Story 3.4 — possession. _vehicle is the currently possessed vehicle (null on foot).
        // _pendingVehicle is a deferred-swap request recorded by PossessVehicle and consumed
        // at the top of the next Update (see PossessVehicle for the re-entrancy rationale).
        VehicleController _vehicle;
        VehicleController _pendingVehicle;

        // Story 4.4 — generic controller swap (On-Fire and future mode-statuses). Mirrors the
        // _pendingVehicle deferred-swap pattern: SwapController records the request here and it
        // is performed at the top of the next Update, never synchronously (the requesting
        // OnApply may run inside a controller Tick). _controllerBeforeSwap remembers the
        // controller to restore to (non-null = currently swapped). Unlike vehicle possession,
        // a controller swap keeps the visual root VISIBLE — the player stays on screen.
        IPlayerController _pendingController;
        IPlayerController _controllerBeforeSwap;

        // Player.prefab's Visual child (SpriteRenderer + Animator), hidden while possessing a
        // vehicle (OQ-3.4-C). Wired in the Inspector; null-guarded in SetVisualRootActive.
        [SerializeField] private GameObject _visualRoot;

        Rigidbody2D _rb;

        // Story 4.3 — per-player status effects. Driven from Update (status -> health ->
        // controller order, FR-10), gated by WrapperState.Active so Suspend pauses ticking.
        StatusController _status;

        public IPlayerController ActiveController => _activeController;

        // Story 4.4 Step 4: true while possessing a vehicle (_vehicle non-null). OnFireEffect
        // uses this to keep On-Fire on-foot-only (Q5).
        public bool IsPossessingVehicle => _vehicle != null;
        public WrapperState State { get; private set; } = WrapperState.Active;

        // Fires when the active IPlayerController changes (possess / eject). Story 7.3 (HUD
        // health-source rebind) subscribes here. First live use: Story 3.4 possession swap.
        public event System.Action ActiveControllerChanged;

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
            _rb = GetComponent<Rigidbody2D>();
            _status = GetComponent<StatusController>();
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
            if (State == WrapperState.Suspended) return;  // idempotent — no redundant StateChanged
            State = WrapperState.Suspended;
            if (_actions != null)
                _actions.Player.Disable();  // D-3 fix: don't consume edges while frozen
            StateChanged?.Invoke(this);
        }

        internal void Resume()
        {
            if (State == WrapperState.Active) return;     // idempotent — no redundant StateChanged
            State = WrapperState.Active;
            if (_user.valid && _actions != null)
                _actions.Player.Enable();
            StateChanged?.Invoke(this);
        }

        internal void RePair(InputDevice newDevice)
        {
            // Drop the stale/lost device(s) before pairing the new one, so the user doesn't
            // accumulate dead pairings across repeated unplug → rejoin-with-a-different-device
            // cycles. UnpairDevices() keeps the InputUser (unlike UnpairDevicesAndRemoveUser).
            if (_user.valid)
                _user.UnpairDevices();
            _user = InputUser.PerformPairingWithDevice(newDevice, _user);
            if (_user.valid)
                _user.AssociateActionsWithUser(_actions);
            Resume();
        }

        void Update()
        {
            if (_actions == null) return;

            // Deferred possession swap (Story 3.4). PossessVehicle is requested from inside
            // OnFootController.Tick; performing the swap there would null the on-foot _sm
            // mid-Tick and NRE on Tick's trailing _sm.UpdateTick(). So we do the real swap
            // here, at the top of the next Update, before any input is built or ticked —
            // the requesting on-foot Tick has already finished on a valid _sm by now.
            if (_pendingVehicle != null)
            {
                var pending = _pendingVehicle;
                _pendingVehicle = null;
                PerformPossess(pending);
            }

            // Deferred controller swap / restore (Story 4.4 On-Fire). Same top-of-Update timing
            // as the vehicle swap above, so the swapped-in controller ticks this frame with fresh
            // input. Mutually exclusive with vehicle possession in practice (controller-swapping
            // statuses are on-foot-only, Q5).
            if (_pendingController != null)
            {
                var next = _pendingController;
                _pendingController = null;
                PerformControllerSwap(next);
            }

            // Poll the paired actions directly. WasPressedThisFrame / WasReleasedThisFrame
            // give the same per-frame edges the 3.1 bridge captured via .performed/.canceled.
            var p = _actions.Player;
            bool interact   = p.Interact.WasPressedThisFrame();
            bool possessing = _activeController != _onFoot;
            var input = new PlayerInputState
            {
                Move              = p.Movement.ReadValue<Vector2>(),
                // OQ-3.4-A → (b): Interact doubles as enter (on foot) / eject (possessing).
                InteractPressed   = !possessing && interact,
                InteractReleased  = p.Interact.WasReleasedThisFrame(),
                UsePressed        = p.UseItem.WasPressedThisFrame(),
                UseReleased       = p.UseItem.WasReleasedThisFrame(),
                SwitchItemPressed = p.SwitchItem.WasPressedThisFrame(),
                PausePressed      = false, // no Pause action in PlayerControl.inputactions (OQ-3.2-D)
                EjectPressed      = possessing && interact,
            };

            if (State != WrapperState.Active)
                return;

            // Story 4.3 — advance status effects before the controller ticks (FR-10
            // status -> health -> controller). Past the Active gate, so Suspend pauses
            // status timing; before the eject check, so an eject frame still ticks status.
            _status?.Tick(Time.deltaTime);

            // Eject interception (Story 3.4): handle eject at the wrapper level, before the
            // vehicle controller is ticked, so it never sees the eject-frame input. Possession
            // ENTRY can't be intercepted here (it needs the state machine's overlap detection),
            // which is why entry uses the deferred-swap path above instead.
            if (possessing && input.EjectPressed)
            {
                Eject();
                return;
            }

            _activeController.Tick(input);
        }

        void FixedUpdate()
        {
            if (State == WrapperState.Active)
                _activeController.FixedTick();
        }

        // --- possession (Story 3.4, C-D: swap only via OnRelease → OnPossess) ---

        // Possession ENTRY request. Called from VehicleInteractable.Interact, which runs
        // synchronously inside OnFootController.Tick (the Interact dispatch). We must NOT swap
        // controllers here: _onFoot.OnRelease() nulls the on-foot _sm, and Tick continues past
        // the dispatch to its trailing _sm.UpdateTick() — a re-entrancy NRE on every possession.
        // Instead we record the request; the actual swap runs from Update via PerformPossess.
        // Approved deferred-swap pattern — see story spec Change Log 2026-06-22.
        public void PossessVehicle(VehicleController vehicle)
        {
            if (vehicle == null) return;
            _pendingVehicle = vehicle;
        }

        // The real controller swap into a vehicle. Only ever called from Update (never from
        // inside a controller Tick), so nulling the on-foot _sm here is safe.
        void PerformPossess(VehicleController vehicle)
        {
            _activeController.OnRelease();   // on-foot: trips stale token, nulls _sm
            _vehicle = vehicle;
            SetVisualRootActive(false);      // player is "inside" the vehicle
            vehicle.OnPossess(this);
            _activeController = vehicle;
            ActiveControllerChanged?.Invoke();
        }

        // Possession EXIT. Reverses PerformPossess: releases the vehicle, restores the player
        // at the vehicle's last position, re-possesses on-foot. Called from Update's eject
        // interception and from VehicleController.OnHealthDepleted (0-HP eject-and-explode).
        public void Eject()
        {
            if (_vehicle == null) return;    // guard: not possessing (also covers null-wrapper case)

            var ejectPos = _vehicle.transform.position;
            _activeController.OnRelease();   // vehicle: nulls its wrapper ref
            _vehicle = null;
            _rb.position = ejectPos;          // player reappears where the vehicle was
            SetVisualRootActive(true);
            _onFoot.OnPossess(this);         // re-links _sm on OnFootController
            _activeController = _onFoot;
            ActiveControllerChanged?.Invoke();
        }

        // --- generic controller swap (Story 4.4, C-D: swap only via OnRelease → OnPossess) ---

        // On-Fire activation (OnFireEffect.OnApply) requests this. Deferred exactly like
        // PossessVehicle — the caller may be mid-Tick, so we only record the request and perform
        // it at the top of the next Update. Refused while possessing a vehicle (controller swaps
        // are on-foot-only in v1, Q5) and refused if a swap is already pending/active (one
        // mode-swap at a time). Graceful no-op + warning, never a crash.
        public void SwapController(IPlayerController next)
        {
            if (next == null) return;
            if (_vehicle != null)
            {
                Debug.LogWarning("[PlayerWrapper] SwapController ignored — controller swaps " +
                    "(e.g. On-Fire) are on-foot-only in v1; player is possessing a vehicle.");
                return;
            }
            if (_pendingController != null || _controllerBeforeSwap != null)
            {
                Debug.LogWarning("[PlayerWrapper] SwapController ignored — a controller swap is " +
                    "already pending or active (one mode-swap at a time in v1).");
                return;
            }
            _pendingController = next;
        }

        // On-Fire expiry (OnFireEffect.OnExpire) requests this — a deferred swap back to the
        // controller active before the swap. No-op if nothing is swapped.
        public void RestoreController()
        {
            if (_controllerBeforeSwap == null) return;
            _pendingController = _controllerBeforeSwap;
        }

        // The real swap. Only ever called from Update (never inside a Tick), so releasing the
        // current controller here is safe. OnRelease BEFORE OnPossess, never simultaneous — same
        // ordering as PerformPossess. Does NOT touch the visual root (player stays visible).
        void PerformControllerSwap(IPlayerController next)
        {
            var previous = _activeController;
            previous.OnRelease();              // release current — trips stale token if it has one (OnFoot does)
            next.OnPossess(this);              // possess next — OnFoot re-links _sm; OnFire grabs rb/animator/health
            _activeController = next;

            // Track the base controller to restore to. Given the SwapController guards, only two
            // paths reach here:
            //   • first swap away from base (_controllerBeforeSwap == null) → record the base we left
            //   • restore back to that base (next == _controllerBeforeSwap)  → clear the slot
            // The implicit third case (swap between two non-base controllers) is unreachable, and
            // we deliberately leave _controllerBeforeSwap UNTOUCHED there so the true base is
            // never lost.
            if (_controllerBeforeSwap == null)
                _controllerBeforeSwap = previous;     // swapping away from base
            else if (next == _controllerBeforeSwap)
                _controllerBeforeSwap = null;         // returning to base

            ActiveControllerChanged?.Invoke();
        }

        // Null-guarded visual toggle (OQ-3.4-C). If the Inspector slot is unwired we log and
        // skip rather than NRE, so missing wiring surfaces as a warning, not a crash.
        void SetVisualRootActive(bool active)
        {
            if (_visualRoot == null)
            {
                Debug.LogWarning("[PlayerWrapper] _visualRoot not assigned in Inspector — visual hiding skipped");
                return;
            }
            _visualRoot.SetActive(active);
            if (active)
                _visualRoot.transform.localRotation = Quaternion.identity;
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
