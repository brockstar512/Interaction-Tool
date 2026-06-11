using UnityEngine;
using System;

namespace IT.Items.Weapons
{
    using IT.Interactables;

    public class WhipItem : Item
    {
        public override void Use(IInteractionContext context)
        {
            ItemFinishedCallback = context.EndInteraction;
            Action();
        }
        
        void Action()
        {
            PutAway();
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
