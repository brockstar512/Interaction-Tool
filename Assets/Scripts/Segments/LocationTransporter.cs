using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using IT.Player.Control;
using IT.Player.Persistence;
using IT.Presentation;

namespace IT.Segments
{
    // Story 5.4 R3 (DD1 ratified): executes Scene-edge transitions behind an ITransition.
    // True-global (C-C), hosted on SystemsRoot beside SegmentRouter; consumes the router's
    // OnSceneRequested (5.2 REQUESTS, 5.4 EXECUTES). Doors call RequestSceneTransition
    // directly (DD7, R5). Transport (in-scene reposition, OQ-7(b)) lands at R4.
    //
    // PHASES (OQ-9 ruled):
    //   T0 accept → COVER (gate + pause suppressed, players frozen, timeScale saved→0, fade
    //   on unscaled time) → CAPTURE at END of cover (latest live state; freeze = transient)
    //   → LOAD (single LoadScene, OQ-1(a)) → ARRIVE (P1 Awake consumes the device thread,
    //   S-2a marker suppresses auto-pair; carry arm restores one frame after Start)
    //   → AUTOSAVE (Scene edges only, post-restore — I-3/DD6) → REVEAL (timeScale restored
    //   VERBATIM, prior policy restored, fade in).
    // R3 SCOPE: P1 only. P2+ carry (S-3) lands at R5 — until then a joined P2 is destroyed
    // with the origin scene and rejoins fresh (the pre-5.4 behavior, named in the commit).
    public class LocationTransporter : MonoBehaviour
    {
        // Read by PauseMenu (DD5: pause-open is suppressed for the whole transition).
        public static bool InFlight { get; private set; }

        const int LoadTimeoutFrames = 600;
        const int ArriveFrames = 10;

        ITransition _transition;
        SegmentRouter _router;

        void Start()
        {
            // Added AFTER SegmentRouter in SystemsRoot.Create, so the router is live here.
            _router = SegmentRouter.TryGetInstance();
            if (_router != null) _router.OnSceneRequested += OnSceneRequested;
            else Debug.LogWarning("[Transition] no SegmentRouter at Start — Scene edges will not transition.");

            var overlay = new GameObject("TransitionOverlay");
            overlay.transform.SetParent(transform, false);   // DDOL via SystemsRoot
            _transition = overlay.AddComponent<FadeTransition>();
        }

        void OnDestroy()
        {
            if (_router != null) _router.OnSceneRequested -= OnSceneRequested;
        }

        void OnSceneRequested(SceneRequest r)
            => RequestSceneTransition(r.TargetScenePath, r.SpawnPointId,
                                      $"Scene edge '{(r.FromSegment != null ? r.FromSegment.SegmentId : "?")}' {r.Edge}");

        // The one entry for Scene transitions (router + doors). Returns false when refused.
        public bool RequestSceneTransition(string targetScenePath, string spawnPointId, string source)
        {
            var refusal = RefusalReason(targetScenePath);
            if (refusal != null)
            {
                Debug.LogWarning($"[Transition] refused ({source}) — {refusal}");
                return false;
            }
            StartCoroutine(RunSceneTransition(targetScenePath, spawnPointId, source));   // sets InFlight synchronously
            return true;
        }

        // OQ-9 T0 refusals (ruled table). Any wrapper Dead covers respawn-in-flight AND game-over
        // (the game-over'd P1 is a retained Dead corpse); Paused = the pause menu is open.
        static string RefusalReason(string path)
        {
            if (InFlight) return "a transition is already in flight (DD1)";
            if (string.IsNullOrEmpty(path) || !Application.CanStreamedLevelBeLoaded(path))
                return $"target scene '{path}' is not loadable (missing from Build Settings?) — no cover started";
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
                    return $"{w.PlayerId} is possessing a vehicle (Scene edges refuse while possessing — 5.1 R1; vehicles deferred, OQ-8)";
                if (w.PlayerId == "P1") hasP1 = true;
            }
            return hasP1 ? null : "no P1 registered";
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
                PlayerRoster.SuppressAutoPairForArrival = false;   // S-2a: Awakes are done
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

                // CAPTURE — end of cover, so an unplug/reconnect during the fade is reflected.
                PlayerWrapper p1 = null;
                foreach (var w in roster.Wrappers)
                    if (w != null && w.PlayerId == "P1") { p1 = w; break; }
                if (p1 == null || p1.State == WrapperState.Dead)
                {
                    Debug.LogWarning("[Transition] aborted at capture — P1 missing or Dead (OQ-9).");
                    yield break;   // finally restores everything
                }
                var dto = PlayerStateBuilder.Capture(p1);
                // The transition freeze is TRANSIENT (C-H's canvas-induced-Suspend class) —
                // a Paused capture means "was Active"; only Suspended (device gone) persists.
                if (dto.wrapperState == WrapperState.Paused) dto.wrapperState = WrapperState.Active;
                var device = p1.PairedDevice;
                // S-2 (ruled): thread ONLY when live-Active with a still-connected device.
                bool threadDevice = dto.wrapperState == WrapperState.Active && device != null && device.added;
                LocationEntryCarry.Stash(p1.PlayerId, dto, targetName);
                Debug.Log($"[Transition] captured {p1.PlayerId} — state {dto.wrapperState}, " +
                          (threadDevice ? $"device '{p1.DeviceId}' threaded" : "NO device threaded (arrives device-less Suspended)"));

                // LOAD — release + deregister BEFORE the load: the arriving P1's Awake re-pairs
                // before the old wrapper's OnDestroy would free the device (the SpawnManager
                // respawn lesson), and the P1 slot must be free so the arriving wrapper
                // allocates "P1". Deregister's shipped held-map capture is flushed by the
                // shipped boundary flush (§5.D runs unchanged; the carry never touches it).
                roster.Deregister(p1);
                p1.ReleaseDevice();
                PlayerRoster.PendingJoinDevice = threadDevice ? device : null;
                PlayerRoster.SuppressAutoPairForArrival = true;   // S-2a — set ONLY here
                loadIssued = true;
                // sceneLoaded fires AFTER the loaded scene's Awake/OnEnable and BEFORE Start —
                // exactly when the device thread has been consumed and the marker may drop.
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
                // ARRIVE — the sceneLoaded handler already dropped the marker + device thread.
                frames = 0;
                while (LocationEntryCarry.Count > 0 && frames++ < ArriveFrames) yield return null;
                yield return null;   // the carry arm restores one frame after Start (DD4)
                yield return null;
                arrived = LocationEntryCarry.Count == 0;
                if (!arrived)
                    Debug.LogWarning($"[Transition] '{targetName}' never consumed the P1 carry — does it hold a scene-placed P1 prefab? (R2 fixture rule)");

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
                if (!arrived) PlayerRoster.PendingJoinDevice = null;
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
