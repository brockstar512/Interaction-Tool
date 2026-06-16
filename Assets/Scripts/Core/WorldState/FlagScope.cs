namespace IT.Core.WorldState
{
    public enum FlagScope
    {
        // Persists across segment re-entry AND across save/load.
        // Examples: opened chests, defeated story bosses, permanently unlocked doors.
        Permanent,

        // Persists across segment re-entry within a play session; resets on save/load.
        // Examples: non-chest pickups, one-time NPC conversation triggers.
        SessionOnly,

        // Resets when the player re-enters the owning segment (point-in-rect re-entry).
        // Examples: respawning enemies, regrown grass, refilled pots.
        // TODO (Epic 5): SegmentManager.OnSegmentReEntered → WorldState.OnSegmentReEntered.
        SegmentScoped,

        // Resets on scene unload (scene-backed segment transitions).
        // Examples: anything tied to a specific Unity scene load.
        // TODO (Epic 5): SceneManager.sceneUnloaded → WorldState.OnSceneUnloaded.
        SceneScoped,
    }
}
