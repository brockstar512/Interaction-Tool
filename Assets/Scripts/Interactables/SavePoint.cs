using UnityEngine;
using IT.Player.Persistence;

namespace IT.Interactables
{
    // SAVE.3 (DD2 / OQ-D-i, owner-ruled): the manual save point — a scene-placed
    // Interactable on the standard interact-press flow (C-E: overlap + press, no
    // physics trigger; C-G: the same seam doors will use at 5.4). Knows NOTHING
    // about serialization (C-H) — the entire effect is one SaveService call.
    // E-4.iii "restore at themselves" holds by PLACEMENT: this saves the scene it
    // stands in; resume enters that scene at its spawn point (SAVE.1 OQ-B scene
    // granularity). v1 feedback = SaveService's [SaveLoad] log line; visual/audio
    // feedback is Epic 4.6 presentation work.
    public class SavePoint : Interactable
    {
        public override InteractionType Kind => InteractionType.Use;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();   // optional — overlap detects the collider, not the body
            UpdateLayerName();                  // self-assign the Interactable layer for InteractableOverlap
        }

        public override bool Interact(IInteractionContext context)
            => SaveService.SaveNow(SaveService.ReasonSavePoint);

        public override void Release(IInteractionContext context) { }
    }
}
