using UnityEngine;

namespace Doors
{
    public class OpenableDoubleDoor : Openable
    {
        private DoubleDoorAnimation _doubleDoorAnimation;

        private void Awake()
        {
            UpdateLayerName();
            _doubleDoorAnimation = new DoubleDoorAnimation(GetComponent<Animator>());
        }

        protected override void OpenAnimation()
        {
            _doubleDoorAnimation.Play();
        }
    }
}