using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "CandleItemObject", menuName = "ScriptableObjects/Candle")]
    public class CandleItem : Item
    {
        public override void Use(Vector3 playerLocation, Vector3 playerDirection, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {  
             ItemFinishedCallback = callbackAction;
             DefaultState = defaultStateArg;

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
