# TODO

## In Progress
### Custom Collision
- [ ] **Make Custom Collision**



### Health
- [ ] **make health and life cycle system**

### Camera Follow
- [ ] **Make camera follow system/camera for reagion**

### Depth Sorting
- [x] Write `YDepthSort.cs` — sets `sortingOrder` from Y position with a configurable anchor offset
- [ ] **Attach `YDepthSort` to these GameObjects in the scene/prefabs:**
  - Normal door prefab (`Single-Door-Key.prefab`) — set `yOffset` to ~half the door's world height
  - Player — attach to the SpriteRenderer child, tune `yOffset` so sort origin is at feet
  - Sword item / whip item — if they render as separate GameObjects during use
  - Enemy prefab (currently `EnemyPlaceholder`) — attach and tune `yOffset`
- [ ] **Set sprite pivots to bottom-center in the Sprite Editor for each sheet:**
  - `Assets/Sprites/Door/` — all 6 frames
  - `Assets/Sprites/Characters/Player/Sword/` — all 11 frames
  - `Assets/Sprites/Characters/Player/Whip/` — all 5 frames
  - `Assets/Sprites/Generic Old West Graphicis/Mobs/Coyote/` — Front, Back, Side sheets
  - `Assets/Sprites/Generic Old West Graphicis/Mobs/Coffin/` — Front, Back, Side sheets
  - `Assets/Sprites/Generic Old West Graphicis/Mobs/Cactus/` — Front, Back, Side sheets
  - Note: use the Sprite Editor per-frame pivot tool, not bulk meta edits

---

## Enemy AI
- [ ] Replace `EnemyPlaceholder.cs` — currently just calls `Destroy(gameObject)` on any hit
- [ ] Decide on AI approach (no NavMesh — use custom patrol/chase with Physics2D or A*)
- [ ] Wire up `ApplyDamage` to a proper health/death flow matching `PlayerStatus` pattern
- [ ] Handle grappling hook hit on enemy properly (`InteractWithHookProjectile` is logged but not implemented beyond calling `ApplyDamage`)
- [ ] Implement enemy animations using the mob sprite sheets (Coyote, Coffin, Cactus)

---

## Sword & Whip
- [ ] `SwordItem.Use()` — only stores the callback, never plays an animation. Hook up sword swing animation before calling `PutAway`
- [ ] `WhipItem.Use()` — calls `PutAway()` immediately with no delay or animation. Add whip-crack animation and hitbox before returning to default state
- [ ] Both items: add a hitbox/damage overlap check during the attack animation frames

---

## Code Cleanup

- [ ] **WorldFlags string-ID safety pass.** `WorldStateFlagSource._flagId` is currently a free-form string Inspector field with no compile-time check or typo protection. Same risk on every `worldState.GetFlag("name")` call site in code. Not a problem at current flag count (~3); becomes one around 15+ flags in real gameplay use, likely mid-Epic 4 or 5.

  Approaches to consider (no decision yet):
  - **Option A (recommended starting point):** static `WorldFlags` class with `public const string X = "x"` for every flag + custom `[FlagId]` PropertyDrawer that turns the Inspector field into a dropdown reading from `WorldFlags` via reflection. ~60 lines of editor code, one new file. Doesn't change WorldState runtime API. Save-file flag IDs stay as strings (forward-compatible).
  - **Option B:** strongly-typed `FlagId` enum, no strings. Better compile-time safety; loses data-format robustness across game versions. Violates "data over code" principle for save files.
  - **Option C:** hashed string IDs at runtime (Animator.StringToHash style). Performance win only; doesn't solve typos.
  - **Out of scope:** ScriptableObject per flag (violates C-B).

  Decide approach when triggering threshold hits, not before. Don't disrupt active epic work for this.

