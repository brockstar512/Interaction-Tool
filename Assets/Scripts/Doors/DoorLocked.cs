using UnityEngine;

namespace Doors
{
    public class DoorLocked : Locked
    {
        private SingleDoorAnimation _singleDoorAnimation;

        private void Awake()
        {
            UpdateLayerName();
            Animator anim = GetComponent<Animator>();
            _singleDoorAnimation = new SingleDoorAnimation(anim);
        }

        protected override void OpenAnimation()
        {
            base.OpenAnimation();
            _singleDoorAnimation.Play();
        }


    }
}
