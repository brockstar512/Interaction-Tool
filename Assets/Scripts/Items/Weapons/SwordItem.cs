using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace IT.Items.Weapons
{
    using IT.Interactables;

    public class SwordItem : ItemBase
    {
        //hold the button and tsushima minigame pops up
        //otherwise its a swipe
        //if another with a sword comes in you can block 
        
        public override void Use(IInteractionContext context)
        {
            ItemFinishedCallback = context.EndInteraction;
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
