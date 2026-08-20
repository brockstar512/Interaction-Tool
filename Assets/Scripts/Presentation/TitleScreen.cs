using UnityEngine;
using UnityEngine.SceneManagement;
using IT.Boot;

namespace IT.Presentation
{
    // 4.6.1 R4 (OQ-A(1)/OQ-D ruled): the minimal Title screen — Quit-to-title's
    // target and the slot picker's ONLY v1 entry point. GUARD RAIL (owner-ruled,
    // recorded verbatim in the R0): boot does NOT land here in v1 — Title is
    // reachable via Quit-to-title only, and EVERY Title action routes through the
    // Boot reload (OQ-B's mechanism; "Play" auto-continues or starts fresh per the
    // active slot's file — the boot branch decides, not this screen).
    // Boot-lands-at-Title later = NAMED REOPENING, never drift.
    public class TitleScreen : MonoBehaviour
    {
        void Start() => ShowMenu();

        void ShowMenu()
        {
            var screen = SystemsRoot.Instance != null && SystemsRoot.Instance.Presentation != null
                ? SystemsRoot.Instance.Presentation.Screen : null;
            if (screen == null)
            {
                Debug.LogWarning("[Title] no Screen module — cannot present (scene entered without systems?).");
                return;
            }
            screen.Enqueue(new PromptRequest
            {
                Title = "SLIDING BLOCK",
                Body = $"Save slot {SessionInfo.ActiveSlot} active.",
                Options = new (string, System.Action)[]
                {
                    ("Play", () => SceneManager.LoadScene(0)),   // through Boot — the branch decides continue-vs-fresh
                    ("Select Save Slot", ShowSlotPicker),
                },
            });
        }

        // 4.6.1 R5 (DD8, OQ-D ruled: Title-only entry in v1): slots 1..limit, label =
        // file presence + the envelope's scene name (presence is a STAT, not an
        // outcome — the OQ-B ruling); choosing sets ActiveSlot and returns to the
        // menu, whose "Play" then routes through Boot for THAT slot.
        void ShowSlotPicker()
        {
            var screen = SystemsRoot.Instance != null && SystemsRoot.Instance.Presentation != null
                ? SystemsRoot.Instance.Presentation.Screen : null;
            if (screen == null) return;

            int limit = SystemsRoot.Instance.Config != null
                ? SystemsRoot.Instance.Config.SaveSlotLimit
                : IT.Core.Config.GameConfig.FallbackSaveSlotLimit;
            var options = new (string, System.Action)[limit];
            for (int i = 0; i < limit; i++)
            {
                int slot = i + 1;
                string label = $"Slot {slot} — empty";
                if (IT.Core.Save.SaveFile.ReadText(slot, out var json, out _) == IT.Core.Save.SaveFile.ReadOutcome.Read
                    && IT.Player.Persistence.PlayerStateBuilder.ParseSaveGame(json, out var save)
                    && !string.IsNullOrEmpty(save.currentSceneId))
                    label = $"Slot {slot} — {save.currentSceneId}";
                options[i] = (label, () => { SessionInfo.ActiveSlot = slot; ShowMenu(); });
            }
            screen.Enqueue(new PromptRequest
            {
                Title = "SELECT SAVE SLOT",
                Body = "Choose the active slot.",
                Options = options,
            });
        }
    }
}
