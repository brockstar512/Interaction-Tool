# PRD — Core Systems v1: Multi-Game Framework (Topdown First)

| | |
|---|---|
| **Status** | Draft for review |
| **Date** | 2026-06-11 |
| **Scope** | Player control wrapper, input & local multiplayer, health/status, level segments & location transport, camera system, game settings config, per-player HUD |
| **Companion docs** | `ARCHITECTURE.md` (current-state reference) · `PRD-refactor.md` (codebase refactor — WS0 below pulls its prerequisites) · `TODO.md` (superseded by this doc for items 1–11) |
| **Out of scope (v1)** | Backpack, save system, crafting, objectives, dialogue — tracked in §9 Future Features with v1 design hooks |

---

## 1. Vision & Background

This project is evolving from a single topdown game into a **reusable game prototype framework**. One codebase, configured by a game-settings layer, will eventually support three game archetypes in this order:

1. **Topdown action** (Goof Troop, A Link to the Past) — *this PRD*
2. **Topdown survival** (Survival Kids)
3. **Side-scrolling platformer** (Metal Warriors, Mario)

The framework will be consumed by other Unity projects as a **UPM package** (not a DLL — see §7). Game-specific content (sprites, items, enemies, levels) lives in the consuming project as prefab variants and JSON config; framework code never references game content directly.

The existing architecture (state-pattern player, polymorphic `Interactable`s, observer-based dependency system, `PlayerContext` facade) is kept and built upon. None of the companion refactor PRD has been executed yet; only the slices the new systems sit on are pulled forward as prerequisites (WS0).

## 2. Approved Conventions (decided with project owner — do not change without asking)

| ID | Decision |
|---|---|
| **C-A** | Migrate to the **new Unity Input System** (required for hot-plug controllers and 4-player device pairing). First task: audit all existing input reads. |
| **C-B** | **No ScriptableObjects.** Global/game-type settings → JSON in `StreamingAssets` (editable without rebuild). Per-segment/per-prefab tuning → serialized fields on components. Identity (keys/locks, item types) → string IDs, not SO assets. This supersedes the refactor PRD's WS7 ScriptableObject recommendations. |
| **C-C** | **Player roster, not player singletons.** A `PlayerRoster` owns up to N `PlayerWrapper`s; HUD panels bind to a wrapper. Singletons remain only for true globals (SegmentManager, CameraController, GameConfig). |
| **C-D** | **Controller-swap rule:** swap the active `IPlayerController` when the moveset/physics fundamentally changes (vehicle, swimming, on-fire panic-run). The existing `PlayerStateMachine` becomes one implementation — the "on foot" controller. Variations within the normal moveset (carry, throw, pull) stay as states inside it. |
| **C-E** | **No trigger colliders for segment/location detection.** Segment membership and edge crossing are pure math against serialized bounds (gizmo-drawn in Scene view). Door-style entries use the existing `Interactable` press-to-interact pattern. |
| **C-F** | **No Cinemachine.** Custom camera controller with pluggable behavior modes. |
| **C-G** | Follow patterns already established in the codebase (namespaces under `IT.*`, observer dependency system, context facade, async convention from refactor WS2). Propose improvements, but **ask the owner before changing an established pattern.** |

## 3. Goals

1. **Multiplayer-shaped from day one** — the input/wrapper layer is built for N players even though v1 ships exercising 1–4; retrofitting device routing later would mean rebuilding the wrapper.
2. **Config drives behavior** — game type, max players, and feature flags come from JSON; camera behavior comes from per-segment component config. Changing them requires no recompile.
3. **One possession model** — player on foot, in a vehicle, swimming, or panic-running are all `IPlayerController`s behind one wrapper; HUD and health follow the wrapper, not the controller.
4. **Segments are the unit of level logic** — camera mode, lock conditions, and transitions are authored per segment on scene objects.
5. **Framework/content separation** — nothing in framework assemblies references game-specific assets; reconfiguring the game for a new prototype means new prefab variants + JSON only.

## 4. Non-Goals (v1)

- No survival or platformer game-type implementations (the config schema reserves them; no behavior ships).
- No backpack, save, crafting, objectives, or dialogue systems (§9 covers hooks only).
- No online networking. "Multiplayer" means local shared-screen.
- No full execution of the refactor PRD — only WS0's prerequisite slices.

## 5. Success Metrics

