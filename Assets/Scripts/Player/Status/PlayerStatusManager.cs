using UnityEngine;

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

        private void Awake()
        {
            playerStatus = new PlayerStatus();
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
                playerStatus.UpdateHealth(-1);
            }
            if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
            {
                _playerStateMachine.EnterDeath();
            }
        }
    }
}
