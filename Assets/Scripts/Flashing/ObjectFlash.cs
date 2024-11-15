using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Flashing{
[RequireComponent(typeof(SpriteRenderer))]
    public abstract class ObjectFlash : MonoBehaviour
    {
        [HideInInspector]
        public float flashTime = 1f;
        [HideInInspector]
        public SpriteRenderer sr;
        protected Tweener FadingTweenDriver;

        public virtual void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            SetFlashTime();
        }
        public abstract void SetFlashTime();
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
