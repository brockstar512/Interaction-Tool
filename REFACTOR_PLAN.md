# Refactor Plan — Interacting Tool
Each phase is a safe, self-contained commit. Do them in order; later phases depend on earlier ones.

---

## Why you have both `ChestPickable` and `OpenableChest`

`ChestPickable` inherits `Pickupable` — it is a world object that holds one `IItem` and hands it to the player's inventory. The open animation is cosmetic on top of a pickup flow.

`OpenableChest` inherits `Openable` — it is a world object that checks for a key and plays an open animation. It grants nothing; it just opens.

Your stated intent is: *chest = extensible reward container, optionally locked.* Neither class represents that cleanly. Phase 8 unifies them.

---

## Phase 1 — Dead code and typo fixes
**Risk:** zero. Rename a file, delete files, done.

### 1a. Fix `IExlosionDamage` typo

Delete `Assets/Scripts/Interface/IExlosionDamage.cs` and create `IExplosionDamage.cs`:

```csharp
// Assets/Scripts/Interface/IExplosionDamage.cs
namespace Explode
{
    public interface IExplosionDamage
    {
        void AnimateExplosion();
    }
}
```

Open `Assets/Scripts/Interactable/SubInteractables/BombThrowableSubitems/ExplosionDamage.cs` and change:
```csharp
// old
public class ExplosionDamage : MonoBehaviour, IExlosionDamage
// new
public class ExplosionDamage : MonoBehaviour, IExplosionDamage
```

### 1b. Delete unused interfaces

Delete these files outright — nothing implements or calls them:
- `Assets/Scripts/Interface/IAnimationState.cs`
- `Assets/Scripts/Interface/ICommand.cs`

### 1c. Delete dead scripts

- `Assets/Scripts/Player/PlayerMovementManager/Old/PlayerMovement.cs` (whole `Old/` folder)
- `Assets/Scripts/Animation/PlayerAnimation/PlayerAnimationController.cs` (empty class body)

### 1d. Delete `AnimationState.cs`

`Assets/Scripts/Animation/PlayerAnimation/AnimationStates/AnimationState.cs` contains only a commented-out method. Nothing inherits from it. Delete it.

---

## Phase 2 — Centralize layer lookups

Right now `LayerMask.NameToLayer(Utilities.InteractableLayer)` and similar calls appear scattered across overlap scripts and `InteractableBase`. Any layer rename breaks them silently at runtime.

Create `Assets/Scripts/Utils/Layers.cs`:

```csharp
using UnityEngine;

public static class Layers
{
    public static readonly int Interactable = LayerMask.NameToLayer(Utilities.InteractableLayer);
    public static readonly LayerMask InteractableMask = 1 << Interactable;
}
```

Then replace every `LayerMask.NameToLayer(Utilities.InteractableLayer)` with `Layers.Interactable` and every `1 << LayerMask.NameToLayer(Utilities.InteractableLayer)` with `Layers.InteractableMask`.

Files to update:
- `Assets/Scripts/Interactable/InteractableBase.cs` — `UpdateLayerName()`
- `Assets/Scripts/OverlapScripts/OverlapObjectCheck.cs` — `AddDetectionLayers()`
- `Assets/Scripts/OverlapScripts/OverlapDoorCheck.cs` — same pattern
- Any other overlap script with a matching call

`InteractableBase.UpdateLayerName()` becomes:
```csharp
protected void UpdateLayerName()
{
    gameObject.layer = Layers.Interactable;
}
```

---

## Phase 3 — Fix `ILocked` (remove `Task<bool>`)

`ILocked.CanOpen` is declared `Task<bool>` but it is called with `await` inside `KeyItem.Use`, making the entire use path async. There is no I/O or real async work — it's just a flag check. Sync is correct here.

### `Assets/Scripts/Interface/ILocked.cs`
```csharp
public interface ILocked
{
    bool CanOpen(Utilities.KeyTypes keyType);
}
```

### `Assets/Scripts/Doors/Locked.cs`
Change every `async Task<bool> CanOpen` override to `bool CanOpen`. Remove `async` keyword and any `await` inside. Example:
```csharp
public override bool CanOpen(Utilities.KeyTypes keyType)
{
    return keyType == _requiredKey;
}
```

