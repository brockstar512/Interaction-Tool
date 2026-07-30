using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using IT.Player.Input;
using IT.Player.Persistence;   // PB.4.5 R3: rejoin restore (PendingRestoreDto consume)
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

        // Story 5.1 (spec Design Decision 4 — the EffectivePosition keystone). Possession-aware,
        // Suspend-agnostic world position: the possessed vehicle's body while possessing, else
        // the player's own body. SegmentManager reads this per roster wrapper for point-in-rect
        // membership (RectShape.Contains) — see Story 5.1 spec.
        //
        // Why the vehicle branch: while possessing, the player's own _rb is FROZEN — nothing
        // writes it until Eject() (Eject does _rb.position = _vehicle.Position on exit). Reading
        // _rb here would report the stale pre-possession spot, so we read the vehicle's live body.
        //
        // Why NO WrapperState check (owner ruling R3): membership is "where the body is," not
        // whether input is live. This must return a correct position even while Suspended, so it
        // never branches on State (unlike Update/FixedUpdate, which gate on Active).
        public Vector2 EffectivePosition
            => IsPossessingVehicle ? _vehicle.Position : _rb.position;

        public WrapperState State { get; private set; } = WrapperState.Active;

        // Fires when the active IPlayerController changes (possess / eject). Story 7.3 (HUD
        // health-source rebind) subscribes here. First live use: Story 3.4 possession swap.
        public event System.Action ActiveControllerChanged;

        // Fires on every WrapperState transition. Story 7.3 (HUD reconnect overlay) subscribes here.
        public event System.Action<PlayerWrapper> StateChanged;

        public InputUser User => _user;
        public bool OwnsDevice(InputDevice device)
            => _user.valid && _user.pairedDevices.ContainsReference(device);

        // Story PB.4 (DD5) — stable slot identity ("P1"/"P2"). Assigned by PlayerRoster.Register at
        // first registration, or adopted from PlayerRoster.PendingSlot when a respawn threads the
        // existing id (R3). The per-playerId HUD map (DD4) keys on it. NOT count-derived (OQ-PB4-D).
        public string PlayerId { get; private set; }
        internal void AssignId(string id) => PlayerId = id;

        // Story PB.4 R5 (OQ-PB4-E) — a STABLE id for the paired device, derived at pair time via
        // DeriveDeviceId (serial → product+manufacturer → device.name). Populated + round-tripped
        // through the DTO; CONSUMING it (reconnect → slot re-association) is SAVE.1/later.
        public string DeviceId => _deviceId;
        string _deviceId = "";

        // Story PB.4 R5 — derive a STABLE device id from the description: serial if the device reports
        // one, else product+manufacturer, else the InputSystem device name (Unity's per-device
        // non-empty guarantee). A paired device must NEVER yield "" — that empty value is the seam's
        // ABSENT marker, so colliding a real device with it would be a categorical bug. LIMITATION
        // (accepted for v1 couch co-op): product+manufacturer and device.name are MODEL ids, not
        // per-UNIT — two identical serial-less pads collide; per-unit uniqueness is a post-v1 OQ-PB4-E
        // concern. Reconnect (RePair) re-derives: it may pair a DIFFERENT device than before.
        internal static string DeriveDeviceId(InputDevice device)   // PB.4.5 R3: roster derives for the reclaim lookup
        {
            if (device == null) return "";
            var d = device.description;
            if (!string.IsNullOrEmpty(d.serial)) return d.serial.Trim();
            var pm = $"{d.product} {d.manufacturer}".Trim();
            if (!string.IsNullOrEmpty(pm)) return pm;
            return (device.name ?? "").Trim();
        }

        // Story PB.4 (R3/DD6) — the device to thread across a respawn for input continuity. First
        // paired device, or null.
        public InputDevice PairedDevice
            => _user.valid && _user.pairedDevices.Count > 0 ? _user.pairedDevices[0] : null;

        // Story PB.4 (R3) — explicit device release for the atomic respawn swap. The deferred
        // OnDestroy unpair runs too late (the fresh wrapper's Awake re-pairs BEFORE the old
        // wrapper's OnDestroy frees the device), so the SpawnManager releases here before
        // Instantiate. Keeps the InputUser (like RePair), just unpairs the device.
        internal void ReleaseDevice()
        {
            if (_user.valid) _user.UnpairDevices();
        }

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

            // PB.4 (DD5): adopt a reserved slot id threaded by a respawn (PlayerRoster.PendingSlot,
            // mirrors PendingJoinDevice). Null on a fresh join — Register then allocates a free slot.
            PlayerId = PlayerRoster.PendingSlot;
            PlayerRoster.PendingSlot = null;

            // PB.4.5 R3 (ruling iii — DD10 mirror), TIMING FIXED at R3.2: adopt a held rejoin DTO
            // threaded by the roster's reclaim path — consumed (cleared) here like its siblings,
            // but APPLIED next frame (see Start), NOT in Awake. The R3 in-Awake restore was a
            // defect: mid-Instantiate, sibling components haven't Awoken (PlayerStatusManager
            // creates playerStatus in ITS Awake; Health/inventory likewise un-init) — the exact
            // DD4 hazard the harness's one-frame-later restore pattern exists to avoid, observed
            // as the R3 smoke's deaf-rejoined-wrapper (Bug 1).
            _pendingRejoinRestore = PlayerRoster.PendingRestoreDto;
            PlayerRoster.PendingRestoreDto = null;

            PlayerRoster.Instance.Register(this);
        }

        PlayerStateDTO? _pendingRejoinRestore;   // R3.2: stashed in Awake, applied next frame

        void Start()
        {
            if (_pendingRejoinRestore.HasValue)
                StartCoroutine(ApplyRejoinRestore());
        }

        // R3.2: the harness's DD4-safe timing — one frame after Instantiate, every component
        // Awake/Start done, Health.Start can no longer clobber the restore. Dead-guarded: a
        // wrapper that died in its first frame does not get resurrected state.
        System.Collections.IEnumerator ApplyRejoinRestore()
        {
            yield return null;
            if (_pendingRejoinRestore.HasValue && State != WrapperState.Dead)
            {
                PlayerStateBuilder.Restore(_pendingRejoinRestore.Value, this, RestoreMode.Load,
                                           PlayerRoster.Instance?.RestoreProvider);
                Debug.Log($"[PlayerWrapper] {PlayerId} rejoin state restored (session-held DTO)");
            }
            _pendingRejoinRestore = null;
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
            _deviceId = DeriveDeviceId(pairDevice);   // PB.4 R5: populate the OQ-PB4-E seam at pair time
        }

        internal void Suspend()
        {
            // PB.4 (R3-Q2): Dead is terminal through the 1s respawn window. A device unplug here must
            // NOT overwrite Dead with Suspended — if it did, roster's reconnect-priority scan
            // (Find(State==Suspended)) would route the next device press to this about-to-be-destroyed
            // corpse, stealing it from the pending respawn and from legitimate joins.
            if (State == WrapperState.Dead) return;
            if (State == WrapperState.Suspended) return;  // idempotent — no redundant StateChanged
            State = WrapperState.Suspended;
            if (_actions != null)
                _actions.Player.Disable();  // D-3 fix: don't consume edges while frozen
            StateChanged?.Invoke(this);
        }

        // Story PB.4 (R3-Q1) — mark this wrapper Dead: assigns the previously-unused WrapperState.Dead
        // (visibly non-Active for the 1s respawn window; the Update Active-gate already halts its tick),
        // and disables the action map like Suspend so no edges accumulate/leak while dead. Fires
        // StateChanged (hands R4/7.3 the HUD hook for free). Idempotent.
        internal void Die()
        {
            if (State == WrapperState.Dead) return;
            State = WrapperState.Dead;
            if (_actions != null)
                _actions.Player.Disable();
            StateChanged?.Invoke(this);
        }

        internal void Resume()
        {
            // PB.4 (R3-Q2): Dead is terminal — a replug (DeviceRegained) during the respawn window must
            // NOT resurrect the corpse to Active, which would re-enable its disabled action map on a
            // dead player. The fresh wrapper, not this one, is what comes back.
            if (State == WrapperState.Dead) return;
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
            _deviceId = DeriveDeviceId(newDevice);   // PB.4 R5: reconnect may pair a DIFFERENT device — re-derive to reflect it
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

            var ejectPos = _vehicle.Position;   // rb.position (codebase convention), not transform.position
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
            // PB.4.5 R4.6 (Finding A, owner-ruled): mirror of the swap-IN seeding — carry the
            // panic-run's final facing back into the FSM, or restore snaps to the pre-fire
            // LookDirection (frozen at swap-in; the FSM isn't driven during the burn).
            // Type-checked, NOT generalized: OnFireController's own NOTE defers a shared
            // facing-handoff until a third controller needs it.
            if (previous is OnFireController fire)
                GetComponent<IT.Player.StateMachine.PlayerStateMachine>()?.SetLookDirection(fire.LookDirection);
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
            // PB.4 (DD3): symmetric deregister. Guarded (TryGetInstance) because at scene/app
            // teardown the roster may already be gone. Fires PlayerLeft (grounding seam 7).
            PlayerRoster.TryGetInstance()?.Deregister(this);

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
