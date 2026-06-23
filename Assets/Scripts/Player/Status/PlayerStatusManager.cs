using UnityEngine;
using IT.Core.Combat;

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

        private void Awake()
        {
            playerStatus = new PlayerStatus();
            _health = GetComponent<Health>();
            if (_health == null)
                Debug.LogWarning("[PlayerStatusManager] No Health component on Player — H key will NRE until prefab is wired");
            healthBox = GetComponentInChildren<Collider2D>();
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
