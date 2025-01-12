using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    public class WhipItem : Item
    {
        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;

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
