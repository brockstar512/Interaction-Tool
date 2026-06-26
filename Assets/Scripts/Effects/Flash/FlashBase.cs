using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace IT.Effects.Flash {
[RequireComponent(typeof(SpriteRenderer))]
    public abstract class FlashBase : MonoBehaviour
    {
        [HideInInspector]
        public float flashTime = 1f;
        [HideInInspector]
        public SpriteRenderer sr;
        protected Tweener FadingTweenDriver;
        Tween _autoStop;

        public virtual void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            SetFlashTime();
        }
        public abstract void SetFlashTime();

        // Start the FadeOut/FadeIn ping-pong. duration <= 0 => unbounded (stop manually
        // via StopFlash). duration > 0 => auto-stops + restores opacity after that window.
        // Scaled time (ignoreTimeScale:false) to match Health's Time.time i-frame clock.
        protected void RunFlash(float duration)
        {
            StopFlash();
            FadeOut();
            if (duration > 0f)
                _autoStop = DOVirtual.DelayedCall(duration, StopFlash, false);
        }

        // Kill the oscillation (and any pending auto-stop) and snap the sprite back to opaque.
        public void StopFlash()
        {
            _autoStop?.Kill();
            _autoStop = null;
            FadingTweenDriver?.Kill();
            if (sr != null)
            {
                var c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }

        protected void FadeOut()
        {
            FadingTweenDriver =sr.DOFade(.33f, flashTime).SetEase(Ease.InSine);
            FadingTweenDriver.onComplete = FadeIn;
        }
        protected void FadeIn()
        {
            FadingTweenDriver =sr.DOFade(1f, flashTime).SetEase(Ease.InSine);
            FadingTweenDriver.onComplete = FadeOut;
        }
    }
}
