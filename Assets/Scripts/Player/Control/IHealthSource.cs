namespace IT.Player.Control
{
    // Read-only health view exposed by an IPlayerController (architecture D3).
    // Story 3.1 ships only a stub (OnFootController.NullHealthSource); the real
    // implementation arrives with the Health component in Story 4.1.
    public interface IHealthSource
    {
        int Current { get; }
        int Max { get; }
        event System.Action<int, int> HealthChanged; // (current, max)
    }
}
