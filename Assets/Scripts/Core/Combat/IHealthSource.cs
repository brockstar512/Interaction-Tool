namespace IT.Core.Combat
{
    // Read-only health view exposed by an IPlayerController (architecture D3).
    // Moved from IT.Player.Control to IT.Core.Combat in Story 4.1 (OQ-4.1-A)
    // so Health : IHealthSource doesn't create a Core→Player dependency inversion.
    public interface IHealthSource
    {
        int Current { get; }
        int Max { get; }
        event System.Action<int, int> HealthChanged; // (current, max)
    }
}
