using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//retactable... fixed... they shoudl be seperate scripts. with fixed having a dependecny 
public class PullableOpenDependant : MonoBehaviour, IPullDependent
{
    private PullableDoorAnimation _doubleDoorAnimation;

   
    private void Awake()
    {
        _doubleDoorAnimation = new PullableDoorAnimation(GetComponent<Animator>());
    }

    public void OnPullChanged(float amount) => _doubleDoorAnimation.Step(amount);
    


}
