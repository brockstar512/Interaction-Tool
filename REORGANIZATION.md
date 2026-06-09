# Project Reorganization Suggestions

> **Read-only audit** — no files were modified. All suggestions below are proposals only.

---

## Namespace Root

Recommend a single short root: **`IT`** (Interacting Tool).  
All namespaces below are prefixed with `IT.` — swap the root for any name you prefer.

---

## Proposed Folder Structure

```
Assets/
├── Art/
│   ├── Animations/            ← Unity AnimatorController / AnimationClip assets
│   ├── Sprites/
│   │   ├── Characters/
│   │   ├── Items/
│   │   ├── UI/
│   │   └── World/
│   └── UI/
├── Prefabs/
│   ├── Characters/
│   │   ├── Player/
│   │   └── Enemies/
│   ├── Effects/
│   ├── Interactables/
│   │   ├── Blocks/
│   │   ├── Chests/
│   │   ├── Doors/
│   │   ├── Levers/
│   │   └── Locks/
│   ├── Items/
│   │   ├── Collectibles/
│   │   └── SubItems/
│   └── World/
├── Scenes/
│   ├── Main/
│   └── Prototypes/
└── Scripts/
    ├── Animation/
    │   ├── Player/
    │   │   └── States/
    │   └── World/
    ├── Core/
    │   ├── Dependency/
    │   ├── StateMachine/
    │   └── Utilities/
    ├── Editor/
    ├── Effects/
    │   └── Flash/
    ├── Enemies/
    ├── Interactables/
    │   ├── Chests/
    │   ├── Doors/
    │   ├── Locks/
    │   ├── Moveable/
    │   ├── Pullable/
    │   ├── Slidable/
    │   └── Throwable/
    ├── Items/
    │   ├── Bell/
    │   ├── Candle/
    │   ├── GrapplingHook/
    │   └── Weapons/
    ├── Overlap/
    ├── Player/
    │   ├── HUD/
    │   ├── Input/
    │   ├── Inventory/
    │   ├── Movement/
    │   ├── StateMachine/
    │   │   └── States/
    │   └── Status/
    └── Prototype/
```

---

## Script Reorganization

### Core — Dependency System

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `DependenyWorkFlow/DependencySource.cs` | `DependencySource<T>` | `Core/Dependency/` | `DependencySource<T>` | `IT.Core.Dependency` |
| `DependenyWorkFlow/Dependent.cs` | `Dependent<T>` | `Core/Dependency/` | `Dependent<T>` | `IT.Core.Dependency` |
| `DependenyWorkFlow/MultiDependent.cs` | `MultiDependent<T>` | `Core/Dependency/` | `MultiDependent<T>` | `IT.Core.Dependency` |
| `DependenyWorkFlow/Observable.cs` | `Observable<T>` | `Core/Dependency/` | `Observable<T>` | `IT.Core.Dependency` |
| `Interface/IDependencySource.cs` | `IDependencySource<T>` | `Core/Dependency/` | `IDependencySource<T>` | `IT.Core.Dependency` |

> **Note:** Fix the typo in the current folder name — `DependenyWorkFlow` → `Dependency`.

---

### Core — State Machine (Base)

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `StateMachine/State Machine/BaseState.cs` | `BaseState` | `Core/StateMachine/` | `StateBase` | `IT.Core.StateMachine` |
| `Interface/IStateMachine.cs` | `IStateMachine` | `Core/StateMachine/` | `IStateMachine` | `IT.Core.StateMachine` |

---

### Core — Utilities