- [x] **Player prefab visual-root refactor.** Currently SpriteRenderer and Animator live on the Player root GameObject; the convention-standard Unity pattern is a dedicated "Visual" child under root that holds all rendering components. Logical components (PlayerWrapper, PlayerStateMachine, PlayerMover, Rigidbody2D, primary collider) stay on root. Functional anchors (HealthBox, Shadow, ItemDetection, ItemOrigin, ItemManager) stay as children.

  Why: cleanly separates "is this player active in the world" (root SetActive) from "is this player visible" (Visual child SetActive). Story 3.4 visual hiding on vehicle possession becomes one line. Future visual effects (sprite flash on damage, scale tweens, hat/cosmetic children) are trivial to add.

  Cost: small refactor (~30-45 min). Move SpriteRenderer + Animator components from Player root to a new Visual child GameObject. Check GetComponent<Animator>/GetComponent<SpriteRenderer> references in code — anything that does `GetComponent` (not `GetComponentInChildren`) needs to update its lookup path. Verify Level 1 Test and Move and Slide sample still render correctly.

  Trigger: ideally before Story 3.4 vehicle possession dev runs (cleaner _visualRoot.SetActive() approach). Acceptable to defer to between epics if 3.4 is in flight. Done in cbf7086 (2026-06-21).

- [ ] **Player prefab pivot adjustments after visual-root refactor.** Moving SpriteRenderer + Animator from Player root onto Visual child shifted some sprite pivots/transforms slightly. Game plays correctly but a few visual offsets are off. Manually fix in Prefab Edit Mode when next touching the Player prefab — adjust Visual child's local position OR sprite pivot points to restore original look. Low priority; not a gameplay bug.

### From code review 2026-06-19

- [ ] **WorldState.OnSegmentReEntered: `segmentId` parameter is unused.** The method body is `=> ClearScope(FlagScope.SegmentScoped)` — it clears ALL SegmentScoped flags regardless of which segment fired. When Epic 5 wires `SegmentManager → OnSegmentReEntered`, this will incorrectly clear flags from all segments on re-entry to any one. Must change the method to filter by segment before Epic 5 wires it.
- [ ] **SlidableBase.CleanUp: drop the try/catch around FindKeyPort.** Story 3.1.5 added `try { if (_targetCheck != null) port = _targetCheck.FindKeyPort(); } catch (Exception ex) { Debug.LogError(...); }`. The Unity null-check already prevents the NRE; the broad `catch (Exception)` silently swallows real programming errors. Remove the try/catch and keep only the null guard when next touching `SlidableBase`.
- [ ] **WorldState.RestorePermanentFlags: surface caller contract in Story 9.2 spec.** The method comment says callers must call `ClearScope(Permanent) + ClearScope(SessionOnly)` before calling `RestorePermanentFlags`. The method doesn't enforce this; silent state corruption results if the save system omits the pre-clear. When drafting Story 9.2, make this an explicit acceptance criterion or enforce it inside the method.
- [ ] **PROJECT-STATUS.md is a one-shot snapshot that goes stale immediately.** The file was generated 2026-06-17 and was already wrong by 2026-06-18 (Story 3.1 completed). Either delete it and rely on the epic-docs guides + story files as authoritative, or wire it to an auto-regeneration step (e.g., a bmad-sprint-status run before each session). Don't commit manual updates — it'll lose the race with active development.

### From REFACTOR_NOTES
- [ ] Extract `OverlapCheckerBase` — five overlap classes duplicate ~500 lines of geometry math
- [ ] Fix `async void EnterState` in `EquipItemState`, `SlideItemState`, `ThrowItemState` — swap to `Awaitable` and add try/catch
- [ ] Delete dead code:
  - `Assets/Scripts/Player/PlayerMovementManager/Old/PlayerMovement.cs`
  - Empty `PlayerAnimationController`
  - Commented-out block in `Slidable.cs` (~250–269)
  - No-op `AddDetectionLayers()` in `OverlapMoveCheck`
- [ ] Fix `IExlosionDamage.cs` typo — rename to `IExplosionDamage` everywhere
- [ ] Delete unused interfaces: `IAnimationState`, `ICommand`
- [ ] Centralize layer lookups — move all `LayerMask.NameToLayer()` calls into a static `Layers` class in `Utilities`
- [ ] `CandleItem` — dispose `_cancellationTokenSource` properly on repeated `ButtonUp` calls
- [ ] **Refactor PlayerThrowState spam-guard to not depend on animation class identity.** Story 3.1.5 added `if (_currentAnimation is not CarryAnimState) return;` in `Action()` as a tight defensive fix. Better long-term: explicit `_isCarrying` bool field, or check against an `ItemManager.IsHolding`-style API. Current guard is correct but couples behavior to animation type — fragile if animation states are refactored later.

