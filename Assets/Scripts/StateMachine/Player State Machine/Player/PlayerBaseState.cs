using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    public virtual float Speed { get { return 5; } }
    public abstract void EnterState(PlayerStateMachineManager stateManager);
    public abstract void UpdateState(PlayerStateMachineManager stateManager);
    public abstract void FixedUpdateState(PlayerStateMachineManager stateManager);
    public abstract void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision);
    public abstract void ExitState(PlayerStateMachineManager stateManager);
    public abstract void Action(PlayerStateMachineManager stateManager);

    public virtual void Move(PlayerStateMachineManager stateManager)
    {
        stateManager.rb.MovePosition(stateManager.rb.position + stateManager.Movement * Speed * Time.deltaTime);
        stateManager.animator.Walk(stateManager.Movement, stateManager.LookDirection);
    } 
}
