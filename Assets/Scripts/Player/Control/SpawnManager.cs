using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.Player.Control
{
    using IT.Player.Status;

    // Story PB.4 (DD2) — the lifecycle orchestrator for death-respawn. Hosted on SystemsRoot
    // (added in SystemsRoot.Create AFTER PlayerRoster; reached via SystemsRoot.Instance.Spawn),
    // so it exists in EVERY scene without Inspector wiring — which is what makes L9's join-race
    // environment automatic (both direct-play and Boot paths). Plain MonoBehaviour, NOT a
    // Singleton<T> (C-C: no new singleton; access goes through the existing SystemsRoot singleton).
    //
    // R2 = INERT SCAFFOLD: owns the SpawnMarker registry (self-registered, mirroring
    // SegmentBounds -> SegmentManager) and exposes the spawn point. The death signal, the 1s
    // respawn timer, the FRESH rebuild, the lives decrement-before-check, and the SETTER for the
    // slot/device respawn reservation (PlayerRoster.PendingSlot / PendingJoinDevice) all land at
    // R3 — nothing calls a respawn here yet, so the join race / slot-collision window does not
    // exist until R3.
    public class SpawnManager : MonoBehaviour
    {
        // One marker per scene in v1 (OQ-PB4-C). If a scene mis-authors two, the latest wins
        // (warned). A scene with none -> HasMarker false -> R3's respawn uses the death position.
        SpawnMarker _marker;

        public bool HasMarker => _marker != null;

        // The respawn point: the registered marker's position, or the caller's fallback (the death
        // position) when no marker is registered (fail-alive, OQ-PB4-C).
        public Vector3 SpawnPoint(Vector3 fallback)
            => _marker != null ? _marker.transform.position : fallback;

        // Self-registration surface (push, mirrors SegmentManager.Register / SegmentBounds).
        public void Register(SpawnMarker marker)
        {
            if (marker == null) return;
            if (_marker != null && _marker != marker)
                Debug.LogWarning($"[SpawnManager] a second SpawnMarker registered ('{marker.name}') — " +
                    "v1 expects one per scene; the latest wins.", marker);
            _marker = marker;
        }

        public void Unregister(SpawnMarker marker)
        {
            if (_marker == marker) _marker = null;
        }

        // --- Story PB.4 R3 — death -> FRESH respawn ---

        // Seconds a dead player waits before respawning (DD8, C-B: hardcoded v1; the LocationConfig
        // extraction hook is PB.5).
        const float RespawnDelaySeconds = 1f;

        // The player prefab for a FRESH respawn. INJECTED by whoever wires the boot path (arch D1):
        // GameBootstrap sets it alongside roster.PlayerPrefab (production); the throwaway harness sets
        // it from its own _playerPrefab (test). SpawnManager never knows the harness exists — this is
        // the IItemPrefabProvider-style injection from PB.3, not coupling (OQ-PB4-F / R3-Q4). Null ->
        // respawn warns and aborts.
        public GameObject PlayerPrefab { get; set; }

        // Called by PlayerStatusManager on death (both HealthDepleted and the K debug converge there).
        public void RequestRespawn(PlayerWrapper wrapper)
        {
            if (wrapper == null) { Debug.LogWarning("[SpawnManager] RequestRespawn(null) — ignored"); return; }

            // Capture identity + the DECREMENTED life count while the dying wrapper is still valid.
            // Decrement BEFORE the check (DD6 B-guard) so a 0 diverts to game-over and never reaches
            // RestoreLives's fail-alive floor of 1 (which would silently resurrect a dead-for-good player).
            var psm = wrapper.GetComponent<PlayerStatusManager>();
            int newLives = (psm != null ? psm.playerStatus.CurrentLives : 0) - 1;
            string slot = wrapper.PlayerId;

            if (newLives <= 0)
            {
                // Lives-exhausted terminal (R3-Q3). The wrapper is DONE — deregister it (leaving it
                // registered forever is the stale-entry bug reborn) but do NOT respawn. Real game-over
                // presentation is OQ-PB4-B (post-v1). The player stays dead (PlayerDeathState).
                Debug.Log($"[SpawnManager] {slot} out of lives — game over (presentation = OQ-PB4-B, post-v1)");
                PlayerRoster.TryGetInstance()?.Deregister(wrapper);
                return;
            }

            // Dead wrapper stays REGISTERED + PAIRED through the delay (R3-Q2): shrinks DD9's join-race
            // window to the swap frame instead of the whole 1s.
            StartCoroutine(RespawnAfterDelay(wrapper, slot, newLives, wrapper.PairedDevice, wrapper.transform.position));
        }

        IEnumerator RespawnAfterDelay(PlayerWrapper old, string slot, int lives, InputDevice device, Vector3 deathPos)
        {
            yield return new WaitForSeconds(RespawnDelaySeconds);

            // Robust to the captured wrapper dying externally mid-window (scene unload during the 1s):
            // abort rather than NRE (R3-Q2 req2). SpawnManager lives on SystemsRoot (DontDestroyOnLoad),
            // so the coroutine host itself is stable across scene loads.
            if (old == null)
            {
                Debug.LogWarning($"[SpawnManager] respawn aborted for {slot} — wrapper destroyed mid-window (scene unload?)");
                yield break;
            }
            if (PlayerPrefab == null)
            {
                Debug.LogWarning($"[SpawnManager] respawn aborted for {slot} — no PlayerPrefab wired " +
                    "(GameBootstrap or the harness must set SpawnManager.PlayerPrefab).");
                yield break;
            }

            // Atomic swap (R2.5-1 order): deregister old + release its device BEFORE Instantiate, so the
            // reclaim of the slot AND the re-pair of the device both see them free (the deferred
            // OnDestroy would run too late). Then thread identity (PendingSlot) + device continuity
            // (PendingJoinDevice) per DD10/DD6, destroy old, spawn FRESH, restore the decremented lives.
            var roster = PlayerRoster.TryGetInstance();
            roster?.Deregister(old);          // OnDestroy fires Deregister again -> idempotent no-op (R2.5)
            old.ReleaseDevice();              // free the device now, not at the deferred OnDestroy

            PlayerRoster.PendingSlot = slot;
            PlayerRoster.PendingJoinDevice = device;

            Vector3 pos = SpawnPoint(deathPos);
            Destroy(old.gameObject);
            var go = Instantiate(PlayerPrefab, pos, Quaternion.identity);   // FRESH (DD1) — prefab defaults, not a DTO restore

            // The fresh Awake re-seeded lives to full from config; overwrite with the decremented count.
            // (lives >= 1 here — the 0 case diverted to game-over above, so RestoreLives's floor never bites.)
            var freshPsm = go.GetComponent<PlayerStatusManager>();
            if (freshPsm != null) freshPsm.playerStatus.RestoreLives(lives);
        }
    }
}