---

## Future Epics

### Player-friendly input wrapper (post-Epic 7)

Goal: hide Unity Input System behind a semantic API so gameplay code subscribes to "Interact pressed/held/released" with one line, never touches InputActionAsset/CallbackContext/PlayerInput components directly.

Design sketch:
- PlayerInputBindings class (or per-PlayerWrapper instance) owns all PlayerInputActions usage.
- One InputBinding wrapper per semantic action exposing: Pressed (one-shot on press), Released (one-shot on release), Held (every frame while held), HoldFor(seconds, callback) (long-press detection).
- Supports gamepad AND keyboard as parallel bindings per action — no consumer code distinguishes.
- Per-player binding instances live on PlayerWrapper (free multi-player support, no global state).
- Rebinding (player remaps a button) lives entirely in this layer; gameplay code never sees binding changes.

Why deferred: Story 3.2 establishes the InputUser pairing pattern; this wrapper sits ON TOP of that. Building the wrapper before 3.2/7.3 risks designing for the wrong constraints. Revisit after multiplayer input routing settles.

Touch points when built: replace direct `PlayerInputActions.Player.*.performed/.canceled` wiring in `PlayerWrapper` + `PlayerInputHandler` (the bridge installed in Story 3.1) with `PlayerInputBindings` subscriptions.

---

## Hardware-Gated Backfill

- [ ] Multi-device backfill testing for Stories 3.2/3.3 — see _bmad-output/implementation-artifacts/deferred-work.md (workspace repo). Hardware now available.

---

## Pre-existing Items (filed during Story 3.1 Task-0 baseline, 2026-06-18)
_Confirmed present BEFORE the 3.1 possession refactor. The refactor must PRESERVE these behaviors as-is so it isn't blamed for them — they get their own stories later._

- [ ] **BUG — key consumption shrinks inventory capacity.** Using a key on a lock and consuming it shrinks the inventory so a second item can't be picked up afterward. Likely an `ItemManager` count/index bug (cf. the `ItemManager` fixes under "Already Done"). Reproduce: pick up key → use on lock → try to pick up another item → fails. Out of 3.1 scope.
- [ ] **MISSING FEATURE — grapple doesn't pull pickup-able items toward player.** Item-holders are not interactable with the grapple hook (only grapple sockets / bridge formation respond). Pulling loose items in is a desired feature, not a regression. Out of 3.1 scope.
- [x] **BUG — spamming Interact during pickup breaks the carry.** Object "goes all over the place" because `PlayerThrowState.Action` had no guard against being invoked while pickup was still in flight. Pre-existing (confirmed by checking out pre-3.1 baseline). Fixed in Story 3.1.5 with a carry-gate at the top of `Action()`.
- [x] **BUG — spamming slide produces MissingReferenceException in SlidableBase.CleanUp.** Late DOTween onComplete fires after `KeyPortOverlap` and `DamageOverlap` components are destroyed. Pre-existing (confirmed by checking out pre-3.1 baseline). Fixed in Story 3.1.5 with defensive null-checks in `CleanUp`. Note: the root cause is DOTween callbacks outliving GameObjects; a fuller fix would migrate `SlidableBase` to `Awaitable`+`destroyCancellationToken` (Story 1.3 convention) — that's a separate future story, not 3.1.5's scope.



---

## Already Done
- [x] Remove NavMesh packages (`com.h8man.2d.navmeshplus`, `com.unity.ai.navigation`)
- [x] Delete baked NavMesh assets from `Move and Slide sample/`
- [x] Fix `ItemManager.GetItem()` — clamp `_currentIndex` before indexing
- [x] Fix `ItemManager.DisposeOfCurrentItem()` — post-decrement bug + wrong bounds comparison
- [x] Fix `ItemManager.GetCurrentSprite()` — add empty-inventory guard