| Current Path | Current Name | New Path | New Name | Namespace | Notes |
|---|---|---|---|---|---|
| `Utils/Singleton.cs` | `Singleton<T>` | `Core/Utilities/` | `Singleton<T>` | `IT.Core.Utilities` | |
| `Utils/Layers.cs` | `Layers` | `Core/Utilities/` | `LayerIndex` | `IT.Core.Utilities` | `Layers` is vague; `LayerIndex` states intent |
| `Utils/Utilities.cs` | `Utilities` | `Core/Utilities/` | `GameUtilities` | `IT.Core.Utilities` | Avoid naming a class the same as its namespace |
| `Utils/YDepthSort.cs` | `YDepthSort` | `Core/Utilities/` | `DepthSorter` | `IT.Core.Utilities` | Noun form matches MonoBehaviour convention |
| `SnapToGrid/LockToGrid.cs` | `LockToGrid` | `Core/Utilities/` | `GridSnapper` | `IT.Core.Utilities` | |
| `OverlapScripts/CustomDebug.cs` | `CustomDebug` | `Core/Utilities/` | `DebugDraw` | `IT.Core.Utilities` | |

---

### Editor Tools

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `SpritePivotValidator.cs` | `SpritePivotValidator` | `Editor/` | `SpritePivotValidator` | `IT.Editor` |
| `Interface/InterfaceReferenceDrawer.cs` | `InterfaceReferenceDrawer` | `Editor/` | `InterfaceRefDrawer` | `IT.Editor` |

---

### Interfaces — Core Contracts

These are scattered in `Interface/`. Move them beside the systems that own them (see each domain below), except for the two truly cross-cutting ones:

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interface/IDamage.cs` | `IDamage` | `Core/` | `IDamage` | `IT.Core` |
| `Interface/IHurt.cs` | `IHurt` | `Core/` | `IDamageable` | `IT.Core` |
| `Interface/InterfaceReference.cs` | `InterfaceReference<T>` | `Core/` | `InterfaceRef<T>` | `IT.Core` |

> **Note:** `IHurt` → `IDamageable` aligns with the standard Unity/C# community naming (the thing that _can be_ damaged).

---

### Overlap Detection

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `OverlapScripts/OverlapAreaChecker.cs` | `OverlapAreaChecker` | `Overlap/` | `OverlapCheckerBase` | `IT.Overlap` |
| `OverlapScripts/OverlapMostObjectChecker.cs` | `OverlapMostObjectChecker<T>` | `Overlap/` | `ClosestOverlapChecker<T>` | `IT.Overlap` |
| `OverlapScripts/OverlapObjectCheck.cs` | `OverlapObjectCheck` | `Overlap/` | `InteractableOverlap` | `IT.Overlap` |
| `OverlapScripts/OverlapMoveCheck.cs` | `OverlapMoveCheck` | `Overlap/` | `MovementOverlap` | `IT.Overlap` |
| `OverlapScripts/OverlapMoveDamageCheck.cs` | `OverlapMoveDamageCheck` | `Overlap/` | `DamageOverlap` | `IT.Overlap` |
| `OverlapScripts/OverlapRopeObstructionCheck.cs` | `OverlapRopeObstructionCheck` | `Overlap/` | `RopeObstructionOverlap` | `IT.Overlap` |
| `OverlapScripts/OverlapTargetCheck.cs` | `OverlapTargetCheck` | `Overlap/` | `KeyPortOverlap` | `IT.Overlap` |
| `OverlapScripts/OverlapHookCheck.cs` | `OverlapHookCheck` | `Overlap/` | `GrappleTargetOverlap` | `IT.Overlap` |
| `OverlapScripts/OverlapHookSocketCheck.cs` | `OverlapHookSocketCheck` | `Overlap/` | `GrappleSocketOverlap` | `IT.Overlap` |
| `Interface/IGetAllOverlap.cs` | `IGetAllOverlap<T>` | `Overlap/` | `IAllOverlap<T>` | `IT.Overlap` |
| `Interface/IGetMostOverlap.cs` | `IGetMostOverlap<T>` | `Overlap/` | `IBestOverlap<T>` | `IT.Overlap` |

---

### Effects — Flash

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Flashing/ObjectFlash.cs` | `ObjectFlash` | `Effects/Flash/` | `FlashBase` | `IT.Effects.Flash` |
| `Flashing/CharacterFlash.cs` | `CharacterFlash` | `Effects/Flash/` | `CharacterFlash` | `IT.Effects.Flash` |
| `Flashing/BombFlash.cs` | `BombFlash` | `Effects/Flash/` | `BombFlash` | `IT.Effects.Flash` |
| `Interface/IFlash.cs` | `IFlash` | `Effects/Flash/` | `IFlashable` | `IT.Effects.Flash` |

