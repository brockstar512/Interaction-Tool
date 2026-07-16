using System;
using UnityEngine;

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

        public PoisonEffect(int damagePerTick, float duration, float tickInterval, bool indefinite = false)
        {
            _damagePerTick = damagePerTick;
            Duration = duration;          // base setters are protected — reachable from subclass ctor
            TickInterval = tickInterval;
            IsIndefinite = indefinite;    // PB.2 — reconstruction is the only v1 caller that passes true
        }

        // First tick fires one interval after Apply (no immediate hit). Controller.Health is
        // bound by StatusController.Apply before any tick runs.
        protected internal override void OnTick() => Controller.Health.DamageOverTime(_damagePerTick);

        // --- PB.2 serialization (blob side, spec DD3/DD4). Fields stay readonly: restore
        // reconstructs through the ctor (FromState, registered in StatusEffectRegistry),
        // never mutates a live instance. Capture and parse live together so the blob
        // format has exactly one owner. ---
        [Serializable]
        struct Params
        {
            public int damagePerTick;
            public float duration;
            public float tickInterval;
            public bool indefinite;
        }

        protected internal override string CaptureState() => JsonUtility.ToJson(new Params
        {
            damagePerTick = _damagePerTick,
            duration = Duration,
            tickInterval = TickInterval,
            indefinite = IsIndefinite,
        });

        internal static PoisonEffect FromState(string instanceState)
        {
            var p = JsonUtility.FromJson<Params>(instanceState);
            return new PoisonEffect(p.damagePerTick, p.duration, p.tickInterval, p.indefinite);
        }
    }
}
