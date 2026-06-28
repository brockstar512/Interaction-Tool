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

        Vector2 _move;                          // cached each Tick, consumed in FixedTick (physics)
        Vector2 _lookDirection = Vector2.down;   // last cardinal facing; seeds Down like a fresh player

        public IHealthSource HealthSource => _health;

        public OnFireController(float speedMultiplier = 2.0f) => _speedMultiplier = speedMultiplier;

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
            // Run-only: Interact / UseItem / SwitchItem are deliberately NOT handled — a burning
            // player can only run. Update facing (shared Facing utility — the Day-3 fix) + animate.
            _move = input.Move;

            Facing? facing = FacingExtensions.FromVector(input.Move);
            if (facing.HasValue)
                _lookDirection = facing.Value.ToVector();

            PlayRunAnim();
        }

        public void FixedTick()
        {
            _rb.MovePosition(_rb.position + _move * (BASE_SPEED * _speedMultiplier) * Time.deltaTime);
        }

        // Duplicated facing→clip mapping (see NOTE above): same clip names + same LookDirection
        // checks as MoveAnimState, so the flipX baked into those clips (Day-3) keeps facing
        // correct. No dedicated "run" clip exists — v1 reuses the Walk clips; the run reads as
        // fast via 2x speed + OnFireEffect's red tint (Step 4). Documented v1 art gap.
        void PlayRunAnim()
        {
            bool moving = _move.x != 0f || _move.y != 0f;

            if (_lookDirection == Vector2.down)
                _animator.Play(moving ? "WalkDown" : "StandDown");
            else if (_lookDirection == Vector2.up)
                _animator.Play(moving ? "WalkUp" : "StandUp");
            else if (_lookDirection == Vector2.right)
                _animator.Play(moving ? "WalkRight" : "StandRight");
            else if (_lookDirection == Vector2.left)
                _animator.Play(moving ? "WalkLeft" : "StandLeft");
        }
    }
}
