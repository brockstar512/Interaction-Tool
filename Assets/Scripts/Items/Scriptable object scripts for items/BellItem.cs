using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    public class BellItem : Item
    {
        public OverlapCircleCollider bellSoundAreaPrefab;
        private IBellSound currentBellSound;

        //animations should be here
        private readonly AnimationBell _animationBell = new AnimationBell();
        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
            Action(stateManager);

        }
        
        async void Action(PlayerStateMachineManager stateManager)
        {
            if (currentBellSound != null)
            {
                PutAway();
                return;
            }
            
            currentBellSound = Instantiate(bellSoundAreaPrefab, stateManager.transform.position,Quaternion.identity).Init();
            await _animationBell.Play(stateManager);
            currentBellSound.Stop();
            currentBellSound = null;
            PutAway();
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