---

### Enemies

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `AI/EnemyPlaceholder.cs` | `EnemyPlaceholder` | `Enemies/` | `EnemyDummy` | `IT.Enemies` |

> **Note:** Rename the folder from `AI/` to `Enemies/`. When real AI is added, an `Enemies/AI/` subfolder can be introduced.

---

### Interactables — Base

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/InteractableBase.cs` | `InteractableBase` | `Interactables/` | `Interactable` | `IT.Interactables` |
| `Interactable/InteractionKind.cs` | `InteractionKind` | `Interactables/` | `InteractionType` | `IT.Interactables` |
| `Interface/IOpenEffect.cs` | `IOpenEffect` | `Interactables/` | `IOpenEffect` | `IT.Interactables` |
| `Interface/IButtonUp.cs` | `IButtonUp` | `Interactables/` | `IButtonUp` | `IT.Interactables` |
| `Interface/InteractionKit.cs` | `InteractionKit` | `Interactables/` | `InteractionContext` | `IT.Interactables` |

> **Note:** `InteractionType` over `InteractionKind` — "Kind" is non-standard in C# enums; "Type" is the conventional suffix. `InteractionContext` over `InteractionKit` — Kit implies a toolbox; Context accurately describes the data carrier.

---

### Interactables — Doors

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/Openable/Openable.cs` | `Openable` | `Interactables/Doors/` | `OpenableBase` | `IT.Interactables.Doors` |
| `Doors/OpenableDoor.cs` | `OpenableDoor` | `Interactables/Doors/` | `SingleDoor` | `IT.Interactables.Doors` |
| `Doors/OpenableDoubleDoor.cs` | `OpenableDoubleDoor` | `Interactables/Doors/` | `DoubleDoor` | `IT.Interactables.Doors` |
| `Doors/MultipleDependentDoubleDoor.cs` | `MultipleDependentDoubleDoor` | `Interactables/Doors/` | `MultiKeyDoubleDoor` | `IT.Interactables.Doors` |

> **Note:** `OpenableBase` keeps clear it's abstract. `SingleDoor`/`DoubleDoor`/`MultiKeyDoubleDoor` are shorter and describe what the object _is_, not what it _can do_.

---

### Interactables — Chests

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/Openable/OpenableChest.cs` | `OpenableChest` | `Interactables/Chests/` | `Chest` | `IT.Interactables.Chests` |
| `Interactable/Openable/GiveItemEffect.cs` | `GiveItemEffect` | `Interactables/Chests/` | `SpawnItemOnOpen` | `IT.Interactables.Chests` |

---

### Interactables — Moveable (Push Blocks)

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/Moveable/Moveable.cs` | `Moveable` | `Interactables/Moveable/` | `PushBlock` | `IT.Interactables.Moveable` |
| `Interface/IPushDirection.cs` | `IPushDirection` | `Interactables/Moveable/` | `IPushInput` | `IT.Interactables.Moveable` |

---

