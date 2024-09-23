using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "SwordItemObject", menuName = "ScriptableObjects/Sword")]
    public class SwordItem : Item
    {
        public override void Use(PlayerStateMachineManager stateManager, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {
            ItemFinishedCallback = callbackAction;
            DefaultState = defaultStateArg;
           // Debug.Log($"Sword {playerDirection}");

        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(DefaultState);
        }
    }
}
