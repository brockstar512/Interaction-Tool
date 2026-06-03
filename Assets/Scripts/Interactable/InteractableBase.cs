using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    public Rigidbody2D rb { get; set; }

    public abstract InteractionKind Kind { get; }

    public abstract bool Interact(IInteractionContext context);

    public abstract void Release(IInteractionContext context);

    protected void UpdateLayerName()
    {
        this.gameObject.layer = Layers.Interactable;
    }
}