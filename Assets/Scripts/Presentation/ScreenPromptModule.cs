using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IT.Presentation
{
    // 4.6.1 (DD3, constraint 3 + DQ-5 ruled): THE Surface-2 module — one reused
    // window, content-injected, auto-sizing (stock VerticalLayoutGroup +
    // ContentSizeFitter per the R2 spike facts: nested control works, ForceRebuild
    // is synchronous, the fitter drives sizeDelta so size changes tween on
    // sizeDelta). Queue semantics per DQ-5(iii): GameOver PREEMPTS and CLEARS,
    // everything else FIFO. ALL UI code-built (OQ-G(3) — reskin = named debt).
    //
    // v1 INPUT: options select via number keys 1..5 (collision-checked: digits
    // 1-5 are free; 0/6-9 are harness keys) read from Keyboard.current — the
    // debug-keys pattern. Device-routed menu navigation is 4.6.2's (DQ-6);
    // gameplay keys stay live under an open prompt until the pause story lands.
    public class ScreenPromptModule : MonoBehaviour, IScreenPromptModule
    {
        readonly List<PromptRequest> _queue = new();
        PromptRequest _current;

        RectTransform _window;
        CanvasGroup _group;
        Text _title;
        Text _body;
        readonly List<Text> _optionRows = new();
        const float TweenSeconds = 0.15f;
        const float ContentWidth = 420f;

        public void Enqueue(PromptRequest request)
        {
            if (request == null) return;
            if (request.Priority == PromptPriority.GameOver)
            {
                _queue.Clear();                 // DQ-5(iii): preempt + clear
                ShowNow(request);
                return;
            }
            if (_current == null) ShowNow(request);
            else _queue.Add(request);
        }

        void ShowNow(PromptRequest request)
        {
            bool wasOpen = _current != null;
            _current = request;
            if (_window == null) BuildWindow();

            Vector2 oldSize = _window.sizeDelta;
            _title.text = request.Title ?? "";
            _body.text = request.Body ?? "";
            for (int i = 0; i < _optionRows.Count; i++)
            {
                bool used = request.Options != null && i < request.Options.Length;
                _optionRows[i].gameObject.SetActive(used);
                if (used) _optionRows[i].text = $"{i + 1}) {request.Options[i].label}";
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_window);   // spike (ii): synchronous
            Vector2 newSize = _window.sizeDelta;

            StopAllCoroutines();
            _window.gameObject.SetActive(true);
            if (!wasOpen)
            {
                _group.alpha = 0f;
                _window.localScale = Vector3.one * 0.8f;
                StartCoroutine(UiTween.Fade(_group, 0f, 1f, TweenSeconds));
                StartCoroutine(UiTween.Scale(_window, Vector3.one * 0.8f, Vector3.one, TweenSeconds));
            }
            else if (newSize != oldSize)
            {
                _window.sizeDelta = oldSize;   // spike (iii): tween sizeDelta old→new
                StartCoroutine(UiTween.Size(_window, oldSize, newSize, TweenSeconds));
            }
        }

        void Choose(int index)
        {
            var chosen = _current;
            if (chosen?.Options == null || index >= chosen.Options.Length) return;
            _current = null;
            chosen.Options[index].onChosen?.Invoke();
            if (_current != null) return;      // the callback enqueued/preempted — it already owns the window
            if (_queue.Count > 0)
            {
                var next = _queue[0];
                _queue.RemoveAt(0);
                ShowNow(next);
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(CloseRoutine());
            }
        }

        System.Collections.IEnumerator CloseRoutine()
        {
            yield return StartCoroutine(UiTween.Fade(_group, 1f, 0f, TweenSeconds));
            _window.gameObject.SetActive(false);
        }

        void Update()
        {
            if (_current == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame) Choose(0);
            else if (kb.digit2Key.wasPressedThisFrame) Choose(1);
            else if (kb.digit3Key.wasPressedThisFrame) Choose(2);
            else if (kb.digit4Key.wasPressedThisFrame) Choose(3);
            else if (kb.digit5Key.wasPressedThisFrame) Choose(4);
        }

        // ── code-built window (OQ-G(3); layout per spike facts) ────────────────
        void BuildWindow()
        {
            var root = GetComponentInParent<PresentationRoot>();
            var canvas = root != null ? root.ScreenCanvas : GetComponentInParent<Canvas>();

            var windowGo = new GameObject("PromptWindow",
                typeof(RectTransform), typeof(Image), typeof(CanvasGroup),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            windowGo.transform.SetParent(canvas.transform, false);
            _window = (RectTransform)windowGo.transform;
            _window.anchorMin = _window.anchorMax = _window.pivot = new Vector2(0.5f, 0.5f);
            windowGo.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.92f);
            _group = windowGo.GetComponent<CanvasGroup>();

            var fitter = windowGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layout = windowGo.GetComponent<VerticalLayoutGroup>();
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(24, 24, 18, 18);
            layout.spacing = 8f;

            _title = BuildText("Title", 28, FontStyle.Bold);
            _body = BuildText("Body", 18, FontStyle.Normal);
            for (int i = 0; i < 5; i++)   // 5 = the option cap (digits 1-5; saveSlotLimit max)
                _optionRows.Add(BuildText($"Option{i + 1}", 20, FontStyle.Normal));

            _window.gameObject.SetActive(false);
        }

        Text BuildText(string name, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(_window, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            go.GetComponent<LayoutElement>().preferredWidth = ContentWidth;
            return text;
        }
    }
}
