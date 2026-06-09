// Assets/Scripts/Interactable/SubInteractables/Pullable/PullableOpenDependent.cs
using UnityEngine;

public class PullableOpenDependent : Dependent<float>
{
    private PullableDoorAnimation _doorAnimation;

    private void Awake() => _doorAnimation = new PullableDoorAnimation(GetComponent<Animator>());

    protected override void OnSourceChanged(float t) => _doorAnimation.Step(t);
}