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
        // PB.4 R4 (DD4 / Directive 1): panels keyed by the wrapper's stable playerId ("P1"/"P2"), so a
        // death→respawn REBINDS P1's existing panel instead of stacking a fresh one (the R-11 symptom).
        // Replaces the old unkeyed List<PlayerStatusHUD>.
        private Dictionary<string, PlayerStatusHUD> _panels;
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
            _panels = new Dictionary<string, PlayerStatusHUD>();
        }

        // PB.4 R4 (Directive 1): GET-OR-REBIND. A panel already mapped to this playerId (a respawn
        // reclaiming its slot) is REBOUND to the fresh wrapper and reused — no flicker, no stacking.
        // Otherwise a new panel is built. The MaxPlayers cap applies ONLY to the new-panel branch so a
        // rebind is never rejected by it; the off-by-one is fixed here too (> → >=).
        public PlayerStatusHUD InitializePlayerHUD(PlayerStateMachine player, string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                // Should not happen — PlayerId is assigned in PlayerWrapper.Awake, before this runs from
                // PlayerStateMachine.Start. Fail loud but still give the player a HUD (unkeyed → it just
                // won't be reused on respawn).
                Debug.LogWarning("[HUDManager] InitializePlayerHUD called with no playerId — building an " +
                    "unkeyed panel (not reused on respawn). Check the wrapper's PlayerId assignment.");
                var orphan = Instantiate(playerHUDPrefab, this.transform);
                orphan.BuildHUD(player);
                return orphan;
            }

            if (_panels.TryGetValue(playerId, out var existing) && existing != null)
            {
                existing.Rebind(player);   // respawn: reuse this playerId's panel, repoint at the fresh wrapper
                return existing;
            }

            if (_panels.Count >= MaxPlayers)
            {
                Debug.LogWarning($"[HUDManager] at MaxPlayers ({MaxPlayers}) — no HUD panel built for '{playerId}'.");
                return null;
            }

            PlayerStatusHUD result = Instantiate(playerHUDPrefab, this.transform);
            result.BuildHUD(player);
            _panels[playerId] = result;
            return result;
        }

        // PB.4 R4: DEAD CODE by design — game-over HUD disposition is assigned to OQ-PB4-B (post-v1
        // game-over presentation). A game-over'd panel is deliberately LEFT showing "X0" and reused if
        // the slot rejoins (owner ruling 2026-07-24), so nothing calls this yet. Kept compiling (now
        // keyed by playerId) so OQ-B can wire it without a signature hunt.
        public void DestoryPlayerHUD(string playerId)
        {
            if (_panels.TryGetValue(playerId, out var leaving))
            {
                _panels.Remove(playerId);
                if (leaving != null) Destroy(leaving.gameObject);
            }
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
