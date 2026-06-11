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

## Code Cleanup (from REFACTOR_NOTES)
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

---

## Already Done
- [x] Remove NavMesh packages (`com.h8man.2d.navmeshplus`, `com.unity.ai.navigation`)
- [x] Delete baked NavMesh assets from `Move and Slide sample/`
- [x] Fix `ItemManager.GetItem()` — clamp `_currentIndex` before indexing
- [x] Fix `ItemManager.DisposeOfCurrentItem()` — post-decrement bug + wrong bounds comparison
- [x] Fix `ItemManager.GetCurrentSprite()` — add empty-inventory guard
