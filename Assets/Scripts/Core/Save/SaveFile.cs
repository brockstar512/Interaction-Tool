using System;
using System.IO;
using UnityEngine;

namespace IT.Core.Save
{
    // SAVE.1 (DD2): file mechanics ONLY — plain static C# (C-C: no singleton, no
    // lifecycle; BootInit/PlayerStateBuilder precedent). Atomic write (E-3), corpse
    // preservation (C-2), tri-state read at the ENVELOPE level (E-4.ii boundary).
    // No serialization and no validation here: the Builder owns parse/serialize (C-H),
    // SAVE.2's validation pass owns per-field corruption. Code-contact adjustment vs
    // the R0 sketch (recorded): Read returns TEXT, not a parsed DTO — parsing lives
    // Builder-side so IT.Core.Save never references player types in that direction.
    public static class SaveFile
    {
        // SAVE.4 (DD2, owner re-ruling 2026-08-19): per-slot by PARAMETER — uniform
        // save-{n}.json naming (OQ-A(2)); mechanics byte-identical per slot. Slot
        // validity is the CALLER's concern (SessionInfo.ActiveSlot is config-clamped);
        // SaveFile stays dumb IO.
        public static string PathToFile(int slot)   => Path.Combine(Application.persistentDataPath, $"save-{slot}.json");
        static string PathToTmp(int slot)           => Path.Combine(Application.persistentDataPath, $"save-{slot}.json.tmp");
        public static string PathToCorpse(int slot) => Path.Combine(Application.persistentDataPath, $"save-{slot}.json.corrupt");

        public enum ReadOutcome { NotFound, Read, IOFailure }

        // Envelope-level IO. NotFound is the legitimate first-launch signal (E-4.i);
        // IOFailure is envelope failure (E-4.ii) — the caller routes to the new-game
        // path with a structured log. Parse failure is detected Builder-side.
        public static ReadOutcome ReadText(int slot, out string json, out string reason)
        {
            json = null; reason = null;
            try
            {
                if (!File.Exists(PathToFile(slot))) return ReadOutcome.NotFound;
                json = File.ReadAllText(PathToFile(slot));
                return ReadOutcome.Read;
            }
            catch (Exception e)
            {
                reason = $"{e.GetType().Name}: {e.Message}";
                return ReadOutcome.IOFailure;
            }
        }

        // SAVE.4 (OQ-A(2), owner-ruled): one-time legacy adoption — a pre-slots bare
        // save.json becomes slot 1, and its corpse adopts in the SAME pass so C-2
        // holds for adopted saves. Move only when the destination is absent
        // (both-exist -> orphan warn, bare left in place). Failure posture: a failed
        // Move leaves the bare file; the NEXT boot retries — no special recovery.
        public static void AdoptLegacySingleSlot()
        {
            AdoptOne(Path.Combine(Application.persistentDataPath, "save.json"),
                     PathToFile(1), "save.json");
            AdoptOne(Path.Combine(Application.persistentDataPath, "save.json.corrupt"),
                     PathToCorpse(1), "save.json.corrupt");
        }

        static void AdoptOne(string barePath, string slottedPath, string label)
        {
            try
            {
                if (!File.Exists(barePath)) return;
                if (File.Exists(slottedPath))
                {
                    Debug.LogWarning($"[SaveLoad] legacy '{label}' present but its slot-1 file already exists — legacy left in place (orphan).");
                    return;
                }
                File.Move(barePath, slottedPath);
                Debug.Log($"[SaveLoad] legacy '{label}' adopted as slot 1 → {slottedPath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveLoad] legacy adopt of '{label}' failed — {e.GetType().Name}: {e.Message} — left in place; next boot retries.");
            }
        }

        // E-3: atomic write — tmp + File.Replace so a crash mid-write can never leave a
        // truncated save.json (the single likeliest real-world corruption source).
        // File.Replace requires an existing destination; first-ever save uses Move.
        public static bool WriteAtomic(int slot, string json, out string reason)
        {
            reason = null;
            try
            {
                File.WriteAllText(PathToTmp(slot), json);
                if (File.Exists(PathToFile(slot))) File.Replace(PathToTmp(slot), PathToFile(slot), null);
                else File.Move(PathToTmp(slot), PathToFile(slot));
                return true;
            }
            catch (Exception e)
            {
                reason = $"{e.GetType().Name}: {e.Message}";
                return false;
            }
        }

        // C-2: ONE corpse, preserved on any corruption log BEFORE the next write, so a
        // playtest "my save got wiped" report always has evidence. Overwrites the prior
        // corpse (multiple corpses = ruled scope creep).
        public static void PreserveCorpse(int slot)
        {
            try
            {
                if (File.Exists(PathToFile(slot))) File.Copy(PathToFile(slot), PathToCorpse(slot), true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveLoad] corpse preservation failed — {e.GetType().Name}: {e.Message}");
            }
        }
    }
}
