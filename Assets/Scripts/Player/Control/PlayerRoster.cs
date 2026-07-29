using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
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
        }

        void OnDestroy()
        {
            InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
            InputSystem.onDeviceChange -= OnDeviceChange;
            InputUser.onChange -= OnInputUserChange;
            --InputUser.listenForUnpairedDeviceActivity;   // symmetric with Awake's arm
        }

        // Called by PlayerWrapper.Awake() — idempotent.
        public void Register(PlayerWrapper wrapper)
        {
            if (_wrappers.Contains(wrapper)) return;

            // PB.4.5 R2 (S3): consume-side empty-guard. DeriveDeviceId guarantees a PAIRED device
            // never yields '' (PB.4 R5 spike) — so an empty DeviceId here means a device-less
            // wrapper, which the R3 DeviceId→slot map must never key on. Loud, not fatal.
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
                if (!SlotHeld(slot)) return slot;
            }
            Debug.LogWarning("[PlayerRoster] no free player slot (all held or reserved) — allocating overflow id");
            return "P" + (_wrappers.Count + 1);
        }

        bool SlotHeld(string slot)
        {
            foreach (var w in _wrappers)
                if (w != null && w.PlayerId == slot) return true;
            return false;
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
