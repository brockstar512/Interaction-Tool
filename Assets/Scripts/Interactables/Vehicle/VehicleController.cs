using UnityEngine;
using UnityEngine.InputSystem;
using IT.Core.Combat;

namespace IT.Interactables.Vehicle
{
    using IT.Player.Control;

    // Story 3.4 placeholder vehicle: a drivable IPlayerController used to prove the
    // possession-swap mechanics, not to model vehicle physics. A Kinematic Rigidbody2D is
    // driven via MovePosition (faster than on-foot, no push/pull — OQ-3.4-D). Real Health
    // component added in Story 4.1; VehicleHealthSource stub removed.
    [RequireComponent(typeof(Rigidbody2D))]
    public class VehicleController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private float _speed = 3f;   // tuned above walk speed (OQ-3.4-D)

        Rigidbody2D _rb;
        PlayerWrapper _wrapper;
        Vector2 _move;
        Health _health;

        public IHealthSource HealthSource => _health;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
        }

        public void OnPossess(PlayerWrapper wrapper)
        {
            _wrapper = wrapper;
            _health.ResetToMax();
            _health.HealthDepleted += OnHealthDepleted;
        }

        public void OnRelease()
        {
            if (_health != null) _health.HealthDepleted -= OnHealthDepleted;
            _wrapper = null;
        }

        public void Tick(in PlayerInputState input)
        {
            _move = input.Move;   // consumed in FixedTick (physics step)
        }

        public void FixedTick()
        {
            // Kinematic body — drive position directly, no velocity/forces to reset.
            _rb.MovePosition(transform.position + (Vector3)(_move * _speed * Time.fixedDeltaTime));
        }

        // DEBUG — reads the keyboard directly (a D2 violation) purely as test scaffolding —
        // same precedent as PlayerStatusManager's H/K debug keys.
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
                _health?.Damage(1); // DEBUG — Story 4.1
        }

        // 0-HP v1 default (FR-12): eject the player FIRST so they land safely on foot, then
        // explode. The null-conditional guards the case where no wrapper is possessing.
        void OnHealthDepleted()
        {
            _wrapper?.Eject();              // player escapes to the vehicle's last position
            Debug.Log("[Vehicle] Exploded");
            Destroy(gameObject);
        }
    }
}
