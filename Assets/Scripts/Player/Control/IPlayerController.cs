namespace IT.Player.Control
{
    // The thing a PlayerWrapper drives (architecture D3). Story 3.1 ships one
    // implementation, OnFootController (the existing on-foot player); vehicles
    // arrive in Story 3.4. The wrapper owns the tick order and calls these — Unity
    // never calls a controller directly.
    public interface IPlayerController
    {
        void OnPossess(PlayerWrapper wrapper);
        void OnRelease();
        void Tick(in PlayerInputState input);
        void FixedTick();
        IHealthSource HealthSource { get; }
    }
}
