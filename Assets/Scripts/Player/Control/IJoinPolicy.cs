using UnityEngine.InputSystem;

namespace IT.Player.Control
{
    // PB.4.5 R3.1 (owner-ruled): swappable join gate held by PlayerRoster — the
    // IPlayerController idiom, and the Epic 4.6 injectable-module posture (locked
    // Constraint 1). Gates JOIN only: suspended re-pair is checked BEFORE the policy in
    // RouteUnpairedActivity, so reconnect-priority (§5.F) survives any policy — a pad
    // hiccup during a locked transition must never lock a live player out. v1 ships
    // Open + Locked; MenuGatePolicy and FirstInputAssignsP1Policy land at Epic 4.6
    // (spec Epic 4.6 handoff) without touching the roster. Policies needing roster
    // state take it at construction — no fat signature preemptively (owner ruling).
    public interface IJoinPolicy
    {
        bool AllowJoin(InputDevice device);
    }

    // Press-any-button drop-in — R2.1's behavior, now named. Stateless → shared instance.
    public sealed class OpenJoinPolicy : IJoinPolicy
    {
        public static readonly OpenJoinPolicy Instance = new OpenJoinPolicy();
        public bool AllowJoin(InputDevice device) => true;
    }

    // Always reject: scene transitions today (GameBootstrap wires it); menus, verses
    // in-match, and single-player-after-P1 when their drivers exist (Epic 4.6).
    public sealed class LockedJoinPolicy : IJoinPolicy
    {
        public static readonly LockedJoinPolicy Instance = new LockedJoinPolicy();
        public bool AllowJoin(InputDevice device) => false;
    }
}
