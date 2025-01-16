using UnityEngine;

namespace Doors
{
    public class MasterLockDoubleLocked : Locked
    {
        private DoubleDoorAnimation _doubleDoorAnimation;

        private void Awake()
        {
            UpdateLayerName();
            Animator anim = GetComponent<Animator>();
            _doubleDoorAnimation = new DoubleDoorAnimation(anim);
        }

        protected override void OpenAnimation()
        {
            base.OpenAnimation();
            _doubleDoorAnimation.Play();
        }
    }
}
