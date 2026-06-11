using UnityEngine;

namespace IT.Interactables.Doors
{
    using IT.Animation.World;

    public class SingleDoor : OpenableBase
    {
        private SingleDoorAnim _singleDoorAnimation;

        private void Awake()
        {
            UpdateLayerName();   // now puts it on the Interactable layer
            _singleDoorAnimation = new SingleDoorAnim(GetComponent<Animator>());
        }

        protected override void OpenAnimation()
        {
            _singleDoorAnimation.Play();
        }
    }
}