### Interactables — Pullable (Levers)

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/Pullable/Pullable.cs` | `Pullable` | `Interactables/Pullable/` | `PullableBase` | `IT.Interactables.Pullable` |
| `Interactable/Pullable/PullableLatched.cs` | `PullableLatched` | `Interactables/Pullable/` | `LatchedLever` | `IT.Interactables.Pullable` |
| `Interactable/Pullable/PullableRetractable.cs` | `PullableRetractable` | `Interactables/Pullable/` | `RetractingLever` | `IT.Interactables.Pullable` |
| `Interactable/SubInteractables/Pullable/PullableMoveDependent.cs` | `PullableMoveDependent` | `Interactables/Pullable/` | `PullDrivenMover` | `IT.Interactables.Pullable` |
| `Interactable/SubInteractables/Pullable/PullableOpenDependent.cs` | `PullableOpenDependent` | `Interactables/Pullable/` | `PullDrivenDoor` | `IT.Interactables.Pullable` |

> **Note:** `LatchedLever` / `RetractingLever` describe the game-world object clearly. The "Dependent" sub-scripts are implementation details and should live beside the lever system that owns them.

---

### Interactables — Slidable (Sliding Blocks)

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/Slidable/Slidable.cs` | `Slidable` | `Interactables/Slidable/` | `SlidableBase` | `IT.Interactables.Slidable` |
| `KeyPads/SlidableBasic.cs` | `SlidableBasic` | `Interactables/Slidable/` | `BasicSlidingBlock` | `IT.Interactables.Slidable` |
| `KeyPads/SlidableSymbol.cs` | `SlidableSymbol` | `Interactables/Slidable/` | `SymbolSlidingBlock` | `IT.Interactables.Slidable` |
| `KeyPads/SlidableTimed.cs` | `SlidableTimed` | `Interactables/Slidable/` | `TimedSlidingBlock` | `IT.Interactables.Slidable` |
| `SlideableObstruction/Obstruction.cs` | `Obstruction` | `Interactables/Slidable/` | `SlideObstruction` | `IT.Interactables.Slidable` |

> **Note:** `KeyPads/` is a misleading folder name — these are sliding blocks, not keypads. The actual lock ports live below.

---

### Interactables — Locks (Key Ports)

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `KeyPads/KeyPort.cs` | `KeyPort` | `Interactables/Locks/` | `KeyPortBase` | `IT.Interactables.Locks` |
| `KeyPads/SlidableKeyPort.cs` | `SlidableKeyPort` | `Interactables/Locks/` | `SlidingKeyPort` | `IT.Interactables.Locks` |
| `KeyPads/SymbolKeyPort.cs` | `SymbolKeyPort` | `Interactables/Locks/` | `SymbolKeyPort` | `IT.Interactables.Locks` |

---

### Interactables — Throwable / Bomb

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/Throwable/Throwable.cs` | `Throwable` | `Interactables/Throwable/` | `ThrowableBase` | `IT.Interactables.Throwable` |
| `Interactable/Throwable/BombThrowable.cs` | `BombThrowable` | `Interactables/Throwable/` | `Bomb` | `IT.Interactables.Throwable` |
| `Interactable/SubInteractables/BombThrowableSubitems/BombExplode.cs` | `BombExplode` | `Interactables/Throwable/` | `BombTimer` | `IT.Interactables.Throwable` |
| `Interactable/SubInteractables/BombThrowableSubitems/Explosion.cs` | `Explosion` | `Interactables/Throwable/` | `ExplosionEffect` | `IT.Interactables.Throwable` |
| `Interactable/SubInteractables/BombThrowableSubitems/ExplosionDamage.cs` | `ExplosionDamage` | `Interactables/Throwable/` | `ExplosionDamageArea` | `IT.Interactables.Throwable` |
| `Interface/IExplosionDamage.cs` | `IExplosionDamage` | `Interactables/Throwable/` | `IExplosionEffect` | `IT.Interactables.Throwable` |

> **Note:** `BombTimer` is clearer than `BombExplode` — the class _starts a countdown_, it doesn't do the exploding. `ExplosionDamageArea` clarifies it is the area-of-effect component.

---

### Interactables — Pickupable

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Interactable/Pickupable/Pickupable.cs` | `Pickupable` | `Items/` | `PickupHolder` | `IT.Items` |
| `Items/IItemPickUp.cs` | `IItemPickUp` | `Items/` | `IItemHolder` | `IT.Items` |

> **Note:** `Pickupable` is a MonoBehaviour placed on world pickups. Moving it beside the Items system (rather than Interactables) clarifies its purpose. `IItemHolder` reads more naturally than `IItemPickUp`.

---

