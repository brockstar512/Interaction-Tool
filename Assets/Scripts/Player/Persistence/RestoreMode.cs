namespace IT.Player.Persistence
{
    // Story PB.1 (spec DD5 — planning §4 Option A): ONE DTO schema, TWO restore policies.
    // Transition — honor dto.wrapperState (a mid-unplug P2 arrives Suspended, waits for its device).
    // Load — normalize to Active (a canvas/device-loss Suspend must never persist a fresh load).
    public enum RestoreMode
    {
        Transition,
        Load,
    }
}
