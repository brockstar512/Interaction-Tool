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
        private readonly AnimationBell _animationBell = new AnimationBell();
        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
            Action(stateManager);

        }
        
        async void Action(PlayerStateMachineManager stateManager)
        {
            await _animationBell.Play(stateManager);
            PutAway();
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
