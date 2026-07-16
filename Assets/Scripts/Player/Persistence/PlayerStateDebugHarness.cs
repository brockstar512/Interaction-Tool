using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using IT.Core.Combat;
using IT.Player.Control;
using IT.Player.Status;
using System.Collections.Generic;

namespace IT.Player.Persistence
{
    // Story PB.1 Rung 5 — THROWAWAY verification harness (pre-ship removal list, alongside
    // SegmentRequestLogger + the debug keys). Drives the V1–V5/V8/V9/V-ord checks in PB1Test:
    // 8 = capture + JSON round-trip self-check, 9 = destroy → respawn → Restore(Transition),
    // 0 = same but Restore(Load). Keys read Keyboard.current directly (the P-key precedent)
    // so they respond while the wrapper is Suspended — required for V4/V5.
    //
    // Story PB.2 R3 extension (OQ-PB2-C/E — cure keys live HERE, not PlayerStatusManager,
    // so they sit behind the same in-flight guard as 8/9/0): 6 = antitoxin → Cure(poison),
    // 7 = water → Cure(on-fire); three ContextMenu probes (unknown-status inject,
    // indefinite rewrite, legacy-JSON parse); CAPTURED/RESTORED lines + FirstMismatch
    // walk activeStatuses. Zero new SerializeFields — R-06 pre-flight unchanged.
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
        bool _roundTripInFlight;   // R5.2 (review R-05): one round-trip at a time — re-entrant
                                   // presses corrupt the console evidence the harness exists to produce