### `Assets/Scripts/Doors/DoorLocked.cs` and `MasterLockDoubleLocked.cs`
Same pattern — remove `async`/`await`, return `bool` directly.

### `Assets/Scripts/Items/Scriptable object scripts for items/KeyItem.cs`
The `Use` method currently awaits `CanOpen`. Remove the await and call it as a plain bool:
```csharp
// before
if (await door.CanOpen(key.keyType)) { ... }

// after
if (door.CanOpen(key.keyType)) { ... }
```
If `Use` itself was marked `async` only because of this, remove the `async` keyword from `Use` too.

---

## Phase 4 — Fix `async void` in state classes

`async void` swallows exceptions silently and has no awaitable handle. Unity 6 ships `Awaitable` as the correct alternative. Use it for all three states.

### `EquipItemState.cs`
```csharp
public override async Awaitable EnterState(PlayerStateMachineManager stateManager)
{
    if (!stateManager.item.Interact(stateManager))
    {
        stateManager.SwitchState(stateManager.defaultState);
        return;
    }
    try
    {
        await EquipItemAnimation.Play(stateManager);
    }
    catch (System.OperationCanceledException) { }
    stateManager.SwitchState(stateManager.defaultState);
}
```

### `SlideItemState.cs`
```csharp
public override async Awaitable Action(PlayerStateMachineManager stateManager)
{
    try
    {
        if (stateManager.item.Interact(stateManager))
            await KickAnimation.Play(stateManager);
        else
        {
            await KickAnimation.Play(stateManager);
            await HurtToeAnimation.Play(stateManager);
        }
    }
    catch (System.OperationCanceledException) { }
    stateManager.SwitchState(stateManager.defaultState);
}
```

### `ThrowItemState.cs`
```csharp
public override async Awaitable EnterState(PlayerStateMachineManager stateManager)
{
    _currentAnimation = _pickUpAnimation;
    try { await _pickUpAnimation.Play(stateManager); }
    catch (System.OperationCanceledException) { return; }
    stateManager.item.Interact(stateManager);
    _currentAnimation = _carryAnimation;
}

public override async Awaitable Action(PlayerStateMachineManager stateManager)
{
    _currentAnimation = _throwAnimation;
    stateManager.item.Release(stateManager);
    try { await _throwAnimation.Play(stateManager); }
    catch (System.OperationCanceledException) { }
    _currentAnimation = null;
    stateManager.SwitchState(stateManager.defaultState);
}
```

**For each of these to compile**, `AnimationStateAsync.Play` must also return `Awaitable` (or keep returning `Task` — either works with `await`). If you keep `Task`, the try/catch still applies. `Awaitable` is preferred for Unity because it integrates with the engine's cancellation model.

---

## Phase 5 — Collapse animation base classes

You have two base classes:
- `AnimationState` — empty abstract class (deleted in Phase 1)
- `AnimationStateAsync` — `abstract Task Play(PlayerStateMachineManager)`

Rename `AnimationStateAsync` to `AnimationBase` so the name is neutral about sync/async, and change the return type to `Awaitable` to match Phase 4:

### `Assets/Scripts/Animation/PlayerAnimation/AnimationStates/AnimationBase.cs` (rename from `AnimationStateAsync.cs`)
```csharp
public abstract class AnimationBase
{
    public abstract Awaitable Play(PlayerStateMachineManager stateMachine);
}
```

All animation classes that currently inherit `AnimationStateAsync` change their base class declaration to `: AnimationBase`. Their `Play` signatures change from `Task` to `Awaitable`.

---

## Phase 6 — Extract `OverlapCheckerBase<T>`

Five overlap scripts (`OverlapObjectCheck`, `OverlapDoorCheck`, `OverlapMoveCheck`, `OverlapTargetCheck`, `OverlapHookCheck`) contain near-identical implementations of:
- `SetMovingOverlappingArea(Vector2)`
- `GetMostOverlappedCol()`
- `DetermineMostOverlap(Collider2D[])`
- `GetOverlappingArea(Collider2D)`
- `GetAABBCorners(Collider2D)`
- `OnDrawGizmos()`

Create `Assets/Scripts/OverlapScripts/OverlapCheckerBase.cs`:

