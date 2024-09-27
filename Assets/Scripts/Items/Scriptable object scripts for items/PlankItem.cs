using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "PlankItemObject", menuName = "ScriptableObjects/Plank")]

    public class PlankItem : Item
    {
        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchState;
            TargetState = stateManager.defaultState;

            Action();

        }
        
        void Action()
        {
            PutAway();
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(TargetState);
        }
    }
}
    
