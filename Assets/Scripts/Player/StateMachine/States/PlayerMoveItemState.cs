using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation.Player.States;

    public class PlayerMoveItemState : PlayerStateBase
    {
        const float PushHoldSeconds = 0.2f;          // keep the push pose on screen; tune to your clip
        private readonly PushPullAnimState _pushAnimation;

        public PlayerMoveItemState()
        {
            _pushAnimation = new PushPullAnimState();
        }

        public override void EnterState(PlayerStateMachine stateManager)
        {
            Action(stateManager);
        }

        public override void UpdateState(PlayerStateMachine stateManager) { }

        public override void ExitState(PlayerStateMachine stateManager)
        {
            _pushAnimation.LeavePushAnimation();
        }

        public override void FixedUpdateState(PlayerStateMachine stateManager) { }

        public override async void Action(PlayerStateMachine stateManager)
        {
            _pushAnimation.EnterPushAnimation(stateManager);
            stateManager.item.Interact(stateManager);   // the single push, on the press
            _pushAnimation.Play(stateManager);
            stateManager.SwitchState(stateManager.defaultState);

        }
    }
}
