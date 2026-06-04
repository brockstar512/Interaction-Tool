using UnityEngine;

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
        _pushAnimation.EnterPushAnimation(stateManager);   // locks the pose to the facing axis
        Action(stateManager);
    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {
        base.UpdateLookDirection(stateManager.movement);
    }

    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision) { }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {
        _pushAnimation.LeavePushAnimation();
    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager) { }

    public override async void Action(PlayerStateMachineManager stateManager)
    {
        try
        {
            stateManager.item.Interact(stateManager);   // pushes one unit if the cell ahead is clear
            _pushAnimation.Play(stateManager);           // push pose; a non-moving block reads as straining
            await Awaitable.WaitForSecondsAsync(PushHoldSeconds);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MoveItemState.Action failed: {ex}");
        }
        finally
        {
            stateManager.SwitchState(stateManager.defaultState);
        }
    }
}