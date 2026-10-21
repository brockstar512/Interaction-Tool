using UnityEngine;
using IT.Boot;

namespace IT.Player.Control
{
    // Story PB.4 (OQ-PB4-C) — a self-registering respawn point, one per scene in v1. Absent from a
    // scene -> R3's respawn falls back to the death position (fail-alive). Grows into Directive 3's
    // Location-owned spawn registry at PB.5. No SerializeField (position comes from the Transform).
    //
    // Registers in Start(), NOT OnEnable (unlike SegmentBounds, which registers via the find-or-create
    // SegmentManager.Instance and so can never miss). SpawnMarker reaches SpawnManager through
    // SystemsRoot.Instance.Spawn, and a marker's OnEnable can fire BEFORE BootGuard.EnsureSystems has
    // created SystemsRoot — betting on scene-object init order vs the boot guard is not safe. Start()
    // runs after that on the direct-play path. A missing SystemsRoot is a REAL failure (marker present,
    // SpawnManager never hears of it -> R3 silently respawns at the death position, and L1 "passes" at
    // the wrong location) — so it LOUD-warns, never a silent skip.
    //
    // v1 limitation (acceptable — one static marker per scene): a marker disabled and re-enabled at
    // runtime does NOT re-register (Start runs once). Runtime re-registration lands with the
    // Location-owned registry if it's ever needed.
    //
    // Story 5.4 R4 (OQ-4 ruled: ONE CLASS, keyed set, "" default — the growth the header above
    // promised): an OPTIONAL id. Empty = THE scene's default marker = the respawn point AND the
    // transition fallback (OQ-5(a)). Named = an arrival target that Transport/Scene edges and
    // doors reference by spawnPointId. A named marker can never become the respawn point.
    // NAMED RESIDUAL (owner-accepted v1 posture): a scene authored with ONLY named markers has
    // no default → respawn falls back to the death position (pre-5.4, its lone marker was it).
    public class SpawnMarker : MonoBehaviour
    {
        [SerializeField] string _id = "";
        public string Id => _id ?? "";

        void Start()
        {
            var root = SystemsRoot.Instance;
            if (root == null || root.Spawn == null)
            {
                Debug.LogWarning("[SpawnMarker] SystemsRoot not present at Start — marker unregistered; " +
                    "respawn will fall back to the death position. Boot via Boot.unity, or ensure the " +
                    "systems exist before this scene loads.", this);
                return;
            }
            root.Spawn.Register(this);
        }

        // Unregister on disable — covers a runtime disable AND scene teardown (OnDisable fires before
        // OnDestroy). Guarded: at app teardown SystemsRoot may already be gone.
        void OnDisable() => SystemsRoot.Instance?.Spawn?.Unregister(this);

        // Visible spawn point in the Scene view without needing a sprite.
        void OnDrawGizmos()
        {
            Gizmos.color = string.IsNullOrEmpty(_id) ? Color.green : Color.cyan;   // 5.4: named = cyan
            Gizmos.DrawWireSphere(transform.position, 0.35f);
        }
    }
}
