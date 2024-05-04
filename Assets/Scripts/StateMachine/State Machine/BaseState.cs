using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
   public abstract void EnterState(StateManager stateManager);
   public abstract void UpdateState(StateManager stateManager);
   public abstract void OnCollisionEnter(StateManager stateManager, Collision collision);



}
