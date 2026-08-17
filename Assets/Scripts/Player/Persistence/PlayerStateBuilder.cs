using System.Collections.Generic;
using UnityEngine;
using IT.Core.Combat;
using IT.Items;
using IT.Player.Control;
using IT.Player.Inventory;
using IT.Player.Status;

namespace IT.Player.Persistence
{
    // Story PB.1 (C-H): the ONE translator between a live PlayerWrapper and PlayerStateDTO.
    // Static utility — plain C#, no Unity lifecycle, no state of its own (C-C: not a
    // singleton, nothing to boot). Reads sibling components off the wrapper's GameObject
    // (the router's composition-read precedent, 5.2/5.3); the wrapper never learns
    // serialization exists — its diff stays empty (V12b). Same assembly, so the internal
    // Suspend()/Resume() are reachable without any wrapper edit (DD2).
    public static class PlayerStateBuilder
    {
        // Snapshot the live player. Items and identity seams are inert here (PB.3 / PB.4
        // fill them). Capturing an already-dead player (health 0) is a caller error —
        // death flows through respawn seeding, not DTO round-trip (spec DD4).
        public static PlayerStateDTO Capture(PlayerWrapper player)
        {
            var health = player.GetComponent<Health>();
            var status = player.GetComponent<PlayerStatusManager>().playerStatus;
            var inventory = player.GetComponentInChildren<PlayerInventory>();
            return new PlayerStateDTO
            {
                playerId = player.PlayerId ?? "",   // PB.4 (DD5) — stable slot string ("P1"/"P2"), assigned at registration
                deviceId = player.DeviceId,         // PB.4 R5 (OQ-PB4-E) — real value derived at pair time (serial/product+mfr/name); populate + round-trip. CONSUMING it (reconnect → slot re-association) is SAVE.1/later
                wrapperState = player.State,
                currentHealth = health.Current,
                lives = status.CurrentLives,
                // PB.3: per-item runtime state crosses as explicit data (C-H). The registry
                // owns the mapping; the held slot is inventory state, not roster state (DD5).
                items = ItemStateRegistry.Capture(inventory),
                currentItemIndex = inventory != null ? inventory.CurrentIndex : 0,
                // PB.2 (Directive 2): active statuses cross as explicit data — the one
                // C-H transient promoted to persistent state. Registry owns the mapping.
                activeStatuses = StatusEffectRegistry.Capture(player.GetComponent<StatusController>()),
            };
        }

        // Write a snapshot onto a live (freshly instantiated) player.
        // CONTRACT: call AFTER the target's Start() — Health.Start() sets current = max
        // and would silently clobber an earlier restore (spec DD4; the harness restores a
        // frame after Instantiate, 5.4's transporter restores post-load).
        public static void Restore(in PlayerStateDTO dto, PlayerWrapper player, RestoreMode mode,
                                   IItemPrefabProvider itemPrefabs = null)
        {
            player.GetComponent<Health>().RestoreCurrent(dto.currentHealth);
            player.GetComponent<PlayerStatusManager>().playerStatus.RestoreLives(dto.lives);

            // Statuses replay AFTER health/lives (a first post-restore poison tick must hit
            // RESTORED health, and one that immediately depletes it must find the death path
            // wired) and BEFORE the wrapperState policy (a to-be-Suspended wrapper gets its
            // effects back frozen, accumulators intact — Tick is gated by WrapperState.Active).
            // Runs under BOTH modes: Directive 2 says ALL boundaries; the RestoreMode split
            // below stays wrapperState-only (PB.2 spec DD5).
            StatusEffectRegistry.Restore(player.GetComponent<StatusController>(), dto.activeStatuses);

            // Items restore AFTER statuses and BEFORE the wrapperState policy (spec DD6).
            // Prefabs come from the caller via IItemPrefabProvider (DD8) — the registry is
            // static and holds no asset references. An item that reactivates an ongoing
            // effect (DD7/B3) does so item-side here; its own clock is WrapperState-gated,
            // so restoring into a to-be-Suspended wrapper leaves the effect frozen rather
            // than ticking, matching how a restored status behaves.
            var inventory = player.GetComponentInChildren<PlayerInventory>();
            ItemStateRegistry.Restore(inventory, dto.items, itemPrefabs);
            if (inventory != null) inventory.RestoreCurrentIndex(dto.currentItemIndex);

            // wrapperState policy (DD5): Load normalizes to Active; Transition honors the
            // capture. Dead is not a restorable state (death flows through respawn seeding,
            // DD4) — normalize to Active with a warning, mirroring the fail-alive posture.
            var target = mode == RestoreMode.Load ? WrapperState.Active : dto.wrapperState;
            if (target == WrapperState.Dead)
            {
                Debug.LogWarning("[PlayerStateBuilder] dto.wrapperState == Dead is not restorable — normalizing to Active (fail-alive)");
                target = WrapperState.Active;
            }
            if (target == WrapperState.Suspended) player.Suspend();
            else                                  player.Resume();   // idempotent — a fresh wrapper is already Active
        }

