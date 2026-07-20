using System;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Items
{
    using IT.Core.Utilities;
    using IT.Items.Bell;
    using IT.Items.Candle;
    using IT.Items.Weapons;
    using IT.Player.Inventory;
    using IT.Player.Persistence;
    // NOTE: IT.Items.GrapplingHook is deliberately NOT imported — the namespace and the
    // class share the name "GrapplingHook", so an unqualified reference is ambiguous.
    // The one usage below is fully qualified instead.

    // Story PB.3 — the item <-> ItemStateDTO translator, both directions. Plain static
    // class (C-B: no ScriptableObject; C-C: no runtime state, nothing to boot; no
    // reflection). Mirrors StatusEffectRegistry's seam discipline, with two deliberate
    // divergences forced by the item model:
    //
    //   1. KEYS ARE RESOLVED PER-INSTANCE, NOT PER-TYPE (DD2). A flat Type->string map is
    //      impossible here: KeyItem.prefab (keyType:1) and MasterKeyItem.prefab (keyType:2)
    //      share typeof(KeyItem) but must round-trip as distinct keys, so KeyItem's
    //      resolver reads the instance's prefab-authored keyType. Every other item's
    //      resolver is a constant.
    //   2. RESTORE INSTANTIATES A PREFAB, IT DOES NOT CTOR-CONSTRUCT (DD2/DD8). Items are
    //      MonoBehaviours with authored prefab wiring, so the prefab comes from an injected
    //      IItemPrefabProvider. This registry never holds an asset reference.
    //
    // KEYS ARE DURABLE IDs (OQ-PB2-B's discipline, inherited): short stable strings authored
    // exactly once. They must survive the v1 refactor rename pass WITHOUT breaking save-file
    // compatibility — never derive a key from a type name.
    //
    // Registration is part of an item's definition of done: an unregistered item warns and
    // is skipped at capture rather than silently vanishing from the save.
    public static class ItemStateRegistry
    {
        // Live item -> durable key. Function-valued so KeyItem can key off its instance.
        static readonly Dictionary<Type, Func<IItem, string>> _keyResolvers = new()
        {
            { typeof(CandleItem),    _ => "candle" },
            { typeof(KeyItem),       i => ((KeyItem)i).keyType == GameUtilities.KeyTypes.MasterKey ? "masterkey" : "key" },
            { typeof(BellItem),      _ => "bell" },
            { typeof(PlankItem),     _ => "plank" },
            { typeof(SwordItem),     _ => "sword" },
            { typeof(WhipItem),      _ => "whip" },
            { typeof(IT.Items.GrapplingHook.GrapplingHook), _ => "grappling" },
        };

        // Snapshot the inventory in slot order. Envelope (itemType) written here by the
        // machinery; blob (instanceState) owned by the item via CaptureState().
        public static List<ItemStateDTO> Capture(PlayerInventory inventory)
        {
            var list = new List<ItemStateDTO>();
            // Items is null until PlayerInventory.Awake runs — capture before that is a
            // caller error, but it must not throw.
            if (inventory == null || inventory.Items == null) return list;

            foreach (var item in inventory.Items)
            {
                if (item == null) continue;

                if (!_keyResolvers.TryGetValue(item.GetType(), out var resolveKey))
                {
                    Debug.LogWarning($"[ItemStateRegistry] Item '{item.GetType().Name}' has no registration — " +
                        "not captured. Register it in _keyResolvers, or mark it throwaway.");
                    continue;
                }

                var stateful = item as ISerializableItem;
                list.Add(new ItemStateDTO
                {
                    itemType = resolveKey(item),
                    instanceState = (stateful != null && stateful.IsStateful) ? stateful.CaptureState() : "",
                });
            }
            return list;
        }

        // Rebuild the inventory onto a (freshly instantiated) player: prefab from the
        // provider, Instantiate, hand to the inventory, THEN restore state — so an item
        // that reactivates an ongoing effect (DD7/B3) does so already parented to the
        // player rather than sitting at the world origin.
        public static void Restore(PlayerInventory inventory, List<ItemStateDTO> items, IItemPrefabProvider prefabs)
        {
            if (inventory == null) return;
            if (items == null) return;   // pre-PB.3 capture (JsonUtility ignores-missing) — nothing to rebuild

            if (prefabs == null)
            {
                Debug.LogWarning("[ItemStateRegistry] No IItemPrefabProvider supplied — " +
                    $"{items.Count} item(s) skipped; the rest of the restore proceeds.");
                return;
            }

            foreach (var dto in items)
            {
                var prefab = prefabs.GetPrefabForKey(dto.itemType);
                if (prefab == null)
                {
                    Debug.LogWarning($"[ItemStateRegistry] Unknown itemType '{dto.itemType}' — " +
                        "entry skipped, remaining state restores normally.");
                    continue;
                }

                var instance = UnityEngine.Object.Instantiate(prefab);
                var item = instance.GetComponent<IItem>();
                if (item == null)
                {
                    Debug.LogWarning($"[ItemStateRegistry] Prefab for '{dto.itemType}' has no IItem component — " +
                        "entry skipped.");
                    UnityEngine.Object.Destroy(instance);
                    continue;
                }

                inventory.PickUpItem(item);
                (item as ISerializableItem)?.RestoreState(dto.instanceState);
            }
        }
    }
}