```csharp
using UnityEngine;

public abstract class OverlapCheckerBase<T> : MonoBehaviour where T : class
{
    protected Vector2 _areaTopRight, _areaBottomLeft;
    [SerializeField] protected LayerMask detectionLayer;
    private SpriteRenderer _sr;

    protected virtual void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        detectionLayer |= Layers.InteractableMask;
    }

    protected void SetOverlapArea()
    {
        var b = _sr.bounds;
        _areaTopRight    = new Vector2(b.center.x + b.extents.x, b.center.y + b.extents.y);
        _areaBottomLeft  = new Vector2(b.center.x - b.extents.x, b.center.y - b.extents.y);
    }

    protected Collider2D GetMostOverlappedCollider()
    {
        var cols = Physics2D.OverlapAreaAll(_areaTopRight, _areaBottomLeft, detectionLayer);
        if (cols.Length == 0) return null;
        Collider2D best = cols[0];
        float bestArea = 0f;
        foreach (var col in cols)
        {
            float area = GetOverlapArea(col);
            if (area > bestArea) { bestArea = area; best = col; }
        }
        return best;
    }

    private float GetOverlapArea(Collider2D col)
    {
        var b = col.bounds;
        var tr = new Vector2(b.center.x + b.extents.x, b.center.y + b.extents.y);
        var bl = new Vector2(b.center.x - b.extents.x, b.center.y - b.extents.y);
        float xLen = Mathf.Min(_areaTopRight.x, tr.x) - Mathf.Max(_areaBottomLeft.x, bl.x);
        float yLen = Mathf.Min(_areaTopRight.y, tr.y) - Mathf.Max(_areaBottomLeft.y, bl.y);
        return xLen * yLen;
    }

    protected void OnDrawGizmos()
    {
        CustomDebug.DrawRectange(_areaTopRight, _areaBottomLeft);
    }

    public abstract T GetOverlapObject(Vector2 characterPos, Vector2 lookDirection);
}
```

Then `OverlapObjectCheck` becomes:
```csharp
public class OverlapObjectCheck : OverlapCheckerBase<InteractableBase>, IGetMostOverlap<InteractableBase>
{
    readonly OverlapCheckHelper _helper = new();

    public override InteractableBase GetOverlapObject(Vector2 characterPos, Vector2 lookDirection)
    {
        transform.localScale    = _helper.UpdateScale(lookDirection);
        transform.localPosition = _helper.UpdatePosition(lookDirection);
        SetOverlapArea();
        return GetMostOverlappedCollider()?.GetComponent<InteractableBase>();
    }

    // OverlapCheckHelper inner class stays here — it's specific to this detector
}
```

Apply the same pattern to the other four overlap scripts, each keeping only their unique logic.

---

## Phase 7 — Move bounce ownership to `ItemManager`

The comment in `ChestPickable.Swap` is correct: the chest should not own drop-holder instantiation logic. `ItemManager` owns item lifecycle; it should own displacement too.

### Step 1: Update `IItemPickUp`

```csharp
// Assets/Scripts/Items/IItemPickUp.cs
using UnityEngine;

public interface IItemPickUp
{
    Sprite Sprite { get; }
    IItem item { get; }
    bool AcceptsDisplaced { get; }   // NEW: true = swap item in-place; false = chest, spawn new holder
    void Swap(IItem displaced);
    void PickedUp();
}
```

### Step 2: Update `Pickupable`

Add the property — ground holders keep the displaced item in-place:
```csharp
public virtual bool AcceptsDisplaced => true;
```

### Step 3: Update `ChestPickable`

Return false and remove all spawn logic from `Swap`:
```csharp
public override bool AcceptsDisplaced => false;

public override void Swap(IItem displaced)
{
    // ItemManager handles displacement when AcceptsDisplaced is false
    PickedUp();
}
```

### Step 4: Update `ItemManager`

Move `_droppedItemHolderPrefab` and `_dropForce` here:

