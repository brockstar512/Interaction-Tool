namespace IT.Player.Status
{
    // Base for all timed status effects (Story 4.3). A plain C# class — NOT a
    // MonoBehaviour or ScriptableObject (C-B: no new SOs). Each application is its
    // OWN instance: a caller constructs a fresh effect and hands it to
    // StatusController.Apply. The effect owns its runtime countdown state, so a single
    // instance must never be applied twice.
    //
    // Timing model (C-G deviation — see the 4.3 as-built): the per-frame countdown is
    // driven SYNCHRONOUSLY by StatusController.Tick(dt), itself called from
    // PlayerWrapper.Update in the FR-10 status -> health -> controller order and gated by
    // WrapperState.Active (so Suspend pauses ticking). It is NOT an Awaitable loop.
    // The ARCHITECTURE.md §11 Awaitable+destroyCancellationToken convention is reserved
    // for discrete lifetime-bound side-effects an effect may trigger later (e.g. Story
    // 4.4 On-Fire's controller swap), not for this tick.
    public abstract class StatusEffectBase
    {
        public enum StackPolicy
        {
            Refresh,   // re-applying the same effect restarts its duration (v1 default)
            Stack,     // RESERVED — not implemented in v1; Apply treats it as Refresh
        }

        // --- authored timing (set by the subclass constructor) ---
        public float Duration { get; protected set; }      // total lifetime in seconds
        public float TickInterval { get; protected set; }   // seconds between OnTick; <= 0 = no periodic tick
        // PB.2 (Directive 2, OQ-PB2-D): an indefinite effect never self-expires — ticks
        // keep firing but Advance never reports completion; it ends ONLY via a cure or
        // death's ClearAll. No v1 content authors this; the mechanism ships regardless.
        public bool IsIndefinite { get; protected set; }
        public virtual StackPolicy Stacking => StackPolicy.Refresh;

        // Match key for Refresh-on-reapply. Defaults to the runtime type, so a second
        // Poison refreshes the first. Override to group/separate effects differently.
        public virtual object StackKey => GetType();

        // Set by StatusController.Apply so the hooks below can reach the target
        // (Controller.Health, Controller.Wrapper). Null until bound.
        protected StatusController Controller { get; private set; }

        // --- runtime countdown state (owned here; advanced by the controller) ---
        float _elapsed;
        float _tickAccumulator;

        internal void Bind(StatusController controller) => Controller = controller;

        // Advance the countdown by dt and fire any due OnTick(s). Returns true once the
        // effect has reached its Duration (the controller then calls OnExpire + removes it).
        // Called once per frame from StatusController.Tick — only while the wrapper is Active.
        internal bool Advance(float dt)
        {
            _elapsed += dt;
            if (TickInterval > 0f)
            {
                _tickAccumulator += dt;
                while (_tickAccumulator >= TickInterval)
                {
                    _tickAccumulator -= TickInterval;
                    OnTick();
                }
            }
            return !IsIndefinite && _elapsed >= Duration;
        }

        // --- PB.2 serialization surface (spec DD3). The base owns the runtime clocks, so
        // it alone reads/writes them — the envelope side of the envelope/blob split; every
        // status ever shipped gets its timing serialized for free. internal: only
        // StatusEffectRegistry speaks DTO. ---
        internal float Elapsed => _elapsed;
        internal float TickAccumulator => _tickAccumulator;

        // Restore runs AFTER StatusController.Apply (replay-not-resurrection, DD2) so the
        // instance was constructed with its AUTHORED Duration — a later Refresh-on-reapply
        // therefore restarts from the full authored window, not a leftover stump (DD3).
        internal void RestoreTiming(float elapsed, float tickAccumulator)
        {
            _elapsed = elapsed;
            _tickAccumulator = tickAccumulator;
        }

        // The blob side: subclass returns its authored params as JSON for
        // StatusStateDTO.instanceState (ISerializableItem-shaped per Directive 2 —
        // realized as a base virtual because all statuses share this base). Base
        // returns null: an effect that doesn't override cannot be reconstructed and
        // must stay unregistered (warn+skip at capture, spec DD6).
        protected internal virtual string CaptureState() => null;

        // Refresh-on-reapply (Refresh policy): restart the duration on the EXISTING
        // instance. OnApply is deliberately NOT re-run (so On-Fire won't re-swap the
        // controller on a refresh); subclasses override OnRefresh for custom behavior.
        internal void Refresh()
        {
            _elapsed = 0f;
            OnRefresh();
        }

        // --- lifecycle hooks (parameterless per the AC; reach the target via Controller).
        // protected internal so StatusController (same assembly) can invoke them while
        // subclasses can still override. ---
        protected internal virtual void OnApply() { }
        protected internal virtual void OnTick() { }
        protected internal virtual void OnExpire() { }
        protected internal virtual void OnRefresh() { }
    }
}
