using UnityEngine;

namespace Doors
{
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