```csharp
public class ItemManager : MonoBehaviour, IItemManager
{
    [SerializeField] private Pickupable _dropHolderPrefab;
    [SerializeField] private float _dropForce = 3f;
    // ... existing fields ...

    public void PickUpItem(IItemPickUp holder)
    {
        ItemSwitch?.Invoke(holder.Sprite);
        var pickup = holder.item;
        pickup.TakeChild(transform);

        if (inventory.Count >= inventoryLimit)
        {
            var displaced = inventory[_currentIndex] as Item;
            inventory.RemoveAt(_currentIndex);

            if (holder.AcceptsDisplaced)
            {
                holder.Swap(displaced);
            }
            else
            {
                SpawnDropHolder(displaced, (holder as MonoBehaviour).transform.position);
                holder.PickedUp();
            }

            inventory.Insert(_currentIndex, pickup);
            return;
        }

        inventory.Add(pickup);
        _currentIndex = inventory.IndexOf(pickup);
        holder.PickedUp();
    }

    private void SpawnDropHolder(Item item, Vector3 position)
    {
        if (_dropHolderPrefab == null) return;
        var holder = Instantiate(_dropHolderPrefab, position, Quaternion.identity);
        holder.InitAsDropHolder(item);
        holder.rb.AddForce(Random.insideUnitCircle.normalized * _dropForce, ForceMode2D.Impulse);
    }
}
```

After this phase, **remove `_droppedItemHolderPrefab` and `_dropForce` from `ChestPickable`** and re-assign them on the `ItemManager` component in the Inspector.

---

## Phase 8 — Merge `ChestPickable` + `OpenableChest` → `Chest` + `IChestReward`

### New interface

```csharp
// Assets/Scripts/Interface/IChestReward.cs
public interface IChestReward
{
    void Grant(PlayerStateMachineManager player);
}
```

### New `ItemReward` component

This replaces the "item child of chest" pattern. Attach it as a child of the Chest prefab alongside the inactive `Item` child.

```csharp
// Assets/Scripts/Interactable/Rewards/ItemReward.cs
using UnityEngine;

public class ItemReward : MonoBehaviour, IChestReward
{
    private IItem _item;

    private void Awake()
    {
        _item = GetComponentInChildren<IItem>(includeInactive: true);
    }

    public void Grant(PlayerStateMachineManager player)
    {
        if (_item == null) return;
        // Reuse the Pickupable drop-holder flow by routing through ItemManager
        player.itemManager.PickUpDirectItem(_item, transform.position);
    }
}
```

You'll need a new method on `ItemManager` (and `IItemManager`) for direct item grant (bypasses the holder):

```csharp
// In IItemManager.cs — add:
void PickUpDirectItem(IItem item, Vector3 worldPosition);

// In ItemManager.cs — add:
public void PickUpDirectItem(IItem item, Vector3 worldPosition)
{
    item.TakeChild(transform);
    ItemSwitch?.Invoke(item.Sprite);

    if (inventory.Count >= inventoryLimit)
    {
        var displaced = inventory[_currentIndex] as Item;
        inventory.RemoveAt(_currentIndex);
        SpawnDropHolder(displaced, worldPosition);
        inventory.Insert(_currentIndex, item);
        return;
    }

    inventory.Add(item);
    _currentIndex = inventory.IndexOf(item);
}
```

### New `Chest` class

```csharp
// Assets/Scripts/Interactable/Chest.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Chest : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes _requiredKey = Utilities.KeyTypes.None;

    private IChestReward[] _rewards;
    private ChestOpenAnimation _openAnimation;
    private bool _opened = false;

    private void Awake()
    {
        UpdateLayerName();
        _openAnimation = new ChestOpenAnimation(GetComponent<Animator>());
        _rewards = GetComponentsInChildren<IChestReward>(includeInactive: true);
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        if (_opened) return false;
        if (_rewards.Length == 0) return false;

        if (_requiredKey != Utilities.KeyTypes.None)
        {
            if (player.itemManager.GetItem() is not Key key || key.keyType != _requiredKey)
                return false;
            key.Use(player);
        }

        _opened = true;
        _openAnimation.Play();
        foreach (var reward in _rewards)
            reward.Grant(player);
        return true;
    }

    public override void Release(PlayerStateMachineManager player) { }

    public override PlayerBaseState GetInteractionState(PlayerStateMachineManager sm)
        => sm.openItemState;
}
```

**In the Unity Editor:** replace the `ChestPickable` and `OpenableChest` components on your chest prefabs with `Chest`. Add an `ItemReward` child GameObject to the prefab and put the item child under that. Set `_requiredKey` on `Chest` if it was previously set on `Openable`.

**Delete:** `ChestPickable.cs` and `OpenableChest.cs` once the prefabs are migrated.

