using UnityEngine;
using IT.Core.Combat;
using IT.Effects.Flash;
using IT.Player.Control;   // DEBUG (Step 4): WrapperState + Suspend/Resume for the P-key pause test

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

        // DEBUG (Story 4.3 Step 4 — remove with the J/P keys in Update below)
        private StatusController _status;
        private PlayerWrapper _wrapper;

        private IFlashable _flash;        // damage feedback (CharacterFlash on Visual Root)
        private int _lastKnownHealth;     // tracks deltas so we flash on damage but not heal

        private void Awake()
        {
            playerStatus = new PlayerStatus();
            _health = GetComponent<Health>();
            _status  = GetComponent<StatusController>();   // DEBUG (Step 4)
            _wrapper = GetComponent<PlayerWrapper>();        // DEBUG (Step 4)
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
            // DEBUG (Story 4.3 Step 4 — AC #5 lifecycle verify; remove before shipping).
            // J applies TWO differently-labeled effects at once: A (10s/1s) + B (6s/1s).
            // One press shows apply ×2, paired tick cadence, B expiring (~6s) while A keeps
            // ticking to ~10s (simultaneous + independent), then A expire. Re-press J before
            // expiry → OnRefresh on the still-active labels (same key → refresh, not duplicate).
            if (UnityEngine.InputSystem.Keyboard.current.jKey.wasPressedThisFrame)
            {
                _status?.Apply(new DebugLogStatusEffect("A", duration: 10f, tickInterval: 1f));
                _status?.Apply(new DebugLogStatusEffect("B", duration: 6f,  tickInterval: 1f));
            }
            // P toggles wrapper Suspend/Resume. The Active gate in PlayerWrapper.Update sits
            // BEFORE _status.Tick, so Suspend halts ticking (OnTick logs STOP) and the duration
            // does NOT advance; Resume continues from where it paused. Proves Suspend pauses
            // status timing. (These debug keys read Keyboard.current directly, not the wrapper's
            // input actions, so J/P/H/K still respond while the wrapper is Suspended.)
            if (UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame && _wrapper != null)
            {
                if (_wrapper.State == WrapperState.Active) _wrapper.Suspend();
                else                                       _wrapper.Resume();
            }
        }
    }
}
