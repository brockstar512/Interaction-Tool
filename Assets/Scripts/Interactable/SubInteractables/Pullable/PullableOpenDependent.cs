using System;
using UnityEngine;

public class PullableOpenDependent : MonoBehaviour, IDependencySource<float>
{
    private PullableDoorAnimation _doubleDoorAnimation;
    public float Value { get; private set; }
    public event Action<float> Apply;
   
    private void Awake()
    {
        _doubleDoorAnimation = new PullableDoorAnimation(GetComponent<Animator>());
        Apply = OpenAmountValuePulled;
    }

    private void OpenAmountValuePulled(float amount)
    {
        Value = amount;
        _doubleDoorAnimation.Step(amount);
    }
}
