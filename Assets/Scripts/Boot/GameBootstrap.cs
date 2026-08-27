using IT.Player.Control;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.Boot
{
    // Lives on a GameObject in Boot.unity (build index 0). Ensures the systems exist, then loads the
    // first gameplay scene. NOTE: BootGuard already runs EnsureSystems() before this scene loads, so
    // the call here is belt-and-suspenders — it's idempotent (a no-op if already initialized).
    //
    // Inspector-wired references live here (architecture D1) — the intended authoring surface for
    // anything that can't be set from JSON or discovered at runtime.
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Scene name to load after boot. Must be in Build Settings.")]
        [SerializeField] private string firstScene = "Level 1 Test";

        [Tooltip("Player prefab instantiated for P2+ join (Story 3.3). " +
                 "Null in direct-play (BootGuard) path — P2 join requires Boot.unity.")]
        [SerializeField] private GameObject playerPrefab;

        void Start()
        {
            BootInit.EnsureSystems();

            // TryGetInstance (not Instance) so we never silently auto-create a throwaway,
            // non-persistent PlayerRoster if EnsureSystems() failed to add the real one —
            // that would swallow the prefab and break P2+ join with no obvious cause.
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null)
                Debug.LogError("[GameBootstrap] PlayerRoster missing after EnsureSystems() — " +
                               "PlayerPrefab not wired; P2+ join will fail.");
            else if (playerPrefab != null)
                roster.PlayerPrefab = playerPrefab;

            // 4.6.1 R6.2 (A-3 fix, owner-ruled 2026-08-26): ANY pass through Boot = the prior
            // session ends. Continue and Title→Play both reload through here (the only
            // LoadScene(0) shape), so the game-over reservation must die HERE — or the returning
            // primary reseats as P2 and the P1-only DTO/mercy gates starve (the A-3 capture).
            // Boot-only by construction (this component lives in Boot.unity; the BootGuard/
            // direct-play path never runs it). ORDERING [S]: nothing in Start pairs or allocates —
            // the scene-placed wrapper registers after the LoadScene at the bottom, and input
            // events cannot interleave a synchronous Start. First boot: a no-op (counts 0).
            roster?.ResetSession();

            // PB.4 (R3-Q4): feed the same player prefab to the SpawnManager for death-respawn (arch D1 —
            // one wiring line here). Null-guarded like the Instance?.Config posture.
            var spawn = SystemsRoot.Instance?.Spawn;
            if (spawn != null && playerPrefab != null)
                spawn.PlayerPrefab = playerPrefab;

            // TODO: wrap in an ITransition fade (Story 5.4) instead of a hard cut.
            // PB.4.5 R3.1: lock joins through the boot load (the only runtime LoadScene site
            // today); the roster reopens itself on activeSceneChanged. Epic 5's Transport
            // loading inherits this same pair of calls when it is built.
            var joinRoster = PlayerRoster.TryGetInstance();
            if (joinRoster != null) joinRoster.JoinPolicy = LockedJoinPolicy.Instance;

            // SAVE.1 R4 (DD4): the v1 auto-continue boot branch — the ONLY read of
            // save.json. Direct-play (BootGuard) never runs this, so it never
            // auto-continues (same posture as P2-join requiring Boot.unity).
            // dtoVersion + per-field validation run inside the Builder's validation
            // pass (SAVE.2 DD6 takeover — this branch never grows an inline version
            // check; SAVE.2 R3 adds the call, single implementation).
            string targetScene = firstScene;
            // SAVE.4: legacy adopt runs BEFORE the read (OQ-A(2) one-time rename;
            // no-op forever after); the boot read addresses the ACTIVE slot — still 1
            // at boot pre-picker (the DD4 pin-by-default).
            IT.Core.Save.SaveFile.AdoptLegacySingleSlot();
            int bootSlot = SessionInfo.ActiveSlot;
            // 4.6.1 R6.3 (A-7 observability + the "load path logs no slot on success"
            // sitting observation): the boot read's slot, logged unconditionally — the
            // twin of the picker's write line. Divergence between the two IS the A-7 bug.
            Debug.Log($"[SaveLoad] loading slot {bootSlot}");
            var readOutcome = IT.Core.Save.SaveFile.ReadText(bootSlot, out var saveJson, out var ioReason);
            if (readOutcome == IT.Core.Save.SaveFile.ReadOutcome.NotFound)
            {
                // E-4.i: file absent -> the PB.5 first-launch route, UNTOUCHED (the
                // collapse promise: no new spawn code on this branch).
                SessionInfo.LoadOutcome = LoadOutcome.NoSaveFound;
            }
            else
            {
                IT.Core.Save.SaveGameDTO save = null;
                bool parsed = readOutcome == IT.Core.Save.SaveFile.ReadOutcome.Read
                              && IT.Player.Persistence.PlayerStateBuilder.ParseSaveGame(saveJson, out save);
                if (!parsed)
                {
                    // E-4.ii envelope failure: no fields to fail alive on -> new-game path.
                    Debug.LogWarning($"[SaveLoad] envelope failure — {(ioReason ?? "unparseable JSON")} — new-game path (E-4.ii)");
                    IT.Core.Save.SaveFile.PreserveCorpse(bootSlot);
                    SessionInfo.LoadOutcome = LoadOutcome.LoadFailedFellBackToNew;
                    SessionInfo.RequestLoadFailedNotice();   // 4.6.1 R5 (DD7): the one v1 LoadOutcome prompt
                }
                else
                {
                    // SAVE.2 R3 (DD1 call site — the ONLY one): the validation pass
                    // normalizes + emits the structured logs (incl. the dtoVersion check,
                    // DD6 takeover); any corruption preserves the corpse BEFORE the next
                    // write (C-2). Load proceeds either way — per-field fail-alive.
                    int corruptFields = IT.Player.Persistence.PlayerStateBuilder.ValidateSaveGame(ref save);
                    if (corruptFields > 0) IT.Core.Save.SaveFile.PreserveCorpse(bootSlot);
                    SessionInfo.LoadOutcome = LoadOutcome.LoadedSuccessfully;
                    SessionInfo.StashPrimaryRestore(save.primaryPlayer);
                    SessionInfo.HeldSavedFlags = save.worldFlags;
                    SessionInfo.ArmFlagsApplyOnce();
                    if (!string.IsNullOrEmpty(save.currentSceneId)
                        && Application.CanStreamedLevelBeLoaded(save.currentSceneId))
                        targetScene = save.currentSceneId;
                    else
                        // C-3 split: unknown scene -> the new-game SPAWN PATH with the DTO
                        // STILL restored — "where's my castle?", never "who am I?".
                        Debug.LogWarning($"[SaveLoad] corruption at path 'currentSceneId' — expected loadable scene name, got '{save.currentSceneId}' — default '{firstScene}' applied (C-3: DTO still restored)");
                }
            }
            SceneManager.LoadScene(targetScene);
        }
    }
}
