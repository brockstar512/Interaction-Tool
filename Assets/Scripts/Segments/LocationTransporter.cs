using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using IT.Boot;
using IT.Player.Control;
using IT.Player.Persistence;
using IT.Presentation;

namespace IT.Segments
{
    // Story 5.4 (DD1 ratified): executes Location transitions behind an ITransition.
    // True-global (C-C), hosted on SystemsRoot beside SegmentRouter; consumes the router's
    // OnSceneRequested / OnTransportRequested (5.2 REQUESTS, 5.4 EXECUTES). Doors call
    // RequestSceneTransition / RequestTransport directly (DD7, R5 — TransportDoor).
    //
    // SCENE PHASES (OQ-9 ruled):
    //   T0 accept → COVER (gate + pause suppressed, players frozen, timeScale saved→0, fade
    //   on unscaled time) → CAPTURE at END of cover (every PRESENT player; freeze = transient)
    //   → LOAD (single LoadScene, OQ-1(a)) → ARRIVE (P1: scene-placed wrapper, its Awake
    //   consumes the device thread; P2+: instantiated here, slot + conditional device threaded
    //   — S-3; the S-2a marker suppresses auto-pair for both; each carry arm restores one
    //   frame after Start with per-field CarryOverMode — OQ-3 SYMMETRIC) → POSITION (named
    //   spawn or the default marker, OQ-5(a); P2+ at a paired offset) + camera request
    //   (OQ-6(c)) → AUTOSAVE (Scene only, post-restore — I-3/DD6) → REVEAL (timeScale restored
    //   VERBATIM, prior policy restored, fade in).
    // TRANSPORT (OQ-7(b) ruled): same cover/freeze/gate, then in-scene reposition of every
    // present player — NO DTO, NO autosave. Arrival is resolved at T0 (same scene: markers
    // are live), so an unresolvable Transport is refused BEFORE the fade.
    // DD3 (ruled): the carry set (LocationEntryCarry) never touches PlayerRoster._heldByDevice;
    // §5.D's "discard at transition" governs that held map (LEFT players) and runs unchanged.
    public class LocationTransporter : MonoBehaviour
    {
        // Read by PauseMenu (DD5: pause-open is suppressed for the whole transition).
        public static bool InFlight { get; private set; }

        // OQ-6(c) ruled: the transporter REQUESTS, Epic 6's camera EXECUTES (no subscriber in
        // v1 — the request is logged). (arrival position, the primary player).
        public event System.Action<Vector3, PlayerWrapper> CameraRelocateRequested;

        const int LoadTimeoutFrames = 600;
        const int ArriveFrames = 10;
        static readonly Vector2 PairOffset = new Vector2(1f, 0f);   // P2+ land beside P1 (I-5 "paired/offset spawn")

        ITransition _transition;
        SegmentRouter _router;

        // One carried PRESENT player: its slot and the device threaded into its arriving
        // wrapper (null = arrives device-less Suspended — S-2).
        struct Carried
        {
            public string slot;
            public InputDevice device;
        }

        void Start()
        {
            // Added AFTER SegmentRouter in SystemsRoot.Create, so the router is live here.
            _router = SegmentRouter.TryGetInstance();
            if (_router != null)
            {
                _router.OnSceneRequested += OnSceneRequested;
                _router.OnTransportRequested += OnTransportRequested;
            }
            else Debug.LogWarning("[Transition] no SegmentRouter at Start — Scene/Transport edges will not transition.");

            var overlay = new GameObject("TransitionOverlay");
            overlay.transform.SetParent(transform, false);   // DDOL via SystemsRoot (R3 hosting deviation)
            _transition = overlay.AddComponent<FadeTransition>();
        }

        void OnDestroy()
        {
            if (_router == null) return;
            _router.OnSceneRequested -= OnSceneRequested;
            _router.OnTransportRequested -= OnTransportRequested;
        }

        void OnSceneRequested(SceneRequest r)
            => RequestSceneTransition(r.TargetScenePath, r.SpawnPointId,
                                      $"Scene edge '{(r.FromSegment != null ? r.FromSegment.SegmentId : "?")}' {r.Edge}");

        void OnTransportRequested(TransportRequest r)
            => RequestTransport(r.SpawnPointId,
                                $"Transport edge '{(r.FromSegment != null ? r.FromSegment.SegmentId : "?")}' {r.Edge} → '{(r.TargetSegment != null ? r.TargetSegment.SegmentId : "?")}'");

        // ─── Scene ───────────────────────────────────────────────────────────────────

