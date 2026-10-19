using UnityEngine.InputSystem;

namespace IT.Player.Control
{
    // Story 5.4 R3 (DD5 ratified): the transition-era join gate. Set by LocationTransporter
    // on accept, restored to the prior policy at REVEAL (the transition owns its own
    // restore). REFERENCE-DISTINCT from LockedJoinPolicy.Instance BY DESIGN — the DD6-ii
    // identity rule: OnSceneBoundary reopens ONLY from boot's Locked instance, so this gate
    // SURVIVES the scene boundary the transition itself causes. Gates JOIN only; reconnect /
    // re-pair stay exempt (PB.4.5 R3.1 — the Suspended branch precedes the policy gate).
    public sealed class TransitionGatePolicy : IJoinPolicy
    {
        public static readonly TransitionGatePolicy Instance = new TransitionGatePolicy();
        public bool AllowJoin(InputDevice device) => false;
    }
}
