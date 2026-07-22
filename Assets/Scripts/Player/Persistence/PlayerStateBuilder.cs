using System.Collections.Generic;
using UnityEngine;
using IT.Core.Combat;
using IT.Items;
using IT.Player.Control;
using IT.Player.Inventory;
using IT.Player.Status;

namespace IT.Player.Persistence
{
    // Story PB.1 (C-H): the ONE translator between a live PlayerWrapper and PlayerStateDTO.
    // Static utility — plain C#, no Unity lifecycle, no state of its own (C-C: not a
    // singleton, nothing to boot). Reads sibling components off the wrapper's GameObject
    // (the router's composition-read precedent, 5.2/5.3); the wrapper never learns
    // serialization exists — its diff stays empty (V12b). Same assembly, so the internal
    // Suspend()/Resume() are reachable without any wrapper edit (DD2).
    public static class PlayerStateBuilder
    {
        // Snapshot the live player. Items and identity seams are inert here (PB.3 / PB.4
        // fill them). Capturing an already-dead player (health 0) is a caller error —
        // death flows through respawn seeding, not DTO round-trip (spec DD4).
        public static PlayerStateDTO Capture(PlayerWrapper player)
        {
            var health = player.GetComponent<Health>();
            var status = player.GetComponent<PlayerStatusManager>().playerStatus;
            var inventory = player.GetComponentInChildren<PlayerInventory>();
            return new PlayerStateDTO
            {
                playerId = player.PlayerId ?? "",   // PB.4 (DD5) — stable slot string ("P1"/"P2"), assigned at registration
                deviceId = "",                      // PB.4 — stays empty until the OQ-PB4-E deviceId spike (R5); a provisional runtime value would risk a save persisting garbage that looks real
                wrapperState = player.State,
                currentHealth = health.Current,
                lives = status.CurrentLives,
                // PB.3: per-item runtime state crosses as explicit data (C-H). The registry
                // owns the mapping; the held slot is inventory state, not roster state (DD5).
                items = ItemStateRegistry.Capture(inventory),
                currentItemIndex = inventory != null ? inventory.CurrentIndex : 0,
                // PB.2 (Directive 2): active statuses cross as explicit data — the one
                // C-H transient promoted to persistent state. Registry owns the mapping.
                activeStatuses = StatusEffectRegistry.Capture(player.GetComponent<StatusController>()),
            };
        }

        // Write a snapshot onto a live (freshly instantiated) player.
        // CONTRACT: call AFTER the target's Start() — Health.Start() sets current = max
        // and would silently clobber an earlier restore (spec DD4; the harness restores a
        // frame after Instantiate, 5.4's transporter restores post-load).
        public static void Restore(in PlayerStateDTO dto, PlayerWrapper player, RestoreMode mode,
                                   IItemPrefabProvider itemPrefabs = null)
        {
            player.GetComponent<Health>().RestoreCurrent(dto.currentHealth);
            player.GetComponent<PlayerStatusManager>().playerStatus.RestoreLives(dto.lives);

            // Statuses replay AFTER health/lives (a first post-restore poison tick must hit
            // RESTORED health, and one that immediately depletes it must find the death path
            // wired) and BEFORE the wrapperState policy (a to-be-Suspended wrapper gets its
            // effects back frozen, accumulators intact — Tick is gated by WrapperState.Active).
            // Runs under BOTH modes: Directive 2 says ALL boundaries; the RestoreMode split
            // below stays wrapperState-only (PB.2 spec DD5).
            StatusEffectRegistry.Restore(player.GetComponent<StatusController>(), dto.activeStatuses);

            // Items restore AFTER statuses and BEFORE the wrapperState policy (spec DD6).
            // Prefabs come from the caller via IItemPrefabProvider (DD8) — the registry is
            // static and holds no asset references. An item that reactivates an ongoing
            // effect (DD7/B3) does so item-side here; its own clock is WrapperState-gated,
            // so restoring into a to-be-Suspended wrapper leaves the effect frozen rather
            // than ticking, matching how a restored status behaves.
            var inventory = player.GetComponentInChildren<PlayerInventory>();
            ItemStateRegistry.Restore(inventory, dto.items, itemPrefabs);
            if (inventory != null) inventory.RestoreCurrentIndex(dto.currentItemIndex);

            // wrapperState policy (DD5): Load normalizes to Active; Transition honors the
            // capture. Dead is not a restorable state (death flows through respawn seeding,
            // DD4) — normalize to Active with a warning, mirroring the fail-alive posture.
            var target = mode == RestoreMode.Load ? WrapperState.Active : dto.wrapperState;
            if (target == WrapperState.Dead)
            {
                Debug.LogWarning("[PlayerStateBuilder] dto.wrapperState == Dead is not restorable — normalizing to Active (fail-alive)");
                target = WrapperState.Active;
            }
            if (target == WrapperState.Suspended) player.Suspend();
            else                                  player.Resume();   // idempotent — a fresh wrapper is already Active
        }
    }
}
