using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Core.StateMachine
{
    using IT.Prototype;

    public abstract class StateBase
    {
       public abstract void EnterState(AppleStateManager stateManager);
       public abstract void UpdateState(AppleStateManager stateManager);
       public abstract void OnCollisionEnter(AppleStateManager stateManager, Collision collision);



    }
}
