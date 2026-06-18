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
    }
}
