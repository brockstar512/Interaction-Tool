// InteractionKit/Runtime/Core/IInteractionContext.cs
using UnityEngine;

namespace InteractionKit
{
    public interface IInteractionContext
    {
        IItemManager Items     { get; }
        Transform    Transform { get; }
        Vector2      LookDirection { get; }

        /// Ask the host to enter the state mapped to this interaction kind.
        // void RequestInteraction(InteractionKind kind, IInteractable source);

        /// Item/interactable is done; host returns to its default state.
        void EndInteraction();
    }
}