using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;   // PB.4.5 R2.1.1: ButtonControl/StickControl for the join filter
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;        // PB.4.5 R3: held-state flush at scene boundaries (§7.4)
using IT.Player.Persistence;              // PB.4.5 R3: capture/restore for rejoin-reclaim (§5.D)
using IT.Items;                           // PB.4.5 R3 fix: IItemPrefabProvider lives in IT.Items, not Persistence
using IT.Boot;
using IT.Core.Utilities;
using UnityEngine.InputSystem.LowLevel;

namespace IT.Player.Control
{
    // True-global singleton (C-C) that owns all PlayerWrapper instances up to
    // GameConfig.maxPlayers (architecture D2 / FR-9 / FR-13). Coordinates:
    //   - Press-to-join: InputUser.onUnpairedDeviceUsed → TryJoin
    //   - Hot-unplug:    InputSystem.onDeviceChange (Disconnected) → wrapper.Suspend()
    //   - Reconnect:     InputUser.onChange (DeviceRegained) → wrapper.Resume()
    //   - Re-pair:       onUnpairedDeviceUsed when a wrapper is Suspended → wrapper.RePair()
    public class PlayerRoster : Singleton<PlayerRoster>
    {
        // Set by GameBootstrap (the intended home for Inspector-wired refs, architecture D1).
        // Null in the BootGuard / direct-play path — TryJoin warns and skips, which is fine
        // because press-to-join requires a second device (deferred to Story 7.1 verification).
        public GameObject PlayerPrefab { get; set; }

        [SerializeField] Vector2 _joinOffset = new Vector2(1f, 0f); // Story 7.1 replaces this

        readonly List<PlayerWrapper> _wrappers = new();
        int _maxPlayers = 4;

        public IReadOnlyList<PlayerWrapper> Wrappers => _wrappers;

        public event System.Action<PlayerWrapper> PlayerJoined;
        public event System.Action<PlayerWrapper> PlayerLeft;

        // Set by TryJoin() before Instantiate; cleared by the new wrapper's Awake on read.
        // Null = scene-placed P1 wrapper (auto-pair to keyboard / first gamepad).
        internal static InputDevice PendingJoinDevice;

        // Story PB.4 (DD5 / slot-collision guard): set by SpawnManager before a respawn Instantiate
        // (R3) so the dying player's slot is RESERVED — excluded from fresh allocation until the
        // respawn's Awake consumes it. Mirrors PendingJoinDevice (a pending respawn reserves BOTH
        // the slot and the device). Null = no respawn in flight. At R2 nothing sets it (respawn is
        // R3), so it is always null here and every registration allocates fresh.
        internal static string PendingSlot;

        // PB.4.5 R3 (ruling iii — third of the DD10 threading trio, symmetric with PendingSlot /
        // PendingJoinDevice): held session DTO for a rejoin-reclaim. Set by RouteUnpairedActivity
        // before TryJoin's Instantiate; consumed (and cleared) by the new wrapper's Awake.
        internal static PlayerStateDTO? PendingRestoreDto;

        // PB.4.5 R3 (rulings i/§7.4/§7.5): session-scoped structures as roster INSTANCE fields —
        // they die with the SystemsRoot GameObject (= session end). The "future ResetSession()"
        // those rulings reserved LANDED at 4.6.1 R6.2 (in-game restart now exists: Continue /
        // Title→Play): ResetSession() below clears all three on every pass through Boot.
        // R2's Register empty-DeviceId guard is load-bearing for all three.
        readonly Dictionary<string, string> _deviceToSlot = new();          // survives transitions (§7.4)
        readonly Dictionary<string, PlayerStateDTO> _heldByDevice = new();  // flushed at transitions (§7.4 row 2)
        readonly HashSet<string> _gameOverReservedSlots = new();            // §7.3 — permanent for the run

        // PB.4.5 R3: prefab provider for rejoin restores (inventory rebuild, DD8). Null on the
        // production path today (Restore warn+skips items); the harness assigns itself in PB1Test.
        public IItemPrefabProvider RestoreProvider { get; set; }

        // PB.4.5 R3.1: the join gate (owner-ruled shape). Defaults Open; GameBootstrap sets
        // Locked through the boot load; OnSceneBoundary reopens. Gates JOIN only — never re-pair.
        public IJoinPolicy JoinPolicy { get; set; } = OpenJoinPolicy.Instance;

