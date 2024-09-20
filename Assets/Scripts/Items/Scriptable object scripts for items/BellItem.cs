using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "BellItemObject", menuName = "ScriptableObjects/Bell")]
    public class BellItem : Item
    {
        //animations should be here
        public override void Use(Vector3 playerLocation, Vector3 playerDirection, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {
            ItemFinishedCallback = callbackAction;
            DefaultState = defaultStateArg;
            Debug.Log($"Bell {playerDirection}");
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
