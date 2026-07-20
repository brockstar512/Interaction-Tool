namespace IT.Items
{
    // Story PB.3 (C-H's named item contract, spec DD1) — the ONE shape per-item runtime
    // state crosses a boundary in. Restore-on-instance: the machinery instantiates the
    // item's prefab and hands the blob back to the LIVE instance.
    //
    // Implemented by RUNTIME-STATEFUL items ONLY. Stateless items implement nothing and
    // stay zero-diff seals — notably KeyItem, whose variant is prefab-encoded
    // (KeyItem.prefab keyType:1 vs MasterKeyItem.prefab keyType:2), so key support costs
    // zero edits to the key (DD9).
    //
    // RestoreState owns the ENTIRE meaning of restore for an item, INCLUDING whether to
    // reactivate an ongoing effect (DD7 / ruling B3). There is deliberately no
    // WasActiveAtCapture and no ReapplyEffect: reconstitution is the item's own business,
    // and an externally-visible flag would be a second source of truth that could
    // disagree with the item's internal guard.
    //
    // Item KEYS do NOT come from this interface — they come from ItemStateRegistry's
    // resolver map (DD2), because KeyItem and MasterKeyItem share typeof(KeyItem) yet
    // need distinct keys, which a self-keying property could not express.
    public interface ISerializableItem
    {
        // False = nothing to serialize. The registry writes an empty blob and never
        // calls CaptureState.
        bool IsStateful { get; }

        // This item's params as JSON. Owned by the item — the machinery never parses it.
        string CaptureState();

        // Apply a captured blob to THIS live instance, and decide whether to reactivate
        // any ongoing effect (DD7 / B3). An empty or malformed blob must fail alive.
        void RestoreState(string state);
    }
}
