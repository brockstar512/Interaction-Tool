using UnityEngine;

namespace IT.Interactables.Vehicle
{
    using IT.Player.Control;

    // Story 3.4 — the Interactable entry point for a vehicle (C-E: overlap + Interact press,
    // no trigger collider). Kind == Possess routes PlayerIdleState.Action straight to Interact
    // below, which hands the player's wrapper a deferred possession request.
    [RequireComponent(typeof(VehicleController))]
    public class VehicleInteractable : Interactable
    {
        public override InteractionType Kind => InteractionType.Possess;

        VehicleController _controller;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            _controller = GetComponent<VehicleController>();
            UpdateLayerName();   // self-assign the Interactable layer so InteractableOverlap detects us
        }

        // Runs synchronously from PlayerIdleState.Action (inside OnFootController.Tick). The
        // wrapper only RECORDS the request here — the actual controller swap is deferred to the
        // next PlayerWrapper.Update to avoid a re-entrancy NRE (see PlayerWrapper.PossessVehicle).
        public override bool Interact(IInteractionContext context)
        {
            var wrapper = context.Transform.GetComponent<PlayerWrapper>();
            if (wrapper == null) return false;
            wrapper.PossessVehicle(_controller);
            return true;
        }

        public override void Release(IInteractionContext context) { }
    }
}