        protected override void Awake()
        {
            base.Awake();
            _maxPlayers = SystemsRoot.Instance?.Config.MaxPlayers ?? 4;
            InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
            InputSystem.onDeviceChange += OnDeviceChange;
            InputUser.onChange += OnInputUserChange;
            // PB.4.5 R2 (S1): subscribing alone does NOT arm unpaired-device listening — this
            // counter does (B0 2026-07-30 / PB.4 R7 §6: with it at 0, onUnpairedDeviceUsed
            // never fires and press-to-join is dead). Decremented symmetrically in OnDestroy.
            ++InputUser.listenForUnpairedDeviceActivity;
            // PB.4.5 R2.1 (Path B, owner-ruled): the armed callback did not deliver in-editor
            // (R2 smoke), so drop-in detection listens to the raw event stream instead. The
            // callback stays armed + subscribed — harmless, and the native-InputUser path
            // remains the post-v1 refactor target.
            InputSystem.onEvent += OnInputEvent;
            // PB.4.5 R3 (ruling ii): DEFENSIVE double-hook — either event flushes held DTOs
            // (Clear on an empty dict is a no-op), so §7.4's state-dies-at-transition holds
            // regardless of teardown/first-fire ordering. Slots survive; only held state dies.
            SceneManager.activeSceneChanged += OnSceneBoundary;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDestroy()
        {
            InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
            InputSystem.onDeviceChange -= OnDeviceChange;
            InputUser.onChange -= OnInputUserChange;
            --InputUser.listenForUnpairedDeviceActivity;   // symmetric with Awake's arm
            InputSystem.onEvent -= OnInputEvent;
            SceneManager.activeSceneChanged -= OnSceneBoundary;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnSceneBoundary(Scene from, Scene to)
        {
            _heldByDevice.Clear();                    // R3: §7.4 row 2
            // R3.1: transition over → joins reopen. 4.6.2 R3 (DD6-ii, owner-ruled
            // minimal): reopen ONLY from boot's transition lock — a menu-set policy
            // (MenuGatePolicy, reference-distinct by design) must survive the
            // boundary; the menu owns its own restore (GATE DEFAULT).
            if (ReferenceEquals(JoinPolicy, LockedJoinPolicy.Instance))
                JoinPolicy = OpenJoinPolicy.Instance;
        }

        // 4.6.2 R3: the seam-fix's one write site (also used by R4's re-pair toggle) —
        // never maps an empty DeviceId (the R2 empty-guard discipline).
        internal void BackfillDeviceMap(PlayerWrapper wrapper)
        {
            if (wrapper != null && !string.IsNullOrEmpty(wrapper.DeviceId))
                _deviceToSlot[wrapper.DeviceId] = wrapper.PlayerId;
        }

        // 4.6.2 R5 (OQ-F(i) ruled): reject presentation — a self-dismissing toast
        // through the ONE window (Normal priority: queues behind an open prompt,
        // which is B-4's FIFO fixture; UNSCALED auto-dismiss so it runs while
        // paused). Null-soft, the SpawnManager game-over posture: no Screen
        // module → the warn above stands alone.
        void ShowRejectToast(string text)
        {
            var screen = SystemsRoot.Instance != null && SystemsRoot.Instance.Presentation != null
                ? SystemsRoot.Instance.Presentation.Screen : null;
            screen?.Enqueue(new IT.Presentation.PromptRequest
            {
                Title = "",
                Body = text,
                AutoDismissSeconds = 2.5f,
                Options = new (string, System.Action)[] { ("OK", () => { }) },
            });
        }
        void OnSceneUnloaded(Scene s) => _heldByDevice.Clear();   // R3: ruling-ii safety net (flush only — mid-transition, policy stays)

        // Called by PlayerWrapper.Awake() — idempotent.
        public void Register(PlayerWrapper wrapper)
        {
            if (_wrappers.Contains(wrapper)) return;

            // PB.4.5 R2 (S3), rationale amended at R3 (ruling i): consume-side empty-guard.
            // DeriveDeviceId guarantees a PAIRED device never yields '' (PB.4 R5 spike) — an empty
            // DeviceId means a device-less wrapper. Load-bearing for THREE structures now, not one:
            // _deviceToSlot, _heldByDevice, and _gameOverReservedSlots must never gain an empty key.
            if (string.IsNullOrEmpty(wrapper.DeviceId))
                Debug.LogWarning("[PlayerRoster] wrapper registering with empty DeviceId (no device paired?) — the slot map will not track it.");

            // PB.4 (DD5): assign / reclaim the stable slot id BEFORE adding to the list.
            if (string.IsNullOrEmpty(wrapper.PlayerId))
            {
                wrapper.AssignId(AllocateFreeSlot());   // fresh join
            }
            else if (SlotHeld(wrapper.PlayerId))
            {
                // Slot-collision guard (the identity twin of the device join race): the slot this
                // respawn reclaimed was taken by a fresh join while the respawn was in flight. NEVER
                // silently duplicate an id (the per-playerId HUD map would rebind the wrong panel) —
                // warn loudly and allocate a free slot instead. The PendingSlot reservation should
                // normally prevent this; this is the belt-and-suspenders half.
                Debug.LogWarning($"[PlayerRoster] respawn reclaimed slot '{wrapper.PlayerId}' but it is " +
                    "already held by a live wrapper — allocating a free slot instead to avoid a duplicate id.");
                wrapper.AssignId(AllocateFreeSlot());
            }
            // else: reclaim honored — the wrapper keeps the id it adopted from PendingSlot.

            _wrappers.Add(wrapper);
            if (!string.IsNullOrEmpty(wrapper.DeviceId))
                _deviceToSlot[wrapper.DeviceId] = wrapper.PlayerId;   // R3 (§5.B): remembered for rejoin-reclaim
            Debug.Log($"[PlayerRoster] {wrapper.PlayerId} joined");   // R2.5-3: symmetric lifecycle logs for the sweep
            PlayerJoined?.Invoke(wrapper);
        }

        // PB.4 (DD3): symmetric with Register-on-Awake. PlayerWrapper.OnDestroy calls this so ANY
        // destroy path (respawn at R3, scene teardown, harness 9/0) cleans up. Fires the
        // previously-dead PlayerLeft — SegmentManager's handler (grounding seam 7) goes live here
        // for the first time (prune-only, benign). Idempotent: a not-registered wrapper is a no-op.
        public void Deregister(PlayerWrapper wrapper)
        {
            if (!_wrappers.Remove(wrapper)) return;   // R2.5: idempotent — silent no-op on the OnDestroy double-fire
            // PB.4.5 R3 (§5.D): capture session state at deregister, keyed by device — the rejoin
            // restore source. ACCEPTED WART (ruling v): the respawn path's Deregister also captures
            // a corpse DTO — harmless TODAY because the device stays owned through the respawn swap,
            // so nothing can rejoin off it and the next Deregister overwrites. If device-unown-
            // between-respawn-steps ever becomes possible, the map would carry stale state — a
            // future refactor must re-verify this assumption before changing the swap.
            if (!string.IsNullOrEmpty(wrapper.DeviceId))
                _heldByDevice[wrapper.DeviceId] = PlayerStateBuilder.Capture(wrapper);
            Debug.Log($"[PlayerRoster] {wrapper.PlayerId} left");   // R2.5-3: symmetric with the joined log
            PlayerLeft?.Invoke(wrapper);
        }

        // First free slot "P1".."Pn", excluding PendingSlot (reserved for an in-flight respawn) and
        // any slot a live wrapper already holds. NOT count-derived (OQ-PB4-D): with deregister live,
        // a count-derived id would misidentify a respawning P1 while P2 is alive.
        string AllocateFreeSlot()
        {
            for (int n = 1; n <= _maxPlayers; n++)
            {
                var slot = "P" + n;
                if (slot == PendingSlot) continue;   // reserved for an in-flight respawn
                if (_gameOverReservedSlots.Contains(slot)) continue;   // R3 (§7.3): reserved ≠ free — second predicate beside SlotHeld, not inside it
                if (!SlotHeld(slot)) return slot;
            }
            Debug.LogWarning("[PlayerRoster] no free player slot (all held or reserved) — allocating overflow id");
            return "P" + (_wrappers.Count + 1);
        }

        // PB.4.5 R4: "P1+P2"-style roster summary for the §5.J session-full line.
        string PresentSlots()
        {
            var slots = new List<string>(_wrappers.Count);
            foreach (var w in _wrappers)
                if (w != null) slots.Add(w.PlayerId);
            return string.Join("+", slots);
        }

        bool SlotHeld(string slot)
        {
            foreach (var w in _wrappers)
                if (w != null && w.PlayerId == slot) return true;
            return false;
        }

        // PB.4.5 R2.1: drop-in detection (Path B). Fires for EVERY input event, so filter order
        // is cheapest-first. Button-only is load-bearing for the DualSense specifically — it
        // streams STAT/sensor state events continuously (B0 observation), and without the
        // button filter the pad would self-join the moment it is plugged in. Consequence,
        // recorded as a §5.I deviation: stick-past-deadzone does NOT join in v1 — buttons only.
        // Keyboard is excluded by design: P1 auto-pairs it at boot (PlayerWrapper.cs:148);
        // a keyboard-only second player is its own future problem, not this rung (owner ruling).
        void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;
            if (!(device is Gamepad)) return;
            foreach (var w in _wrappers)
                if (w.OwnsDevice(device)) return;   // paired pads exit before the button scan
            var press = InputControlExtensions.GetFirstButtonPressOrNull(eventPtr);
            if (press == null || !IsJoinButton(press)) return;   // R2.1.1: see IsJoinButton
            RouteUnpairedActivity(device);
        }

        // PB.4.5 R2.1.1 (V2.5 fix): a StickControl is COMPOSED of synthetic child ButtonControls
        // (leftStick/up, /down, /left, /right — they exist so sticks can bind to button actions),
        // so a plain ButtonControl check passes a stick nudge. A control is a JOIN button only if
        // no ancestor is a stick. Dpad directions pass (parent is DpadControl); triggers pass.
        // Known limit (accepted): if a stick-synthetic and a real button press land in the SAME
        // event, first-press may be the stick and that event is dropped — the next button event
        // joins; negligible for v1.
        static bool IsJoinButton(InputControl control)
        {
            if (!(control is ButtonControl)) return false;
            for (var p = control.parent; p != null; p = p.parent)
                if (p is StickControl) return false;
            return true;
        }

        // R2.1.1: the §5.I button-only rule gates BOTH detection sources. The callback hands us
        // the actuated control directly — filter it the same way, or a delivering callback would
        // reopen the stick-join hole the listener just closed.
        void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
        {
            if (!IsJoinButton(control)) return;
            RouteUnpairedActivity(control.device);
        }

        // Extracted verbatim from the pre-R2.1 OnUnpairedDeviceUsed body — ONE routing for both
        // detection sources, so reconnect-priority (§5.F) and the max-players gate hold no
        // matter which source detects. Double-fire safe: whichever source pairs first, the
        // other exits on OwnsDevice.
        void RouteUnpairedActivity(InputDevice device)
        {
            // Ignore mouse and anything that isn't a playable device.
            if (device is Mouse) return;
            if (!(device is Keyboard || device is Gamepad)) return;

            // Ignore devices already owned by an active wrapper.
            foreach (var w in _wrappers)
                if (w.OwnsDevice(device)) return;

            // Reconnect-priority: if any wrapper is suspended, re-pair it to this device
            // instead of spawning a new player (architecture D2).
            var suspended = _wrappers.Find(w => w.State == WrapperState.Suspended);
            if (suspended != null)
            {
                suspended.RePair(device);
                // 4.6.2 R3 (owner-accepted seam fix): Register maps device→slot only at
                // registration, and a device-less-spawned P1 (DD4) registered UNMAPPED —
                // back-fill at seat/re-pair time so the seated device owns rejoin-reclaim
                // memory (B-17's row). Existing structure, already in ResetSession.
                BackfillDeviceMap(suspended);
                Debug.Log($"[PlayerRoster] {suspended.PlayerId} seated to device '{suspended.DeviceId}' (first-input / re-pair)");
                return;
            }

            // PB.4.5 R3.1: policy gate — placed AFTER the suspended re-pair above (deliberately
            // exempt, owner ruling: a transition-locked policy must never lock out reconnect)
            // and BEFORE reclaim/join. §5.J reject shape, loud and grep-able.
            if (!(JoinPolicy?.AllowJoin(device) ?? true))
            {
                Debug.LogWarning($"[PlayerRoster] join rejected — policy locked; device '{PlayerWrapper.DeriveDeviceId(device)}' ignored.");
                ShowRejectToast("Can't join right now.");   // 4.6.2 R5 (OQ-F(i))
                return;
            }

            // PB.4.5 R3 (§5.B/§5.D): rejoin-reclaim. A remembered device reclaims its slot (only
            // if still free — no reservation against other joiners, §5.B) and its held session
            // state. A lost race falls through to a fresh join; Register's collision guard stays
            // the loud backstop.
            var deviceId = PlayerWrapper.DeriveDeviceId(device);
            if (_deviceToSlot.TryGetValue(deviceId, out var rememberedSlot))
            {
                // OQ-R1-1 (owner-ruled REJECT): a game-over'd device does not rejoin in v1 — that
                // would be in-game game-over recovery, Epic 4.6's design space. Plain return: the
                // reject path must not wedge the roster (V3.4b) — other devices route normally.
                if (_gameOverReservedSlots.Contains(rememberedSlot))
                {
                    Debug.LogWarning($"[PlayerRoster] join rejected — slot game-over-reserved; device '{deviceId}' ignored.");
                    ShowRejectToast("That player is out of lives — no rejoining this run.");   // 4.6.2 R5 (OQ-F(i))
                    return;
                }
                if (!SlotHeld(rememberedSlot))
                {
                    PendingSlot = rememberedSlot;                 // DD10 threading — no new pattern
                    var hasHeld = _heldByDevice.TryGetValue(deviceId, out var held);
                    if (hasHeld)
                        PendingRestoreDto = held;                 // consumed by the wrapper's Awake
                    // PB.4.5 R5: reclaim observability — completes the lifecycle line set
                    // (joined/left/rejected-full/rejected-policy/rejected-reserved/reserved/restored).
                    Debug.Log($"[PlayerRoster] slot {rememberedSlot} reclaimed for device '{deviceId}'{(hasHeld ? " + held session state" : " (no held state — fresh)")}");
                }
            }

            if (_wrappers.Count < _maxPlayers)
            {
                TryJoin(device);
            }
            else
            {
                // PB.4.5 R4 (§5.J): session-full is one of the two LOUD reject reasons (the other
                // is R3.1's policy-locked). Device-already-paired stays deliberately UNLOGGED
                // (owner-sanctioned §5.J deviation — spam-prohibitive at the event layer).
                Debug.LogWarning($"[PlayerRoster] join rejected — session full ({PresentSlots()} present); device '{deviceId}' ignored.");
                ShowRejectToast("Game is full.");   // 4.6.2 R5 (OQ-F(i))
            }

            // Belt-and-suspenders (mirrors TryJoin's PendingJoinDevice clear): if nothing consumed
            // them (session full, prefab missing, inactive prefab), clear so stale identity/state
            // cannot leak into the next join. The respawn path sets PendingSlot outside this method
            // and is untouched.
            PendingSlot = null;
            PendingRestoreDto = null;
        }

        // PB.4.5 R3 (§7.3 Option A): game-over marks the slot reserved — a joiner must not inherit
        // the primary role by allocation timing. Permanent for the run (§7.5 corollary — no in-game
        // recovery in v1); ResetSession() below is the §7.5-reserved clear, landed at 4.6.1 R6.2.
        internal void ReserveSlotGameOver(string slot)
        {
            if (string.IsNullOrEmpty(slot)) return;
            _gameOverReservedSlots.Add(slot);
            Debug.Log($"[PlayerRoster] slot {slot} game-over-reserved");
        }

        // 4.6.1 R6.2 (Session A A-3, owner-ruled 2026-08-26): Continue / Play-from-Title = FULL
        // SESSION RESET — any pass through Boot ends the run. This is the §7.3/§7.5 "future
        // ResetSession()" landing, called from GameBootstrap.Start (Boot-only; direct-play never
        // runs it). RULED BOUNDARY, pinned — a reset outside it is a defect: clears the three
        // roster session structures ONLY. SessionInfo is deliberately untouched (mercy, ActiveSlot,
        // LoadOutcome, notices, PendingPrimaryRestore are the message INTO the next session — this
        // method takes no SessionInfo dependency). JoinPolicy is NOT in the reset set: Boot's
        // existing R3.1 line sets Locked immediately after this call, and OnSceneBoundary reopens
        // it post-load, unchanged. Accepted loss (ruled): pad-reclaims-old-slot-number across
        // Continue — P2+ state is session-scoped; fresh join is the design.
        internal void ResetSession()
        {
            int reservations = _gameOverReservedSlots.Count;
            int devices = _deviceToSlot.Count;
            int held = _heldByDevice.Count;
            _gameOverReservedSlots.Clear();
            _deviceToSlot.Clear();
            _heldByDevice.Clear();
            // R5-shape observability (owner-required): A-3/A-4's evidence the reset RAN, not inference.
            Debug.Log($"[PlayerRoster] session reset — reservations:{reservations} devices:{devices} held:{held} cleared");
        }

        // PB.4.5 R3: harness entry (PB.1 9/0-pattern ContextMenu drives this) — identical routing
        // to real input so a synthesized rejoin exercises the same reclaim/reject/join code.
        internal void SimulateUnpairedPress(InputDevice device) => RouteUnpairedActivity(device);

        // 4.6.2 R4 (PB.4.5 §9.2 cascade LANDING; OQ-C re-ruled: §5.D RESTORE stands):
        // voluntary drop-out. Refusals per the record: §5.E (P1 is the primary — quit is
        // not drop-out) and §7.2 (Dead is terminal, accepts no further wrapper actions).
        // The §5.D mechanism is the SHIPPED Deregister capture: DTO held keyed by
        // DeviceId, restored on rejoin (session-bounded — flushed at transitions and
        // cleared by ResetSession, so restore never crosses a Continue). Slot releases
        // with the wrapper; the retained _deviceToSlot entry is the reclaim memory.
        internal void Leave(PlayerWrapper wrapper)
        {
            if (wrapper == null) return;
            if (wrapper.PlayerId == "P1")
            {
                Debug.LogWarning("[PlayerRoster] leave refused — P1 cannot voluntarily leave (§5.E: quit ≠ drop-out).");
                return;
            }
            if (wrapper.State == WrapperState.Dead)
            {
                Debug.LogWarning($"[PlayerRoster] leave refused — {wrapper.PlayerId} is Dead (§7.2).");
                return;
            }
            Deregister(wrapper);          // §5.D capture happens HERE (keyed by DeviceId)
            wrapper.ReleaseDevice();      // device frees now; OnDestroy's Deregister double-fire is idempotent
            Debug.Log($"[PlayerRoster] {wrapper.PlayerId} left voluntarily — slot released; same device rejoins with held state (§5.D)");
            Destroy(wrapper.gameObject);
        }

        void TryJoin(InputDevice device)
        {
            if (PlayerPrefab == null)
            {
                Debug.LogWarning("[PlayerRoster] PlayerPrefab is not set — cannot join P2+. " +
                                 "Set PlayerRoster.Instance.PlayerPrefab from GameBootstrap.");
                return;
            }

            var spawnPos = _wrappers.Count > 0
                ? (Vector3)((Vector2)_wrappers[0].transform.position + _joinOffset)
                : Vector3.zero;

            // Thread the joining device into the new wrapper's Awake via the static.
            // Awake fires synchronously during Instantiate and clears PendingJoinDevice on read.
            PendingJoinDevice = device;
            Instantiate(PlayerPrefab, spawnPos, Quaternion.identity);
            // Belt-and-suspenders: in the normal path the wrapper's Awake already cleared this on
            // read. But if the prefab lacks a PlayerWrapper (or was instantiated inactive), nothing
            // consumed the static — clear it here so a stale device can't leak into the next join.
            PendingJoinDevice = null;
        }

        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Disconnected) return;
            foreach (var wrapper in _wrappers)
            {
                if (wrapper.OwnsDevice(device))
                {
                    wrapper.Suspend();
                    return;
                }
            }
        }

        void OnInputUserChange(InputUser user, InputUserChange change, InputDevice device)
        {
            if (change != InputUserChange.DeviceRegained) return;
            foreach (var wrapper in _wrappers)
            {
                if (wrapper.User == user)
                {
                    wrapper.Resume();
                    return;
                }
            }
        }
    }
}
