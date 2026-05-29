# Refactor & Modularity Analysis

## Code Smells

### CRITICAL

#### 1. Massive Duplication in Overlap Checkers (~500 LOC)
`OverlapObjectCheck`, `OverlapDoorCheck`, `OverlapHookCheck`, `OverlapTargetCheck`, and `OverlapMoveCheck` all re-implement the same core logic:
- `GetOverlappingArea()` — duplicated 5x
- `GetAABBCorners()` — duplicated 5x
- `DetermineMostOverlap()` — duplicated 5x

Each also spawns its own nested helper class (`OverlapCheckHelper`, `OverlapMoveCheckHelper`, etc.) that never gets reused. A bug fix in the overlap logic has to be applied to every file manually.

**Fix:** Extract an `OverlapCheckerBase` abstract class with the shared geometry math. Each checker only overrides what's different (which layers to query, what type to return).

---

### HIGH

#### 2. `async void` Throughout State Classes
`EnterState` is `async void` in `EquipItemState`, `SlideItemState`, `ThrowItemState`, and others. `async void` can't be awaited and swallows exceptions silently.

**Fix:** State entry should return `UniTask` / `Awaitable` / or use a callback pattern. At minimum, wrap the body in try/catch.

#### 3. Magic Numbers Scattered Everywhere
- `90.0f` threshold in `OverlapTargetCheck` line ~144
- `const float speed = 8.5f` hardcoded in `GrapplingHookItem`
- `WALK_SPEED = 5f` in `PlayerMovementManager`
- Animation timings hardcoded in TimeSheet dictionaries across 14+ animation classes
- Repeated `0x1 << layerIndex` bit operations duplicated across 17+ files

**Fix:** Pull all tunable values into a single `GameConfig` ScriptableObject or into `Utilities`. Serialized fields on the relevant components are fine too — anything that isn't buried inside a method body.

#### 4. Inconsistent Async Patterns
Mixed use of:
- `await Task.Delay()` (CandleItem)
- `await Awaitable.WaitForSecondsAsync()` (animation classes)
- `await Task.CompletedTask` (unnecessary, just remove it)
- CancellationToken used in some places, ignored in others