        public bool RequestSceneTransition(string targetScenePath, string spawnPointId, string source)
        {
            var refusal = CommonRefusal("Scene");
            if (refusal == null && (string.IsNullOrEmpty(targetScenePath) || !Application.CanStreamedLevelBeLoaded(targetScenePath)))
                refusal = $"target scene '{targetScenePath}' is not loadable (missing from Build Settings?) — no cover started";
            if (refusal != null)
            {
                Debug.LogWarning($"[Transition] refused ({source}) — {refusal}");
                return false;
            }
            StartCoroutine(RunSceneTransition(targetScenePath, spawnPointId, source));   // sets InFlight synchronously
            return true;
        }

        // ─── Transport (in-scene) ────────────────────────────────────────────────────

        public bool RequestTransport(string spawnPointId, string source)
        {
            var refusal = CommonRefusal("Transport");
            Vector3 arrival = default;
            if (refusal == null && !ResolveArrival(spawnPointId, out arrival))
                refusal = $"no SpawnMarker named '{spawnPointId}' and no default marker in this scene — nowhere to land (OQ-5)";
            if (refusal != null)
            {
                Debug.LogWarning($"[Transition] refused ({source}) — {refusal}");
                return false;
            }
            StartCoroutine(RunTransport(arrival, spawnPointId, source));
            return true;
        }

        // OQ-9 T0 refusals (ruled table). Any wrapper Dead covers respawn-in-flight AND game-over
        // (the game-over'd P1 is a retained Dead corpse); Paused = the pause menu is open.
        // Possession: Scene edges refuse while possessing (5.1 R1, never router-enforced);
        // Transport-with-vehicle (5.1 R1's allowance) is DEFERRED by name (OQ-8) — refused.
        static string CommonRefusal(string kind)
        {
            if (InFlight) return "a transition is already in flight (DD1)";
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null) return "no PlayerRoster";
            bool hasP1 = false;
            foreach (var w in roster.Wrappers)
            {
                if (w == null) continue;
                if (w.State == WrapperState.Dead)
                    return $"{w.PlayerId} is Dead (respawn in flight or game over — OQ-9)";
                if (w.State == WrapperState.Paused)
                    return "the pause menu is open (OQ-9)";
                if (w.IsPossessingVehicle)
                    return $"{w.PlayerId} is possessing a vehicle ({kind} transitions with vehicles are deferred — OQ-8)";
                if (w.PlayerId == "P1") hasP1 = true;
            }
            return hasP1 ? null : "no P1 registered";
        }

        // OQ-5(a) ruled: the named marker, else the scene's DEFAULT marker, loud. False = neither.
        static bool ResolveArrival(string spawnPointId, out Vector3 position)
        {
            position = default;
            var spawn = SystemsRoot.Instance?.Spawn;
            if (spawn == null) return false;
            if (spawn.TryGetNamed(spawnPointId, out position)) return true;
            if (!spawn.HasMarker) return false;
            Debug.LogWarning(string.IsNullOrEmpty(spawnPointId)
                ? "[Transition] no spawnPointId authored — landing on the default SpawnMarker (OQ-5(a))."
                : $"[Transition] unknown spawnPointId '{spawnPointId}' — landing on the default SpawnMarker (OQ-5(a)).");
            position = spawn.SpawnPoint(Vector3.zero);
            return true;
        }

