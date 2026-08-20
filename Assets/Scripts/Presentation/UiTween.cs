using System.Collections;
using UnityEngine;

namespace IT.Presentation
{
    // 4.6.1 (DQ-5(i) ruled): hand-rolled tween helpers — NO new dependency; converts
    // with the M4 UniTask pass. ALL tweens run on UNSCALED time (DQ-7 named
    // obligation: prompts animate under timeScale=0). Spike fact (iii): the fitter
    // drives sizeDelta directly, so Size() animates sizeDelta.
    public static class UiTween
    {
        public static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            group.alpha = to;
        }

        public static IEnumerator Scale(RectTransform rt, Vector3 from, Vector3 to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                rt.localScale = Vector3.Lerp(from, to, t / duration);
                yield return null;
            }
            rt.localScale = to;
        }

        public static IEnumerator Size(RectTransform rt, Vector2 from, Vector2 to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                rt.sizeDelta = Vector2.Lerp(from, to, t / duration);
                yield return null;
            }
            rt.sizeDelta = to;
        }
    }
}
