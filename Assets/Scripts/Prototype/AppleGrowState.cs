using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Prototype
{
    using IT.Core.StateMachine;

    public class AppleGrowState : StateBase
    {
        public override void EnterState(AppleStateManager stateManager)
        {

        }
        public override void UpdateState(AppleStateManager stateManager)
        {
            //some condition switch state
            if (true)
            {
                //switch state
                stateManager.SwitchState(stateManager.WholeState);//this could be cached... manager should have the logice
            }
        }
        public override void OnCollisionEnter(AppleStateManager stateManager, Collision collision)
        {

        }
    }
}
