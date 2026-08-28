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

        // 4.6.2 R2 (DQ-6 ruled: the OPENER'S device navigates; OQ-E ruled: direct
        // per-device polling, no .inputactions touch — actions migration is named
        // M4 debt): while set, THIS device's dpad moves a highlight and south
        // selects. Digits 1-5 remain the ratified keyboard fallback. Other paired
        // devices deliberately do NOT navigate (only _navDevice is polled).
        InputDevice _navDevice;
        int _highlight;
        internal void SetNavigationDevice(InputDevice device) => _navDevice = device;

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
            _highlight = 0;              // R2: fresh content, highlight to the top
            RenderOptions();
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
            PollBootNotices();
            if (_current == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame) Choose(0);
            else if (kb.digit2Key.wasPressedThisFrame) Choose(1);
            else if (kb.digit3Key.wasPressedThisFrame) Choose(2);
            else if (kb.digit4Key.wasPressedThisFrame) Choose(3);
            else if (kb.digit5Key.wasPressedThisFrame) Choose(4);

            // 4.6.2 R2 (DQ-6): opener-device navigation — dpad moves the
            // highlight, south selects. Only the routed device is polled.
            if (_navDevice is UnityEngine.InputSystem.Gamepad pad && _current?.Options != null)
            {
                int count = _current.Options.Length;
                if (count == 0) return;
                if (pad.dpad.down.wasPressedThisFrame) { _highlight = (_highlight + 1) % count; RenderOptions(); }
                else if (pad.dpad.up.wasPressedThisFrame) { _highlight = (_highlight - 1 + count) % count; RenderOptions(); }
                else if (pad.buttonSouth.wasPressedThisFrame) Choose(_highlight);
            }
        }

        // 4.6.2 R2: option rows in one place — ShowNow's content fill and the
        // highlight redraw share it. The "▶ " marker renders only while a
        // navigation device is routed (keyboard-only prompts look as before).
        void RenderOptions()
        {
            var request = _current;
            if (request == null) return;
            for (int i = 0; i < _optionRows.Count; i++)
            {
                bool used = request.Options != null && i < request.Options.Length;
                _optionRows[i].gameObject.SetActive(used);
                if (used)
                    _optionRows[i].text = $"{(_navDevice != null && i == _highlight ? "▶ " : "")}{i + 1}) {request.Options[i].label}";
            }
        }

        // 4.6.1 R5 (DD7 + OQ-E): boot/restore notices POLL here because module
        // lifetime vs restore timing is unordered (ApplyLevelBaseline runs a frame
        // after scene load — a Start-time check could race it). Consume-on-read.
        void PollBootNotices()
        {
            // 4.6.1 R6.1 (Session A defect, consumption half — belt for BOTH fixes
            // above): only the REGISTERED module may consume boot notices. An
            // unregistered module (ordering accident, future stray root) must never
            // eat a notice it cannot reliably present — consume-then-die was the
            // observed failure.
            var coordinator = IT.Boot.SystemsRoot.Instance != null
                ? IT.Boot.SystemsRoot.Instance.Presentation : null;
            if (coordinator == null || !ReferenceEquals(coordinator.Screen, this)) return;

            if (IT.Boot.SessionInfo.ConsumeLoadFailedNotice())
                Enqueue(new PromptRequest
                {
                    Title = "Save Data Damaged",
                    Body = "Your save could not be read — started fresh. A backup of the damaged file was kept.",
                    Options = new (string, System.Action)[] { ("OK", () => { }) },
                });
            if (IT.Boot.SessionInfo.ConsumeRestoreDegraded())
                Enqueue(new PromptRequest
                {
                    Title = "Restore Incomplete",
                    Body = "Restore was incomplete — saving will keep it that way.",   // the #31 line, verbatim
                    Options = new (string, System.Action)[] { ("OK", () => { }) },
                });
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
