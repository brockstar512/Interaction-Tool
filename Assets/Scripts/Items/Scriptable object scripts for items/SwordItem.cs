using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    public class SwordItem : Item
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
