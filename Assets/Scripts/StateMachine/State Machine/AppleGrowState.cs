using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleGrowState : BaseState
{
    public override void EnterState(StateManager stateManager)
    {

    }
    public override void UpdateState(StateManager stateManager)
    {
        //some condition switch state
        if (true)
        {
            //switch state
            stateManager.SwitchState(stateManager.WholeState);//this could be cached... manager should have the logice
        }
    }
    public override void OnCollisionEnter(StateManager stateManager, Collision collision)
    {

    }
}
