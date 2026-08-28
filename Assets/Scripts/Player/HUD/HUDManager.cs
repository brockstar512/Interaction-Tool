using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using IT.Boot;   // PB.4.5 R4 (OQ-PB45-G): SystemsRoot → GameConfig.MaxPlayers cap read

namespace IT.Player.HUD
{
    using IT.Player.StateMachine;

    using IT.Player.Control;   // 4.6.3: PlayerLeftVoluntarily subscription (§5.C destroy-on-leave)

    // 4.6.3 (DQ-4(b) RULED — THE STATIC DIES THIS STORY): HUDManager is now the
    // Surface-3 MODULE, reached through the coordinator handle
    // (SystemsRoot.Instance?.Presentation?.Hud) — the sole legacy call site
    // (PlayerStatusManager.Init, census-confirmed) is rewired there. Still a
    // SCENE object (the panel prefab is Inspector-wired), registering/
    // deregistering like PresentationRoot's modules.
    public class HUDManager : MonoBehaviour, IT.Presentation.IHudModule
    {
        // PB.4 R4 (DD4 / Directive 1): panels keyed by the wrapper's stable playerId ("P1"/"P2"), so a
        // death→respawn REBINDS P1's existing panel instead of stacking a fresh one (the R-11 symptom).
        // Replaces the old unkeyed List<PlayerStatusHUD>.
        private Dictionary<string, PlayerStatusHUD> _panels;
        [SerializeField] PlayerStatusHUD playerHUDPrefab;
        // PB.4.5 R4 (OQ-PB45-G, owner-ruled Option 1): the hardcoded cap (const = 2) was the
        // defect — it conflated "v1 couch co-op is two people" (actual scope) with maxPlayers
        // (the hard limit). Read config exactly as PlayerRoster does, so the two caps share
        // one source and cannot drift. 4.6.3 (OQ-C ruled): the fallback literal now reads
        // GameConfig.FallbackMaxPlayers — audit #28's consolidation.
        int _maxPlayers = IT.Core.Config.GameConfig.FallbackMaxPlayers;

        private void Awake()
        {
            _panels = new Dictionary<string, PlayerStatusHUD>();
            // PB.4.5 R4 (G ruling's exact expression): cached once; the direct-play
            // path (no SystemsRoot) still works via the fail-alive fallback.
            _maxPlayers = SystemsRoot.Instance?.Config.MaxPlayers
                ?? IT.Core.Config.GameConfig.FallbackMaxPlayers;
        }

        // 4.6.3: module registration (the PresentationRoot pattern) + the §5.C
        // destroy-on-leave wire. Subscribes the DISTINCT PlayerLeftVoluntarily —
        // NOT plain PlayerLeft, which also fires on death-respawn deregisters
        // (panel must REBIND) and game-over (retain-at-X0 is the RULED
        // INTENTIONAL SPLIT, §5.C verbatim) — the deviation named in the R1.
        void OnEnable()
        {
            SystemsRoot.Instance?.Presentation?.Register((IT.Presentation.IHudModule)this);
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null) roster.PlayerLeftVoluntarily += OnPlayerLeftVoluntarily;
        }

        void OnDisable()
        {
            SystemsRoot.Instance?.Presentation?.Deregister((IT.Presentation.IHudModule)this);
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null) roster.PlayerLeftVoluntarily -= OnPlayerLeftVoluntarily;
        }

        // §5.C: DESTROY on voluntary leave — rebuilt fresh under the same key on
        // rejoin (get-or-rebind falls to the build branch; §5.D restores state).
        // §8.2 watch (carried from the record): an unkeyed orphan panel is not in
        // _panels and will not be found by this lookup.
        void OnPlayerLeftVoluntarily(PlayerWrapper wrapper)
        {
            if (wrapper == null) return;
            Debug.Log($"[HUDManager] {wrapper.PlayerId} left voluntarily — panel destroyed (§5.C; rebuilt on rejoin)");
            DestroyPlayerHUD(wrapper.PlayerId);
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

        // PB.4 R4 wrote this as dead-by-design; 4.6.3 wires its FIRST CALLER (§5.C
        // destroy-on-leave, above) and fixes the spelling at the wiring moment
        // (OQ-D ruled: rename-with-caller). Game-over panels still deliberately
        // retain at X0 (the 07-24 ruling — game-over-specific, untouched).
        public void DestroyPlayerHUD(string playerId)
        {
            if (_panels.TryGetValue(playerId, out var leaving))
            {
                _panels.Remove(playerId);
                if (leaving != null) Destroy(leaving.gameObject);
            }
        }
        // 4.6.3 (OQ-B ruled): the HealthUI/ItemUI/LivesUI stubs — empty since
        // Epic 4 — are DELETED.
    }
}
