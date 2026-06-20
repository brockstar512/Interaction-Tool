using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using IT.Boot;
using IT.Core.Utilities;

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

        protected override void Awake()
        {
            base.Awake();
            _maxPlayers = SystemsRoot.Instance?.Config.MaxPlayers ?? 4;
            InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
            InputSystem.onDeviceChange += OnDeviceChange;
            InputUser.onChange += OnInputUserChange;
        }

        void OnDestroy()
        {
            InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
            InputSystem.onDeviceChange -= OnDeviceChange;
            InputUser.onChange -= OnInputUserChange;
        }

        // Called by PlayerWrapper.Awake() — idempotent.
        public void Register(PlayerWrapper wrapper)
        {
            if (_wrappers.Contains(wrapper)) return;
            _wrappers.Add(wrapper);
            PlayerJoined?.Invoke(wrapper);
        }

        void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
        {
            var device = control.device;

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
                return;
            }

            if (_wrappers.Count < _maxPlayers)
                TryJoin(device);
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
            // PendingJoinDevice is null by here — wrapper's Awake cleared it.
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
