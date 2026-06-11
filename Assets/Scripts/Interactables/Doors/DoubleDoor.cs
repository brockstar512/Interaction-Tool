using UnityEngine;

namespace IT.Interactables.Doors
{
    using IT.Animation.World;

    public class DoubleDoor : OpenableBase
    {
        private DoubleDoorAnim _doubleDoorAnimation;

        private void Awake()
        {
            UpdateLayerName();
            _doubleDoorAnimation = new DoubleDoorAnim(GetComponent<Animator>());
        }

        protected override void OpenAnimation()
        {
            _doubleDoorAnimation.Play();
        }
    }
}