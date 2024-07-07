using AnimationStates;
using UnityEngine;

public class ThrowItemState : PlayerBaseState
{
    protected override float Speed => 4;

    readonly AnimationPickUp _pickUpAnimation;
    readonly AnimationThrow _throwAnimation;
    readonly AnimationCarry _carryAnimation;
    AnimationState _currentAnimation = null;

    public ThrowItemState()
    {
        _pickUpAnimation = new AnimationPickUp();
        _throwAnimation = new AnimationThrow(); 
        _carryAnimation = new AnimationCarry();
    }
    

    public override async void EnterState(PlayerStateMachineManager stateManager)
    {
        _currentAnimation = _pickUpAnimation;
        await _pickUpAnimation.Play(stateManager);
        stateManager.item.Interact(stateManager);
        _currentAnimation = _carryAnimation;
    }
    public override void UpdateState(PlayerStateMachineManager stateManager)
    {
        UpdateLookDirection(stateManager.movement);
    }
    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {
        
    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        if (_currentAnimation is not AnimationCarry)
            return;
        
        base.Move(stateManager);
        _carryAnimation.Play(stateManager);
    }

    public override async void Action(PlayerStateMachineManager stateManager)
    {
        _currentAnimation = _throwAnimation;
        stateManager.item.Release(stateManager);
        await _throwAnimation.Play(stateManager);
        _currentAnimation = null;
        stateManager.SwitchState(stateManager.defaultState);
        
    }
}
