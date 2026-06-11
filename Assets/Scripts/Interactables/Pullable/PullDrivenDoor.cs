// Assets/Scripts/Interactable/SubInteractables/PullableBase/PullDrivenDoor.cs
using UnityEngine;

namespace IT.Interactables.Pullable
{
    using IT.Animation.World;
    using IT.Core.Dependency;

    public class PullDrivenDoor : Dependent<float>
    {
        private PullDoorAnim _doorAnimation;

        private void Awake() => _doorAnimation = new PullDoorAnim(GetComponent<Animator>());

        protected override void OnSourceChanged(float t) => _doorAnimation.Step(t);
    }
}
