using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation.Player.States;

    public class PlayerSlideState : PlayerStateBase
    {
        KickAnimState KickAnimation;
        HurtToeAnimState HurtToeAnimation;
    

        public PlayerSlideState()
        {
            KickAnimation = new KickAnimState();
            HurtToeAnimation = new HurtToeAnimState();
        }

        public override void EnterState(PlayerStateMachine stateManager)
        {
            //Debug.Log("Sliding item");
            Action(stateManager);
        }
    
        public override void UpdateState(PlayerStateMachine stateManager)
        {
            base.UpdateLookDirection(stateManager.movement);
        }

        public override void OnCollisionEnter(PlayerStateMachine stateManager, Collision collision)
        {

        }

        public override void ExitState(PlayerStateMachine stateManager)
        {

        }

        public override void FixedUpdateState(PlayerStateMachine stateManager)
        {

        }

        public override async void Action(PlayerStateMachine stateManager)
        {
            try
            {
                if (stateManager.item.Interact(stateManager))
                {
                    await KickAnimation.Play(stateManager);
                }
                else
                {
                    await KickAnimation.Play(stateManager);
                    await HurtToeAnimation.Play(stateManager);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"SlideItemState.Action failed: {ex}");
            }
            finally
            {
                stateManager.SwitchState(stateManager.defaultState);
            }
        }
    }
}
