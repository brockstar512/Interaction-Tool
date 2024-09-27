using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "SwordItemObject", menuName = "ScriptableObjects/Sword")]
    public class SwordItem : Item
    {
        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchState;;
            TargetState = stateManager.defaultState;
           // Debug.Log($"Sword {playerDirection}");

        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(TargetState);
        }
    }
}
