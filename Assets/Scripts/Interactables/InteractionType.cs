namespace IT.Interactables
{
    public enum InteractionType
    {
        None,
        Throw,
        Move,
        Pull,
        Slide,
        Equip,
        Open,
        Possess,
        // SAVE.3 R3: the "simple-use" interaction shape (quality-audit #26 taxonomy) —
        // press → effect → done; dispatched as a direct Interact() hand-off (the
        // Possess pattern), no player state switch. First consumer: SavePoint.
        // APPENDED — existing members keep their values.
        Use
    }
}
