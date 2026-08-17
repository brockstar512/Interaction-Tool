#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using IT.Player.Persistence;

namespace IT.Core.Save.EditorTools
{
    // SAVE.2 R2 SPIKE (owner-ruled scope): the three JsonUtility unknowns become
    // FACTUAL BASIS for DD2, not assumptions. Run ONCE at sweep-open (before any
    // fixture), paste the three [Spike] lines into the sweep report — X2/X4's pass
    // criteria fill from them. ESCALATION (owner-ruled): if (i) reports sentinels NOT
    // preserved in the nested struct, STOP the sweep and raise a ruling request —
    // the detection-scope reshape is the owner's call, never a silent workaround.
    static class SaveSpikeMenu
    {
        [MenuItem("Tools/SAVE.2 R2 Spike — JsonUtility facts")]
        static void Run()
        {
            // (i) nested-struct sentinel preservation: does FromJsonOverwrite leave
            // primaryPlayer's seeded sentinels intact when the JSON omits the struct?
            var a = PlayerStateBuilder.CreateSentinelSeeded();
            JsonUtility.FromJsonOverwrite("{\"dtoVersion\": 1}", a);
            bool preserved = a.primaryPlayer.lives == PlayerStateBuilder.SentinelInt
                          && a.primaryPlayer.currentHealth == PlayerStateBuilder.SentinelInt;
            Debug.Log($"[Spike] (i) nested-struct sentinel preservation: {(preserved ? "PRESERVED" : "NOT PRESERVED — STOP, ruling request")} " +
                      $"(lives raw {a.primaryPlayer.lives}, health raw {a.primaryPlayer.currentHealth})");

            // (ii) missing-List behavior: null (seed intact) or replaced with empty?
            Debug.Log($"[Spike] (ii) missing-List: worldFlags {(a.worldFlags == null ? "NULL (seed intact — missing detectable)" : $"NON-NULL, count {a.worldFlags.Count} (JsonUtility replaced the seed)")}; " +
                      $"items {(a.primaryPlayer.items == null ? "NULL" : $"count {a.primaryPlayer.items.Count}")}");

            // (iii) type mismatch (string where int expected): throw vs silent default?
            var b = PlayerStateBuilder.CreateSentinelSeeded();
            try
            {
                JsonUtility.FromJsonOverwrite("{\"dtoVersion\": \"banana\"}", b);
                Debug.Log($"[Spike] (iii) type-mismatch: NO throw — dtoVersion now {(b.dtoVersion == PlayerStateBuilder.SentinelInt ? "SENTINEL (unparsed → detectable as missing)" : b.dtoVersion.ToString() + " (silently coerced)")}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[Spike] (iii) type-mismatch: THREW {e.GetType().Name} ('{e.Message}') — mismatches route to the ENVELOPE path (E-4.ii)");
            }
        }
    }
}
#endif
