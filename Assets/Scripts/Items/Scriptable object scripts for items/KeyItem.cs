using UnityEngine;

namespace Items
{
    public class Key : Item
    {
        public Utilities.KeyTypes keyType;

        public override void Use(IInteractionContext context)
        {
            // A key does nothing on its own — an Openable consumes it on interact.
            ItemFinishedCallback = context.EndInteraction;
            PutAway();
        }

        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}