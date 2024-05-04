using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{

    BaseState currentState;
    public AppleGrowState GrowState = new AppleGrowState();
    public AppleWholeState WholeState = new AppleWholeState();
    public AppleRottenState RottenState = new AppleRottenState();
    public AppleChewedState ChewedState = new AppleChewedState();

    // Start is called before the first frame update
    void Start()
    {
        currentState = GrowState;
        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(this);
    }

    public void SwitchState(BaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        currentState.OnCollisionEnter(this, collision);
    }
}

//public enum PlayerState
//{
//    Default,
//    Move,
//    Use,
//    Slide,
//    Throw,
//    END,

//}