using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] Image Health;
    [SerializeField] Transform Icon;
    [SerializeField] Image currentItem;
    [SerializeField] TextMeshProUGUI Lives;



    private void UpdateItemUI(Sprite sprite)
    {
        currentItem.sprite = sprite;
    }

    private void UpdateLives(int lives)
    {
        Lives.text = $"X{lives}";
    }
    private void UpdateHealth(int health)
    {
        Lives.text = $"X{health}";
    }

    public void BuildHUD(PlayerStateMachineManager player)
    {
        player.itemManager.ItemSwitch += UpdateItemUI;
        player.playerStatus.HealthChange += UpdateHealth;
        player.playerStatus.LivesChange += UpdateLives;
    }
}
