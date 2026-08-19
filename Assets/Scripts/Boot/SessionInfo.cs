using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IT.Core.Save;
using IT.Core.WorldState;
using IT.Player.Persistence;

namespace IT.Boot
{
    // SAVE.1 (E-4 addition): set exactly ONCE at the boot branch. v1 consumes nothing
    // from it (structured-log only); Epic 4.6's conditional prompts ("load attempted
    // and failed" != "no save existed") read it instead of reconstructing boot history.
    public enum LoadOutcome { NoSaveFound, LoadedSuccessfully, LoadFailedFellBackToNew }

    // SAVE.1 (DD7, kickoff-delegated home decision): session-scoped boot signals as a
    // plain static class per C-C (BootInit precedent) — SystemsRoot stays a host for
    // SYSTEMS, not a grab-bag of fields. Domain reload resets everything per Play
    // session, which is exactly the wanted lifetime ("session").
    public static class SessionInfo
    {
        public static LoadOutcome LoadOutcome { get; internal set; } = LoadOutcome.NoSaveFound;

        // DD5 threading: the boot-loaded primary DTO, cleared-on-read (the DD10-trio
        // precedent). Consumed by ApplyLevelBaseline — P1 only; P2+ fresh joins stay
        // DTO-less (A-3).
        static PlayerStateDTO? _pendingPrimaryRestore;
        internal static void StashPrimaryRestore(in PlayerStateDTO dto) => _pendingPrimaryRestore = dto;
        public static PlayerStateDTO? ConsumePendingPrimaryRestore()
        {
            var value = _pendingPrimaryRestore;
            _pendingPrimaryRestore = null;
            return value;
        }

        // OQ-A: the loaded wire list, held session-long for BOTH consumers — (a) the
        // post-scene-load flags apply below, and (b) CaptureSaveGame's merge-on-save
        // superset carry-through (saved-but-never-registered keys survive save→load→save).
        public static List<FlagEntry> HeldSavedFlags { get; internal set; }

        // SAVE.4 (DD3): the ONE slot signal both call sites read. Default 1 IS the
        // pre-picker pin (DD4: a default, not a branch — boot never sets it; 4.6.1's
        // picker sets it before triggering load/save). Clamped against config on
        // write as a belt for future caller bugs.
        static int _activeSlot = 1;
        public static int ActiveSlot
        {
            get => _activeSlot;
            set
            {
                int limit = SystemsRoot.Instance != null
                    ? SystemsRoot.Instance.Config.SaveSlotLimit
                    : IT.Core.Config.GameConfig.FallbackSaveSlotLimit;
                if (value < 1 || value > limit)
                {
                    Debug.LogWarning($"[SaveLoad] ActiveSlot {value} outside [1..{limit}] — clamped.");
                    _activeSlot = Mathf.Clamp(value, 1, limit);
                }
                else
                {
                    _activeSlot = value;
                }
            }
        }

        // DD6: flags apply POST-scene-load — flag consumers register in their scene
        // Awake, and sceneLoaded fires after those; applying at boot would warn-skip
        // everything. STATIC one-shot by necessity: GameBootstrap dies with the Boot
        // scene during the load, so the handler cannot live on it.
        internal static void ArmFlagsApplyOnce() => SceneManager.sceneLoaded += ApplyFlagsOnce;

        static void ApplyFlagsOnce(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= ApplyFlagsOnce;
            var world = SystemsRoot.Instance?.WorldState;
            if (world == null || HeldSavedFlags == null) return;
            // WorldState's own restore contract: clear Permanent + SessionOnly first.
            // Load-only wiring (WorldState D2) — never segment/scene events.
            world.ClearScope(FlagScope.Permanent);
            world.ClearScope(FlagScope.SessionOnly);
            world.RestorePermanentFlags(PlayerStateBuilder.FlagsToDictionary(HeldSavedFlags));
            Debug.Log($"[SaveLoad] permanent flags restored post-scene-load ({HeldSavedFlags.Count} saved entries) — scene '{scene.name}'");
        }
    }
}
