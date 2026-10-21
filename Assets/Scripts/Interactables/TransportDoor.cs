using UnityEngine;
using IT.Boot;

namespace IT.Interactables
{
    // Story 5.4 R5 (DD7 ratified): a door / cave entrance on the EXISTING interact-press flow
    // (C-E: overlap + press, no physics trigger — the SavePoint seam, InteractionType.Use).
    // Knows nothing about transitions beyond the transporter's two entry points:
    //   _targetScene set   → Scene transition (cross-scene; P2+ carried — S-3).
    //   _targetScene empty → in-scene Transport to _spawnPointId (OQ-7(b): reposition only,
    //                        no DTO, no autosave) — the PRD's "door fades and teleports
    //                        player(s) to an interior" case.
    // _spawnPointId names a SpawnMarker id in the DESTINATION; empty/unknown → the default
    // marker, loud (OQ-5(a)). Every refusal (OQ-9) is the transporter's, logged there.
    public class TransportDoor : Interactable
    {
        [Tooltip("Scene name or path in Build Settings. Empty = in-scene Transport.")]
        [SerializeField] string _targetScene = "";
        [Tooltip("SpawnMarker id to arrive on (destination scene, or this scene for Transport).")]
        [SerializeField] string _spawnPointId = "";

        public override InteractionType Kind => InteractionType.Use;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();   // optional — overlap detects the collider, not the body
            UpdateLayerName();                  // self-assign the Interactable layer for InteractableOverlap
        }

        public override bool Interact(IInteractionContext context)
        {
            var transporter = SystemsRoot.Instance != null ? SystemsRoot.Instance.Transporter : null;
            if (transporter == null)
            {
                Debug.LogWarning($"[TransportDoor] '{name}': no LocationTransporter (SystemsRoot missing?) — door inert.", this);
                return false;
            }
            return string.IsNullOrEmpty(_targetScene)
                ? transporter.RequestTransport(_spawnPointId, $"door '{name}'")
                : transporter.RequestSceneTransition(_targetScene, _spawnPointId, $"door '{name}'");
        }

        public override void Release(IInteractionContext context) { }
    }
}
