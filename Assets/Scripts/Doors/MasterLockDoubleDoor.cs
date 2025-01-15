using UnityEngine;

namespace Doors
{
    public class MasterLockDoubleDoor : Door
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
            Debug.Log("Play animatione");
            _doubleDoorAnimation.Play();
        }
    }
}
