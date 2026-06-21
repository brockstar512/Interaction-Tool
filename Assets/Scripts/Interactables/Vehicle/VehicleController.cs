using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.Interactables.Vehicle
{
    using IT.Player.Control;

    // Story 3.4 placeholder vehicle: a drivable IPlayerController used to prove the
    // possession-swap mechanics, not to model vehicle physics. A Kinematic Rigidbody2D is
    // driven via MovePosition (faster than on-foot, no push/pull — OQ-3.4-D). Health is a
    // minimal stub; the real Health component arrives in Story 4.1.
    [RequireComponent(typeof(Rigidbody2D))]
    public class VehicleController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private int _maxHealth = 5;
        [SerializeField] private float _speed = 3f;   // tuned above walk speed (OQ-3.4-D)

        Rigidbody2D _rb;
        PlayerWrapper _wrapper;
        Vector2 _move;
        int _currentHealth;

        readonly VehicleHealthSource _healthSource = new VehicleHealthSource();
        public IHealthSource HealthSource => _healthSource;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void OnPossess(PlayerWrapper wrapper)
        {
            _wrapper = wrapper;
            _currentHealth = _maxHealth;
            _healthSource.NotifyChanged(_currentHealth, _maxHealth);
        }

        public void OnRelease()
        {
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

        // DEBUG — Story 4.1 replaces with real Health.Damage(). Reads the keyboard directly
        // (a D2 violation) purely as test scaffolding — same precedent as PlayerStatusManager's
        // H/K debug keys. Do NOT move this into Tick(): controllers must not read input devices.
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
                TakeDamage(1);
        }

        void TakeDamage(int amount)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _healthSource.NotifyChanged(_currentHealth, _maxHealth);
            if (_currentHealth == 0)
                OnHealthDepleted();
        }

        // 0-HP v1 default (FR-12): eject the player FIRST so they land safely on foot, then
        // explode. The null-conditional guards the case where no wrapper is possessing.
        void OnHealthDepleted()
        {
            _wrapper?.Eject();              // player escapes to the vehicle's last position
            Debug.Log("[Vehicle] Exploded");
            Destroy(gameObject);
        }

        // Minimal IHealthSource stub. Story 4.1 swaps this for the real Health component.
        sealed class VehicleHealthSource : IHealthSource
        {
            public int Current { get; private set; }
            public int Max { get; private set; }
            public event System.Action<int, int> HealthChanged;

            public void NotifyChanged(int current, int max)
            {
                Current = current;
                Max = max;
                HealthChanged?.Invoke(current, max);
            }
        }
    }
}
