using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Flashing
{
    public class BombFlash : ObjectFlash, IFlash
    {
        public override void Awake()
        {
            base.Awake();
            StartFlash();
        }
        public override void SetFlashTime()
        {
            flashTime = .25f;
        }
        public void StartFlash()
        {
            FadeOut();
        }

        public void EmergencyStop()
        {
            FadingTweenDriver.Kill();
        }
        
        private void OnDestroy()
        {
            EmergencyStop();
        }
    }
}
