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
        public override void Use(IInteractionContext context)
        {
            ItemFinishedCallback = context.EndInteraction;
            Action(context);
        }

        async void Action(IInteractionContext context)
        {
            if (currentBellSound != null)
            {
                PutAway();
                return;
            }

            try
            {
                currentBellSound = Instantiate(bellSoundAreaPrefab, context.Transform.position, Quaternion.identity).Init();
                await _animationBell.Play(context);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BellItem.Action failed: {ex}");
            }
            finally
            {
                currentBellSound?.Stop();
                currentBellSound = null;
                PutAway();
            }
        }
        
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
