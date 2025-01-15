using UnityEngine;

namespace Doors
{
    public class LockedDoor : Door
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
            _singleDoorAnimation.Play();
        }


    }
}
