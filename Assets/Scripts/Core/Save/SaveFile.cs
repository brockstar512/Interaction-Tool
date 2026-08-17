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
        public static string PathToFile   => Path.Combine(Application.persistentDataPath, "save.json");
        static string PathToTmp           => Path.Combine(Application.persistentDataPath, "save.json.tmp");
        public static string PathToCorpse => Path.Combine(Application.persistentDataPath, "save.json.corrupt");

        public enum ReadOutcome { NotFound, Read, IOFailure }

        // Envelope-level IO. NotFound is the legitimate first-launch signal (E-4.i);
        // IOFailure is envelope failure (E-4.ii) — the caller routes to the new-game
        // path with a structured log. Parse failure is detected Builder-side.
        public static ReadOutcome ReadText(out string json, out string reason)
        {
            json = null; reason = null;
            try
            {
                if (!File.Exists(PathToFile)) return ReadOutcome.NotFound;
                json = File.ReadAllText(PathToFile);
                return ReadOutcome.Read;
            }
            catch (Exception e)
            {
                reason = $"{e.GetType().Name}: {e.Message}";
                return ReadOutcome.IOFailure;
            }
        }

        // E-3: atomic write — tmp + File.Replace so a crash mid-write can never leave a
        // truncated save.json (the single likeliest real-world corruption source).
        // File.Replace requires an existing destination; first-ever save uses Move.
        public static bool WriteAtomic(string json, out string reason)
        {
            reason = null;
            try
            {
                File.WriteAllText(PathToTmp, json);
                if (File.Exists(PathToFile)) File.Replace(PathToTmp, PathToFile, null);
                else File.Move(PathToTmp, PathToFile);
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
        public static void PreserveCorpse()
        {
            try
            {
                if (File.Exists(PathToFile)) File.Copy(PathToFile, PathToCorpse, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveLoad] corpse preservation failed — {e.GetType().Name}: {e.Message}");
            }
        }
    }
}
