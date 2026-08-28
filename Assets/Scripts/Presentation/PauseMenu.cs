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
            {
                foreach (var w in roster.Wrappers) w?.PauseFreeze();   // DQ-7 player half (DD2)
                // 4.6.2 R3 (DD3/DQ-6 cession): the menu owns the gate while open.
                roster.JoinPolicy = MenuGatePolicy.Instance;
            }
            // Opener-device navigation (DQ-6): sibling access is presentation-
            // internal wiring, not a coordinator bypass — Enqueue still routes
            // through the registered handle above.
            GetComponent<ScreenPromptModule>()?.SetNavigationDevice(opener);
            Debug.Log($"[Pause] opened by '{PlayerWrapper.DeriveDeviceId(opener)}' — timeScale {_savedTimeScale}→0, players frozen");
            ShowMenu(screen);
        }

        void ShowMenu(IScreenPromptModule screen)
        {
            var roster = PlayerRoster.TryGetInstance();
            var options = new System.Collections.Generic.List<(string, System.Action)>
            {
                ("Resume", Close),
            };

            // 4.6.2 R4 (OQ-D(i) ruled): "P1 Input Device" cycles keyboard↔pad. The
            // choice persists as SessionInfo.PreferredPrimaryDevice (DD5 justified
            // survivor). Shown only when P1 exists AND an alternative device does.
            var p1 = FindWrapper(roster, "P1");
            if (p1 != null && CycleTarget(roster, p1) != null)
                options.Add(($"P1 Input Device: {(p1.PairedDevice is Gamepad ? "Gamepad" : "Keyboard")}", () =>
                {
                    var target = CycleTarget(roster, p1);
                    if (target == null) return;   // vanished between render and press
                    p1.RePair(target);            // Resume()s — the open menu's Update re-freezes (B-16 shape)
                    roster.BackfillDeviceMap(p1); // R3 seam fix's second caller
                    SessionInfo.PreferredPrimaryDevice = target is Gamepad ? "gamepad" : "keyboard";
                    Debug.Log($"[Pause] P1 re-paired to '{PlayerWrapper.DeriveDeviceId(target)}' — preference saved ('{SessionInfo.PreferredPrimaryDevice}')");
                    var s = SystemsRoot.Instance?.Presentation?.Screen;
                    if (s != null) ShowMenu(s);   // reshow with the updated label (TitleScreen pattern)
                }));

            // 4.6.2 R4 (§9.2 landing; §5.E): Leave is JOINER-ONLY and offered to the
            // OPENER only (DQ-6: the opener's device navigates — leave applies to the
            // player driving the menu; another joiner opens their own pause to leave).
            // Dead-refusal (§7.2) is enforced at the roster entry; the item also
            // hides then, matching the game-over screen's fixed Continue+Quit set.
            var opener = OwnerOf(roster, _opener);
            if (opener != null && opener.PlayerId != "P1" && opener.State != WrapperState.Dead)
                options.Add(($"Leave Game ({opener.PlayerId})", () =>
                {
                    Close();                       // unpause everyone first…
                    roster?.Leave(opener);         // …then the §5.D-preserving exit
                }));

            screen.Enqueue(new PromptRequest
            {
                Title = "PAUSED",
                Body = "",
                Options = options.ToArray(),
            });
        }

        static PlayerWrapper FindWrapper(PlayerRoster roster, string id)
        {
            if (roster == null) return null;
            foreach (var w in roster.Wrappers)
                if (w != null && w.PlayerId == id) return w;
            return null;
        }

        static PlayerWrapper OwnerOf(PlayerRoster roster, InputDevice device)
        {
            if (roster == null || device == null) return null;
            foreach (var w in roster.Wrappers)
                if (w != null && w.OwnsDevice(device)) return w;
            return null;
        }

        // The cycle's destination: P1 on keyboard → first pad NOT owned by anyone;
        // P1 on pad → the keyboard, if unowned. Null = no legal target (item hidden).
        static InputDevice CycleTarget(PlayerRoster roster, PlayerWrapper p1)
        {
            if (p1.PairedDevice is Gamepad)
            {
                var kb = Keyboard.current;
                return kb != null && OwnerOf(roster, kb) == null ? kb : null;
            }
            foreach (var pad in Gamepad.all)
                if (OwnerOf(roster, pad) == null) return pad;
            return null;
        }

        void Close()
        {
            _open = false;
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null)
            {
                foreach (var w in roster.Wrappers) w?.PauseThaw();     // thaws ONLY Paused
                // 4.6.2 R3 (GATE DEFAULT, owner-stated): the menu restores Open on close.
                roster.JoinPolicy = OpenJoinPolicy.Instance;
            }
            Time.timeScale = _savedTimeScale;          // restore VERBATIM (B-1 evidence)
            GetComponent<ScreenPromptModule>()?.SetNavigationDevice(null);
            Debug.Log($"[Pause] closed — timeScale restored to {_savedTimeScale}");
        }
    }
}
