using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using IT.Core.Combat;
using IT.Player.Control;
using IT.Player.Status;

namespace IT.Player.Persistence
{
    // Story PB.1 Rung 5 — THROWAWAY verification harness (pre-ship removal list, alongside
    // SegmentRequestLogger + the debug keys). Drives the V1–V5/V8/V9/V-ord checks in PB1Test:
    // 8 = capture + JSON round-trip self-check, 9 = destroy → respawn → Restore(Transition),
    // 0 = same but Restore(Load). Keys read Keyboard.current directly (the P-key precedent)
    // so they respond while the wrapper is Suspended — required for V4/V5.
    //
    // The harness tracks its OWN wrapper reference (the Instantiate return). It never queries
    // PlayerRoster — each 9/0 cycle leaves one stale (destroyed) entry in the roster because
    // deregister-on-destroy doesn't exist yet (PlayerLeft is dead code). That is PB.4's named
    // problem: a documented symptom here, never a fix (PlayerRoster.cs stays zero-diff, V12c).
    public class PlayerStateDebugHarness : MonoBehaviour
    {
        // Deliberately the harness's OWN prefab slot — PlayerRoster.PlayerPrefab is null on
        // the BootGuard/direct-play path (Rung 1 authoring plan).
        [SerializeField] GameObject _playerPrefab;
        [SerializeField] Transform _spawnPoint;
        // V-ord hazard toggle (spec DD4): ON restores synchronously after Instantiate —
        // BEFORE the respawn's Start() — so Health.Start() clobbers the restore to max,
        // reproducing the bug the Restore-after-Start() contract exists to prevent. OFF
        // (normal) restores one frame later. Flip ON only for the V-ord probe.
        [SerializeField] bool _restoreSameFrame = false;

        PlayerWrapper _tracked;
        PlayerStateDTO _held;
        bool _hasCapture;

        void Awake()
        {
            Debug.Log("=== PB1 Harness Keys ===\n" +
                "8 — Capture (+ JSON round-trip self-check)\n" +
                "9 — Destroy → Respawn → Restore(Transition)\n" +
                "0 — Destroy → Respawn → Restore(Load)\n" +
                "ContextMenu — Corrupt held DTO: health = 999 / 0\n" +
                "========================");
        }

        void Start()
        {
            // Initial tracked player = the scene-placed P1. After the first 9/0 cycle the
            // tracked ref is the Instantiate return, never a Find or a roster read.
            _tracked = FindFirstObjectByType<PlayerWrapper>();
            if (_tracked == null)
                Debug.LogWarning("[PB1Harness] No scene-placed PlayerWrapper found — 8/9/0 inert until one exists");
        }

        void Update()
        {
            if (Keyboard.current.digit8Key.wasPressedThisFrame) Capture();
            if (Keyboard.current.digit9Key.wasPressedThisFrame) StartCoroutine(RoundTrip(RestoreMode.Transition));
            if (Keyboard.current.digit0Key.wasPressedThisFrame) StartCoroutine(RoundTrip(RestoreMode.Load));
        }

        void Capture()
        {
            if (_tracked == null) { Debug.LogWarning("[PB1Harness] Capture: no tracked player"); return; }
            _held = PlayerStateBuilder.Capture(_tracked);
            _hasCapture = true;

            var max = _tracked.GetComponent<Health>().Max;
            Debug.Log($"[PB1Harness] CAPTURED — health {_held.currentHealth}/{max}, lives {_held.lives}, " +
                      $"state {_held.wrapperState}, items {_held.items.Count}, id '{_held.playerId}'/'{_held.deviceId}'");

            // V1 self-check: the DTO is honest JSON — serialize → deserialize → field-equal.
            var json = JsonUtility.ToJson(_held);
            Debug.Log($"[PB1Harness] JSON: {json}");
            var back = JsonUtility.FromJson<PlayerStateDTO>(json);
            var mismatch = FirstMismatch(_held, back);
            Debug.Log(mismatch == null ? "[PB1Harness] ROUNDTRIP OK"
                                       : $"[PB1Harness] ROUNDTRIP FAIL {mismatch}");
        }

        IEnumerator RoundTrip(RestoreMode mode)
        {
            if (!_hasCapture) { Debug.LogWarning("[PB1Harness] Round-trip: no held DTO — press 8 first"); yield break; }
            if (_tracked != null)
                Destroy(_tracked.gameObject);

            var go = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
            _tracked = go.GetComponent<PlayerWrapper>();

            if (!_restoreSameFrame)
                yield return null;   // one frame — after the respawn's Start() (the DD4 contract)
            PlayerStateBuilder.Restore(_held, _tracked, mode);

            var health = _tracked.GetComponent<Health>();
            var lives = _tracked.GetComponent<PlayerStatusManager>().playerStatus.CurrentLives;
            Debug.Log($"[PB1Harness] RESTORED ({mode}) — health {health.Current}, lives {lives}, state {_tracked.State}");
        }

        // V8 setup without JSON hand-editing mid-session (Rung 1 authoring plan).
        [ContextMenu("Corrupt held DTO: health = 999")]
        void CorruptHealthHigh() { _held.currentHealth = 999; Debug.Log("[PB1Harness] held DTO corrupted: health = 999"); }

        [ContextMenu("Corrupt held DTO: health = 0")]
        void CorruptHealthZero() { _held.currentHealth = 0; Debug.Log("[PB1Harness] held DTO corrupted: health = 0"); }

        static string FirstMismatch(in PlayerStateDTO a, in PlayerStateDTO b)
        {
            if (a.playerId != b.playerId) return "playerId";
            if (a.deviceId != b.deviceId) return "deviceId";
            if (a.wrapperState != b.wrapperState) return "wrapperState";
            if (a.currentHealth != b.currentHealth) return "currentHealth";
            if (a.lives != b.lives) return "lives";
            if ((a.items?.Count ?? 0) != (b.items?.Count ?? 0)) return "items";
            if (a.currentItemIndex != b.currentItemIndex) return "currentItemIndex";
            return null;
        }

        // Makes the spawn marker visible in the Scene view without needing a sprite.
        void OnDrawGizmos()
        {
            if (_spawnPoint == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_spawnPoint.position, 0.4f);
        }
    }
}
