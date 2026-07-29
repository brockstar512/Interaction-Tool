using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using IT.Boot;   // PB.4.5 R4 (OQ-PB45-G): SystemsRoot → GameConfig.MaxPlayers cap read

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
        // PB.4.5 R4 (OQ-PB45-G, owner-ruled Option 1): the hardcoded cap (const = 2) was the
        // defect — it conflated "v1 couch co-op is two people" (actual scope) with maxPlayers
        // (the hard limit). Read config exactly as PlayerRoster.cs:48 does, so the two caps
        // share one source and cannot drift. JSON stays 4. The third `?? 4` literal site is a
        // recorded quality-audit candidate (capture-only, not R4 scope).
        int _maxPlayers = 4;
    


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
            // PB.4.5 R4 (G ruling's exact expression — matches PlayerRoster.cs:48): cached once;
            // the direct-play path (no SystemsRoot) still works via the fail-alive fallback.
            _maxPlayers = SystemsRoot.Instance?.Config.MaxPlayers ?? 4;
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
                // R5.1 (L5 sweep observability): transform.childCount = actual HUD panels under the manager,
                // so a stacking regression shows the count climbing instead of staying pinned at 1.
                Debug.Log($"[HUDManager] {playerId} HUD rebound → same panel reused ({transform.childCount} total)");
                return existing;
            }

            if (_panels.Count >= _maxPlayers)
            {
                Debug.LogWarning($"[HUDManager] at MaxPlayers ({_maxPlayers}, config) — no HUD panel built for '{playerId}'.");
                return null;
            }

            PlayerStatusHUD result = Instantiate(playerHUDPrefab, this.transform);
            result.BuildHUD(player);
            _panels[playerId] = result;
            // R5.1 (L5 sweep observability): first-spawn build; the count contrasts with the rebind line
            // above so "built once then rebound, count stays 1" reads straight off the console.
            Debug.Log($"[HUDManager] {playerId} HUD built (new panel; {transform.childCount} total)");
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