        // ─── PB.5 R3 (DD3/DD4): the ONE resolution seam. Precedence lives in these
        // signatures (SAVE.A A-2, extended) — PB.5/SAVE.1/SAVE.2 consume, never invent
        // private chains. Pure functions: same inputs, same outputs (loggable twice).
        // v1 runtime only exercises the no-DTO path (Location-entry transition machinery
        // is 5.4/SAVE.3's); the DTO branches ship [S]-verified against the R1 matrix. ───

        // The META field-valid pattern (SAVE.1 kickoff): present + valid-range,
        // fallthrough-not-floor, structured log on Strict violation. ONE shape, per-field
        // parameters — SAVE.2 inherits this, not three variants.
        static bool UseDtoField(IT.Core.Config.CarryOverMode mode, bool dtoPresent, bool dtoValid,
                                string fieldPath, string actual, string chainDefault)
        {
            if (mode == IT.Core.Config.CarryOverMode.Fresh) return false;   // authored: ignore DTO
            // PB.5 R6.1 (DD3 carve-out): NO DTO is the legitimate first-launch / fresh-join
            // route (E-4.i), not a violation — silent fallthrough under ALL modes. Strict
            // polices a crossing that HAPPENED with bad data; P2+ joins never carry a DTO in
            // v1 (A-3), so warning here would fire on every pad join by design. Also makes a
            // genuine Strict violation warn ONCE: the Awake seed always passes null (silent),
            // only the DTO-holding caller can trip the warn.
            if (!dtoPresent) return false;
            if (dtoValid) return true;                                       // CarryOver/Strict: take it
            if (mode == IT.Core.Config.CarryOverMode.Strict)
                // SAVE.0 structured shape: path — expected — actual — default applied.
                Debug.LogWarning($"[SpawnState] Strict violation at '{fieldPath}' — expected DTO value, got {actual} — default {chainDefault} applied");
            return false;
        }

        // SAVE.A A-1, verbatim order: DTO.lives (present + int >= 0) -> LevelConfig
        // override (> 0) -> GameConfig.DefaultLivesCount -> fallback. `source` feeds the
        // R5 resolution line (V5.1) without a second chain implementation.
        public static int ResolveInitialLives(PlayerStateDTO? dto,
                                              IT.Core.Config.LevelConfig levelConfig,
                                              IT.Core.Config.GameConfig gameConfig,
                                              out string source)
        {
            var mode = levelConfig != null ? levelConfig.LivesCarry
                                           : IT.Core.Config.CarryOverMode.CarryOver;
            bool dtoValid = dto.HasValue && dto.Value.lives >= 0;   // A-1 field-valid
            if (UseDtoField(mode, dto.HasValue, dtoValid, "lives",
                            dto.HasValue ? dto.Value.lives.ToString() : "no DTO",
                            "chain"))
            {
                source = "DTO";
                return dto.Value.lives;
            }
            if (levelConfig != null && levelConfig.HasLivesOverride)
            {
                source = "LevelConfig";
                return levelConfig.LivesOverride;
            }
            source = "GameConfig";
            return gameConfig?.DefaultLivesCount ?? IT.Core.Config.GameConfig.FallbackDefaultLives;
        }

        public static int ResolveInitialLives(PlayerStateDTO? dto,
                                              IT.Core.Config.LevelConfig levelConfig,
                                              IT.Core.Config.GameConfig gameConfig)
            => ResolveInitialLives(dto, levelConfig, gameConfig, out _);

        // Inventory half of the seam: TRUE -> restore dto.items (Location-entry
        // CarryOver/Strict path, unused at runtime in v1); FALSE -> the caller seeds
        // startingInventory (DD6 first-launch / fresh-join route).
        public static bool UseDtoInventory(PlayerStateDTO? dto, IT.Core.Config.LevelConfig levelConfig)
        {
            var mode = levelConfig != null ? levelConfig.InventoryCarry
                                           : IT.Core.Config.CarryOverMode.CarryOver;
            bool dtoValid = dto.HasValue && dto.Value.items != null && dto.Value.items.Count > 0;
            return UseDtoField(mode, dto.HasValue, dtoValid, "inventory",
                               dto.HasValue ? $"{(dto.Value.items?.Count ?? 0)} items" : "no DTO",
                               "startingInventory");
        }

        // ─── SAVE.1 R3: the disk boundary (C-H — the Builder is the ONE serialization
        // owner; SaveFile does IO only). Capture + wire conversion + the remaining two
        // seam halves. All pure functions. ───

