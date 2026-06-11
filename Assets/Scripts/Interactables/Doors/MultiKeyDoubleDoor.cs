// Assets/Scripts/Doors/MultiKeyDoubleDoor.cs

using System;
using UnityEngine;

namespace IT.Interactables.Doors
{
    using IT.Animation.World;
    using IT.Core.Dependency;

    public class MultiKeyDoubleDoor : MultiDependent<bool>
    {
        private bool _opened;
        private DoubleDoorAnim _doubleDoorAnimation;

        private void Awake()
        {
            _doubleDoorAnimation = new DoubleDoorAnim(GetComponent<Animator>());
        }

        protected override void Reevaluate()
        {
            if (_opened) return;

            bool any = false;
            foreach (bool v in Values())
            {
                any = true;
                if (!v) return;          // one false → bail
            }

            if (any) Open();             // all assigned sources were true
        }

        private void Open()
        {
            _opened = true;
            _doubleDoorAnimation.Play();
            Destroy(this);
        }
    }
}
