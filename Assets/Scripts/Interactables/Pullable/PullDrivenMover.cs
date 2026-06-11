// Assets/Scripts/Interactable/SubInteractables/PullableBase/PullDrivenMover.cs
using UnityEngine;

namespace IT.Interactables.Pullable
{
    using IT.Core.Dependency;

    public class PullDrivenMover : Dependent<float>
    {
        private enum Axis { Horizontal, Vertical }

        [SerializeField] private Axis axis = Axis.Vertical;
        [SerializeField] private float distance = 3f;

        private Vector3 _origin;

        private void Awake() => _origin = transform.position;

        protected override void OnSourceChanged(float t)
        {
            Vector3 dir = axis == Axis.Horizontal ? Vector3.right : Vector3.up;
            transform.position = _origin + dir * (distance * t);
        }
    }
}
