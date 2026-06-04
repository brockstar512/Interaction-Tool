using UnityEngine;
//tetris japanese game show type minigame where you have to pull or hold an item to fit a mold and let go to get it to fit
public class PullItemState : PlayerBaseState
{
    private Pullable _pullable;

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        _pullable = stateManager.item as Pullable;
        if (_pullable == null || !_pullable.Interact(stateManager))
            stateManager.SwitchState(stateManager.defaultState);
        // play a pull/strain animation here if you have one
    }

    public override void UpdateState(PlayerStateMachineManager stateManager) { }
    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision) { }
    public override void ExitState(PlayerStateMachineManager stateManager) { _pullable = null; }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        _pullable?.PullStep(Time.fixedDeltaTime);   // pulls while the button is held
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        _pullable?.EndPull();                        // reached on release
        stateManager.SwitchState(stateManager.defaultState);
    }
}