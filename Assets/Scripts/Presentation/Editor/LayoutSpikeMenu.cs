#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IT.Presentation.EditorTools
{
    // 4.6.1 R2 SPIKE (DQ-5(ii) ruled, SAVE.2 pattern): the three UGUI layout
    // unknowns become FACTUAL BASIS for R3's window implementation — run ONCE
    // (edit mode, ~5 min), paste the three [Spike] lines. ESCALATION (ruled):
    // if the stock VerticalLayoutGroup + ContentSizeFitter pair fails (sizes
    // wrong/frame-late beyond ForceRebuild), STOP — back to the owner before R3.
    static class LayoutSpikeMenu
    {
        [MenuItem("Tools/4.6.1 R2 Spike — UGUI layout facts")]
        static void Run()
        {
            var canvasGo = new GameObject("SpikeCanvas", typeof(Canvas));
            try
            {
                var window = new GameObject("Window", typeof(RectTransform),
                    typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                window.transform.SetParent(canvasGo.transform, false);
                var fitter = window.GetComponent<ContentSizeFitter>();
                fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                var group = window.GetComponent<VerticalLayoutGroup>();
                group.childControlHeight = true;  group.childControlWidth = true;
                group.childForceExpandHeight = false; group.childForceExpandWidth = false;

                var rt = (RectTransform)window.transform;

                AddContent(window.transform, "A", height: 120f, width: 300f);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                Debug.Log($"[Spike] (i) one injected child: window sizeDelta = {rt.sizeDelta} " +
                          "(~300x120 = the nested control chain works)");

                AddContent(window.transform, "B", height: 80f, width: 300f);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                Debug.Log($"[Spike] (ii) second child + ForceRebuild: sizeDelta = {rt.sizeDelta} " +
                          "(~300x200 IMMEDIATELY = sizing is force-rebuild-synchronous; " +
                          "stale = frame-late, R3 must tween on the following frame)");

                Debug.Log($"[Spike] (iii) tween-target readback: rect.size = {rt.rect.size} vs " +
                          $"sizeDelta = {rt.sizeDelta} (match = the fitter drives sizeDelta " +
                          "directly and R3's UiTween can animate it; mismatch = tween " +
                          "rect via anchors instead)");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void AddContent(Transform parent, string name, float height, float width)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = height;
            le.preferredWidth  = width;
        }
    }
}
#endif