### Items — Base

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Items/Item.cs` | `Item` | `Items/` | `ItemBase` | `IT.Items` |
| `Items/IItem.cs` | `IItem` | `Items/` | `IItem` | `IT.Items` |
| `Interface/IItemManager.cs` | `IItemManager` | `Items/` | `IInventory` | `IT.Items` |

> **Note:** `ItemBase` (abstract base pattern) over bare `Item` to avoid confusion in code like `new Item()`. `IInventory` over `IItemManager` — the interface describes what it manages, not how it manages it.

---

### Items — Key & General Items

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Items/Scriptable object scripts for items/KeyItem.cs` | `KeyItem` | `Items/` | `KeyItem` | `IT.Items` |
| `Items/Scriptable object scripts for items/PlankItem.cs` | `PlankItem` | `Items/` | `PlankItem` | `IT.Items` |

> **Note:** Fix the folder name — `Scriptable object scripts for items` → `Items/` (they all live in the same domain already).

---

### Items — Bell

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Items/Scriptable object scripts for items/BellItem.cs` | `BellItem` | `Items/Bell/` | `BellItem` | `IT.Items.Bell` |
| `Items/SubItems/BellSubItems/OverlapCircleCollider.cs` | `OverlapCircleCollider` | `Items/Bell/` | `BellRingDetector` | `IT.Items.Bell` |
| `Interface/IBellSound.cs` | `IBellSound` | `Items/Bell/` | `IBellRinger` | `IT.Items.Bell` |

> **Note:** `OverlapCircleCollider` is a purely mechanical name — `BellRingDetector` says what it does in game context.

---

### Items — Candle

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Items/Scriptable object scripts for items/CandleItem.cs` | `CandleItem` | `Items/Candle/` | `CandleItem` | `IT.Items.Candle` |
| `Items/SubItems/CandleSubItems/CandleLight.cs` | `CandleLight` | `Items/Candle/` | `CandleLightController` | `IT.Items.Candle` |
| `Interface/ICandleLight.cs` | `ICandleLight` | `Items/Candle/` | `ICandleLight` | `IT.Items.Candle` |

---

### Items — Grappling Hook

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Items/Scriptable object scripts for items/GrapplingHookItem.cs` | `GrapplingHookItem` | `Items/GrapplingHook/` | `GrapplingHook` | `IT.Items.GrapplingHook` |
| `Items/SubItems/GrapplingHookSubitems/HookProjectile.cs` | `HookProjectile` | `Items/GrapplingHook/` | `GrappleProjectile` | `IT.Items.GrapplingHook` |
| `Items/SubItems/GrapplingHookSubitems/HookConnector.cs` | `HookConnector` | `Items/GrapplingHook/` | `GrappleSocket` | `IT.Items.GrapplingHook` |
| `Items/SubItems/GrapplingHookSubitems/HookRopeBridge.cs` | `HookRopeBridge` | `Items/GrapplingHook/` | `RopeBridge` | `IT.Items.GrapplingHook` |
| `Interface/IInteractWithHookProjecile.cs` | `IInteractWithHookProjectile` | `Items/GrapplingHook/` | `IGrappleTarget` | `IT.Items.GrapplingHook` |

> **Note:** Also fix the typo in the current interface filename: `IInteractWithHookProjecile.cs` → `IGrappleTarget.cs`. `GrappleSocket` over `HookConnector` — socket is the standard term for the receiving end of a connector.

---

### Items — Weapons

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Items/Scriptable object scripts for items/SwordItem.cs` | `SwordItem` | `Items/Weapons/` | `SwordItem` | `IT.Items.Weapons` |
| `Items/Scriptable object scripts for items/WhipItem.cs` | `WhipItem` | `Items/Weapons/` | `WhipItem` | `IT.Items.Weapons` |

---

### Player — State Machine

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `StateMachine/Player State Machine/Player/IPlayerState.cs` | `IPlayerState` | `Player/StateMachine/` | `IPlayerState` | `IT.Player.StateMachine` |
| `StateMachine/Player State Machine/Player/PlayerBaseState.cs` | `PlayerBaseState` | `Player/StateMachine/` | `PlayerStateBase` | `IT.Player.StateMachine` |
| `StateMachine/Player State Machine/Player/PlayerStateMachineManager.cs` | `PlayerStateMachineManager` | `Player/StateMachine/` | `PlayerStateMachine` | `IT.Player.StateMachine` |
| `StateMachine/Player State Machine/Player/PlayerWrapper.cs` | `PlayerWrapper` | `Player/StateMachine/` | `PlayerContext` | `IT.Player.StateMachine` |

