using UnityEngine;
using IT.Core.Combat;
using IT.Effects.Flash;

namespace IT.Player.Status
{
    using IT.Player.HUD;
    using IT.Player.StateMachine;

    public class PlayerStatusManager : MonoBehaviour
    {
        public PlayerStatus playerStatus { get; private set; }
        public PlayerStatusHUD playerHUD { get; private set; }
        public Collider2D healthBox { get; private set; }
        private PlayerStateMachine _playerStateMachine;   // DEBUG: for the Story 1.4 death-entry test key

        private Health _health;
        public Health health => _health;

        private IFlashable _flash;        // damage feedback (CharacterFlash on Visual Root)
        private int _lastKnownHealth;     // tracks deltas so we flash on damage but not heal

        private void Awake()
        {
            playerStatus = new PlayerStatus();
            _health = GetComponent<Health>();
            if (_health == null)
                Debug.LogWarning("[PlayerStatusManager] No Health component on Player — H key will NRE until prefab is wired");
            healthBox = GetComponentInChildren<Collider2D>();

            _flash = GetComponentInChildren<IFlashable>();
            if (_flash == null)
                Debug.LogWarning("[PlayerStatusManager] No IFlashable (CharacterFlash) in children — damage flash disabled");

            if (_health != null)
            {
                _lastKnownHealth = _health.Max;
                _health.HealthChanged += OnHealthChanged;
                _health.HealthDepleted += OnHealthDepleted;
            }
        }

        // 0 HP → enter the minimal PlayerDeathState (Story 1.4). Fires once per depletion
        // (Health transition-guards HealthDepleted). _playerStateMachine is set in Init().
        private void OnHealthDepleted()
        {
            _flash?.StopFlash();   // killing blow: cut the "hurt but lived" flash so the death visual takes over
            if (_playerStateMachine != null)
                _playerStateMachine.EnterDeath();
            else
                Debug.LogWarning("[PlayerStatusManager] HealthDepleted before Init() — no state machine to enter death");
        }

        // HealthChanged also fires on heal / ResetToMax / Start, so flash only on a decrease.
        private void OnHealthChanged(int current, int max)
        {
            if (current < _lastKnownHealth)
                _flash?.StartFlash(_health.IFramesDuration);
            _lastKnownHealth = current;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.HealthChanged -= OnHealthChanged;
                _health.HealthDepleted -= OnHealthDepleted;
            }
        }

        public void Init(PlayerStateMachine playerStateMachineManager)
        {
            _playerStateMachine = playerStateMachineManager;
            playerHUD = HUDManager.instance.InitializePlayerHUD(playerStateMachineManager);
        }

        void Update()
        {
            // DEBUG (owner test scaffolding — remove before shipping): H drains 1 health;
            // K enters the death state directly. Real 0-HP → death wiring is Story 4.2;
            // K is the Story 1.4 death-state test hook.
            if (UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame)
            {
                _health?.Damage(1);
            }
            if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
            {
                _playerStateMachine.EnterDeath();
            }
        }
    }
}
