using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.Persistence
{
    // Story 5.4 R2 (DD3 ruled — the transition-scoped carry set): PRESENT players' DTOs
    // crossing a Location boundary, keyed by slot. Captured pre-load, consumed post-load
    // by the arriving wrapper's carry arm (PlayerWrapper.Start). It NEVER touches
    // PlayerRoster._heldByDevice — PB.4.5 §5.D's "discard at transition" governs that held
    // map (LEFT players) and runs unchanged; this set governs PRESENT players (I-5). Two
    // rulings, two populations. In-memory only: never serialized (A-3 untouched).
    //
    // Production writer = LocationTransporter (R3), which owns the lifecycle and clears
    // leftovers at the end of every arrival, success or failure. Throwaway writer = the
    // PB1 harness Step-1 round-trip (R2). Survivor audit (4.6.2 pad item 1, "no unaudited
    // survivors"): each entry is STAMPED with its target scene and only consumable in that
    // scene — a stale entry (failed load, Boot re-entry) can never be applied elsewhere; it
    // is discarded loudly on the next lookup or Clear. Deliberately NOT in
    // PlayerRoster.ResetSession (that boundary is pinned to the roster's three structures).
    public static class LocationEntryCarry
    {
        struct Entry
        {
            public PlayerStateDTO dto;
            public string targetScene;
        }

        static readonly Dictionary<string, Entry> _bySlot = new();

        public static int Count => _bySlot.Count;

        public static void Stash(string slot, in PlayerStateDTO dto, string targetScene)
        {
            if (string.IsNullOrEmpty(slot))
            {
                Debug.LogWarning("[LocationEntry] carry refused — empty slot id (a device-less/unregistered wrapper cannot be carried).");
                return;
            }
            _bySlot[slot] = new Entry { dto = dto, targetScene = targetScene };
            Debug.Log($"[LocationEntry] carry stashed for {slot} → '{targetScene}'");
        }

        // Consumed (removed) on read. A scene-stamp mismatch is a STALE carry: discarded
        // loudly, never applied (returns false so the caller falls through to its next arm).
        public static bool TryConsume(string slot, string activeScene, out PlayerStateDTO dto)
        {
            dto = default;
            if (string.IsNullOrEmpty(slot) || !_bySlot.TryGetValue(slot, out var entry)) return false;
            _bySlot.Remove(slot);
            if (entry.targetScene != activeScene)
            {
                Debug.LogWarning($"[LocationEntry] stale carry for {slot} discarded — stamped '{entry.targetScene}', arrived '{activeScene}'.");
                return false;
            }
            dto = entry.dto;
            return true;
        }

        // The owner's end-of-arrival sweep (success OR failure). Loud when anything was
        // left unconsumed — a carried player that never arrived is a real defect.
        public static void Clear(string reason)
        {
            if (_bySlot.Count == 0) return;
            Debug.LogWarning($"[LocationEntry] {_bySlot.Count} unconsumed carry entr{(_bySlot.Count == 1 ? "y" : "ies")} cleared ({reason}): {string.Join(", ", _bySlot.Keys)}");
            _bySlot.Clear();
        }
    }
}
