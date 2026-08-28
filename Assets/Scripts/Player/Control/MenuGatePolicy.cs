using UnityEngine.InputSystem;

namespace IT.Player.Control
{
    // 4.6.2 R3 (DQ-6 + register item 10; DD3/DD6-ii ruled): the menu-era join
    // gate. Unpaired-device input at any menu is JOIN FLOW, never navigation —
    // and v1 menus host no join UI, so the flow is a LOUD reject (the roster's
    // policy-locked warn; OQ-F's toast rides it at R5). REFERENCE-DISTINCT from
    // LockedJoinPolicy.Instance BY DESIGN: OnSceneBoundary's reopen (DD6-ii)
    // keys on the boot instance's identity, so a menu-set gate survives a scene
    // boundary — the menu owns its own restore (GATE DEFAULT: Open on close).
    public sealed class MenuGatePolicy : IJoinPolicy
    {
        public static readonly MenuGatePolicy Instance = new MenuGatePolicy();
        public bool AllowJoin(InputDevice device) => false;
    }
}
