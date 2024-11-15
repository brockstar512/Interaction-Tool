using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Flashing
{
    public class CharacterFlash : ObjectFlash, IFlash
    {
        public override void Awake()
        {
            base.Awake();
            SetFlashTime();
        }
        public override void SetFlashTime()
        {
            flashTime = .1f;
        }
        public void StartFlash()
        {
            FadeOut();
        }

        public void EmergencyStop()
        {
            FadingTweenDriver.Kill();
        }
        
    }

}
