using UnityEngine;

public class PullItemState : PlayerBaseState
{
    protected override float Speed => 2f;          // drag speed, like the old Move
    private Vector2 _axis;
    private Pullable _pullable;
    private AnimationPushAndPull _animation;

    public PullItemState()
    {
        _animation = new AnimationPushAndPull();
    }

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
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

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        Move(stateManager);
    }

    protected override void Move(PlayerStateMachineManager stateManager)
    {
        if (_pullable == null) return;

        Vector2 movement = stateManager.movement;

        // lock to the facing axis
        if (_axis == Vector2.up || _axis == Vector2.down) movement.x = 0;
        else movement.y = 0;

        // pull only: back away from the lever, never toward it (no forward, no idle move)
        if (movement != -LookDirection)
        {
            _animation.Play(stateManager);
            return;
        }

        float applied = _pullable.Pull(Speed * Time.fixedDeltaTime);   // advance the handle as far as allowed
        if (applied <= 0f)                                             // maxed or blocked → nobody moves
        {
            _animation.Play(stateManager);
            return;
        }

        stateManager.rb.MovePosition(stateManager.rb.position + (-LookDirection * applied));
        _animation.Play(stateManager);
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        _pullable = null;
        _animation.LeavePushAnimation();
        stateManager.item.Release(stateManager);
        stateManager.SwitchState(stateManager.defaultState);
    }
}