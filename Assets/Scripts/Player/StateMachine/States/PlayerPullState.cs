using UnityEngine;
//tetris japanese game show type minigame where you have to pull or hold an item to fit a mold and let go to get it to fit
namespace IT.Player.StateMachine.States
{
    using IT.Animation.Player.States;
    using IT.Interactables;
    using IT.Interactables.Pullable;

    public class PlayerPullState : PlayerStateBase, IButtonUp
    {
        protected override float Speed => 2f;
        private Vector2 _axis;
        private PullableBase _pullable;
        private PlayerStateMachine _stateManager;
        private PushPullAnimState _animation;

        public PlayerPullState()
        {
            _animation = new PushPullAnimState();
        }

        public override void EnterState(PlayerStateMachine stateManager)
        {
            _stateManager = stateManager;
            _pullable = stateManager.item as PullableBase;
            if (_pullable == null || !_pullable.Interact(stateManager))
            {
                stateManager.SwitchState(stateManager.defaultState);
                return;
            }
            _axis = LookDirection;
            _animation.EnterPushAnimation(stateManager);
        }

        public override void UpdateState(PlayerStateMachine stateManager) { }
        public override void OnCollisionEnter(PlayerStateMachine stateManager, Collision collision) { }
        public override void ExitState(PlayerStateMachine stateManager) { }

        public override void FixedUpdateState(PlayerStateMachine stateManager) => Move(stateManager);

        protected override void Move(PlayerStateMachine stateManager)
        {
            if (_pullable == null) return;

            Vector2 movement = stateManager.movement;
            if (_axis == Vector2.up || _axis == Vector2.down) movement.x = 0;
            else movement.y = 0;

            if (movement != -LookDirection)          // pull only — back away, never forward
            {
                _animation.Play(stateManager);
                return;
            }

            float applied = _pullable.Pull(Speed * Time.fixedDeltaTime);
            if (applied <= 0f)                       // maxed or blocked → nobody moves
            {
                _animation.Play(stateManager);
                return;
            }

            stateManager.rb.MovePosition(stateManager.rb.position + (-LookDirection * applied));
            _animation.Play(stateManager);
        }

        // pull is entered through PlayerIdleState's dispatch, not via Action, so this stays empty
        public override void Action(PlayerStateMachine stateManager) { }

        // interact button-up routes here through ReleaseInteraction → IButtonUp
        public void ButtonUp()
        {
            _pullable = null;
            _animation.LeavePushAnimation();
            _stateManager.item.Release(_stateManager);
            _stateManager.SwitchState(_stateManager.defaultState);
        }
    }
}
