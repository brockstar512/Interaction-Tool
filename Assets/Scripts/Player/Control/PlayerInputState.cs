using UnityEngine;

namespace IT.Player.Control
{
    // Immutable, per-frame snapshot of player intent (architecture D3).
    // Built by PlayerWrapper from the input bridge and passed by `in` to
    // IPlayerController.Tick — a readonly struct so there is no heap allocation
    // on the per-frame hot path.
    //
    // NOTE: the `init` accessors rely on the IsExternalInit compiler shim
    // (Assets/Scripts/Core/Compatibility/IsExternalInit.cs) because Unity's
    // .NET Standard 2.1 profile does not ship that type.
    public readonly struct PlayerInputState
    {
        public Vector2 Move { get; init; }
        public bool InteractPressed { get; init; }
        public bool InteractReleased { get; init; }
        public bool UsePressed { get; init; }
        public bool UseReleased { get; init; }
        public bool SwitchItemPressed { get; init; }
        public bool PausePressed { get; init; }   // no Pause action in PlayerControl.inputactions (OQ-3.2-D)
        public bool EjectPressed { get; init; }    // wired in Story 3.4
    }
}