        void Awake()
        {
            // R5.2 (review R-06): validate the required Inspector wiring BEFORE anything can
            // destroy the player — a mis-wired slot must log-and-refuse, never brick the session.
            if (_playerPrefab == null)
            {
                Debug.LogError("[PB1Harness] _playerPrefab is unassigned on PlayerStateDebugHarness — " +
                    "drag Assets/Prefabs/Player/Player.prefab into the Inspector slot. Harness disabled.", this);
                enabled = false; return;
            }
            if (_playerPrefab.GetComponent<PlayerWrapper>() == null)
            {
                Debug.LogError("[PB1Harness] _playerPrefab has no PlayerWrapper component — wrong asset in the slot " +
                    "(expected Assets/Prefabs/Player/Player.prefab). Harness disabled.", this);
                enabled = false; return;
            }
            if (_spawnPoint == null)
            {
                Debug.LogError("[PB1Harness] _spawnPoint is unassigned on PlayerStateDebugHarness — " +
                    "drag the SpawnMarker's Transform into the Inspector slot. Harness disabled.", this);
                enabled = false; return;
            }

            Debug.Log("=== PB1 Harness Keys ===\n" +
                "8 — Capture (+ JSON round-trip self-check)\n" +
                "9 — Destroy → Respawn → Restore(Transition)\n" +
                "0 — Destroy → Respawn → Restore(Load)\n" +
                "6 — Cure poison (antitoxin)   [PB.2]\n" +
                "7 — Cure on-fire (water)      [PB.2]\n" +
                "ContextMenu — Corrupt held DTO: health = 999 / 0, lives = 0,\n" +
                "              add unknown status / make first status indefinite / legacy-JSON probe\n" +
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
            // R5.2 (review R-05, subsumes deferred R-17): while a round-trip is mid-flight,
            // ALL harness keys are ignored — a second 9/0 would restore a pre-Start() instance
            // (false-pass evidence), and an 8 would overwrite the held DTO with the fresh spawn.
            // PB.2 R3: 6/7 join the guard (OQ-PB2-C required scope) — a cure firing mid-flight
            // would mutate _active during destroy/respawn, the exact phantom-PASS class R-05 closed.
            if (_roundTripInFlight)
            {
                if (Keyboard.current.digit8Key.wasPressedThisFrame ||
                    Keyboard.current.digit9Key.wasPressedThisFrame ||
                    Keyboard.current.digit0Key.wasPressedThisFrame ||
                    Keyboard.current.digit6Key.wasPressedThisFrame ||
                    Keyboard.current.digit7Key.wasPressedThisFrame)
                    Debug.Log("[PB1Harness] round-trip in flight — key ignored");
                return;
            }
            if (Keyboard.current.digit8Key.wasPressedThisFrame) Capture();
            if (Keyboard.current.digit9Key.wasPressedThisFrame) StartCoroutine(RoundTrip(RestoreMode.Transition));
            if (Keyboard.current.digit0Key.wasPressedThisFrame) StartCoroutine(RoundTrip(RestoreMode.Load));
            if (Keyboard.current.digit6Key.wasPressedThisFrame) Cure(typeof(PoisonEffect), "poison");
            if (Keyboard.current.digit7Key.wasPressedThisFrame) Cure(typeof(OnFireEffect), "onfire");
        }

        void Capture()
        {
            if (_tracked == null) { Debug.LogWarning("[PB1Harness] Capture: no tracked player"); return; }
            _held = PlayerStateBuilder.Capture(_tracked);
            _hasCapture = true;

            var max = _tracked.GetComponent<Health>().Max;
            Debug.Log($"[PB1Harness] CAPTURED — health {_held.currentHealth}/{max}, lives {_held.lives}, " +
                      $"state {_held.wrapperState}, items {_held.items.Count}, " +
                      $"statuses {DescribeStatuses(_held.activeStatuses)}, id '{_held.playerId}'/'{_held.deviceId}'");

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
            _roundTripInFlight = true;   // set AFTER the no-capture early-out; cleared in finally
            try
            {
                if (_tracked != null)
                    Destroy(_tracked.gameObject);

                var go = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
                _tracked = go.GetComponent<PlayerWrapper>();

                if (!_restoreSameFrame)
                    yield return null;   // one frame — after the respawn's Start() (the DD4 contract)
                PlayerStateBuilder.Restore(_held, _tracked, mode);

                var health = _tracked.GetComponent<Health>();
                var lives = _tracked.GetComponent<PlayerStatusManager>().playerStatus.CurrentLives;
                // statuses = LIVE post-replay count (not the DTO count — unknown entries skip),
                // so the line reports what actually landed on the rebuilt player.
                var statuses = _tracked.GetComponent<StatusController>().Active.Count;
                Debug.Log($"[PB1Harness] RESTORED ({mode}) — health {health.Current}, lives {lives}, " +
                          $"state {_tracked.State}, statuses {statuses}");
            }
            finally
            {
                _roundTripInFlight = false;   // guard can never stick, even if a restore throws mid-flight
            }
        }

        // V8 setup without JSON hand-editing mid-session (Rung 1 authoring plan).
        [ContextMenu("Corrupt held DTO: health = 999")]
        void CorruptHealthHigh() { _held.currentHealth = 999; Debug.Log("[PB1Harness] held DTO corrupted: health = 999"); }

        [ContextMenu("Corrupt held DTO: health = 0")]
        void CorruptHealthZero() { _held.currentHealth = 0; Debug.Log("[PB1Harness] held DTO corrupted: health = 0"); }

        // R5.2: probes the R5.1 lives fail-alive floor (review R-01) — expect
        // "[PlayerStatus] RestoreLives clamped 0 → 1 (fail-alive floor)" on the next 9/0.
        [ContextMenu("Corrupt held DTO: lives = 0")]
        void CorruptLivesZero() { _held.lives = 0; Debug.Log("[PB1Harness] held DTO corrupted: lives = 0"); }

        // --- PB.2 R3: cure keys + status probes ---

        // 6/7 handler. Targets by StackKey (typeof — Apply's refresh match rule), NOT the
        // registry string; the label is only for the echo. A false return is informational,
        // not an error (owner ruling at R3 approval): the R5 sweep must distinguish
        // "key didn't register" from "key registered, no target" (V8's quiet no-op).
        void Cure(System.Type effectType, string label)
        {
            if (_tracked == null) { Debug.LogWarning("[PB1Harness] Cure: no tracked player"); return; }
            var cured = _tracked.GetComponent<StatusController>().Cure(effectType);
            Debug.Log(cured ? $"[PB1Harness] CURED {label}"
                            : $"[PB1Harness] Cure {label}: not active — no-op");
        }

        // V9 setup: an entry the registry can't construct — expect warn+skip on the next
        // 9/0 while health/lives/known statuses restore intact.
        [ContextMenu("Corrupt held DTO: add unknown status 'phantom-status'")]
        void CorruptAddUnknownStatus()
        {
            _held.activeStatuses ??= new List<StatusStateDTO>();
            _held.activeStatuses.Add(new StatusStateDTO { statusType = "phantom-status", elapsed = 1f, instanceState = "{}" });
            Debug.Log("[PB1Harness] held DTO corrupted: unknown status 'phantom-status' appended");
        }

        // V-ind setup: flip the FIRST captured status's blob to indefinite=true. Deliberately
        // rides the DTO restore path (no test-only construction API): a PASS proves the flag
        // round-trips AND the mechanism works. String replace is type-agnostic and safe here —
        // JsonUtility's output shape is stable and every registered blob carries the field.
        [ContextMenu("Corrupt held DTO: make first status indefinite")]
        void CorruptMakeFirstStatusIndefinite()
        {
            if (_held.activeStatuses == null || _held.activeStatuses.Count == 0)
            { Debug.LogWarning("[PB1Harness] no captured status to make indefinite — press 8 mid-status first"); return; }
            var entry = _held.activeStatuses[0];
            var rewritten = entry.instanceState.Replace("\"indefinite\":false", "\"indefinite\":true");
            if (rewritten == entry.instanceState)
            { Debug.LogWarning($"[PB1Harness] indefinite rewrite changed nothing — blob: {entry.instanceState}"); return; }
            entry.instanceState = rewritten;
            _held.activeStatuses[0] = entry;   // struct — write back
            Debug.Log($"[PB1Harness] held DTO: first status ({entry.statusType}) rewritten indefinite=true");
        }

        // V1b: a PB.1-era capture (no activeStatuses key) must deserialize null-safe —
        // JsonUtility ignores-missing leaves the list null; registry Restore treats null
        // as nothing-to-replay. Deterministic, no eyeballing.
        [ContextMenu("Legacy-JSON probe (PB.1-era DTO, no activeStatuses)")]
        void LegacyJsonProbe()
        {
            const string legacy = "{\"playerId\":\"\",\"deviceId\":\"\",\"wrapperState\":0," +
                "\"currentHealth\":7,\"lives\":3,\"items\":[],\"currentItemIndex\":0}";
            var dto = JsonUtility.FromJson<PlayerStateDTO>(legacy);
            Debug.Log($"[PB1Harness] LEGACY OK — health {dto.currentHealth}, lives {dto.lives}, " +
                      $"activeStatuses {(dto.activeStatuses == null ? "null (restore no-ops)" : dto.activeStatuses.Count.ToString())}");
        }

        // CAPTURED-line segment: envelope facts only ("poison@2.0s"); authored duration
        // (hence remaining) lives in each blob and is readable from the full-JSON line.
        static string DescribeStatuses(List<StatusStateDTO> list)
        {
            if (list == null || list.Count == 0) return "0";
            var parts = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
                parts[i] = $"{list[i].statusType}@{list[i].elapsed:0.0}s";
            return $"{list.Count} [{string.Join(", ", parts)}]";
        }

        static string FirstMismatch(in PlayerStateDTO a, in PlayerStateDTO b)
        {
            if (a.playerId != b.playerId) return "playerId";
            if (a.deviceId != b.deviceId) return "deviceId";
            if (a.wrapperState != b.wrapperState) return "wrapperState";
            if (a.currentHealth != b.currentHealth) return "currentHealth";
            if (a.lives != b.lives) return "lives";
            if ((a.items?.Count ?? 0) != (b.items?.Count ?? 0)) return "items";
            if (a.currentItemIndex != b.currentItemIndex) return "currentItemIndex";
            // PB.2 R3: the V1 self-check covers the new schema — count, then all four
            // fields per entry (envelope + blob), so a lossy status round-trip can never
            // print ROUNDTRIP OK.
            var ac = a.activeStatuses; var bc = b.activeStatuses;
            if ((ac?.Count ?? 0) != (bc?.Count ?? 0)) return "activeStatuses.Count";
            for (int i = 0; i < (ac?.Count ?? 0); i++)
            {
                if (ac[i].statusType != bc[i].statusType) return $"activeStatuses[{i}].statusType";
                if (ac[i].elapsed != bc[i].elapsed) return $"activeStatuses[{i}].elapsed";
                if (ac[i].tickAccumulator != bc[i].tickAccumulator) return $"activeStatuses[{i}].tickAccumulator";
                if (ac[i].instanceState != bc[i].instanceState) return $"activeStatuses[{i}].instanceState";
            }
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
