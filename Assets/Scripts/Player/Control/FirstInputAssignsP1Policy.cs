using UnityEngine.InputSystem;

namespace IT.Player.Control
{
    // 4.6.2 R3 (OQ-B(ii) ruled + DD4 accepted): the v1 primary-pairing posture.
    // Deliberately NOT an IJoinPolicy — GATE DEFAULT (owner-stated) keeps
    // OpenJoinPolicy as the gameplay JOIN gate; this governs PRIMARY AUTO-PAIR at
    // spawn only. Active = the scene-placed P1 wrapper does NOT auto-pair: it
    // waits device-less in Suspended posture, and the FIRST meaningful input from
    // ANY device seats it through the roster's policy-exempt reconnect-priority
    // path (RouteUnpairedActivity). Gap 1 — the phantom keyboard-P1 that made a
    // solo pad player's state never save (PB.4.5 §11.1 item 1) — dies at the root.
    //
    // R4 extends ConsultAutoPair with the surviving "P1 Input Device" preference
    // (OQ-D(i)): an explicit player choice auto-pairs that device class instead.
    public static class FirstInputAssignsP1Policy
    {
        public static bool Active = true;   // the ruled v1 default

        // Consulted by PlayerWrapper.Awake's spawn fallback (the DD4 cited line).
        // null = wait for first input. With Active false this returns the exact
        // pre-4.6.2 fallback (keyboard, else first pad) — the rollback lever.
        // R4 (OQ-D(i)): the surviving "P1 Input Device" preference OVERRIDES —
        // an explicit choice auto-pairs that device class on later boots (B-12's
        // survives-a-Continue evidence). Falls through if the device is absent.
        public static InputDevice ConsultAutoPair()
        {
            var pref = IT.Boot.SessionInfo.PreferredPrimaryDevice;
            if (pref == "keyboard" && Keyboard.current != null) return Keyboard.current;
            if (pref == "gamepad" && Gamepad.current != null) return Gamepad.current;
            return Active ? null : ((InputDevice)Keyboard.current ?? Gamepad.current);
        }
    }
}
