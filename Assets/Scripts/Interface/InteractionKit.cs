using UnityEngine;

public interface IInteractionContext
{
    IItemManager Items { get; }
    Transform Transform { get; }
    Vector2 LookDirection { get; }
    Animator Animator { get; }
    void EndInteraction(InteractableBase next = null);
}