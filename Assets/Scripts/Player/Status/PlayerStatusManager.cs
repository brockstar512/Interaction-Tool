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

        private void Awake()
        {
            playerStatus = new PlayerStatus();
            healthBox = GetComponentInChildren<Collider2D>();
        }

        public void Init(PlayerStateMachine playerStateMachineManager)
        {
            playerHUD = HUDManager.instance.InitializePlayerHUD(playerStateMachineManager);

        }
    }
}
