using UnityEngine;
using IT.Core.Combat;
using IT.Core.Utilities;

namespace IT.Player.Control
{
    // On-Fire mode (Story 4.4): a self-contained IPlayerController whose ONLY behavior is fast
    // directional running. No interact, no use-item, no item/state-machine — those modes simply
    // don't exist in this controller. Sibling to OnFootController (NOT a wrapper over its
    // PlayerStateMachine), matching the future Frozen/Swimming per-mode-controller pattern.
    //
    // The "burning" visual (sustained red tint) is owned by OnFireEffect (Step 4), not this
    // controller — OnFireController is movement + animation only.
    //
    // NOTE: facing→clip mapping is duplicated here from MoveAnimState to keep this controller
    // from touching Day-3-debugged animation code. When a third controller needs the same
    // mapping (FrozenController, SwimmingController), extract to a shared
    // PlayerMoveAnimator.Play(animator, move, look) helper.
    public sealed class OnFireController : IPlayerController
    {
        // Mirrors the existing walk speed (PlayerStateBase.Speed / PlayerMover.WALK_SPEED).
        // Pre-existing tech debt: that magic 5 is already duplicated across those two sites and
        // there is no shared speed constant to reference — mirror it, do NOT diverge. Extract a
        // shared SpeedConstants when a third site needs it.
        const float BASE_SPEED = 5f;

        readonly float _speedMultiplier;

        Rigidbody2D _rb;
        Animator _animator;
        Health _health;

        // Panic-run (Issue 2): the burning player CANNOT STOP. _lastNonZeroMove holds the last
        // non-zero steering input and is what FixedTick moves along EVERY frame — releasing all
        // keys does not halt the player; input only REDIRECTS. _lookDirection is the cardinal
        // facing for the Walk clip (diverges from _lastNonZeroMove on diagonals). Both seeded from
        // the constructor's initialDirection (Step 4): the player's facing when On-Fire starts, or
        // Down as a fallback (so the first frame is WalkDown, not a frozen prior OnFoot pose).
        Vector2 _lastNonZeroMove;
        Vector2 _lookDirection;

        public IHealthSource HealthSource => _health;

        // initialDirection: the seed for the run + facing. OnFireEffect passes the player's actual
        // facing; the F debug key passes nothing (default Vector2.zero) → fall back to Down so an
        // idle catch-fire still immediately panics downward.
        public OnFireController(float speedMultiplier = 2.0f, Vector2 initialDirection = default)
        {
            _speedMultiplier = speedMultiplier;
            _lastNonZeroMove = initialDirection == Vector2.zero ? Vector2.down : initialDirection;
            _lookDirection = _lastNonZeroMove;
        }

        public void OnPossess(PlayerWrapper wrapper)
        {
            _rb       = wrapper.GetComponent<Rigidbody2D>();
            _animator = wrapper.GetComponentInChildren<Animator>();
            _health   = wrapper.GetComponent<Health>();
        }

        public void OnRelease()
        {
            // No PlayerStateMachine driven here, so there is no stale-continuation token to trip
            // (unlike OnFootController.OnRelease) — just drop the references.
            _rb       = null;
            _animator = null;
            _health   = null;
        }

        public void Tick(in PlayerInputState input)
        {
            // Run-only AND can't-stop (Issue 2): Interact/UseItem/SwitchItem ignored; releasing
            // all keys does NOT stop the player. Non-zero input REDIRECTS the run (and updates
            // facing on a cardinal); zero input leaves the last direction + facing unchanged.
            if (input.Move != Vector2.zero)
            {
                _lastNonZeroMove = input.Move;

                Facing? facing = FacingExtensions.FromVectorDominant(input.Move);   // PB.4.5 R4.5 (ruling c): same stick-blindness defect as PlayerStateBase — panic-run direction now takes from analog too
                if (facing.HasValue)
                    _lookDirection = facing.Value.ToVector();
            }

            PlayRunAnim();
        }

        public void FixedTick()
        {
            _rb.MovePosition(_rb.position + _lastNonZeroMove * (BASE_SPEED * _speedMultiplier) * Time.deltaTime);
        }

        // Always the Walk clip (never Stand) — the panic-runner never stops. Keyed off
        // _lookDirection (always a cardinal, even when _lastNonZeroMove is diagonal), so the
        // flipX baked into those clips (Day-3) keeps facing correct. Duplicated clip names from
        // MoveAnimState (see NOTE above). No dedicated "run" clip — v1 reuses Walk; the run reads
        // as fast via 2x speed + OnFireEffect's red tint (Step 4). Documented v1 art gap.
        void PlayRunAnim()
        {
            if (_lookDirection == Vector2.down)
                _animator.Play("WalkDown");
            else if (_lookDirection == Vector2.up)
                _animator.Play("WalkUp");
            else if (_lookDirection == Vector2.right)
                _animator.Play("WalkRight");
            else if (_lookDirection == Vector2.left)
                _animator.Play("WalkLeft");
        }
    }
}
