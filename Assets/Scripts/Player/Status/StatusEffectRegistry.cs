using System;
using System.Collections.Generic;
using UnityEngine;
using IT.Player.Persistence;

namespace IT.Player.Status
{
    // Story PB.2 (Directive 2) — the key ↔ effect mapping for status serialization; owns
    // the StatusStateDTO translation BOTH ways. Plain static class (C-B: no ScriptableObject;
    // C-C: fixed registrations, no runtime state, nothing to boot).
    //
    // KEYS ARE DURABLE IDs (OQ-PB2-B): short stable strings authored exactly once. They must
    // survive the v1 refactor rename pass (Sector→Location, any class renames) WITHOUT
    // breaking save-file compatibility — never derive a key from a type name. This is
    // quality-audit #1 (strings-to-typed-keys) arriving early, deliberately.
    //
    // Factories are explicit method references (DD4 — no reflection: a key can never
    // construct an unexpected type). Registration is part of a status's definition of done
    // (DD6): an active effect with no entry is warn+skipped at capture — DebugLogStatusEffect
    // stays unregistered deliberately (pre-ship throwaway; doubles as the V10 probe). An
    // unknown key at restore warns and skips THAT entry, the rest proceeds (fail-alive
    // spirit; the full corruption posture is SAVE.2's).
    public static class StatusEffectRegistry
    {
        // restore side: key → params-blob → freshly constructed effect (ctor path, DD4)
        static readonly Dictionary<string, Func<string, StatusEffectBase>> _factories = new()
        {
            { "poison", PoisonEffect.FromState },
            { "onfire", OnFireEffect.FromState },
        };

        // capture side: live effect type → its durable key
        static readonly Dictionary<Type, string> _keys = new()
        {
            { typeof(PoisonEffect), "poison" },
            { typeof(OnFireEffect), "onfire" },
        };

        // Snapshot a controller's active effects into DTO entries. Envelope (key + clocks)
        // written here by the machinery; blob written by the effect's CaptureState().
        public static List<StatusStateDTO> Capture(StatusController controller)
        {
            var list = new List<StatusStateDTO>();
            foreach (var effect in controller.Active)
            {
                if (!_keys.TryGetValue(effect.GetType(), out var key))
                {
                    Debug.LogWarning($"[StatusEffectRegistry] Active effect '{effect.GetType().Name}' " +
                        "has no registration — not captured. Register it, or mark it throwaway.");
                    continue;
                }
                list.Add(new StatusStateDTO
                {
                    statusType = key,
                    elapsed = effect.Elapsed,
                    tickAccumulator = effect.TickAccumulator,
                    instanceState = effect.CaptureState(),
                });
            }
            return list;
        }

        // Replay DTO entries onto a (freshly rebuilt) player: construct via factory, then
        // hand to StatusController.Apply — THE one apply path (DD2: replay, not resurrection).
        // OnApply re-runs, so a restored On-Fire re-swaps the controller; the refresh-by-key
        // guard, vehicle soft-refusal, and every future Apply invariant hold for restored
        // effects for free. Timing restores AFTER Apply so a later Refresh still restarts
        // from the authored Duration (DD3).
        public static void Restore(StatusController controller, List<StatusStateDTO> statuses)
        {
            if (statuses == null) return;   // pre-PB.2 capture (JsonUtility ignores-missing) — nothing to replay
            foreach (var dto in statuses)
            {
                if (dto.statusType == null || !_factories.TryGetValue(dto.statusType, out var factory))
                {
                    Debug.LogWarning($"[StatusEffectRegistry] Unknown statusType '{dto.statusType}' — " +
                        "entry skipped, remaining state restores normally.");
                    IT.Boot.SessionInfo.RestoreDegraded = true;   // 4.6.1 R5 (OQ-E/#31): warn text above BYTE-IDENTICAL
                    continue;
                }
                var effect = factory(dto.instanceState);
                controller.Apply(effect);
                effect.RestoreTiming(dto.elapsed, dto.tickAccumulator);
            }
        }
    }
}
