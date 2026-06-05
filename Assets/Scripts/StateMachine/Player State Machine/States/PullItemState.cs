using UnityEngine;
using Interactable;

public class PullItemState : PlayerBaseState, IButtonUp
{
    protected override float Speed => 2f;
    private Vector2 _axis;
    private Pullable _pullable;
    private PlayerStateMachineManager _stateManager;
    private AnimationPushAndPull _animation;

    public PullItemState()
    {
        _animation = new AnimationPushAndPull();
    }

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        _stateManager = stateManager;
        _pullable = stateManager.item as Pullable;
        if (_pullable == null || !_pullable.Interact(stateManager))
        {
            stateManager.SwitchState(stateManager.defaultState);
            return;
        }
        _axis = LookDirection;
        _animation.EnterPushAnimation(stateManager);
    }

    public override void UpdateState(PlayerStateMachineManager stateManager) { }
    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision) { }
    public override void ExitState(PlayerStateMachineManager stateManager) { }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager) => Move(stateManager);

    protected override void Move(PlayerStateMachineManager stateManager)
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

    // pull is entered through DefaultState's dispatch, not via Action, so this stays empty
    public override void Action(PlayerStateMachineManager stateManager) { }

    // interact button-up routes here through ReleaseInteraction → IButtonUp
    public void ButtonUp()
    {
        _pullable = null;
        _animation.LeavePushAnimation();
        _stateManager.item.Release(_stateManager);
        _stateManager.SwitchState(_stateManager.defaultState);
    }
}