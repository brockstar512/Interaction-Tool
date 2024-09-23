using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "WhipItemObject", menuName = "ScriptableObjects/Whip")]

    public class WhipItem : Item
    {
        public override void Use(PlayerStateMachineManager stateManager, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {
            ItemFinishedCallback = callbackAction;
            DefaultState = defaultStateArg;
            //Debug.Log($"Whip {playerDirection}");
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
