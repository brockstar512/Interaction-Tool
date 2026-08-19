using UnityEngine;
using IT.Boot;
using IT.Core.Save;
using IT.Player.Control;

namespace IT.Player.Persistence
{
    // SAVE.3 (DD1, owner-ruled home: Builder-adjacent): THE one write path. Every
    // trigger — debug key, save point, 5.4's transition — is a one-line caller of
    // SaveNow; the slot-limit story (queued) refactors pathing at exactly ONE site.
    // NOT an event bus (the AC's own constraint): a method, called explicitly.
    public static class SaveService
    {
        public const string ReasonDebugKey  = "debug-key";   // OQ-E: retires with the debug table pre-ship
        public const string ReasonSavePoint = "save-point";  // DD2/OQ-D-i: the SavePoint interactable

        // SAVE.3 OQ-B(a) — THIN-WIRE, the designated transition autosave call
        // (InVehicle precedent). NAMED GAP: nothing calls this in SAVE.3 — 5.4's
        // LocationTransporter fires it POST-load (OQ-C destination capture: E-2's
        // "active scene at save time" = the ARRIVED scene), per the 5.4 inheritance
        // register. Runtime verification of this half lands at 5.4's sweep.
        // (Owner-recorded hazard that killed the generic-scene-hook alternative: a
        // boot-time hook would autosave right after a corrupt-save fallback,
        // overwriting the live save from the failure path.)
        public const string ReasonTransition = "transition";

        // The former S-key body, verbatim in sequence (SAVE.1 R4), now reason-tagged.
        // Fail-safe guards, both warn-and-skip: no P1 (nothing to save), P1 Dead
        // (capturing dead is a caller error — PB.1 DD4; death flows through respawn
        // seeding, not DTO round-trip).
        public static bool SaveNow(string reason)
        {
            PlayerWrapper primary = null;
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null)
                foreach (var wrapper in roster.Wrappers)
                    if (wrapper != null && wrapper.PlayerId == "P1") { primary = wrapper; break; }
            if (primary == null)
            {
                Debug.LogWarning($"[SaveLoad] save '{reason}' skipped — no P1 registered.");
                return false;
            }
            if (primary.State == WrapperState.Dead)
            {
                Debug.LogWarning($"[SaveLoad] save '{reason}' skipped — P1 is Dead (capture-dead is a caller error, PB.1).");
                return false;
            }

            var save = PlayerStateBuilder.CaptureSaveGame(
                primary,
                SystemsRoot.Instance?.WorldState,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                SessionInfo.HeldSavedFlags);
            int slot = SessionInfo.ActiveSlot;   // SAVE.4 (DD3): the one slot signal
            if (SaveFile.WriteAtomic(slot, PlayerStateBuilder.SerializeSaveGame(save), out var failReason))
            {
                Debug.Log($"[SaveLoad] saved ({reason}: P1 + {save.worldFlags.Count} flags, scene '{save.currentSceneId}', slot {slot}) → {SaveFile.PathToFile(slot)}");
                return true;
            }
            Debug.LogWarning($"[SaveLoad] save '{reason}' FAILED — {failReason}");
            return false;
        }
    }
}
