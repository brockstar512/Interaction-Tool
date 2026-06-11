using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation.Player.States;

    public class MoveItemState : PlayerBaseState
    {
        const float PushHoldSeconds = 0.2f;          // keep the push pose on screen; tune to your clip
        private readonly AnimationPushAndPull _pushAnimation;

        public MoveItemState()
        {
            _pushAnimation = new AnimationPushAndPull();
        }

        public override void EnterState(PlayerStateMachineManager stateManager)
        {
            Action(stateManager);
        }

        public override void UpdateState(PlayerStateMachineManager stateManager) { }

        public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision) { }

        public override void ExitState(PlayerStateMachineManager stateManager)
        {
            _pushAnimation.LeavePushAnimation();
        }

        public override void FixedUpdateState(PlayerStateMachineManager stateManager) { }

        public override async void Action(PlayerStateMachineManager stateManager)
        {
            _pushAnimation.EnterPushAnimation(stateManager);
            stateManager.item.Interact(stateManager);   // the single push, on the press
            _pushAnimation.Play(stateManager);
            stateManager.SwitchState(stateManager.defaultState);

        }
    }
}