        // SAVE.1 seam halves (the LevelConfig modes existed since PB.5 R2; lives and
        // inventory got resolvers at PB.5 R3 — these complete the four-field set).
        // health valid = int >= 1 (matches Health's floor; capturing dead is a caller
        // error, PB.1). statuses valid = list PRESENT (empty is a legitimate state —
        // a player with no active effects; unlike inventory, absence-of-list is the
        // only invalid shape).
        public static bool UseDtoHealth(PlayerStateDTO? dto, IT.Core.Config.LevelConfig levelConfig)
        {
            var mode = levelConfig != null ? levelConfig.HealthCarry
                                           : IT.Core.Config.CarryOverMode.CarryOver;
            bool dtoValid = dto.HasValue && dto.Value.currentHealth >= 1;
            return UseDtoField(mode, dto.HasValue, dtoValid, "currentHealth",
                               dto.HasValue ? dto.Value.currentHealth.ToString() : "no DTO",
                               "max (prefab-authored)");
        }

        public static bool UseDtoStatuses(PlayerStateDTO? dto, IT.Core.Config.LevelConfig levelConfig)
        {
            var mode = levelConfig != null ? levelConfig.StatusesCarry
                                           : IT.Core.Config.CarryOverMode.CarryOver;
            bool dtoValid = dto.HasValue && dto.Value.activeStatuses != null;
            return UseDtoField(mode, dto.HasValue, dtoValid, "activeStatuses",
                               dto.HasValue ? "null list" : "no DTO",
                               "none (fresh)");
        }

        // SAVE.1 (DD3): capture the whole envelope. MERGE-ON-SAVE (OQ-A ruling):
        // THE SAVE FILE IS A SUPERSET OF WHAT THE CURRENT SESSION KNOWS ABOUT —
        // the live registered snapshot wins per key; saved-but-never-registered keys
        // (scenes not yet revisited this session) carry through VERBATIM. WorldState
        // is never written by the save layer (a load must not mutate the registry).
        public static IT.Core.Save.SaveGameDTO CaptureSaveGame(
            PlayerWrapper primary,
            IT.Core.WorldState.WorldState world,
            string activeSceneName,
            IReadOnlyList<IT.Core.Save.FlagEntry> heldSavedFlags)
        {
            var flags = new List<IT.Core.Save.FlagEntry>();
            var seen = new HashSet<string>();
            if (world != null)
                foreach (var kvp in world.GetPermanentSnapshot())
                {
                    flags.Add(new IT.Core.Save.FlagEntry { key = kvp.Key, value = kvp.Value });
                    seen.Add(kvp.Key);
                }
            if (heldSavedFlags != null)
                foreach (var entry in heldSavedFlags)
                    if (entry.key != null && !seen.Contains(entry.key))
                        flags.Add(entry);   // superset carry-through

            return new IT.Core.Save.SaveGameDTO
            {
                dtoVersion = IT.Core.Save.SaveGameDTO.CurrentVersion,   // B-4: stamped at write
                primaryPlayer = Capture(primary),
                worldFlags = flags,
                currentSceneId = activeSceneName,                        // E-2: caller passes active scene
            };
        }

        // E-1 load-side conversion: duplicate keys -> structured log + FIRST-WINS
        // (deterministic, natural iteration order). Null keys skipped (per-field
        // corruption reporting is SAVE.2's ValidateSaveGame).
        public static Dictionary<string, bool> FlagsToDictionary(IReadOnlyList<IT.Core.Save.FlagEntry> list)
        {
            var dict = new Dictionary<string, bool>();
            if (list == null) return dict;
            foreach (var entry in list)
            {
                if (entry.key == null) continue;
                if (dict.ContainsKey(entry.key))
                    Debug.LogWarning($"[SaveLoad] corruption at path 'worldFlags[{entry.key}]' — expected unique key, got duplicate — FIRST-WINS applied");
                else
                    dict[entry.key] = entry.value;
            }
            return dict;
        }

        public static string SerializeSaveGame(IT.Core.Save.SaveGameDTO save)
            => JsonUtility.ToJson(save, true);   // E-3: pretty — v1 debuggability at single-slot scale

        // Envelope parse: false = unparseable JSON (E-4.ii -> new-game path). SAVE.2 R2
        // upgrades the internals to the sentinel-seeded FromJsonOverwrite mechanism so
        // missing fields become detectable; the signature is stable across that change.
        public static bool ParseSaveGame(string json, out IT.Core.Save.SaveGameDTO save)
        {
            try
            {
                save = JsonUtility.FromJson<IT.Core.Save.SaveGameDTO>(json);
                return save != null;
            }
            catch (System.Exception)
            {
                save = null;
                return false;
            }
        }
    }
}