        // P1 on the arrival point, P2+ on the paired offset (I-5), then the camera request.
        void PositionPlayers(Vector3 arrival, string spawnPointId)
        {
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null) return;
            PlayerWrapper p1 = null;
            int i = 0;
            foreach (var w in SlotOrdered(roster))
            {
                w.Relocate((Vector2)arrival + PairOffset * i);
                if (i == 0) p1 = w;
                i++;
            }
            Debug.Log($"[Transition] positioned {i} player(s) at {(Vector2)arrival}{(string.IsNullOrEmpty(spawnPointId) ? "" : $" ('{spawnPointId}')")}");
            Debug.Log($"[Transition] camera relocate requested → {(Vector2)arrival} (Epic 6 executes — OQ-6(c))");
            CameraRelocateRequested?.Invoke(arrival, p1);
        }

        // P1 first, then the rest in slot order — deterministic offsets.
        static List<PlayerWrapper> SlotOrdered(PlayerRoster roster)
        {
            var list = new List<PlayerWrapper>();
            foreach (var w in roster.Wrappers)
                if (w != null && w.State != WrapperState.Dead) list.Add(w);
            list.Sort((a, b) => string.CompareOrdinal(a.PlayerId, b.PlayerId));
            return list;
        }

        IEnumerator RunTransport(Vector3 arrival, string spawnPointId, string source)
        {
            InFlight = true;
            var roster = PlayerRoster.TryGetInstance();
            var priorPolicy = roster.JoinPolicy;
            float savedTimeScale = Time.timeScale;
            bool revealed = false;
            Debug.Log($"[Transition] accepted ({source}) — Transport (in-scene, no DTO, no autosave — OQ-7(b)) — cover");
            try
            {
                roster.JoinPolicy = TransitionGatePolicy.Instance;
                foreach (var w in roster.Wrappers) w?.PauseFreeze();
                Time.timeScale = 0f;
                yield return _transition?.Cover();

                PositionPlayers(arrival, spawnPointId);

                foreach (var w in roster.Wrappers) w?.PauseThaw();   // thaws ONLY what the cover froze
                Time.timeScale = savedTimeScale;
                if (ReferenceEquals(roster.JoinPolicy, TransitionGatePolicy.Instance)) roster.JoinPolicy = priorPolicy;
                InFlight = false;
                yield return _transition?.Reveal();
                revealed = true;
                Debug.Log($"[Transition] Transport complete (timeScale {Time.timeScale})");
            }
            finally
            {
                foreach (var w in roster.Wrappers) w?.PauseThaw();
                Time.timeScale = savedTimeScale;
                if (ReferenceEquals(roster.JoinPolicy, TransitionGatePolicy.Instance)) roster.JoinPolicy = priorPolicy;
                InFlight = false;
                if (!revealed) _transition?.ResetImmediate();
            }
        }

        IEnumerator RunSceneTransition(string path, string spawnPointId, string source)
        {
            InFlight = true;
            var roster = PlayerRoster.TryGetInstance();
            var priorPolicy = roster.JoinPolicy;
            float savedTimeScale = Time.timeScale;
            string targetName = System.IO.Path.GetFileNameWithoutExtension(path);
            bool loadIssued = false, arrived = false, revealed = false, sceneLoaded = false;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onLoaded = (scene, mode) =>
            {
                if (scene.name != targetName) return;
                sceneLoaded = true;
                PlayerRoster.SuppressAutoPairForArrival = false;   // S-2a: the scene's Awakes are done
                PlayerRoster.PendingJoinDevice = null;             // belt: a destination with no P1 never read it
            };
            Debug.Log($"[Transition] accepted ({source}) → '{targetName}'{(string.IsNullOrEmpty(spawnPointId) ? "" : $" @ '{spawnPointId}'")} — cover");

            try
            {
                // COVER — DD5 gate + pause suppression (InFlight) + the pause-pair freeze.
                roster.JoinPolicy = TransitionGatePolicy.Instance;
                foreach (var w in roster.Wrappers) w?.PauseFreeze();
                Time.timeScale = 0f;
                yield return _transition?.Cover();

                // CAPTURE — end of cover (an unplug/reconnect during the fade is reflected).
                // Every PRESENT player (DD3 / I-5): P1 first, then P2+ in slot order.
                var present = SlotOrdered(roster);
                if (present.Count == 0 || present[0].PlayerId != "P1")
                {
                    Debug.LogWarning("[Transition] aborted at capture — P1 missing or Dead (OQ-9).");
                    yield break;   // finally restores everything
                }
                var prefab = roster.PlayerPrefab != null ? roster.PlayerPrefab : SystemsRoot.Instance?.Spawn?.PlayerPrefab;
                var carried = new List<Carried>();
                foreach (var w in present)
                {
                    bool isP1 = w.PlayerId == "P1";
                    if (!isP1 && prefab == null)
                    {
                        // Direct-play has no roster PlayerPrefab (the R2 fixture note): a P2 that
                        // cannot be rebuilt is NOT carried — it drops with the scene and rejoins
                        // fresh (the pre-5.4 behavior), loudly.
                        Debug.LogWarning($"[Transition] {w.PlayerId} NOT carried — no player prefab to rebuild it (direct-play?). It rejoins fresh after the load.");
                        continue;
                    }
                    var dto = PlayerStateBuilder.Capture(w);
                    // The transition freeze is TRANSIENT (C-H's canvas-induced-Suspend class) —
                    // a Paused capture means "was Active"; only Suspended (device gone) persists.
                    if (dto.wrapperState == WrapperState.Paused) dto.wrapperState = WrapperState.Active;
                    var device = w.PairedDevice;
                    // S-2 (ruled, symmetric for P2+ — S-3): thread ONLY when live-Active with a
                    // still-connected device; otherwise the player arrives device-less Suspended.
                    bool threadDevice = dto.wrapperState == WrapperState.Active && device != null && device.added;
                    LocationEntryCarry.Stash(w.PlayerId, dto, targetName);
                    carried.Add(new Carried { slot = w.PlayerId, device = threadDevice ? device : null });
                    Debug.Log($"[Transition] captured {w.PlayerId} — state {dto.wrapperState}, " +
                              (threadDevice ? $"device '{w.DeviceId}' threaded" : "NO device threaded (arrives device-less Suspended)"));
                }

                // LOAD — release + deregister every carried player BEFORE the load: an arriving
                // wrapper's Awake re-pairs before the old OnDestroy would free the device (the
                // SpawnManager respawn lesson), and each slot must be free for its arriving
                // wrapper. Deregister's shipped held-map capture is flushed by the shipped
                // boundary flush (§5.D unchanged; the carry never touches the held map).
                foreach (var c in carried)
                {
                    var w = present.Find(x => x.PlayerId == c.slot);
                    roster.Deregister(w);
                    w.ReleaseDevice();
                }
                PlayerRoster.PendingJoinDevice = carried[0].device;   // P1's thread (scene-placed P1 reads it in Awake)
                PlayerRoster.SuppressAutoPairForArrival = true;      // S-2a — set ONLY by this class
                loadIssued = true;
                // sceneLoaded fires AFTER the loaded scene's Awake/OnEnable and BEFORE Start —
                // exactly when P1's device thread has been consumed and the marker may drop.
                // (An active-scene NAME poll would pass instantly on a same-scene reload.)
                SceneManager.sceneLoaded += onLoaded;
                SceneManager.LoadScene(path);

                int frames = 0;
                while (!sceneLoaded && frames++ < LoadTimeoutFrames)
                    yield return null;
                if (!sceneLoaded)
                {
                    Debug.LogError($"[Transition] load of '{targetName}' did not activate within {LoadTimeoutFrames} frames — aborting.");
                    yield break;
                }

                // ARRIVE (S-3) — P2+ rebuilt here, sequentially (one set of DD10 statics at a
                // time — quality-audit #27's clobber is out of reach; joins + respawns are barred
                // for the duration). Slot threaded via PendingSlot; device per S-2; the S-2a
                // marker brackets each Instantiate so a device-less P2 never auto-pairs.
                Vector3 provisional = Vector3.zero;
                foreach (var w in roster.Wrappers)
                    if (w != null && w.PlayerId == "P1") { provisional = w.transform.position; break; }
                for (int i = 1; i < carried.Count; i++)
                {
                    PlayerRoster.PendingSlot = carried[i].slot;
                    PlayerRoster.PendingJoinDevice = carried[i].device;
                    PlayerRoster.SuppressAutoPairForArrival = true;
                    Instantiate(prefab, provisional + (Vector3)(PairOffset * i), Quaternion.identity);
                    PlayerRoster.SuppressAutoPairForArrival = false;
                    PlayerRoster.PendingSlot = null;          // belt (Awake clears on read)
                    PlayerRoster.PendingJoinDevice = null;
                }

                frames = 0;
                while (LocationEntryCarry.Count > 0 && frames++ < ArriveFrames) yield return null;
                yield return null;   // each carry arm restores one frame after its Start (DD4)
                yield return null;
                arrived = LocationEntryCarry.Count == 0;
                if (!arrived)
                    Debug.LogWarning($"[Transition] '{targetName}' left carries unconsumed — does it hold a scene-placed P1 prefab? (R2 fixture rule)");

                // POSITION (R4) — resolved post-load: cross-scene markers exist only now.
                if (ResolveArrival(spawnPointId, out var arrival))
                    PositionPlayers(arrival, spawnPointId);
                else
                    Debug.LogWarning($"[Transition] '{targetName}' has no SpawnMarker named '{spawnPointId}' and no default — players stay at their authored/instantiated positions (OQ-5).");

                // AUTOSAVE — I-3 thin-wire: post-load, post-restore, destination capture.
                if (arrived) SaveService.SaveNow(SaveService.ReasonTransition);

                // REVEAL — timeScale VERBATIM (never a literal), prior policy, then fade in.
                Time.timeScale = savedTimeScale;
                if (ReferenceEquals(roster.JoinPolicy, TransitionGatePolicy.Instance)) roster.JoinPolicy = priorPolicy;
                InFlight = false;
                yield return _transition?.Reveal();
                revealed = true;
                Debug.Log($"[Transition] complete → '{targetName}' (timeScale {Time.timeScale})");
            }
            finally
            {
                SceneManager.sceneLoaded -= onLoaded;
                // S2-c (ruled): the arrival marker clears even if the transition fails.
                PlayerRoster.SuppressAutoPairForArrival = false;
                if (!arrived)
                {
                    PlayerRoster.PendingJoinDevice = null;
                    PlayerRoster.PendingSlot = null;
                }
                LocationEntryCarry.Clear(arrived ? "arrival complete" : "transition aborted");
                if (!loadIssued)
                    foreach (var w in roster.Wrappers) w?.PauseThaw();   // aborted pre-load: origin players unfreeze
                Time.timeScale = savedTimeScale;
                if (ReferenceEquals(roster.JoinPolicy, TransitionGatePolicy.Instance)) roster.JoinPolicy = priorPolicy;
                InFlight = false;
                if (!revealed) _transition?.ResetImmediate();
            }
        }
    }
}
