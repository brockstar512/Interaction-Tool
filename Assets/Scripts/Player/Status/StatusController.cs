using System.Collections.Generic;
using UnityEngine;
using IT.Core.Combat;

namespace IT.Player.Status
{
    using IT.Player.Control;

    // Per-player status-effect manager (Story 4.3, FR-15). Lives on the Player ROOT
    // alongside Health and PlayerWrapper (C-C: "on the wrapper"). Owns the active-effect
    // list and advances it once per frame — but deliberately has NO Update of its own:
    // PlayerWrapper.Update drives Tick(dt) in the FR-10 status -> health -> controller
    // order, gated by WrapperState.Active so Suspend pauses all status timing (no reliance
    // on Script Execution Order).
    public class StatusController : MonoBehaviour
    {
        readonly List<StatusEffectBase> _active = new();

        // Targets that effects reach through StatusEffectBase.Controller. Cached in Awake;
        // both live on the Player root with this component. (Story 4.4 Poison reads Health;
        // On-Fire reads Wrapper for the possession-path controller swap.)
        public Health Health { get; private set; }
        public PlayerWrapper Wrapper { get; private set; }

        void Awake()
        {
            Health  = GetComponent<Health>();
            Wrapper = GetComponent<PlayerWrapper>();
        }

        // Apply a FRESHLY-CONSTRUCTED effect instance. The contract is one new instance per
        // Apply (the instance owns its countdown state) — callers write e.g.
        //   statusController.Apply(new PoisonEffect(damagePerTick: 1, duration: 5f, tickInterval: 1f));
        //
        // Two reapply paths collapse to a refresh in v1:
        //  (1) Caller-bug guard (per owner note, Step 1 review): if the EXACT SAME instance
        //      is already active, refresh it instead of adding it twice — otherwise it would
        //      tick from two list positions and double its effect. We log a warning so the
        //      caller bug surfaces. This is a distinct, cheap check from (2): a subclass that
        //      overrode StackKey to something instance-specific could slip past key-matching,
        //      but never past the reference check.
        //  (2) Refresh-by-key: an effect with the same StackKey (default = runtime type) is
        //      already active -> restart THAT instance and discard the new one (so On-Fire
        //      won't re-run OnApply / re-swap the controller on a refresh).
        // StackPolicy.Stack is RESERVED — not implemented; everything refreshes in v1.
        public void Apply(StatusEffectBase effect)
        {
            if (effect == null) return;

            if (_active.Contains(effect))   // (1) same instance handed in twice — caller bug
            {
                Debug.LogWarning($"[StatusController] Same {effect.GetType().Name} instance " +
                    "applied twice — refreshing instead of duplicating. Construct a fresh " +
                    "instance per Apply.");
                effect.Refresh();
                return;
            }

            var existing = FindByKey(effect.StackKey);   // (2) same kind already active
            if (existing != null)
            {
                existing.Refresh();
                return;
            }

            effect.Bind(this);
            _active.Add(effect);
            effect.OnApply();
        }

        // Per-frame advance, called by PlayerWrapper.Update only while the wrapper is Active
        // (Suspend pauses status timing). Iterates backwards so an effect that expires this
        // frame can be removed in place before OnExpire runs.
        public void Tick(float dt)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var effect = _active[i];
                if (effect.Advance(dt))
                {
                    _active.RemoveAt(i);
                    effect.OnExpire();
                }
            }
        }

        StatusEffectBase FindByKey(object key)
        {
            for (int i = 0; i < _active.Count; i++)
                if (Equals(_active[i].StackKey, key))
                    return _active[i];
            return null;
        }
    }
}
