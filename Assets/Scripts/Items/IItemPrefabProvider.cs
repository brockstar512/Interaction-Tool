using UnityEngine;

namespace IT.Items
{
    // Story PB.3 DD8 — the key -> prefab bridge, deliberately SEPARATE from
    // ItemStateRegistry so the registry stays static and stateless (C-C) and never holds
    // Unity asset references.
    //
    // This is PB.3's one real divergence from PB.2 (DD2): a StatusEffect is reconstructed
    // through a ctor, but an item is a prefab-instantiated MonoBehaviour, so restore needs
    // a prefab from somewhere. Whoever drives a restore supplies it:
    //   * R3  — PB1Harness implements this with a [SerializeField] key->prefab table,
    //           mirroring SpawnItemOnOpen's serialized-prefab convention. (The project has
    //           NO Resources.Load and NO Addressables anywhere, so a string->asset loader
    //           would introduce a pattern nothing else uses.)
    //   * Later — a production catalog (PB.5 LocationConfig or beyond) implements this SAME
    //           interface with zero change to the registry, the DTO, or the Builder.
    public interface IItemPrefabProvider
    {
        // Return null for an unknown key: the registry warns and skips that entry, and the
        // rest of the inventory restores normally (fail-alive; full corruption posture is
        // SAVE.2's).
        GameObject GetPrefabForKey(string key);
    }
}
