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

namespace IT.Player.Persistence
{
    // Story 5.4 R2 (OQ-2(c) + S-1 ruled): which route reached the ONE Location-entry call
    // site (PlayerStateBuilder.RestoreForLocationEntry). BootLoad = first launch / fresh
    // join / SAVE.1 boot-load (the only kind whose caller may consume the mercy flag).
    // Transition = 5.4's carry (never consumes mercy; honors a captured Suspended state).
    public enum LocationEntryKind
    {
        BootLoad,
        Transition,
    }
}
