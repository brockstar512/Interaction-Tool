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

        // PB.2: capture-side read surface for StatusEffectRegistry. internal + read-only —
        // the list itself stays private, and the only mutation paths remain Apply/ClearAll
        // (the Cure path lands at R3).
        internal IReadOnlyList<StatusEffectBase> Active => _active;

        // Reusable snapshot buffer for Tick (Step 4.5): lets Tick iterate without the live
        // _active list being mutated under it. Member field to avoid a per-frame allocation.
        readonly List<StatusEffectBase> _tickBuffer = new();

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
        // (Suspend pauses status timing). Iterates a SNAPSHOT (not the live list) because an
        // effect's Advance/OnTick may mutate _active mid-iteration — e.g. a poison tick that
        // drops HP to 0 -> death -> ClearAll, or an effect applying another effect. An in-place
        // index loop would then read past the end of a shrunk list. The Contains guard skips
        // anything removed mid-tick; the Remove return-value gates OnExpire so an effect cleared
        // during its own Advance is not expired twice.
        public void Tick(float dt)
        {
            _tickBuffer.Clear();
            _tickBuffer.AddRange(_active);
            foreach (var effect in _tickBuffer)
            {
                // Contains + Remove are each O(n); with n typically 1-3 in v1 the O(n^2) loop is
                // negligible. If the active count ever grows, add a HashSet membership sidecar or
                // a version counter instead.
                if (!_active.Contains(effect)) continue;            // removed mid-tick — skip
                if (effect.Advance(dt) && _active.Remove(effect))   // remove-first, then OnExpire
                    effect.OnExpire();
            }
        }

        // Force-clear all active effects, firing each one's OnExpire. Called on player death
        // (PlayerStatusManager.OnHealthDepleted) so a mode-changing status doesn't strand the
        // player — OnFireEffect.OnExpire -> RestoreController hands movement back to OnFoot so
        // PlayerDeathState actually stops the player. Remove EVERYTHING first (Clear), THEN fire
        // OnExpire on a snapshot, so _active is empty during every callback (re-entrancy-safe — a
        // callback can't observe a half-cleared list). Uses a LOCAL snapshot, not _tickBuffer,
        // because ClearAll can run from inside Tick (a poison tick that kills the player); reusing
        // the Tick buffer would corrupt its in-flight iteration. List.ToArray() (not LINQ).
        public void ClearAll()
        {
            if (_active.Count == 0) return;
            var snapshot = _active.ToArray();
            _active.Clear();
            foreach (var effect in snapshot)
                effect.OnExpire();
        }

        // PB.2 R3 (Directive 2 clear-condition 3): cure = FORCED EARLY EXPIRY of one
        // status. Targeted by StackKey — the same match rule as Apply's refresh path —
        // NOT by registry string key (the controller stays registry-blind; "poison" is a
        // serialization ID, not a runtime handle). Fires OnExpire (OQ-PB2-A: one hook —
        // water must un-burn, so On-Fire's controller restore runs on cure exactly as on
        // expiry). Remove-first, then fire, mirroring Tick's discipline. Returns false if
        // no matching status is active (callers own their own echo). NEVER clear-all —
        // ClearAll remains death's path only.
        public bool Cure(object stackKey)
        {
            var effect = FindByKey(stackKey);
            if (effect == null) return false;
            _active.Remove(effect);
            effect.OnExpire();
            return true;
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
