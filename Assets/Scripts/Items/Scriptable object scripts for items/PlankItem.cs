using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "PlankItemObject", menuName = "ScriptableObjects/Plank")]

    public class PlankItem : Item
    {
        public override void Use(PlayerStateMachineManager stateManager, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {
            ItemFinishedCallback = callbackAction;
            DefaultState = defaultStateArg;
            //Debug.Log($"PLank {playerDirection}");
            Action();

        }
        
        void Action()
        {
            PutAway();
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(DefaultState);
        }
    }
}
    
