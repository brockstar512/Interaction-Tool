using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IT.Presentation
{
    // Story 5.4 R3 (DD1): v1's ITransition — a full-screen black overlay faded on UNSCALED
    // time. HOSTING DEVIATION (named in the R3 commit): DD1 said "Presentation-owned
    // overlay", but PresentationRoot is SCENE-LOCAL (dies with its scene), so an overlay
    // there would vanish mid-load and flash the scene swap. This component lives on a
    // DontDestroyOnLoad child of SystemsRoot (created by LocationTransporter) and builds
    // its own top-sorted overlay canvas. Code-built UI reskin = the standing named debt.
    public class FadeTransition : MonoBehaviour, ITransition
    {
        [SerializeField] float _seconds = 0.35f;
        Image _image;

        void Awake()
        {
            var canvasGo = new GameObject("FadeCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;   // above every scene canvas (prompts, HUD, fx)

            var imageGo = new GameObject("Fade");
            imageGo.transform.SetParent(canvasGo.transform, false);
            _image = imageGo.AddComponent<Image>();
            _image.raycastTarget = false;
            var rt = _image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            SetAlpha(0f);
        }

        public IEnumerator Cover() => FadeTo(1f);
        public IEnumerator Reveal() => FadeTo(0f);
        public void ResetImmediate() => SetAlpha(0f);

        IEnumerator FadeTo(float target)
        {
            float start = _image.color.a;
            float t = 0f;
            while (t < _seconds)
            {
                t += Time.unscaledDeltaTime;   // timeScale is 0 during the cover
                SetAlpha(Mathf.Lerp(start, target, t / _seconds));
                yield return null;
            }
            SetAlpha(target);
        }

        void SetAlpha(float a)
        {
            if (_image != null) _image.color = new Color(0f, 0f, 0f, a);
        }
    }
}
