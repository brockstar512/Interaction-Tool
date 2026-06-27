using UnityEngine;

namespace IT.Player.Status
{
    // DEBUG / THROWAWAY (Story 4.3 Step 4 — remove with the J/P keys in PlayerStatusManager
    // before shipping). Minimal StatusEffectBase that logs every lifecycle hook so the
    // apply/tick/expire/refresh path + simultaneous-effect behavior can be eyeballed in the
    // Console (AC #5). StackKey is the label, so two instances with DIFFERENT labels coexist
    // and tick at once; re-applying the SAME label refreshes it.
    public sealed class DebugLogStatusEffect : StatusEffectBase
    {
        readonly string _label;
        int _tickCount;

        public DebugLogStatusEffect(string label, float duration, float tickInterval)
        {
            _label = label;
            Duration = duration;          // base setters are protected — reachable from subclass ctor
            TickInterval = tickInterval;
        }

        public override object StackKey => _label;   // label, not GetType() — lets two coexist

        protected internal override void OnApply()
            => Debug.Log($"[DebugStatus:{_label}] OnApply (duration={Duration}s, interval={TickInterval}s)");

        protected internal override void OnTick()
            => Debug.Log($"[DebugStatus:{_label}] OnTick #{++_tickCount}");

        protected internal override void OnExpire()
            => Debug.Log($"[DebugStatus:{_label}] OnExpire (after {_tickCount} ticks)");

        protected internal override void OnRefresh()
        {
            _tickCount = 0;
            Debug.Log($"[DebugStatus:{_label}] OnRefresh — duration restarted");
        }
    }
}
