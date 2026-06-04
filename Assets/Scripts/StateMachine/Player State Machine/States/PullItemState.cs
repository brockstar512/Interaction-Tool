using UnityEngine;

public class PullItemState : PlayerBaseState
{
    private Pullable _pullable;

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        _pullable = stateManager.item as Pullable;
        if (_pullable == null || !_pullable.Interact(stateManager))
        {
            stateManager.SwitchState(stateManager.defaultState);
            return;
        }
        _pullable.BeginPull(stateManager.rb.position);   // anchor the drag to where the player grabbed
    }

    public override void UpdateState(PlayerStateMachineManager stateManager) { }
    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision) { }
    public override void ExitState(PlayerStateMachineManager stateManager) => _pullable = null;

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        if (_pullable == null) return;

        Vector2 intended = stateManager.rb.position + stateManager.movement * Speed * Time.fixedDeltaTime;
        stateManager.rb.MovePosition(_pullable.Drag(intended));   // lever follows + leashes the player
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        _pullable?.EndPull();
        stateManager.SwitchState(stateManager.defaultState);
    }
}