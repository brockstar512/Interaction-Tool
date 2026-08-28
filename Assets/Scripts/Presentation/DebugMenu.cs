using UnityEngine;
using UnityEngine.InputSystem;
using IT.Boot;
using IT.Player.Persistence;

namespace IT.Presentation
{
    // Debug-menu story R2 (R0 ruled 2026-08-28): window consumer #4 — a dev
    // front-end for the PB-series harness (DD2: the harness stays the OWNER —
    // every option invokes its handlers verbatim). F9 CYCLES closed → page 1 →
    // page 2 → closed (the window caps at 5 option rows and page 1 uses all 5 —
    // the R1 paging mechanism); choosing any action closes and resets the cycle.
    // Does NOT pause (OQ-D ruled). The OPENER is dev-gated (OQ-C ruled): this
    // class compiles always; only the key poll is #if'd out of release builds.
    public class DebugMenu : MonoBehaviour
    {
        int _page;   // 0 = closed; 1..2 = the open page
        PromptRequest _shown;   // identity guard: only OUR prompt may be dismissed (D-3)

        void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var kb = Keyboard.current;
            if (kb == null || !kb.f9Key.wasPressedThisFrame) return;
            _page = (_page + 1) % 3;
            var module = GetComponent<ScreenPromptModule>();
            if (module == null) { _page = 0; return; }
            if (_page == 0)
            {
                if (ReferenceEquals(module.CurrentRequest, _shown)) module.DismissCurrent();
                _shown = null;
                Debug.Log("[DebugMenu] closed (F9 cycle)");
                return;
            }
            var harness = FindFirstObjectByType<PlayerStateDebugHarness>();
            if (harness == null)
            {
                Debug.LogWarning("[DebugMenu] no PlayerStateDebugHarness in this scene — menu unavailable.");
                _page = 0;
                return;
            }
            ShowPage(_page, module, harness);
            Debug.Log($"[DebugMenu] page {_page} (F9 cycle)");
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void ShowPage(int page, ScreenPromptModule module, PlayerStateDebugHarness harness)
        {
            // Page-advance = enqueue the new page behind the current one, then
            // dismiss the current — the queue advances into the new page (R1).
            var options = page == 1
                ? new (string, System.Action)[]
                {
                    ("Capture (8)",              Wrap(harness.MenuCapture)),
                    ("RoundTrip Transition (9)", Wrap(harness.MenuRoundTripTransition)),
                    ("RoundTrip Load (0)",       Wrap(harness.MenuRoundTripLoad)),
                    ("Cure poison (6)",          Wrap(harness.MenuCurePoison)),
                    ("Cure on-fire (7)",         Wrap(harness.MenuCureOnFire)),
                }
                : new (string, System.Action)[]
                {
                    ("Toggle ActiveSlot 1<->2",  Wrap(harness.MenuToggleActiveSlot)),
                    // Reason "debug-menu", NOT "debug-key" — deliberate distinct
                    // provenance in the save log (R1).
                    ("SaveNow",                  Wrap(() => IT.Player.Persistence.SaveService.SaveNow("debug-menu"))),
                };
            var previous = _shown;
            _shown = new PromptRequest
            {
                Title = $"DEBUG {page}/2",
                Body = "F9 cycles pages / closes.",
                Options = options,
            };
            module.Enqueue(_shown);
            // Page-advance: the new page queued behind OUR open page — dismiss
            // ours and the queue advances into it. Never dismiss a foreign
            // prompt (identity guard): behind e.g. the pause menu, our page
            // simply waits its FIFO turn (D-3's coexistence behavior).
            if (page == 2 && ReferenceEquals(module.CurrentRequest, previous))
                module.DismissCurrent();
        }

        // Any chosen action closes the window (queue is empty) — reset the cycle
        // so the next F9 opens page 1 again.
        System.Action Wrap(System.Action action) => () => { _page = 0; action(); };
#endif
    }
}