---

## Phase 9 — Eliminate `DefaultState` type-switch

The switch in `DefaultState.Action` hardcodes which interactable class maps to which player state. Every new interactable type requires editing `DefaultState`. Push that knowledge into the interactable itself.

### Step 1: Add `GetInteractionState` to `InteractableBase`

```csharp
// InteractableBase.cs — add abstract method:
public abstract PlayerBaseState GetInteractionState(PlayerStateMachineManager sm);
```

### Step 2: Implement in each concrete interactable

```csharp
// Throwable.cs
public override PlayerBaseState GetInteractionState(PlayerStateMachineManager sm) => sm.throwItemState;

// Moveable.cs
public override PlayerBaseState GetInteractionState(PlayerStateMachineManager sm) => sm.moveItemState;

// Slidable.cs
public override PlayerBaseState GetInteractionState(PlayerStateMachineManager sm) => sm.slideItemState;

// Pickupable.cs
public override PlayerBaseState GetInteractionState(PlayerStateMachineManager sm) => sm.equipItemState;

// Openable.cs
public override PlayerBaseState GetInteractionState(PlayerStateMachineManager sm) => sm.OpenItemState;

// Chest.cs — already added in Phase 8
public override PlayerBaseState GetInteractionState(PlayerStateMachineManager sm) => sm.openItemState;
```

### Step 3: Simplify `DefaultState.Action`

```csharp
public override void Action(PlayerStateMachineManager stateManager)
{
    if (stateManager.item == null)
    {
        Debug.Log("no item to interact with");
        return;
    }
    stateManager.SwitchState(stateManager.item.GetInteractionState(stateManager));
}
```

The entire `switch` block is deleted.

---

## Phase 10 — `CandleItem` disposal fix

`CandleItem` creates a `CancellationTokenSource` on each `ButtonUp` call but never disposes the previous one. Multiple rapid presses leak tokens.

In `CandleItem.cs`, store the token source as a field and dispose before replacing:

```csharp
private CancellationTokenSource _cts;

public void ButtonUp(PlayerStateMachineManager player)
{
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = new CancellationTokenSource();
    StartTimer(_cts.Token).Forget(); // or however you fire it
}

private void OnDestroy()
{
    _cts?.Cancel();
    _cts?.Dispose();
}
```

---

## Phase 11 — `GameConfig` ScriptableObject (quality-of-life, do last)

Magic numbers scattered across the codebase (`inventoryLimit = 2`, `_dropForce = 3f`, `yOffset`, timing values) should be designer-tunable. Create a ScriptableObject singleton:

```csharp
// Assets/Scripts/Utils/GameConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Inventory")]
    public int inventoryLimit = 2;

    [Header("Item Drop")]
    public float dropForce = 3f;

    [Header("Depth Sort")]
    public float playerYOffset = 0f;
    public float enemyYOffset  = 0f;
    public float doorYOffset   = 0.5f;

    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<GameConfig>("GameConfig");
            return _instance;
        }
    }
}
```

Create the asset at `Assets/Resources/GameConfig.asset`. Then replace each magic number with `GameConfig.Instance.fieldName`.

---

## Commit order summary

| Phase | Files touched | Safe to merge alone? |
|-------|--------------|----------------------|
| 1 | IExlosionDamage → IExplosionDamage, delete 4 files | Yes |
| 2 | New Layers.cs, InteractableBase, overlap scripts | Yes |
| 3 | ILocked, Locked, DoorLocked, MasterLockDoubleLocked, KeyItem | Yes |
| 4 | EquipItemState, SlideItemState, ThrowItemState | Yes |
| 5 | Rename AnimationStateAsync → AnimationBase, update all animation classes | Yes |
| 6 | New OverlapCheckerBase, update 5 overlap scripts | Yes |
| 7 | IItemPickUp, Pickupable, ChestPickable, ItemManager | Yes |
| 8 | New Chest.cs, new ItemReward.cs, IChestReward.cs, IItemManager, ItemManager, Editor prefab work | Yes — prefab work after code |
| 9 | InteractableBase, all interactables, DefaultState | Yes — depends on Phase 8 |
| 10 | CandleItem | Yes |
| 11 | New GameConfig.cs, multiple files | Yes — do last |
