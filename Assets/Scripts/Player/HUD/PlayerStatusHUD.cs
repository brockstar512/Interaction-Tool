using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] Image Health;
    [SerializeField] Transform Icon;
    [SerializeField] Image currentItem;
    [SerializeField] TextMeshProUGUI Lives;



    private void UpdateItemUI([CanBeNull] Sprite sprite)
    {
        currentItem.sprite = sprite;
    }

    private void UpdateLives(int lives)
    {
        Lives.text = $"X{lives}";
    }
    private void UpdateHealth(int health)
    {
        Health.fillAmount = health;
    }

    public void BuildHUD(PlayerStateMachineManager player)
    {
        player.itemManager.ItemSwitch += UpdateItemUI;
        player.playerStatusManager.playerStatus.HealthChange += UpdateHealth;
        player.playerStatusManager.playerStatus.LivesChange += UpdateLives;
    }
}
