using UnityEngine;
using IT.Boot;             // Story PB.1: SystemsRoot → GameConfig.DefaultLivesCount lives seed
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
            // Story PB.1: lives seed from the config chain (the ?? covers the BootGuard /
            // direct-play path, mirroring PlayerRoster's MaxPlayers read).
            playerStatus = new PlayerStatus(SystemsRoot.Instance?.Config.DefaultLivesCount ?? 3);
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

            // DEBUG (throwaway — remove pre-ship with the keys). Reference table for playtests.
            Debug.Log("=== Debug Keys ===\n" +
                "H — Drain Health (1 dmg)\n" +
                "K — Direct death (bypasses Health)\n" +
                "O — Apply Poison (1 dmg/1s, 5s)\n" +
                "N — Apply OnFire (5s, 2x)\n" +
                "F — Raw OnFireController swap\n" +
                "G — Restore OnFoot\n" +
                "J — Apply DebugLog A+B\n" +
                "P — Suspend toggle\n" +
                "==================");
        }

        // 0 HP → enter the minimal PlayerDeathState (Story 1.4). Fires once per depletion
        // (Health transition-guards HealthDepleted). _playerStateMachine is set in Init().
        private void OnHealthDepleted()
        {
            _flash?.StopFlash();   // killing blow: cut the "hurt but lived" flash so the death visual takes over
            // Story 4.4 Step 4.5: clear active status effects BEFORE entering death. Order matters —
            // a mode-changing status (On-Fire) fires OnFireEffect.OnExpire -> RestoreController here,
            // so the controller swap-back is requested before PlayerDeathState owns movement.
            // Without this, OnFireController keeps panic-running the corpse until On-Fire's natural
            // expiry (~5s). (K-key debug death bypasses Health/HealthDepleted, so it is NOT covered.)
            _status?.ClearAll();
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
            // DEBUG (Story 4.4 Step 1 — AC1 verify; remove before shipping). O (pOison) applies
            // a PoisonEffect (1 dmg / 1s / 5s) — ticks Health via DamageOverTime (bypasses
            // i-frames, so no flash and unaffected by a recent H/bomb hit), no controller touch.
            if (UnityEngine.InputSystem.Keyboard.current.oKey.wasPressedThisFrame)
            {
                _status?.Apply(new PoisonEffect(damagePerTick: 1, duration: 5f, tickInterval: 1f));
            }
            // DEBUG (Story 4.4 Step 3 — remove pre-ship). F swaps to OnFireController (2x run,
            // no interact/use); G restores the previous controller. Verifies the swap mechanism
            // in isolation before OnFireEffect (Step 4) drives it.
            if (UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame && _wrapper != null)
            {
                _wrapper.SwapController(new OnFireController());   // default 2.0x multiplier
            }
            if (UnityEngine.InputSystem.Keyboard.current.gKey.wasPressedThisFrame && _wrapper != null)
            {
                _wrapper.RestoreController();
            }
            // DEBUG (Story 4.4 Step 4 — AC verify; remove pre-ship). N (oN fire) applies an
            // OnFireEffect (5s / 2x): swaps to OnFireController panic-run, restores on expiry.
            // Counterpart to O (poison) for the FR-16 simultaneity check (press O then N).
            if (UnityEngine.InputSystem.Keyboard.current.nKey.wasPressedThisFrame)
            {
                _status?.Apply(new OnFireEffect(duration: 5f, speedMultiplier: 2.0f));
            }
        }
    }
}
