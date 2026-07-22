using UnityEngine;

namespace IT.Player.Control
{
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
    }
}
