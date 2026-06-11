using UnityEngine;

namespace IT.Interactables.Doors
{
    using IT.Animation.World;

    public class OpenableDoor : Openable
    {
        private SingleDoorAnimation _singleDoorAnimation;

        private void Awake()
        {
            UpdateLayerName();   // now puts it on the Interactable layer
            _singleDoorAnimation = new SingleDoorAnimation(GetComponent<Animator>());
        }

        protected override void OpenAnimation()
        {
            _singleDoorAnimation.Play();
        }
    }
}