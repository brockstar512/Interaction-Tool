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

        // PB.4 R4 (DD4 / Directive 1): the player this panel is currently bound to. Retained so Rebind
        // can UNSUBSCRIBE from the old wrapper's events before repointing at the fresh one — BuildHUD
        // stored nothing before, so there was nothing to detach (the R0.1 refinement; also the standing
        // "PlayerStatusHUD never unsubscribes" deferred-work item from Story 4.1).
        private PlayerStateMachine _boundPlayer;

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
            _boundPlayer = player;   // PB.4 R4: retain so Rebind can unsubscribe later
            player.itemManager.ItemSwitch += UpdateItemUI;
            UpdateItemUI(player.itemManager.GetCurrentSprite());  // PB.4.5 R6.1: seed initial value — the ONE display BuildHUD didn't seed. A fresh (empty) respawn fires no ItemSwitch, so the rebind-persistent panel kept the pre-death sprite (phantom item, R6 sweep finding; inventory itself fresh, DD1 intact)
            player.playerStatusManager.health.HealthChanged += UpdateHealth;
            UpdateHealth(player.playerStatusManager.health.Current, player.playerStatusManager.health.Max);  // seed initial value (Health.Start() may fire before BuildHUD subscribes)
            player.playerStatusManager.playerStatus.LivesChange += UpdateLives;
            UpdateLives(player.playerStatusManager.playerStatus.CurrentLives);  // seed initial value (ctor seeds lives before BuildHUD subscribes)
        }

        // PB.4 R4 (Directive 1): repoint this SAME panel at a respawned wrapper — detach the old
        // subscriptions, then rebuild against the new player (subscribe + reseed). At every current
        // rebind site the old player is ALREADY destroyed (SpawnManager/harness Destroy-before-
        // Instantiate), so Unsubscribe's fake-null guard skips and there is nothing to leak (the old
        // player's event-owning components died with it). The retained ref + guard is still the correct
        // shape: it is right for a hypothetical live rebind and it closes the never-unsubscribes note.
        public void Rebind(PlayerStateMachine newPlayer)
        {
            Unsubscribe();
            BuildHUD(newPlayer);
        }

        void Unsubscribe()
        {
            if (_boundPlayer == null) return;   // Unity fake-null when the old player was destroyed — skip
            _boundPlayer.itemManager.ItemSwitch -= UpdateItemUI;
            _boundPlayer.playerStatusManager.health.HealthChanged -= UpdateHealth;
            _boundPlayer.playerStatusManager.playerStatus.LivesChange -= UpdateLives;
            _boundPlayer = null;
        }
    }
}
