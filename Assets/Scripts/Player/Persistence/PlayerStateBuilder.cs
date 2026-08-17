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

        // ─── SAVE.2 R2 (DD2, OQ-A ruled): sentinel-seeded detection. JsonUtility gives
        // no presence signal (missing/mismatched fields silently default), so the parse
        // overwrites onto a sentinel-seeded instance — any field still sentinel after
        // FromJsonOverwrite was missing/unparsed. SPIKE (Tools menu, run at sweep-open)
        // records the three factual unknowns; if (i) nested-struct preservation FAILS,
        // the rung PAUSES on a ruling request (owner-ruled escalation — never a silent
        // workaround). ───

        public const int SentinelInt = int.MinValue;   // out-of-band: no legal save field is MinValue

        public static IT.Core.Save.SaveGameDTO CreateSentinelSeeded() => new IT.Core.Save.SaveGameDTO
        {
            dtoVersion = SentinelInt,
            currentSceneId = null,
            worldFlags = null,
            primaryPlayer = new PlayerStateDTO
            {
                playerId = null,
                deviceId = null,
                lives = SentinelInt,
                currentHealth = SentinelInt,
                currentItemIndex = SentinelInt,
                items = null,
                activeStatuses = null,
            },
        };

        // Envelope parse: false = unparseable JSON (E-4.ii -> new-game path).
        // Sentinel-seeded per DD2 — ValidateSaveGame reads the sentinels as "missing".
        public static bool ParseSaveGame(string json, out IT.Core.Save.SaveGameDTO save)
        {
            try
            {
                save = CreateSentinelSeeded();
                JsonUtility.FromJsonOverwrite(json, save);
                return true;
            }
            catch (System.Exception)
            {
                save = null;
                return false;
            }
        }

        // SAVE.2 (DD1/DD5): THE validation pass — normalizes the DTO in place per the
        // DD5 rule table, emits the ONE structured log per corrupt field (DD4: downstream
        // guards keep their warns as genuinely last-ditch; post-validation they should
        // never fire on the load path), returns the corruption count (caller preserves
        // the corpse when > 0, C-2). currentSceneId is NOT validated here — SAVE.1's
        // boot branch owns it (C-3 name-lookup). dtoVersion IS owned here (DD6 takeover
        // — the single version-check implementation, B-1 log-and-proceed).
        public static int ValidateSaveGame(ref IT.Core.Save.SaveGameDTO save)
        {
            int count = 0;

            if (save.dtoVersion == SentinelInt || save.dtoVersion < 1)
            {
                LogCorruption("dtoVersion", "int >= 1", FmtInt(save.dtoVersion),
                              IT.Core.Save.SaveGameDTO.CurrentVersion.ToString());
                save.dtoVersion = IT.Core.Save.SaveGameDTO.CurrentVersion;
                count++;
            }
            else if (save.dtoVersion != IT.Core.Save.SaveGameDTO.CurrentVersion)
            {
                // B-1: mismatch is post-parse by definition — loud, then proceed with the
                // per-field fail-alive load. No migration machinery in v1 (B-3 transfer).
                Debug.LogWarning($"[SaveLoad] dtoVersion {save.dtoVersion} != expected {IT.Core.Save.SaveGameDTO.CurrentVersion} — proceeding with per-field fail-alive load (B-1; no migration in v1)");
            }

            var p = save.primaryPlayer;
            if (p.lives == SentinelInt || p.lives < 0)
            {
                // DD3 (OQ-B ruled): -1 in-band marker — the shipped seam's lives>=0 check
                // routes it to chain fallthrough (A-1 fallthrough-not-floor) unchanged.
                LogCorruption("primaryPlayer.lives", "int >= 0", FmtInt(p.lives), "-1 marker (chain fallthrough)");
                p.lives = -1;
                count++;
            }
            if (p.currentHealth == SentinelInt || p.currentHealth < 1)
            {
                LogCorruption("primaryPlayer.currentHealth", "int >= 1", FmtInt(p.currentHealth), "1 (Health floor)");
                p.currentHealth = 1;
                count++;
            }
            // Upper health bound is NOT validatable here — max is prefab-authored,
            // unknown pre-restore; Health's silent Min ceilings it (DD5 named limitation).
            if (p.currentItemIndex == SentinelInt || p.currentItemIndex < 0)
            {
                LogCorruption("primaryPlayer.currentItemIndex", "int >= 0", FmtInt(p.currentItemIndex), "0");
                p.currentItemIndex = 0;
                count++;
            }
            if (p.items == null)
            {
                LogCorruption("primaryPlayer.items", "list", "null/missing", "empty list");
                p.items = new List<ItemStateDTO>();
                count++;
            }
            if (p.activeStatuses == null)
            {
                LogCorruption("primaryPlayer.activeStatuses", "list", "null/missing", "empty list");
                p.activeStatuses = new List<StatusStateDTO>();
                count++;
            }
            else
            {
                for (int i = 0; i < p.activeStatuses.Count; i++)
                {
                    var s = p.activeStatuses[i];
                    bool touched = false;
                    if (s.elapsed < 0f)         { LogCorruption($"primaryPlayer.activeStatuses[{i}].elapsed", "float >= 0", s.elapsed.ToString(), "0"); s.elapsed = 0f; touched = true; }
                    if (s.tickAccumulator < 0f) { LogCorruption($"primaryPlayer.activeStatuses[{i}].tickAccumulator", "float >= 0", s.tickAccumulator.ToString(), "0"); s.tickAccumulator = 0f; touched = true; }
                    if (touched) { p.activeStatuses[i] = s; count++; }
                }
            }
            if (save.worldFlags == null)
            {
                LogCorruption("worldFlags", "list", "null/missing", "empty list");
                save.worldFlags = new List<IT.Core.Save.FlagEntry>();
                count++;
            }
            save.primaryPlayer = p;
            return count;
        }

        // SAVE.0's exact structured shape — path, expected, actual, default applied.
        static void LogCorruption(string path, string expected, string actual, string appliedDefault)
            => Debug.LogWarning($"[SaveLoad] corruption at path '{path}' — expected {expected}, got {actual} — default {appliedDefault} applied");

        static string FmtInt(int value) => value == SentinelInt ? "missing" : value.ToString();
    }
}