> **Note:** `PlayerStateMachine` over `PlayerStateMachineManager` — "Manager" is redundant; the class _is_ the state machine.

---

### Player — States

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `StateMachine/Player State Machine/States/DefaultState.cs` | `DefaultState` | `Player/StateMachine/States/` | `PlayerIdleState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/DeathState.cs` | `DeathState` | `Player/StateMachine/States/` | `PlayerDeathState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/EquipItemState.cs` | `EquipItemState` | `Player/StateMachine/States/` | `PlayerEquipState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/MoveItemState.cs` | `MoveItemState` | `Player/StateMachine/States/` | `PlayerMoveItemState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/OpenItemState.cs` | `OpenItemState` | `Player/StateMachine/States/` | `PlayerOpenState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/PullItemState.cs` | `PullItemState` | `Player/StateMachine/States/` | `PlayerPullState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/SlideItemState.cs` | `SlideItemState` | `Player/StateMachine/States/` | `PlayerSlideState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/ThrowItemState.cs` | `ThrowItemState` | `Player/StateMachine/States/` | `PlayerThrowState` | `IT.Player.StateMachine.States` |
| `StateMachine/Player State Machine/States/UseItemState.cs` | `UseItemState` | `Player/StateMachine/States/` | `PlayerUseState` | `IT.Player.StateMachine.States` |

> **Note:** Add `Player` prefix to every state — state names are often displayed in debuggers and editor tools; the prefix removes ambiguity. `DefaultState` → `PlayerIdleState` is the most impactful rename: "Default" describes the fallback mechanic, "Idle" describes the player's actual behavior.

---

### Player — Input

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Player/InputManager/InputManager.cs` | `InputManager` | `Player/Input/` | `PlayerInputHandler` | `IT.Player.Input` |
| `Player/PlayerMovementManager/PlayerController.cs` | `PlayerController` | `Player/Input/` | `PlayerInputActions` | `IT.Player.Input` |

> **Note:** `PlayerInputActions` clarifies this is the auto-generated input binding class, not a controller MonoBehaviour.

---

### Player — Movement

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Player/PlayerMovementManager/PlayerMovementManager.cs` | `PlayerMovementManager` | `Player/Movement/` | `PlayerMover` | `IT.Player.Movement` |
| `Player/OriginPoint.cs` | `OriginPoint` | `Player/Movement/` | `ItemAnchorPoint` | `IT.Player.Movement` |

> **Note:** `ItemAnchorPoint` explains what the origin point is _for_ — it's the visual anchor for held items.

---

### Player — Inventory

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Player/ItemManager/ItemManager.cs` | `ItemManager` | `Player/Inventory/` | `PlayerInventory` | `IT.Player.Inventory` |
| `Player/ItemManager/ItemDropper.cs` | `ItemDropper` | `Player/Inventory/` | `ItemDropper` | `IT.Player.Inventory` |

---

### Player — Status

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Player/PlayerStatus/PlayerStatus.cs` | `PlayerStatus` | `Player/Status/` | `PlayerStatus` | `IT.Player.Status` |
| `Player/PlayerStatus/PlayerStatusManager.cs` | `PlayerStatusManager` | `Player/Status/` | `PlayerStatusManager` | `IT.Player.Status` |

---

