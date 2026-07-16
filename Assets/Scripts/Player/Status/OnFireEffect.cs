using System;
using UnityEngine;
using IT.Player.Control;
using IT.Player.StateMachine;

namespace IT.Player.Status
{
    // On-Fire (Story 4.4 Step 4) — a mode-changing status. While active it swaps the player to
    // OnFireController (panic-run, can't stop) via the wrapper's generic controller-swap path,
    // and on expiry restores the previous controller. No DoT and no controller touch beyond the
    // swap (that's Poison's job). On-foot-only in v1 (Q5): refused while possessing a vehicle.
    //
    // No visual tint in v1 (Q1 deferred): the "burning" cue is purely the can't-stop panic-run +
    // 2x speed. CharacterFlash only does an alpha blink (not a color), and that blink collides
    // with the damage flash (last StartFlash wins) — a real sustained tint is a future visual pass.
    public sealed class OnFireEffect : StatusEffectBase
    {
        readonly float _speedMultiplier;

        public OnFireEffect(float duration = 5f, float speedMultiplier = 2.0f, bool indefinite = false)
        {
            Duration = duration;
            TickInterval = 0f;          // no periodic tick — the controller swap IS the effect
            _speedMultiplier = speedMultiplier;
            IsIndefinite = indefinite;  // PB.2 — reconstruction is the only v1 caller that passes true
        }

        protected internal override void OnApply()
        {
            // On-foot-only (Q5): refuse while possessing a vehicle. SOFT refusal — the effect
            // stays in StatusController._active and ticks no-op for its Duration, then OnExpire
            // no-ops (nothing was swapped). Documented v1 behavior; a hard abort-on-apply would
            // need new StatusController API, deferred until a second status needs it.
            if (Controller.Wrapper.IsPossessingVehicle)
            {
                Debug.LogWarning("[OnFireEffect] Ignored — On-Fire is on-foot-only in v1; player " +
                    "is possessing a vehicle. Effect expires harmlessly.");
                return;
            }

            // Seed the panic-run in the player's current facing (Q2) so a burning player keeps
            // heading the way they were looking rather than snapping to Down. Zero facing (player
            // never moved) falls back to Down inside the OnFireController constructor.
            Vector2 facing = Controller.GetComponent<PlayerStateMachine>().LookDirection;

            Controller.Wrapper.SwapController(
                new OnFireController(speedMultiplier: _speedMultiplier, initialDirection: facing));
        }

        protected internal override void OnExpire()
        {
            // Restore the controller active before the swap. No-op if nothing was swapped (the
            // vehicle-refusal path above, or a guard-refused swap).
            Controller.Wrapper.RestoreController();
        }

        // --- PB.2 serialization (blob side, spec DD3/DD4). Restore reconstructs through
        // the ctor and replays via StatusController.Apply — so a restored On-Fire re-swaps
        // the controller through OnApply, and C-D holds: no controller-mode field exists,
        // the panic-run is re-DERIVED from the effect (spec DD2). ---
        [Serializable]
        struct Params
        {
            public float duration;
            public float speedMultiplier;
            public bool indefinite;
        }

        protected internal override string CaptureState() => JsonUtility.ToJson(new Params
        {
            duration = Duration,
            speedMultiplier = _speedMultiplier,
            indefinite = IsIndefinite,
        });

        internal static OnFireEffect FromState(string instanceState)
        {
            var p = JsonUtility.FromJson<Params>(instanceState);
            return new OnFireEffect(p.duration, p.speedMultiplier, p.indefinite);
        }
    }
}
