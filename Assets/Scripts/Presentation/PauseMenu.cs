using UnityEngine;
using UnityEngine.InputSystem;
using IT.Boot;
using IT.Player.Control;

namespace IT.Presentation
{
    // 4.6.2 R2 (OQ-A ruled IN; DD1/DD2/DQ-7): the pause menu — a Surface-2 CONTENT
    // PROVIDER (no new window class, DD1) living on PresentationRoot beside the
    // ScreenPromptModule (scene-local: pause state dies with its scene, the DD5
    // disposition). DUAL FREEZE (DQ-7): players via the DD2 Paused state, world via
    // timeScale = 0 (BombTimer is scaled — R0 G2 — so fuses hold free); prompt
    // tweens run UNSCALED (4.6.1 UiTween), so the menu animates while the world
    // holds. DQ-6: any PAIRED device may open (keyboard Escape / pad Start); the
    // OPENER'S device navigates (routed to the window below).
    //
    // Known accepted edge (recorded): game-over PREEMPTS an open pause without
    // closing it (DQ-5(iii)) — timeScale stays 0 under the game-over screen (the
    // world staying frozen there is fine); Continue reloads through Boot, whose
    // timeScale belt restores 1 (the DD5 ratified home — B-3's runtime half).
    public class PauseMenu : MonoBehaviour
    {
        bool _open;
        float _savedTimeScale = 1f;
        InputDevice _opener;

        void Update()
        {
            if (!_open)
            {
                TryOpenFromInput();
                return;
            }
            // DD2/B-16 (owner row): re-assert freeze while open — a wrapper
            // RePair'd mid-pause lands Active via Resume; the still-open menu
            // re-freezes it (PauseFreeze is idempotent and Active-only).
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null) return;
            foreach (var w in roster.Wrappers)
                if (w != null && w.State == WrapperState.Active) w.PauseFreeze();
        }

        // DQ-6: only a PAIRED device opens pause. With no wrappers (Title, boot)
        // this is inert — no paired devices exist to open it.
        void TryOpenFromInput()
        {
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null || roster.Wrappers.Count == 0) return;
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame && Owned(roster, kb))
            {
                Open(kb);
                return;
            }
            foreach (var w in roster.Wrappers)
                if (w != null && w.PairedDevice is Gamepad pad && pad.startButton.wasPressedThisFrame)
                {
                    Open(pad);
                    return;
                }
        }

        static bool Owned(PlayerRoster roster, InputDevice device)
        {
            foreach (var w in roster.Wrappers)
                if (w != null && w.OwnsDevice(device)) return true;
            return false;
        }

        void Open(InputDevice opener)
        {
            var screen = SystemsRoot.Instance != null && SystemsRoot.Instance.Presentation != null
                ? SystemsRoot.Instance.Presentation.Screen : null;
            if (screen == null)
            {
                Debug.LogWarning("[Pause] no Screen module — cannot pause.");
                return;
            }
            _open = true;
            _opener = opener;
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;                       // DQ-7 world half
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null)
                foreach (var w in roster.Wrappers) w?.PauseFreeze();   // DQ-7 player half (DD2)
            // Opener-device navigation (DQ-6): sibling access is presentation-
            // internal wiring, not a coordinator bypass — Enqueue still routes
            // through the registered handle above.
            GetComponent<ScreenPromptModule>()?.SetNavigationDevice(opener);
            Debug.Log($"[Pause] opened by '{PlayerWrapper.DeriveDeviceId(opener)}' — timeScale {_savedTimeScale}→0, players frozen");
            ShowMenu(screen);
        }

        void ShowMenu(IScreenPromptModule screen)
        {
            var options = new System.Collections.Generic.List<(string, System.Action)>
            {
                ("Resume", Close),
            };
            // R4 appends: "P1 Input Device" cycle (OQ-D(i)) · joiner Leave (§9.2).
            screen.Enqueue(new PromptRequest
            {
                Title = "PAUSED",
                Body = "",
                Options = options.ToArray(),
            });
        }

        void Close()
        {
            _open = false;
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null)
                foreach (var w in roster.Wrappers) w?.PauseThaw();     // thaws ONLY Paused
            Time.timeScale = _savedTimeScale;          // restore VERBATIM (B-1 evidence)
            GetComponent<ScreenPromptModule>()?.SetNavigationDevice(null);
            Debug.Log($"[Pause] closed — timeScale restored to {_savedTimeScale}");
        }
    }
}
