using UnityEngine;

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

    public void Init(PlayerStateMachineManager playerStateMachineManager)
    {
        playerHUD = HUDReader.instance.InitializePlayerHUD(playerStateMachineManager);

    }
}