| Metric | Target |
|---|---|
| Players supported | Up to `maxPlayers` from JSON (default 4), keyboard + hot-plugged controllers |
| Changing `maxPlayers` or `gameType` | JSON edit only, no recompile |
| Adding a camera behavior to a segment | Edit one component on the segment object, no code |
| Vehicle enter/eject | Control, HUD health source, and input routing swap with zero orphaned state |
| Simultaneous status effects | Poison (health tick) + on fire (controller swap) run concurrently without conflict |
| Trigger colliders used for segments/locations | 0 |
| Framework → game-content references | 0 (enforced by asmdef boundaries) |

---

## 6. Workstreams (dependency order — build top to bottom)

### WS0 — Refactor Prerequisites (P0)

Pulled from the companion refactor PRD; the new systems sit directly on these. Execute first, exactly as specified there:

| Slice | From refactor PRD | Why it gates this PRD |
|---|---|---|
| Build breakers & correctness | WS1 (all of 1.1–1.5) | Player builds must compile; HUD events must be null-safe before HUD instancing |
| Async convention & lifetime | WS2.1–2.2 | The wrapper and status system await animations/timers; they need the `Awaitable` + `destroyCancellationToken` convention |
| State machine hardening | WS3.1–3.3, plus minimal WS3.5 | `PlayerStateMachine` becomes the on-foot `IPlayerController`; stale-continuation guards must exist before possession can swap it out mid-action |
| `Facing` direction utility | WS5.1 | Camera look-ahead, segment edges, and sprite selection all consume it |

**Acceptance:** as defined in the refactor PRD for each slice. The remaining refactor workstreams (WS4 overlap consolidation, WS5.2+ animation, WS6–WS10) proceed in parallel or after, per that doc's own phasing — they do not block this PRD.

### WS1 — Game Settings, Config Schema & World State (P0)

- **1.1 JSON game settings.** `StreamingAssets/Config/game-settings.json` loaded at boot by a `GameConfigLoader`; exposed via an immutable `GameConfig` accessor. v1 schema: `gameType` (`Topdown` — `Survival`/`Platformer` reserved), `maxPlayers` (default 4), feature flags (`backpackEnabled`, `itemsUpgradable`, `saveEnabled`, `craftingEnabled`, `dialogueEnabled` — all `false` in v1), input options. Plain `[Serializable]` C# classes mirror the JSON.
- **1.2 Validation.** Missing/malformed JSON → one clear error log naming the field, then safe defaults. Never a silent NRE three systems downstream.
- **1.3 Config placement rule.** Global/game-type-level → JSON. Per-segment / per-prefab tuning → serialized component fields (per C-B). Document the rule in `CLAUDE.md`/`CONTRIBUTING` so agents don't guess.
- **1.4 WorldState registry.** Runtime flag store (string ID → value) for opened chests, unlocked doors, defeated bosses. Observable, so the existing dependency system can subscribe (segment locks in WS4.3 consume this). API designed to be serializable — the future save system (§9.2) persists it, but v1 ships it in-memory only.

**Acceptance:** editing `maxPlayers` or a feature flag changes runtime behavior with no recompile; deleting the JSON produces one readable error and a playable default; a chest-opened flag set in WorldState is observable by a `Dependent`.

### WS2 — Input Layer & Player Wrapper (P0/P1)