### Player — HUD

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Player/HUD/HUDReader.cs` | `HUDReader` | `Player/HUD/` | `HUDManager` | `IT.Player.HUD` |
| `Player/HUD/PlayerStatusHUD.cs` | `PlayerStatusHUD` | `Player/HUD/` | `PlayerStatusHUD` | `IT.Player.HUD` |

> **Note:** `HUDManager` over `HUDReader` — the class both reads state _and_ instantiates/manages the HUD. "Reader" undersells it.

---

### Animation — Base

| Current Path | Current Name | New Path | New Name | Namespace | Notes |
|---|---|---|---|---|---|
| `Animation/AnimationBaseScripts/AnimationClip.cs` | `AnimationClip` | `Animation/` | `AnimClipBase` | `IT.Animation` | Rename avoids collision with `UnityEngine.AnimationClip` |
| `Animation/PlayerAnimation/AnimationStates/AnimationState.cs` | `AnimationState` | `Animation/` | `AnimStateBase` | `IT.Animation` | |

---

### Animation — Player

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Animation/PlayerAnimation/PlayerAnimationController.cs` | `PlayerAnimationController` | `Animation/Player/` | `PlayerAnimator` | `IT.Animation.Player` |
| `Animation/PlayerAnimation/AnimationStates/AnimationBell.cs` | `AnimationBell` | `Animation/Player/States/` | `BellAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationCarry.cs` | `AnimationCarry` | `Animation/Player/States/` | `CarryAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationEquipItem.cs` | `AnimationEquipItem` | `Animation/Player/States/` | `EquipAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationGrapplingHook.cs` | `AnimationGrapplingHook` | `Animation/Player/States/` | `GrappleAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationGrapplingHookSetUp.cs` | `AnimationGrapplingHookSetUp` | `Animation/Player/States/` | `GrappleSetupAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationHurtToe.cs` | `AnimationHurtToe` | `Animation/Player/States/` | `HurtToeAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationKick.cs` | `AnimationKick` | `Animation/Player/States/` | `KickAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationMove.cs` | `AnimationMove` | `Animation/Player/States/` | `MoveAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationPickUp.cs` | `AnimationPickUp` | `Animation/Player/States/` | `PickUpAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationPushAndPull.cs` | `AnimationPushAndPull` | `Animation/Player/States/` | `PushPullAnimState` | `IT.Animation.Player.States` |
| `Animation/PlayerAnimation/AnimationStates/AnimationThrow.cs` | `AnimationThrow` | `Animation/Player/States/` | `ThrowAnimState` | `IT.Animation.Player.States` |

---

### Animation — World Objects

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `Animation/DoorAnimation/SingleDoorAnimation.cs` | `SingleDoorAnimation` | `Animation/World/` | `SingleDoorAnim` | `IT.Animation.World` |
| `Animation/DoorAnimation/DoubleDoorAnimation.cs` | `DoubleDoorAnimation` | `Animation/World/` | `DoubleDoorAnim` | `IT.Animation.World` |
| `Animation/DoorAnimation/PullableDoorAnimation.cs` | `PullableDoorAnimation` | `Animation/World/` | `PullDoorAnim` | `IT.Animation.World` |
| `Animation/ChestAnimation/ChestOpenAnimation.cs` | `ChestOpenAnimation` | `Animation/World/` | `ChestOpenAnim` | `IT.Animation.World` |
| `Animation/ExplosionAnimation/ExplosionAnimation.cs` | `ExplosionAnimation` | `Animation/World/` | `ExplosionAnim` | `IT.Animation.World` |

---

### Prototype / Example Code

These scripts implement the apple lifecycle state machine, which appears to be a learning/demonstration example rather than live game code.

| Current Path | Current Name | New Path | New Name | Namespace |
|---|---|---|---|---|
| `StateMachine/State Machine/StateManager.cs` | `StateManager` | `Prototype/` | `AppleStateManager` | `IT.Prototype` |
| `StateMachine/State Machine/AppleGrowState.cs` | `AppleGrowState` | `Prototype/` | `AppleGrowState` | `IT.Prototype` |
| `StateMachine/State Machine/AppleWholeState.cs` | `AppleWholeState` | `Prototype/` | `AppleWholeState` | `IT.Prototype` |
| `StateMachine/State Machine/AppleRottenState.cs` | `AppleRottenState` | `Prototype/` | `AppleRottenState` | `IT.Prototype` |
| `StateMachine/State Machine/AppleChewedState.cs` | `AppleChewedState` | `Prototype/` | `AppleChewedState` | `IT.Prototype` |

