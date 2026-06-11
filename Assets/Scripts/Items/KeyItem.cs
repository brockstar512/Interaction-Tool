using UnityEngine;

namespace IT.Items
{
    using IT.Core.Utilities;
    using IT.Interactables;

    public class KeyItem : ItemBase
    {
        public GameUtilities.KeyTypes keyType;

        public override void Use(IInteractionContext context)
        {
            // A key does nothing on its own — an OpenableBase consumes it on interact.
            ItemFinishedCallback = context.EndInteraction;
            PutAway();
        }

        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}