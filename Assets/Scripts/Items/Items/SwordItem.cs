using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scripts
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
