using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Items.Scripts
{

    public class PlankItem : Item
    {
        Action _disposeOfItem = null;

        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
            _disposeOfItem = stateManager.itemManager.DisposeOfCurrentItem;

            Action();

        }
        
        void Action()
        {
            PutAway();
        }
        
        public override void PutAway()
        {
            _disposeOfItem?.Invoke();
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
    
