using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Flashing{

    public class ObjectFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashTime = .25f;
        private SpriteRenderer _sr;
        private Material _mat;
        private Coroutine _flashCoroutine;
        private Tweener _fadingTweenDriver;

        // [SerializeField] private float _frequency = .5f;
        // Start is called before the first frame update
        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _mat = _sr.material;
        }

        [ContextMenu("Flash")]
        public void FlashItem()
        {
            // _flashCoroutine = StartCoroutine(Flash());
            // _flashCoroutine = StartCoroutine(FlashAlpha());
            FadeOut();
        }

        void FadeOut()
        {
            _sr.DOFade(.5f, flashTime).onComplete();
            _fadingTweenDriver =_sr.DOFade(.5f, flashTime).SetEase(Ease.InSine);
            _fadingTweenDriver.onComplete = FadeIn;
        }
        void FadeIn()
        {
            
        }
   
        private IEnumerator Flash()
        {
            //set the color
            SetFlashColor();
            float currentFlashAmount = 0f;
            //lerp flash amount
            float elapsedTime = 0f;
            while (elapsedTime < flashTime)
            {
                elapsedTime += Time.deltaTime;
                currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime / flashTime));
                SetFlashAmount(currentFlashAmount);
                yield return null;
            }
        }

        private void SetFlashColor()
        {
            _mat.SetColor("_FlashColor",flashColor);
        }

        private void SetFlashAmount(float amount)
        {
            _mat.SetFloat("_FlashAmount",amount);
        }
        
        private IEnumerator FlashAlpha()
        {
            //set the color
            SetFlashColor();
            float currentFlashAmount = 0f;
            //lerp flash amount
            float elapsedTime = 0f;
            while (elapsedTime < flashTime)
            {
                elapsedTime += Time.deltaTime;
                currentFlashAmount = Mathf.Lerp(1f, .5f, (elapsedTime / flashTime));
                Color tmp = _sr.color;
                tmp.a = currentFlashAmount;
                _sr.color = tmp;
                yield return null;
            }
        }
        // private async Task FlashAlphaAsync(bool isFading)
        // {
        //     float startingColor = isFading ? 1 : .5f;
        //
        //     float destinationColor = isFading ? .5f : 1;
        //     float currentFlashAmount = 0f;
        //     //lerp flash amount
        //     float elapsedTime = 0f;
        //     while (elapsedTime < flashTime)
        //     {
        //         Debug.Log("running");
        //         elapsedTime += Time.deltaTime;
        //         currentFlashAmount = Mathf.Lerp(startingColor, destinationColor, (elapsedTime / flashTime));
        //         Color tmp = _sr.color;
        //         tmp.a = currentFlashAmount;
        //         _sr.color = tmp;
        //         
        //     }
        //     await Task.CompletedTask;
        // }

    }
}