- **2.1 Input System migration.** Audit every existing input read (location of current reads is an open question — OQ-1), then migrate to Input System action maps (`Move`, `Interact`, `Use`, `SwitchItem`, `Pause`, `Eject`). Per-player input routing via device pairing (`PlayerInput` per wrapper or manual `InputUser` pairing — architect's choice, justify in the architecture doc).
- **2.2 `PlayerWrapper`.** One per player. Owns: player index, paired device(s), `PlayerHealth` reference, `StatusController` reference, and the **active `IPlayerController`**. Explicit tick order each frame: status → health → active controller (use a single owned `Update` that calls them in order — do not rely on Script Execution Order settings).
- **2.3 `IPlayerController` contract.** `OnPossess(PlayerWrapper)`, `OnRelease()`, per-frame tick receiving the wrapper's input state, and access to the existing `PlayerContext` facade. Refactor the existing `PlayerStateMachine` to implement it as `OnFootController` (per C-D) — its internal states are untouched beyond WS0 hardening.
- **2.4 Possession & vehicles.** A vehicle is a scene object implementing `IPlayerController` with its **own `Health` component**. Entering (via `Interactable`): wrapper releases on-foot controller, hides/parents the player visuals to the vehicle, possesses the vehicle, and **switches the HUD health source to the vehicle's health** (Blaster Master / Metal Warriors model). `Eject` input reverses everything. Define what happens at vehicle HP 0 — eject-and-explode is the v1 default (OQ-5 if owner wants damage spill-over to the player).
- **2.5 `PlayerRoster`.** Owns wrappers up to `GameConfig.maxPlayers`. Join/leave flow (join mechanism is OQ-4; default: press any button on an unpaired device). Raises `PlayerJoined`/`PlayerLeft` events that HUD (WS6) and camera (WS5) consume. HUD and camera never reference "the player" — always a wrapper from the roster (per C-C).

**Acceptance:** the existing single-player game plays identically through `OnFootController`; plugging in a controller mid-game pairs a new device; entering and ejecting a vehicle swaps control, visuals, and HUD health source with no orphaned input or stale async continuations (WS0 guard verified here).

### WS3 — Health & Status System (P1)

- **3.1 `Health` component.** Current/max, `Damage`/`Heal` API, events that pass **current totals** (fixes refactor 1.3's delta bug at the source), optional i-frames window. Shared by player, vehicles, and future enemies (aligns with refactor WS6's `DamageInfo` direction — adopt `DamageInfo` as the damage payload here rather than building a second path).
- **3.2 `StatusEffectBase`.** Inheritable class with `Duration`, `TickInterval`, `OnApply`/`OnTick`/`OnExpire`. A `StatusController` on the wrapper manages the active list: simultaneous effects supported; per-effect stacking policy (`Refresh` duration is the v1 default, `Stack` reserved).
- **3.3 Two effect categories.**
  - *Health-affecting* (e.g., **Poison**): ticks damage/effects via `Health`; never touches the controller.
  - *Controller-affecting* (e.g., **On Fire**): on apply, requests the wrapper swap to a dedicated controller (`PanicRunController` — faster move speed, burning sprite sheet); on expire, restores the previous controller. Uses the same possession path as vehicles (C-D) — no special cases.
- **3.4 Death.** HP 0 → terminal death effect → death controller/state (builds on WS0's minimal `PlayerDeathState`: input stops, no crash). Full death presentation is feature work beyond this PRD.

**Acceptance:** poison ticks while on fire is active, both expire independently; on-fire swaps the controller and restores cleanly even if the player was mid-animation (stale guard) or in a vehicle (effect queues or is suppressed — architect documents the choice); 0 HP enters death without exceptions.

### WS4 — Level Segments & Location Transport (P1)

- **4.1 Segment authoring.** Each segment is a scene GameObject with a `SegmentBounds` component: serialized `Rect` (world space), drawn and draggable via gizmos/handles in the Scene view. **No trigger colliders** (C-E). The `SegmentManager` runs per-frame point-in-rect checks for each roster player to track membership and detect edge crossings.
- **4.2 Segment config component.** On the same object: camera behavior mode + its parameters (consumed by WS5), lock conditions, and per-edge behavior — `Seamless` (camera scrolls, no transport), `Slide` (LttP-style camera pan to neighbor), `Transport` (teleport elsewhere in scene), `Scene` (load another scene). Per C-B this is all serialized component data, not assets.
- **4.3 Locks & conditions.** A segment can be locked (e.g., exits blocked until all enemies dead) by subscribing to the existing `Observable`/`Dependent` system and/or WorldState flags (WS1.4) — same pattern the door dependency system already uses.
- **4.4 `LocationTransporter`.** Executes `Transport` and `Scene` edge behaviors and door-style entries: fade/wipe via a modular `ITransition` interface (transitions are pluggable; v1 ships `Fade`), in-scene teleport repositioning player(s) + camera, or additive scene load/unload behind the same transition. Doors and cave entrances use the existing `Interactable` interact-press flow — no physics triggers.
- **4.5 Multi-player edge policy.** Edge behaviors fire based on a configurable policy (v1 default: any player crosses a `Seamless` edge freely within camera limits; `Slide`/`Transport` edges require — default — the crossing player, and remaining players are brought along by the transition). The exact lagging-player behavior during a slide is OQ-2; implement behind one policy seam so changing the answer is cheap.

**Acceptance:** walking across a seamless edge scrolls without transport; an interact-press door fades and teleports player(s) to an interior; a `Scene` edge additively loads the target scene behind a fade; a locked segment's exits stay inert until its condition fires; zero colliders involved in any of it.

### WS5 — Camera System (P1)

- **5.1 Architecture.** One `CameraController` (no Cinemachine, C-F) delegating to an `ICameraBehavior` selected by the current segment's config (WS4.2). Behaviors are pure classes parameterized by serialized config — adding a mode adds a file, never a switch in the controller.
- **5.2 Behavior modes (v1 set):**
  1. **Fixed** — locked to the segment.
  2. **Fixed-locked** — fixed, and segment exits gated by condition (pairs with WS4.3).
  3. **Deadzone follow** — follows once the player passes an offset threshold.
  4. **Always follow** — centered on the player.
  5. **Spline follow + nudge** — camera tracks a spline; pushes the player when they're against the edge.
  6. **Spline follow, no nudge** — same, player can fall behind the edge freely.
  7. **Directional favor** (Super Mario World) — biases view toward the player's facing; doesn't flip bias until the player crosses a threshold on the new side. Consumes `Facing` (WS0).
  8. **Relocate + transition** — camera jumps to a new position behind an `ITransition`.
  9. **Segment slide** (LttP) — camera pans to the neighboring segment with configurable ease type and speed.
- **5.3 Pixel-perfect.** Pixel-art game: behaviors compute in world space; the controller snaps the final camera position to the pixel grid each frame. PPU/reference resolution is OQ-3 — read it from `GameConfig` so it's data either way.
- **5.4 Multi-player framing.** Shared single camera. Follow-type modes track the roster centroid clamped to segment bounds; fixed modes are unaffected. Per-mode multiplayer policy lives in the behavior config (one seam, per WS4.5).
- **5.5 Spline source.** Prefer Unity's official Splines package over a hand-rolled spline (OQ-6 — confirm the owner accepts the package dependency; it is not a ScriptableObject workflow).

**Acceptance:** a test scene demonstrates every mode; changing a segment's camera mode is a component edit with no code change; camera motion is pixel-snapped; two players in a deadzone-follow segment keep both on screen within bounds.

### WS6 — Multiplayer Activation & Per-Player HUD (P1/P2)

- **6.1 Join/leave.** Device pairing per WS2.5 up to `maxPlayers`; keyboard counts as a device. Spawn position policy for joiners (v1: at/near player 1).
- **6.2 Per-player HUD.** A HUD panel prefab instanced per `PlayerJoined` event, placed per player index (screen corners), bound to that wrapper. Shows health + equipped item; **shows the vehicle's health while that player possesses a vehicle** (WS2.4). Unbinds on `PlayerLeft`.
- **6.3 HUD plumbing rework.** `HUDManager`/`PlayerStatusHUD` move off singleton-player assumptions onto roster events (per C-C; aligns with refactor WS9.4). Health bars consume the totals-based events from WS3.1.

**Acceptance:** four devices join and each corner HUD tracks its own wrapper independently; player 2 entering a vehicle changes only player 2's HUD; unplugging a controller removes that player cleanly (or pauses for re-pair — architect documents the choice).

---

## 7. Packaging & Reuse (design constraint now, milestone later)

**Workflow:** the owner will build games by importing this framework into fresh Unity projects and configuring content there.

- **Mechanism: UPM package** (local folder or git URL with `package.json`), *not* a DLL. DLLs were considered and rejected: no source debugging in consuming projects, rebuild-reimport loop on every tweak, and Unity serialization fragility across DLL rebuilds.
- **Prerequisite:** assembly definitions per refactor WS9.1 (`IT.Framework.Runtime`, `IT.Framework.Editor`, tests), executed during M4 of the refactor plan or alongside this PRD's M4.
- **Separation rule (enforced from v1, day one):** framework assemblies contain code + base prefabs only. Game projects supply: JSON config, prefab variants of framework base prefabs (this is where sprite references live — sprites are Unity assets and cannot come from JSON), and string-ID content definitions. Items/enemies/player skins are reconfigured per game by variant + JSON, never by editing framework code.
- **v1 deliverable:** the separation discipline + asmdef boundaries. Actually extracting the `package.json` package is a follow-up milestone once v1 systems stabilize.

## 8. Phasing

| Milestone | Contents | Exit check |
|---|---|---|
| **M1 — Foundations** | WS0 (all), WS1 | Player build compiles; JSON config drives `maxPlayers`/flags; WorldState observable |
| **M2 — Possession** | WS2, WS3 | Game plays identically via `OnFootController`; vehicle + on-fire + poison demos work |
| **M3 — World** | WS4, WS5 | Segment test scene with all 9 camera modes and all 4 edge behaviors |
| **M4 — Players** | WS6, asmdef/packaging prerequisites (§7) | 4-player session with per-player HUD; framework/game asmdef boundary compiles |

Per-PR rule (solo project, BMAD story-sized slices): one workstream slice, game boots, relevant tests pass, `ARCHITECTURE.md` updated if a diagram changed.

## 9. Future Features (do not build — leave these hooks in v1)

| # | Feature | Summary | v1 hooks to leave |
|---|---|---|---|
| 9.1 | **Backpack system** | Gated by `backpackEnabled`. Modular sections (items, key items, consumables — categories extensible). Capacity limits, upgradable. Full-screen items (notebook, map) that open over the game and back out into the pack. Equip-through-backpack alongside the existing item cycling. Layout mode per game settings: RE-style grid (fit items) **or** list. | `backpackEnabled`/`itemsUpgradable` flags exist (WS1.1); items keep string-ID identity (C-B); inventory contract work in refactor WS8 keeps one acquisition path the backpack can later wrap |
| 9.2 | **Save system** | Per game settings: persists player state, current segment, segment state (unlockables, items, enemies), and game-type-specific world changes (homestead/upgrade-station builds for survival). | WorldState registry (WS1.4) designed serializable; segment state flows through WorldState, not ad-hoc fields |
| 9.3 | **Crafting** | Combine items into new items; grid or list presentation per game settings (shares 9.1's layout option). | String-ID items; recipe data will be JSON (C-B) |
| 9.4 | **Objectives / non-visual triggers** | Tracks finished objectives and fired triggers that alter NPC behavior or the environment without visual triggers. | This is WorldState (WS1.4) plus a future query layer — register all such flags through WorldState from day one |
| 9.5 | **Dialogue system** | Fixed (non-branching) dialogue, triggered by interact-press on NPCs or by entering a location; supports animation triggers for sequenced scenes. Player-choice dialogue is an optional future extension, not the default. | NPC interaction uses the existing `Interactable` pattern; location triggers use SegmentManager events (WS4.3), not colliders; `dialogueEnabled` flag exists |

## 10. Risks & Mitigations

| Risk | Likelihood | Mitigation |
|---|---|---|
| Input System migration breaks existing feel | High | Audit-first (WS2.1); migrate behind the wrapper so `OnFootController` internals change minimally; manual playtest checklist before/after |
| `PlayerStateMachine` → `IPlayerController` refactor destabilizes states | Medium | WS0 hardening (stale guards, clear-on-exit) lands first; characterization playtests per state |
| Per-frame point-in-rect checks for 4 players × many segments | Low | Trivial math; only optimize (spatial hash) if profiling demands — note in code, don't pre-build |
| Camera mode sprawl (9 modes) stalls M3 | Medium | Modes 1, 3, 4, 9 first (cover the topdown v1 game); 5–8 follow within the same `ICameraBehavior` seam |
| Multi-player edge cases (slides, transports, vehicles) explode scope | High | One policy seam (WS4.5); v1 picks simple defaults and records alternatives as open questions |
| Refactor PRD work (WS4–WS10 there) collides with this PRD's diffs | Medium | Sequence per §6 WS0 table; overlap/animation refactors touch different files than segments/camera — coordinate via the refactor doc's milestones |

## 11. Open Questions

1. **OQ-1:** Where do current input reads live and which API (legacy `Input.*` vs. Input System)? → answered by the WS2.1 audit; record findings in the architecture doc.
2. **OQ-2:** During an LttP segment slide with a lagging second player: nudged along, blocked, or teleported with the transition? (v1 default: brought along by the transition; one policy seam.)
3. **OQ-3:** PPU and reference resolution for pixel-perfect snapping? → put in `GameConfig` once decided.
4. **OQ-4:** Join flow: press-any-button on an unpaired device, or a join screen? (v1 default: press-to-join.)
5. **OQ-5:** Vehicle destroyed at 0 HP: eject-and-explode only (v1 default), or spill damage to the player?
6. **OQ-6:** Spline camera modes: Unity Splines package acceptable as a dependency, or hand-rolled?
7. **OQ-7:** Status stacking default is `Refresh` — any effect that should `Stack` in v1?

---

## Appendix A — How this PRD relates to the refactor PRD

| Refactor PRD item | Disposition here |
|---|---|
| WS1, WS2.1–2.2, WS3.1–3.3 (+3.5 minimal), WS5.1 | Pulled forward as **WS0 prerequisites** of this PRD |
| WS6 (damage/`DamageInfo`) | Adopted as the damage payload in WS3.1 — build once, shared |
| WS7.1–7.2 (ScriptableObject configs/keys) | **Superseded** by C-B: JSON + serialized components + string IDs |
| WS9.1 (asmdefs), WS9.4 (HUD singleton) | Consumed by §7 packaging and WS6.3 respectively |
| Everything else (WS4, WS5.2–5.5, WS8, WS9.2–9.3, WS10) | Unchanged; proceeds per the refactor doc's own phasing |