> **Note:** If this code is truly just a usage example, consider deleting it entirely and putting the example in a comment or doc file. If it's live, move it to a real domain.

---

## Prefab Naming Convention

**Format:** `[Domain]_[Name]_[Variant]`

- Domain is always present and capitalized.
- Variant is optional — only include when multiple variants exist.
- Use PascalCase within each segment.
- No spaces; underscores separate segments only.

### Characters

| Suggested Prefab Name | Description |
|---|---|
| `Player` | The player prefab |
| `Enemy_Dummy` | Placeholder enemy |

### Doors

| Suggested Prefab Name | Description |
|---|---|
| `Door_Single` | One-panel door |
| `Door_Double` | Two-panel door |
| `Door_Double_MultiKey` | Double door requiring multiple keys/sources |
| `Door_Single_PullDriven` | Door driven by a lever pull progress |

### Chests

| Suggested Prefab Name | Description |
|---|---|
| `Chest_Locked` | Key-requiring chest |
| `Chest_Basic` | No-key chest or chest with item spawn |

### Blocks & Levers

| Suggested Prefab Name | Description |
|---|---|
| `Block_Push` | Grid-moveable push block |
| `Block_Slide_Basic` | Basic sliding block |
| `Block_Slide_Symbol` | Symbol-matching sliding block |
| `Block_Slide_Timed` | Timed sliding block |
| `Lever_Latched` | Pull-and-lock lever |
| `Lever_Retracting` | Pull-and-release lever |

### Locks

| Suggested Prefab Name | Description |
|---|---|
| `Lock_KeyPort_Sliding` | Sliding block key port |
| `Lock_KeyPort_Symbol` | Symbol matching key port |

### Items (Collectibles)

| Suggested Prefab Name | Description |
|---|---|
| `Item_Bell` | Bell item pickup |
| `Item_Candle` | Candle item pickup |
| `Item_GrapplingHook` | Grappling hook item pickup |
| `Item_Key_[Color]` | Key item — append color/type variant |
| `Item_Plank` | Plank item |
| `Item_Sword` | Sword item |
| `Item_Whip` | Whip item |

### Items (Sub-Objects)

| Suggested Prefab Name | Description |
|---|---|
| `SubItem_GrappleProjectile` | Hook projectile in flight |
| `SubItem_GrappleSocket` | Connectable socket anchor |
| `SubItem_RopeBridge` | Rope bridge between sockets |
| `SubItem_BellRingArea` | Bell overlap detection area |

### Throwables

| Suggested Prefab Name | Description |
|---|---|
| `Throwable_Bomb` | Bomb throwable |

### Effects

| Suggested Prefab Name | Description |
|---|---|
| `Effect_Explosion` | Explosion visual effect |

---

## Naming Convention Rules Summary

| Rule | Rationale |
|---|---|
| Interfaces: `I` prefix | Standard C# convention |
| Abstract bases: `*Base` suffix | `PullableBase` is clearer than bare `Pullable` when subclasses exist |
| Avoid `Manager` unless the class truly orchestrates multiple subsystems | `PlayerInventory` > `ItemManager`; `PlayerMover` > `PlayerMovementManager` |
| Avoid `Handler` in class names that aren't event handlers | `PlayerInputHandler` is acceptable for the input dispatcher |
| No folder names with spaces | `Scriptable object scripts for items` → handled by namespace alone |
| Fix `DependenyWorkFlow` typo | `Dependency` |
| Fix `IInteractWithHookProjecile` typo | `IGrappleTarget` |
| States get their owner as prefix | `PlayerIdleState` not `DefaultState` — readable in debuggers |
| Animation state classes: `*AnimState` suffix | Distinguishes from player states and Unity's built-in animation types |
| Prefabs: `[Domain]_[Name]_[Variant]` | Flat Project window list stays organized by domain without subfolders |
