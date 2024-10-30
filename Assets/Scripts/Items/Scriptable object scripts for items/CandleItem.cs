using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "CandleItemObject", menuName = "ScriptableObjects/Candle")]
    public class CandleItem : Item
    {
        //what if this had a timed usage so if it's dark you have to use it
        //but like bell it attracts back guys in the dark
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
