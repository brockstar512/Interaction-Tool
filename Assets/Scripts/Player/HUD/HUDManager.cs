using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

namespace IT.Player.HUD
{
    using IT.Player.StateMachine;

    public class HUDManager : MonoBehaviour
    {
    
        public static HUDManager instance { get; private set; }
        private List<PlayerStatusHUD> _currentPlayers;
        [SerializeField] PlayerStatusHUD playerHUDPrefab;
        const int MaxPlayers = 2;
    


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
            _currentPlayers = new List<PlayerStatusHUD>();
        }
    
        public PlayerStatusHUD InitializePlayerHUD(PlayerStateMachine player)
        {
            if (_currentPlayers.Count > MaxPlayers)
                return null;

            PlayerStatusHUD result = Instantiate(playerHUDPrefab, this.transform);
            result.BuildHUD(player);
            _currentPlayers.Add(result);
            return result;
        }

        public void DestoryPlayerHUD(PlayerStatusHUD playersHUD)
        {
            PlayerStatusHUD leaving = playersHUD;
            _currentPlayers.Remove(leaving);
            Destroy(leaving.gameObject);
        }


        void HealthUI(int healthPoints)
        {

        }

        void ItemUI()
        {

        }

        void LivesUI()
        {

        }
    
    }
}
