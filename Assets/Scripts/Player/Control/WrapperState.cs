namespace IT.Player.Control
{
    // Lifecycle state of a PlayerWrapper's active controller.
    // Story 3.1 only ever sets Active; Suspended/Dead are wired in later possession stories
    // (3.4 vehicle possession, 4.2 death) — declared now so the contract is stable.
    public enum WrapperState
    {
        Active,
        Suspended,
        Dead,
        // 4.6.2 R2 (DD2, owner-accepted mechanism): pause is a DISTINCT state
        // from device-loss Suspended — the roster's reconnect-priority scan
        // matches Suspended EXACTLY, so a Paused wrapper is never a re-pair
        // candidate BY CONSTRUCTION, and a device-less wrapper waiting for its
        // first input (DD4) keeps Suspended's meaning ("no device").
        Paused,
    }
}
