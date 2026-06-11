using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace IT.Items.Bell
{
    using IT.Animation.Player.States;
    using IT.Interactables;

    public class BellItem : ItemBase
    {
        public BellRingDetector bellSoundAreaPrefab;
        private IBellRinger currentBellSound;

        //animations should be here
        private readonly BellAnimState _animationBell = new BellAnimState();
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
