using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "CandleItemObject", menuName = "ScriptableObjects/Candle")]
    public class CandleItem : Item
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
