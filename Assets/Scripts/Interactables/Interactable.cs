using UnityEngine;

namespace IT.Interactables
{
    using IT.Core.Utilities;

    public abstract class Interactable : MonoBehaviour
    {
        public Rigidbody2D rb { get; set; }

        public abstract InteractionType Kind { get; }

        public abstract bool Interact(IInteractionContext context);

        public abstract void Release(IInteractionContext context);

        protected void UpdateLayerName()
        {
            this.gameObject.layer = LayerIndex.Interactable;
        }
    }
}
