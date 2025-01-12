using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    public class SwordItem : Item
    {
        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;;
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