**Fix:** Pick one async model (Unity's `Awaitable` is the native choice) and use it everywhere. Always thread a `CancellationToken` through async chains.

---

### MEDIUM

#### 5. `PlayerStateMachineManager` Doing Too Much
At ~146 lines it handles: state switching, item management coordination, overlap detection delegation, and player status initialization. That's four jobs.

**Fix:** Extract a `PlayerStateContext` or `PlayerBlackboard` — a plain data/reference bag the state classes read from — so the state machine itself only switches states.

#### 6. Items Reach Into the State Machine
`CandleItem`, `GrapplingHookItem`, and others call `stateManager.SwitchStateFromEquippedItem(...)` directly. Items shouldn't know about state management.

**Fix:** Items raise an event or invoke a delegate. The state machine subscribes and reacts. Dependency flows one direction.

#### 7. Unused / Broken Interfaces
- `IAnimationState` — empty, nothing implements it
- `ICommand` — defined, never used
- `IPlayerState` — contains nested classes instead of methods (not a real interface)

**Fix:** Delete `IAnimationState` and `ICommand` until there's a real use case. Gut `IPlayerState` and make it an honest interface.

#### 8. Layer Management Fragmentation
`LayerMask.NameToLayer()` called in 17+ files. If a layer gets renamed, every one of those files breaks silently at runtime.

**Fix:** Centralize all layer lookups in `Utilities`. Other classes read the cached `LayerMask` from there, never call `LayerMask.NameToLayer()` themselves.

#### 9. Inconsistent Animation Base Classes
Some animation classes inherit `AnimationState` (empty), others inherit `AnimationStateAsync`. Some hash their animation parameter names, others pass string literals.

**Fix:** One base class. Always hash. Remove the empty `AnimationState` if it's just a marker.

#### 10. Resource Leaks
- `CandleItem._cancellationTokenSource` is not disposed if `ButtonUp` fires multiple times
- `GrapplingHookItem` tries to Kill() a DOTween animation that may already be killed
- `OverlapHookProjectile` creates two `OverlapHookSocketCheck` instances and only cleans up in `OnDestroy`

**Fix:** Wrap CTS creation in a null check + dispose pattern. Use `DOTween.Kill(target)` with the `complete` overload or null-guard. Prefer single ownership for dynamically created components.

---

### LOW

#### 11. Dead Code
- `Assets/Scripts/Old/PlayerMovement.cs` — references a `PlayerController` that no longer exists
- `PlayerAnimationController` class body is essentially empty
- `BaseState.OnCollisionEnter()` is never meaningfully implemented
- Commented-out block in `Slidable.cs` lines ~250–269
- `AddDetectionLayers()` in `OverlapMoveCheck` does nothing (lines ~27–30)

**Fix:** Delete them. Git has history if you ever need them back.

#### 12. Naming Inconsistencies
- Typo: `IExlosionDamage` (missing `p`)
- `Interact()` vs `InteractWithHookProjectile()` — inconsistent verb style on the same concept
- Some classes use the `Manager` suffix, others covering the same scope don't

---

## Modularity Improvements

### 1. Extract `OverlapCheckerBase`

```
OverlapCheckerBase (abstract)
  ├── geometry helpers (GetOverlappingArea, GetAABBCorners, DetermineMostOverlap)
  ├── abstract LayerMask GetLayers()
  └── abstract T FilterResults(Collider2D[])

OverlapObjectCheck   : OverlapCheckerBase<InteractableBase>
OverlapDoorCheck     : OverlapCheckerBase<DoorLocked>
OverlapHookCheck     : OverlapCheckerBase<HookSocket>
OverlapTargetCheck   : OverlapCheckerBase<KeyPort>
OverlapMoveCheck     : OverlapCheckerBase<Collider2D>
```

This one change eliminates ~500 lines of duplication and makes the overlap system easy to extend.

---

### 2. Player State Blackboard

Pull the shared references states need into one object:

```csharp
public class PlayerBlackboard
{
    public InputManager Input;
    public ItemManager Items;
    public PlayerMovementManager Movement;
    public PlayerStatus Status;
    // overlap checkers live here too
}
```

States receive the blackboard in their constructor. `PlayerStateMachineManager` owns only the state array and the current-state pointer.

---

### 3. Item Event Bus (Decouple Items from States)

Replace direct `stateManager.SwitchState(...)` calls inside items with:

```csharp
public static event Action<PlayerStateType> OnItemRequestStateChange;
```

Items fire the event. The state machine subscribes. Items no longer import state machine types.

---

### 4. Animation Clip Registry (ScriptableObject)

Replace the manually maintained `TimeSheet` dictionaries with a single `AnimationClipRegistry` ScriptableObject that maps clip names → durations. Animation states read from it. Adding a new clip means editing the asset, not the code.

---

### 5. Centralized Config ScriptableObject

```csharp
[CreateAssetMenu]
public class GameConfig : ScriptableObject
{
    public float playerWalkSpeed = 5f;
    public float hookProjectileSpeed = 8.5f;
    public float candleBurnDuration = 30f;
    // etc.
}
```

Inject via `[SerializeField]` on the relevant MonoBehaviours. Eliminates magic numbers and makes tuning possible without code changes.

---

### 6. Interactable System — Separate Physics from Behavior

Currently `Throwable` handles physics, `Openable` handles animation, and `Pickupable` handles both — all in the same inheritance layer. Consider flipping to composition:

```
InteractableBase
  └── [SerializeField] IInteractableBehavior[] behaviors
         ├── ThrowBehavior
         ├── OpenBehavior
         └── PickupBehavior
```

Each behavior is a small, testable component. The base only routes `Interact()` to the behaviors.

---

### 7. Layer Registry (static or ScriptableObject)

```csharp
public static class Layers
{
    public static readonly int Interactable = LayerMask.NameToLayer("Interactable");
    public static readonly int Ground       = LayerMask.NameToLayer("Ground");
    // ...
    public static readonly LayerMask InteractableMask = 1 << Interactable;
}
```

One initialization point. Fail fast at startup if a layer is missing.

---

## Priority Order

| Priority | Change | Effort | Impact |
|----------|--------|--------|--------|
| 1 | Extract `OverlapCheckerBase` | Medium | Eliminates 500 LOC duplication |
| 2 | Fix `async void` → proper return types | Low | Stops silent exception swallowing |
| 3 | Delete dead code | Very Low | Reduces noise immediately |
| 4 | Centralize layer lookups | Low | Prevents silent runtime breakage |
| 5 | Player state blackboard | Medium | Makes state machine maintainable |
| 6 | Item event bus | Low | Decouples items from state machine |
| 7 | GameConfig ScriptableObject | Low | Eliminates magic numbers |
| 8 | Animation clip registry | Medium | Centralizes animation timing data |
| 9 | Fix `IExlosionDamage` typo | Trivial | Correctness |
| 10 | Composition-based interactables | High | Long-term extensibility |
