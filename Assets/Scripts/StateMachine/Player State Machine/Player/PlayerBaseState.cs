using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    public virtual Vector2 LookDirection { get; private set; }
    protected virtual float Speed { get { return 5; } }
    public abstract void EnterState(PlayerStateMachineManager stateManager);
    public abstract void UpdateState(PlayerStateMachineManager stateManager);
    public abstract void FixedUpdateState(PlayerStateMachineManager stateManager);
    public abstract void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision);
    public abstract void ExitState(PlayerStateMachineManager stateManager);
    public abstract void Action(PlayerStateMachineManager stateManager);
    
    protected virtual void Move(PlayerStateMachineManager stateManager)
    {
        stateManager.rb.MovePosition(stateManager.rb.position + stateManager.Movement * Speed * Time.deltaTime);
    }
    protected void UpdateLookDirection(Vector2 movement)
    {
        if (movement == Vector2.up)
        {
            LookDirection = movement;
        }
        if (movement == Vector2.down)
        {
            LookDirection = movement;
        }
        if (movement == Vector2.right)
        {
            LookDirection = movement;
        }
        if (movement == Vector2.left)
        {
            LookDirection = movement;
        }
    }
}
