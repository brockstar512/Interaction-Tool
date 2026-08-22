using UnityEngine;

namespace IT.Presentation
{
    // 4.6.1 (DD1, DQ-1(c) ruled): the presentation COORDINATOR — typed handles,
    // REGISTRATION ONLY (constraint 1: routes to modules, never sees internals;
    // items and lifecycle owners talk to handles, modules implement behavior).
    // Hosted by SystemsRoot.Create (the SpawnManager shape, C-C) and reached via
    // SystemsRoot.Instance?.Presentation. A NULL handle is a named no-op at each
    // caller, never a throw (a scene without a PresentationRoot simply has no
    // screen surface — the LevelConfig warn-once posture).
    public class Presentation : MonoBehaviour
    {
        public IScreenPromptModule Screen { get; private set; }
        public IWorldPromptModule  World  { get; private set; }   // registers at 4.6.4
        public IHudModule          Hud    { get; private set; }   // registers at 4.6.3
        public IScreenFxModule     Fx     { get; private set; }   // registers at 4.6.4

        // 4.6.1 R6.1 (Session A defect): the ensure hook is NO LONGER armed from
        // Awake — Awake runs DURING SystemsRoot.Create's AddComponent, BEFORE the
        // Presentation property is assigned, so the immediately-ensured root's
        // module registered against a null coordinator (the "no coordinator"
        // orphan). SystemsRoot.Create now arms the hook AFTER the assignment.

        public void Register(IScreenPromptModule module)   { Screen = module; }
        public void Deregister(IScreenPromptModule module) { if (ReferenceEquals(Screen, module)) Screen = null; }
        public void Register(IWorldPromptModule module)    { World = module; }
        public void Deregister(IWorldPromptModule module)  { if (ReferenceEquals(World, module)) World = null; }
        public void Register(IHudModule module)            { Hud = module; }
        public void Deregister(IHudModule module)          { if (ReferenceEquals(Hud, module)) Hud = null; }
        public void Register(IScreenFxModule module)       { Fx = module; }
        public void Deregister(IScreenFxModule module)     { if (ReferenceEquals(Fx, module)) Fx = null; }
    }
}
