using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IT.Core.Combat;

namespace IT.Player.HUD
{
    using IT.Player.StateMachine;

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

        private void UpdateHealth(int current, int max)
        {
            Health.fillAmount = (float)current / max;
        }

        public void BuildHUD(PlayerStateMachine player)
        {
            player.itemManager.ItemSwitch += UpdateItemUI;
            player.playerStatusManager.health.HealthChanged += UpdateHealth;
            player.playerStatusManager.playerStatus.LivesChange += UpdateLives;
        }
    }
}
