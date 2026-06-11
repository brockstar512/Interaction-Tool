// Assets/Scripts/Interactable/SubInteractables/Pullable/PullableOpenDependent.cs
using UnityEngine;

namespace IT.Interactables.Pullable
{
    using IT.Animation.World;
    using IT.Core.Dependency;

    public class PullableOpenDependent : Dependent<float>
    {
        private PullableDoorAnimation _doorAnimation;

        private void Awake() => _doorAnimation = new PullableDoorAnimation(GetComponent<Animator>());

        protected override void OnSourceChanged(float t) => _doorAnimation.Step(t);
    }
}
