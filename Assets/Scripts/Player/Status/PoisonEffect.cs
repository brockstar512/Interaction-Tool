namespace IT.Player.Status
{
    // Poison (Story 4.4, AC1) — ticks damage through Health on a fixed cadence and NEVER
    // touches the controller. Plain StatusEffectBase subclass (C-B). Re-applying refreshes
    // the duration via the default StackKey (GetType()).
    //
    // Routes through Health.DamageOverTime (bypasses i-frames): a 1s poison cadence would
    // otherwise be eaten by the test scene's 1.0s i-frame window, and any recent bomb hit
    // would suppress the next tick. DoT is independent of the discrete-hit debounce.
    public sealed class PoisonEffect : StatusEffectBase
    {
        readonly int _damagePerTick;

        public PoisonEffect(int damagePerTick, float duration, float tickInterval)
        {
            _damagePerTick = damagePerTick;
            Duration = duration;          // base setters are protected — reachable from subclass ctor
            TickInterval = tickInterval;
        }

        // First tick fires one interval after Apply (no immediate hit). Controller.Health is
        // bound by StatusController.Apply before any tick runs.
        protected internal override void OnTick() => Controller.Health.DamageOverTime(_damagePerTick);
    }
}
