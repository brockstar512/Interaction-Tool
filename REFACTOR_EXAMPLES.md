# Refactor PRD — Code Examples (Before → After)

Companion to **`REFACTOR_PRD.md`**. For each workstream: which files change, what the code looks like **today** (snippets are taken from the actual files), and a concrete example of what it looks like **after**. The "after" code is illustrative — names and exact shapes can shift during implementation, the *intent* shouldn't.

---

## WS1 — Build & Correctness Blockers

### 1.1 Editor-only APIs in runtime scripts

**Files:** `Animation/Player/PlayerAnimator.cs` (delete), `Core/Utilities/GridSnapper.cs` (guard or move)

**Before** — both compile in the editor but fail a player build:

```csharp
// PlayerAnimator.cs — vestigial class, and the using alone breaks builds
using UnityEditor.Animations;        // ❌ editor assembly
public class PlayerAnimator : MonoBehaviour
{
    public Animator anim { get; private set; }
    void Awake() { anim = GetComponent<Animator>(); }
}

// GridSnapper.cs
using UnityEditor;                   // ❌ editor assembly
[ExecuteInEditMode]
public class GridSnapper : MonoBehaviour
{
    void Update()
    {
        if(!EditorApplication.isPlaying) { /* snap transform */ }
    }
}
```

**After** — `PlayerAnimator.cs` is deleted (nothing reads `anim`). `GridSnapper` keeps working in the editor but compiles out of builds:

```csharp
// GridSnapper.cs
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
public class GridSnapper : MonoBehaviour
{
    public int tileSize = 1;
    public Vector3 tileOffset = Vector3.zero;

    void Update()
    {
        if (EditorApplication.isPlaying) return;
        Vector3 p = transform.position;
        transform.position = new Vector3(
            Mathf.Round(p.x / tileSize) * tileSize + tileOffset.x,
            Mathf.Round(p.y / tileSize) * tileSize + tileOffset.y,
            tileOffset.z);
    }
}
#endif
```

> WS9.1's asmdefs make this *class* of bug a compile error at the source instead of a build-time surprise.

### 1.2 3D collision callback in a 2D game

**Files:** `Player/StateMachine/PlayerStateMachine.cs`, `PlayerStateBase.cs`, every state

**Before** — `OnCollisionEnter(Collision)` is the **3D** callback; with a `Rigidbody2D` it never fires. Every state then carries a dead override:

```csharp
// PlayerStateMachine.cs
void OnCollisionEnter(Collision collision)              // never called
{
    currentState.OnCollisionEnter(this, collision);
}

// PlayerStateBase.cs — forces 9 empty overrides across the states
public abstract void OnCollisionEnter(PlayerStateMachine stateManager, Collision collision);
```

**After** — delete the chain entirely (no state uses it). If a state ever needs collisions, add the 2D version then:

```csharp
// PlayerStateMachine.cs — method removed.
// PlayerStateBase.cs  — abstract member removed; ~30 lines of empty overrides disappear.

// (future, only when actually needed)
void OnCollisionEnter2D(Collision2D collision) => currentState.OnCollision(this, collision);
```

### 1.3 HUD events: null-unsafe, and deltas where totals are expected

**Files:** `Player/Status/PlayerStatus.cs`, `Player/HUD/PlayerStatusHUD.cs`

**Before** — `Invoke` without `?.` (throws if no HUD subscribed), and the event passes the *change*, which the HUD writes straight into a 0–1 `fillAmount`:

```csharp
// PlayerStatus.cs
int Health;                                   // starts at 10
public event Action<int> HealthChange;

public void UpdateHealth(int HP)
{
    Health += HP;
    HealthChange.Invoke(HP);                  // ❌ null-deref risk, ❌ sends the delta
}

// PlayerStatusHUD.cs
private void UpdateHealth(int health)
{
    Health.fillAmount = health;               // ❌ expects 0–1, receives e.g. -2
}
```

**After** — totals + max flow through the event; normalization happens once, at the UI boundary:

```csharp
// PlayerStatus.cs
public int MaxHealth { get; } = 10;
public int Health   { get; private set; } = 10;
public event Action<int, int> HealthChanged;          // (current, max)

public void ApplyHealthDelta(int delta)
{
    Health = Mathf.Clamp(Health + delta, 0, MaxHealth);
    HealthChanged?.Invoke(Health, MaxHealth);
}

// PlayerStatusHUD.cs
private void UpdateHealth(int current, int max)
{
    Health.fillAmount = (float)current / max;
}
```

### 1.4 Singleton race in `PlayerStatusManager`

**Before:**

```csharp
public void Init(PlayerStateMachine playerStateMachineManager)
{
    playerHUD = HUDManager.instance.InitializePlayerHUD(playerStateMachineManager);  // ❌ NRE if HUD awakes later / is missing
}
```

**After** — `HUDManager` adopts the existing (currently unused) `Singleton<T>`, and callers fail loudly instead of mysteriously:

```csharp
// HUDManager.cs
public class HUDManager : Singleton<HUDManager> { /* hand-rolled instance code deleted */ }

// PlayerStatusManager.cs
public void Init(PlayerStateMachine player)
{
    if (!HUDManager.HasInstance)
    {
        Debug.LogError("PlayerStatusManager.Init: no HUDManager in scene", this);
        return;
    }
    playerHUD = HUDManager.Instance.InitializePlayerHUD(player);
}
```

### 1.5 Global physics flag mutation

**Before** (`SlidableBase.ClosestContactPointHelper.SetColliderHit`):

```csharp
public void SetColliderHit()
{
    Physics2D.queriesStartInColliders = false;   // ❌ global, never restored
    RaycastHit2D hit = Physics2D.Raycast(_originPoint, _direction, int.MaxValue, _obstructionLayer);
    ...
}
```

**After** — set **Project Settings ▸ Physics 2D ▸ Queries Start In Colliders = off** once; delete the runtime write:

```csharp
public void SetColliderHit()
{
    RaycastHit2D hit = Physics2D.Raycast(_originPoint, _direction, int.MaxValue, _obstructionLayer);
    ...
}
```

---

## WS2 — Async & Lifetime Safety

### 2.2 `destroyCancellationToken` replaces hand-rolled CTS plumbing

**Files:** `Interactables/Throwable/BombTimer.cs` (shown), `Items/Candle/CandleItem.cs`, `Interactables/Slidable/SlidableBase.cs`

**Before** — ~60 lines of CTS lifecycle around one delay:

```csharp
public class BombTimer : MonoBehaviour
{
    private CancellationTokenSource _cancellationTokenSource;

    async void Start()
    {
        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await StartTimer(_timer, _cancellationTokenSource.Token);
        }
        catch (System.OperationCanceledException) { }
        catch (System.Exception ex) { Debug.LogError($"BombExplode.Start failed: {ex}"); }
    }

    private async Task StartTimer(float waitTime, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay((int)(waitTime * 1000), cancellationToken);  // ❌ thread-pool timer, ms cast
            Explode();
        }
        catch (TaskCanceledException) { Debug.Log("Task was cancelled!"); }
    }

    private void CancelTask() { if (_cancellationTokenSource != null) _cancellationTokenSource.Cancel(); }

    private void OnDestroy()
    {
        if (_cancellationTokenSource != null) { CancelTask(); _cancellationTokenSource.Dispose(); }
        Destroy(parentBody.gameObject);
    }
}
```

**After** — Unity owns the token; cancellation on destroy is automatic:

```csharp
public class BombTimer : MonoBehaviour
{
    [SerializeField] private Transform parentBody;
    [SerializeField] private ExplosionEffect explosion;
    [SerializeField] private float fuseSeconds = 10f;        // WS7 moves this to BombConfig

    async void Start()                                        // ok: Unity entry point (rule 2.1)
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(fuseSeconds, destroyCancellationToken);
            Explode();
        }
        catch (OperationCanceledException) { }                // bomb was picked up/destroyed first
    }

    private void OnDestroy() => Destroy(parentBody.gameObject);
}
```

### 2.3 Remove fake async

**Files:** `Overlap/KeyPortOverlap.cs` (shown), `Overlap/GrappleSocketOverlap.cs`, plus callers

**Before** — no awaiting happens; the `Task` wrapper just obscures it and forces callers into `async void`:

```csharp
public Task<KeyPortBase> FindKeyPort()
{
    SetMovingOverlappingArea(transform.position);
    Collider2D col = GetMostOverlappedCol();
    if (col == null) return Task.FromResult<KeyPortBase>(null);
    ...
    return Task.FromResult(port);
}

// caller — SlidableBase.CleanUp
private async void CleanUp()
{
    KeyPortBase port = null;
    try { port = await _targetCheck.FindKeyPort(); }
    catch (Exception ex) { Debug.LogError($"Slidable.CleanUp failed: {ex}"); }
    ...
}
```

**After** — synchronous in, synchronous out:

```csharp
public KeyPortBase FindKeyPort()
{
    SetMovingOverlappingArea(transform.position);
    Collider2D col = GetMostOverlappedCol();
    if (col == null) return null;
    ...
    return port;
}

// caller
private void CleanUp()
{
    KeyPortBase port = _targetCheck.FindKeyPort();
    _moverCheck.CleanUp();
    _targetCheck.CleanUp();
    if (port != null && AcceptsPort(port)) _locked.Value = true;
}
```

### 2.4 One guarded runner for async state actions

See WS3.1 below — the runner and the stale-continuation guard are the same mechanism.

---

## WS3 — State Machine Hardening

### 3.1 + 2.4 Stale-continuation guard via a transition version

**Files:** `Player/StateMachine/PlayerStateMachine.cs` + every state with an `async void` body

**Before** — `PlayerThrowState.EnterState` awaits, then acts on whatever the world looks like *now*; nothing checks that the state is still current. Each state also hand-rolls its own try/catch/recover:

```csharp
public override async void EnterState(PlayerStateMachine stateManager)
{
    try
    {
        _currentAnimation = _pickUpAnimation;
        await _pickUpAnimation.Play(stateManager);
        stateManager.item.Interact(stateManager);   // ❌ may run after the state already changed
        _currentAnimation = _carryAnimation;
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"ThrowItemState.EnterState failed: {ex}");
        stateManager.SwitchState(stateManager.defaultState);
    }
}
```

**After** — the machine owns one runner; `SwitchState` bumps a version; continuations check it:

```csharp
// PlayerStateMachine.cs
public int TransitionVersion { get; private set; }

public void SwitchState(PlayerStateBase newState)
{
    TransitionVersion++;                       // invalidates in-flight continuations
    newState.LookDirection = currentState.LookDirection;
    currentState.ExitState(this);
    currentState = newState;
    currentState.EnterState(this);
    if (currentState is PlayerIdleState) item = null;
}

/// Runs an async state action; on completion or failure, returns to idle
/// unless the state already changed underneath us.
public async void RunStateAction(Func<Awaitable> action)
{
    int version = TransitionVersion;
    try { await action(); }
    catch (OperationCanceledException) { return; }
    catch (Exception ex) { Debug.LogError($"State action failed: {ex}", this); }
    if (TransitionVersion == version) SwitchState(defaultState);
}
```

```csharp
// PlayerSlideState.cs — the whole try/catch/finally body becomes:
public override void Action(PlayerStateMachine sm)
{
    sm.RunStateAction(async () =>
    {
        if (sm.item.Interact(sm))
            await KickAnimation.Play(sm);
        else
        {
            await KickAnimation.Play(sm);
            await HurtToeAnimation.Play(sm);
        }
    });
}
```

### 3.2 Clear per-state fields on exit

**File:** `Player/StateMachine/States/PlayerPullState.cs` (same pattern: `PlayerUseState`, `PlayerThrowState`)

**Before** — shared state instance keeps references after the interaction:

```csharp
private Vector2 _axis;
private PullableBase _pullable;
private PlayerStateMachine _stateManager;

public override void ExitState(PlayerStateMachine stateManager) { }   // ❌ leaks last target
```

**After:**

```csharp
public override void ExitState(PlayerStateMachine stateManager)
{
    _pullable = null;
    _stateManager = null;
    _axis = default;
}
```

### 3.3 Data dispatch instead of the `Kind` switch

**Files:** `PlayerIdleState.cs`, `PlayerStateMachine.cs`

**Before:**

```csharp
switch (stateManager.item.Kind)
{
    case InteractionType.Throw: stateManager.SwitchState(stateManager.throwItemState); break;
    case InteractionType.Move:  stateManager.SwitchState(stateManager.moveItemState);  break;
    case InteractionType.Slide: stateManager.SwitchState(stateManager.slideItemState); break;
    case InteractionType.Equip: stateManager.SwitchState(stateManager.equipItemState); break;
    case InteractionType.Open:  stateManager.SwitchState(stateManager.PlayerOpenState); break;
    case InteractionType.Pull:  stateManager.SwitchState(stateManager.pullItemState);  break;
    default: Debug.Log("is default"); break;
}
```

**After** — adding `InteractionType.Carry` someday means one dictionary entry, not idle-state surgery:

```csharp
// PlayerStateMachine.Awake
_interactionStates = new Dictionary<InteractionType, PlayerStateBase>
{
    [InteractionType.Throw] = throwItemState,
    [InteractionType.Move]  = moveItemState,
    [InteractionType.Slide] = slideItemState,
    [InteractionType.Equip] = equipItemState,
    [InteractionType.Open]  = openItemState,
    [InteractionType.Pull]  = pullItemState,
};
public bool TrySwitchToInteraction(InteractionType kind)
{
    if (!_interactionStates.TryGetValue(kind, out var state)) return false;
    SwitchState(state);
    return true;
}

// PlayerIdleState.Action
if (stateManager.item != null)
    stateManager.TrySwitchToInteraction(stateManager.item.Kind);
```

---

## WS4 — Overlap System Consolidation

### 4.2 Serialized direction placement replaces three hard-coded helper classes

**Files:** `Overlap/DamageOverlap.cs`, `Overlap/MovementOverlap.cs`, `Overlap/OverlapCheckerBase.cs`

**Before** — each class embeds a private helper with its own magic numbers (these are `DamageOverlap`'s; `MovementOverlap`'s differ slightly, `OverlapCheckerBase`'s differ again):

```csharp
private class OverlapMoveCheckHelper
{
    readonly Vector2 verticalScale   = new Vector2(1.0f, .1f);
    readonly Vector2 horizontalScale = new Vector2(0.1f, 1f);
    readonly Vector2 upPos    = new Vector2(0, -0.82f);
    readonly Vector2 downPos  = new Vector2(0, 0.25f);
    readonly Vector2 leftPos  = new Vector2(.6f, -.34f);
    readonly Vector2 rightPos = new Vector2(-.6f, -.34f);

    public Vector2 UpdateScale(Vector2 lookDirection) { /* 4-way if-chain */ }
    public Vector2 UpdatePosition(Vector2 lookDirection) { /* 4-way if-chain */ }
}
```

**After** — the mechanism lives once in `OverlapCheckerBase`; the numbers live on the prefab where they belong:

```csharp
[Serializable]
public struct DirectionalPlacement
{
    public Vector2 offset;
    public Vector2 scale;
}

// OverlapCheckerBase.cs
[SerializeField] private DirectionalPlacement up, down, left, right;

public void AimAt(Facing facing)                       // Facing comes from WS5
{
    DirectionalPlacement p = facing switch
    {
        Facing.Up => up, Facing.Down => down,
        Facing.Left => left, _ => right,
    };
    transform.localPosition = p.offset;
    transform.localScale    = p.scale;
}
```

`DamageOverlap` and `MovementOverlap` then extend `OverlapCheckerBase` and keep only their unique behavior (damage application; the boolean probe).

### 4.3 Fix the intersection math (after characterization tests)

**File:** `Overlap/OverlapCheckerBase.cs` (`KeyPortOverlap.GetPercentOfOverlap` gets the same treatment)

**Before** — corners are mixed (`overlappingTopRightCornerAABB.x` appears where a *bottom-left* corner belongs; the `y` term has the mirror-image mistake):

```csharp
float GetOverlappingArea(Collider2D overlappingObject)
{
    (Vector2 overlappingTopRightCornerAABB, Vector2 overlappingBottomLeftCornerAABB) = GetAABBCorners(overlappingObject);
    float xLength = Mathf.Min(_areaTopRightCornerAABB.x, overlappingTopRightCornerAABB.x)
                  - Mathf.Max(_areaBottomLeftCornerAABB.x, overlappingTopRightCornerAABB.x);   // ❌
    float yLength = Mathf.Min(_areaTopRightCornerAABB.y, overlappingBottomLeftCornerAABB.y)    // ❌
                  - Mathf.Max(_areaBottomLeftCornerAABB.y, overlappingBottomLeftCornerAABB.y);
    return xLength * yLength;
}
```

**After** — standard AABB intersection, clamped, unit-testable in isolation:

```csharp
public static float IntersectionArea(Bounds a, Bounds b)
{
    float x = Mathf.Max(0f, Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x));
    float y = Mathf.Max(0f, Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y));
    return x * y;
}

float GetOverlappingArea(Collider2D other) => IntersectionArea(CurrentBounds, other.bounds);
```

> ⚠️ Per PRD WS4.3 / WS10.3: land characterization tests for the *current* ranking behavior first, then fix, then re-tune `KeyPortOverlap`'s 60 % seat threshold by playtest.

### 4.5 Layer-mask presets

**File:** `Core/Utilities/LayerIndex.cs` + consumers (`SlidableBase`, `PushBlock`)

**Before** — every consumer assembles "what blocks movement" by hand:

```csharp
// SlidableBase.Awake
obstructionLayer |= (1 << LayerIndex.SlidableObstruction)
                  | (1 << LayerIndex.Interactable)
                  | (1 << LayerIndex.Locked);

// PushBlock.Awake — same thing, re-derived
_obstructionMask  = 0;
_obstructionMask |= 1 << LayerIndex.SlidableObstruction;
_obstructionMask |= 1 << LayerIndex.Interactable;
_obstructionMask |= 1 << LayerIndex.Locked;
```

**After:**

```csharp
// LayerIndex.cs
public static class LayerMasks
{
    /// Things a sliding/pushed block cannot pass through.
    public static readonly LayerMask BlocksMovement =
        (1 << LayerIndex.SlidableObstruction) | (1 << LayerIndex.Interactable) | (1 << LayerIndex.Locked);
}

// consumers
obstructionLayer = LayerMasks.BlocksMovement;
```

---

## WS5 — Direction & Animation Unification

### 5.1 The `Facing` utility

**New file:** `Core/Utilities/Facing.cs`

**Before** — this shape appears 8+ times (`PlayerStateBase`, `MoveAnimState`, `CarryAnimState`, `KickAnimState`, `PushPullAnimState`, `GrappleProjectile.SetHookSprite`, two overlap helpers):

```csharp
protected void UpdateLookDirection(Vector2 movement)
{
    if (movement == Vector2.up)    { LookDirection = movement; }
    if (movement == Vector2.down)  { LookDirection = movement; }
    if (movement == Vector2.right) { LookDirection = movement; }
    if (movement == Vector2.left)  { LookDirection = movement; }
}
```

**After:**

```csharp
public enum Facing { Up, Down, Left, Right }

public static class FacingUtil
{
    /// Cardinal directions only; returns null for diagonal/zero input.
    public static Facing? FromVector(Vector2 v)
    {
        if (v == Vector2.up)    return Facing.Up;
        if (v == Vector2.down)  return Facing.Down;
        if (v == Vector2.left)  return Facing.Left;
        if (v == Vector2.right) return Facing.Right;
        return null;
    }

    public static Vector2 ToVector(this Facing f) => f switch
    {
        Facing.Up => Vector2.up, Facing.Down => Vector2.down,
        Facing.Left => Vector2.left, _ => Vector2.right,
    };

    public static bool IsHorizontal(this Facing f) => f is Facing.Left or Facing.Right;
}

// PlayerStateBase.cs
protected void UpdateLookDirection(Vector2 movement)
{
    if (FacingUtil.FromVector(movement) is Facing f)
        LookDirection = f.ToVector();          // LookDirection stays Vector2 for now (migration step)
}
```

### 5.2 + 5.3 `DirectionalAnim`: four hashes, real clip lengths, one awaitable

**Files:** `Animation/AnimStateBase.cs` (gains the shared API), every `Animation/Player/States/*.cs`

**Before** — `KickAnimState` is ~75 lines: 8 hand-declared hashes, a hand-typed duration dictionary that silently drifts when clips are re-timed, and a 4-way if-chain that repeats `Play + await` per branch:

```csharp
readonly int KickRightHash = Animator.StringToHash("KickRight");
readonly int KickUpHash    = Animator.StringToHash("KickUp");
// ... 6 more hashes ...
TimeSheet = new()
{
    { KickRightHash, 0.333f },        // ❌ must match the clip by hand
    { KickUpHash,    0.333f },
    // ...
};

public async Task Play(PlayerStateMachine state)
{
    if (state.currentState.LookDirection == Vector2.down)
    {
        state.animator.Play(KickDownHash);
        await Awaitable.WaitForSecondsAsync(TimeSheet[KickDownHash]);
        return;
    }
    if (state.currentState.LookDirection == Vector2.up) { /* same again */ }
    // ... twice more ...
}
```

**After** — convention over configuration: animator states are named `{Base}{Facing}` (they already are: `KickUp`, `WalkLeft`, `HoldStillRight`…). Duration is read from the entered state, so re-timing a clip in the editor Just Works:

```csharp
// Animation/AnimStateBase.cs (repurposed per PRD Appendix A)
public sealed class DirectionalAnim
{
    private readonly Dictionary<Facing, int> _hashes;

    public DirectionalAnim(string baseName)
    {
        _hashes = new Dictionary<Facing, int>();
        foreach (Facing f in Enum.GetValues(typeof(Facing)))
            _hashes[f] = Animator.StringToHash(baseName + f);   // "Kick" + "Up" → KickUp
    }

    public void Play(Animator animator, Facing facing) =>
        animator.Play(_hashes[facing]);

    public async Awaitable PlayAndWait(Animator animator, Facing facing, CancellationToken ct)
    {
        animator.Play(_hashes[facing], 0, 0f);
        animator.Update(0f);                                    // enter the state now
        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        await Awaitable.WaitForSecondsAsync(length, ct);
    }
}
```

```csharp
// KickAnimState.cs — the whole class, after
public class KickAnimState
{
    private static readonly DirectionalAnim Kick = new("Kick");

    public Awaitable Play(PlayerStateMachine state) =>
        Kick.PlayAndWait(state.animator, state.Facing, state.destroyCancellationToken);
}
```

`MoveAnimState`/`CarryAnimState` (looping, not awaited) become two `DirectionalAnim`s (`Walk`/`Stand`, `HoldWalk`/`HoldStill`) and one `movement == zero ? stand : walk` line. `GrappleProjectile`'s four serialized sprites get the same treatment with a `DirectionalSpriteSet`.

---

## WS6 — Damage & Status Completion

### 6.1 + 6.3 A payload instead of a marker

**Files:** `Core/IDamage.cs` → `Core/DamageInfo.cs`, `Core/IDamageable.cs`, `Interactables/Throwable/ExplosionDamageArea.cs`, `Enemies/EnemyDummy.cs`

**Before** — damage has no amount; the explosion computes falloff and throws it away:

```csharp
public interface IDamage { /* the damamges */ }                     // empty marker
public interface IDamageable { void ApplyDamage(IDamage damagingThing); }

// ExplosionDamageArea.OnTriggerEnter2D
float SplashRange = 5;                                              // ❌ hard-coded
float SplashDamage = 5f;
var damagePercent = Mathf.InverseLerp(SplashDamage, 0, distance);   // ❌ computed…
enemy.ApplyDamage(this);                                            // ❌ …and discarded
```

**After:**

```csharp
public readonly struct DamageInfo
{
    public readonly int Amount;
    public readonly Vector2 SourcePosition;     // for knockback / flash direction
    public DamageInfo(int amount, Vector2 sourcePosition)
    {
        Amount = amount;
        SourcePosition = sourcePosition;
    }
}

public interface IDamageable { void ApplyDamage(in DamageInfo damage); }

// ExplosionDamageArea.cs
[SerializeField] private float splashRange = 5f;
[SerializeField] private int maxDamage = 5;

private void OnTriggerEnter2D(Collider2D other)
{
    foreach (var hit in Physics2D.OverlapCircleAll(transform.position, splashRange))
    {
        if (hit.GetComponent<IDamageable>() is not { } target) continue;
        float distance = Vector2.Distance(hit.ClosestPoint(transform.position), transform.position);
        int amount = Mathf.CeilToInt(maxDamage * Mathf.InverseLerp(splashRange, 0f, distance));
        if (amount > 0) target.ApplyDamage(new DamageInfo(amount, transform.position));
    }
}
```

### 6.2 The player takes damage

**New:** `Player/Status/PlayerHealth.cs` (or folded into `PlayerStatusManager`)

```csharp
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float invulnerableSeconds = 1f;
    private PlayerStatusManager _status;
    private CharacterFlash _flash;            // exists today, unused by the player
    private float _vulnerableAt;

    public void ApplyDamage(in DamageInfo damage)
    {
        if (Time.time < _vulnerableAt) return;                  // i-frames
        _vulnerableAt = Time.time + invulnerableSeconds;

        _status.playerStatus.ApplyHealthDelta(-damage.Amount);  // WS1.3 event → HUD
        _flash?.StartFlash();

        if (_status.playerStatus.Health <= 0)
            GetComponent<PlayerStateMachine>().SwitchState(/* deathState — WS3.5 */);
    }
}
```

`EnemyDummy.ApplyDamage` drops its unconditional `Destroy(gameObject)` and runs the same pattern with its own (tiny) health pool — one damage path for everything.

---

## WS7 — Data-Driven Configuration

### 7.1 ScriptableObject configs for buried constants

**Files:** new `ScriptableObjects/Configs/*`, consumers like `Interactables/Moveable/PushBlock.cs`

**Before** — tuning requires a recompile, and a "heavy slow block" variant would require a subclass:

```csharp
public class PushBlock : Interactable
{
    private const float pushDistance   = 1f;
    private const float pushSpeed      = .75f;
    private const float wiggleStrength = 0.05f;
    private const float wiggleTime     = 0.1f;
    ...
}
```

**After:**

```csharp
[CreateAssetMenu(menuName = "IT/Config/Push Block", fileName = "PushBlockConfig")]
public class PushBlockConfig : ScriptableObject
{
    [Min(0.1f)] public float pushDistance   = 1f;    // one grid unit
    [Min(0.1f)] public float pushSpeed      = .75f;  // units/sec
    public float wiggleStrength = 0.05f;
    public float wiggleTime     = 0.1f;
}

public class PushBlock : Interactable
{
    [SerializeField] private PushBlockConfig config;
    // usages: config.pushSpeed, config.pushDistance, ...
}
```

A heavy block is now a second `.asset` file assigned on the prefab variant — zero new code. Same pattern for `SlideConfig`, `GrappleConfig`, `BombConfig`, `CandleConfig`, `PlayerConfig`.

### 7.2 Key/lock identity as assets (M4, after everything stabilizes)

**Files:** `Core/Utilities/GameUtilities.cs` (enum removed), `Items/KeyItem.cs`, `Interactables/Doors/OpenableBase.cs`, `Interactables/Locks/*`

**Before** — adding key type #7 edits a shared enum, and symbol matching is a bolted-on string compare:

```csharp
public enum KeyTypes { None, Key, MasterKey, SlidingBlock, MovingBlock, SymbolSlidingBlock }

// OpenableBase.cs
private bool CorrectKey(IItem item) => item is KeyItem keyItem && keyItem.keyType == key;

// SymbolSlidingBlock.cs — separate string channel for symbols
protected override bool AcceptsPort(KeyPortBase port) =>
    base.AcceptsPort(port) && port is SymbolKeyPort sp && sp.Symbol == symbol;
```

**After** — identity is an asset; matching is reference equality; "symbol" stops being a special case:

```csharp
[CreateAssetMenu(menuName = "IT/Key Definition", fileName = "Key_")]
public class KeyDefinition : ScriptableObject
{
    [TextArea] public string designerNotes;   // the asset *is* the identity; fields are documentation
}

// KeyItem.cs
[SerializeField] private KeyDefinition key;
public KeyDefinition Key => key;

// OpenableBase.cs
[SerializeField] private KeyDefinition requiredKey;   // null = unlocked
private bool CorrectKey(IItem item) => item is KeyItem k && k.Key == requiredKey;

// KeyPortBase.cs — SymbolKeyPort is deleted; a "triangle port" is just a port
// whose KeyDefinition asset is Key_Triangle.asset
[SerializeField] private KeyDefinition accepts;
public bool Matches(KeyDefinition key) => key == accepts;
```

---

## WS8 — Items & Inventory Contract

### 8.1 One acquisition path in `PlayerInventory`

**File:** `Player/Inventory/PlayerInventory.cs`

**Before** — two overloads, each with its own copy of the full-inventory branch (and different displacement behavior — that part is intentional and stays):

```csharp
public void PickUpItem(IItemHolder holder)
{
    ItemSwitch?.Invoke(holder.Sprite);
    var pickup = holder.item;
    pickup.TakeChild(transform);
    if (inventory.Count >= inventoryLimit)
    {
        var displaced = inventory[_currentIndex] as ItemBase;
        inventory.RemoveAt(_currentIndex);
        holder.Swap(displaced);
        inventory.Insert(_currentIndex, pickup);
        return;
    }
    inventory.Add(pickup);
    _currentIndex = inventory.IndexOf(pickup);
    holder.PickedUp();
}

public void PickUpItem(IItem item)
{
    item.TakeChild(transform);
    ItemSwitch?.Invoke(item.Sprite);
    if (inventory.Count >= inventoryLimit)
    {
        ItemBase displaced = inventory[_currentIndex] as ItemBase;
        inventory[_currentIndex] = item;
        _itemDropper?.Drop(displaced, transform.position);
        return;
    }
    inventory.Add(item);
    _currentIndex = inventory.IndexOf(item);
}
```

**After** — one core; the two entry points only choose where the displaced item goes:

```csharp
public void PickUpItem(IItemHolder holder) =>
    Acquire(holder.item,
        onDisplaced: d => holder.Swap(d),        // displaced item stays in the world holder
        onAdded:     () => holder.PickedUp());

public void PickUpItem(IItem item) =>
    Acquire(item,
        onDisplaced: d => _itemDropper?.Drop(d, transform.position));   // displaced item drops

private void Acquire(IItem item, Action<ItemBase> onDisplaced, Action onAdded = null)
{
    item.TakeChild(transform);

    if (inventory.Count >= inventoryLimit)
    {
        var displaced = (ItemBase)inventory[_currentIndex];
        inventory[_currentIndex] = item;
        onDisplaced(displaced);
    }
    else
    {
        inventory.Add(item);
        _currentIndex = inventory.Count - 1;
        onAdded?.Invoke();
    }

    ItemSwitch?.Invoke(item.Sprite);
}
```

### 8.2 `Finish()` codifies the item lifecycle

**Files:** `Items/ItemBase.cs` + every item

**Before** — each item re-implements the callback dance (and could double-fire it):

```csharp
// BellItem.cs
public override void PutAway()
{
    ItemFinishedCallback?.Invoke(null);   // callback survives, could fire again
}
```

**After:**

```csharp
// ItemBase.cs
/// End this interaction exactly once. Pass `next` to drop the player
/// straight into carrying an Interactable (grapple retrieval).
protected void Finish(Interactable next = null)
{
    var callback = ItemFinishedCallback;
    ItemFinishedCallback = null;          // single-shot
    callback?.Invoke(next);
}

public override void PutAway() => Finish();          // most items
public override void PutAway() => Finish(item);      // GrapplingHook passes the retrieved throwable
```

### 8.3 Resolve the `ThrowableBase` parenting quirk

**File:** `Interactables/Throwable/ThrowableBase.cs`

**Before** — `SetParent` is commented out but the local-position write remains; with no parent, `localPosition = zero` is a teleport to world origin (current behavior depends on scene setup):

```csharp
protected ThrowableBase PickUp(Transform parent)
{
    this.transform.GetComponent<BoxCollider2D>().isTrigger = true;
    //this.transform.SetParent(parent);                      // ❓ archaeology
    this.transform.localPosition = new Vector3(0, 0, 0);
    return this;
}
```

**After** — pick the intended model (parent to the carry anchor) and delete the history:

```csharp
public override bool Interact(IInteractionContext context)
{
    var anchor = context.Transform.GetComponentInChildren<ItemAnchorPoint>();
    GetComponent<BoxCollider2D>().isTrigger = true;
    transform.SetParent(anchor.transform);
    transform.localPosition = Vector3.zero;       // now actually means "over the player's head"
    PlayerSpriteBounds = context.Transform.GetComponent<SpriteRenderer>().bounds;
    return true;
}
// Toss() already does transform.SetParent(null) — unchanged.
```

---

## WS9 — Project Structure (asmdefs)

**New files:** `Assets/Scripts/IT.Runtime.asmdef`, `Assets/Scripts/Editor/IT.Editor.asmdef`

```json
// Assets/Scripts/IT.Runtime.asmdef
{
    "name": "IT.Runtime",
    "rootNamespace": "IT",
    "references": [ "Unity.InputSystem", "Unity.TextMeshPro", "DOTween.Modules" ],
    "noEngineReferences": false
}
```

```json
// Assets/Scripts/Editor/IT.Editor.asmdef
{
    "name": "IT.Editor",
    "rootNamespace": "IT.Editor",
    "references": [ "IT.Runtime" ],
    "includePlatforms": [ "Editor" ]
}
```

With these in place, WS1.1-style mistakes (`using UnityEditor` in `IT.Runtime`) fail at compile time in the editor, not at build time. (DOTween's exact asmdef name depends on its install — check `Assets/Plugins`.)

---

## WS10 — Test Safety Net

**New files:** `Assets/Tests/EditMode/IT.Tests.EditMode.asmdef` + test classes

### Inventory behavior (WS8 guard)

```csharp
public class PlayerInventoryTests
{
    private PlayerInventory _inventory;       // built on a fresh GameObject per test

    [Test]
    public void PickUp_WhenFull_ReplacesCurrentAndRoutesDisplacedToSink()
    {
        var (first, second, third) = (FakeItem(), FakeItem(), FakeItem());
        _inventory.PickUpItem(first);
        _inventory.PickUpItem(second);        // limit is 2

        _inventory.PickUpItem(third);         // displaces the current item

        Assert.AreSame(third, _inventory.GetItem());
        Assert.AreEqual(1, _dropper.DroppedItems.Count);   // test double for ItemDropper
        Assert.AreSame(_currentBefore, _dropper.DroppedItems[0]);
    }

    [Test]
    public void Dispose_LastItem_FiresItemSwitchWithNullSprite()
    {
        _inventory.PickUpItem(FakeItem());
        Sprite reported = Sprite.Create(...);
        _inventory.ItemSwitch += s => reported = s;

        _inventory.DisposeOfCurrentItem();

        Assert.IsNull(reported);
        Assert.IsNull(_inventory.GetItem());
    }
}
```

### Characterization before WS4.3 (lock in today's ranking, then fix)

```csharp
public class OverlapMathCharacterizationTests
{
    // Captured from the CURRENT GetOverlappingArea implementation before any change.
    // These pin the *ranking* behavior the game actually depends on
    // (which collider wins), not the raw area values.
    [Test]
    public void MostOverlapped_PrefersColliderWithGreaterTrueIntersection()
    {
        var checker = BoundsFrom(center: (0, 0), size: (1, 1));
        var mostlyOver = BoundsFrom(center: (0.1f, 0), size: (1, 1));   // ~90% overlap
        var barelyOver = BoundsFrom(center: (0.9f, 0), size: (1, 1));   // ~10% overlap

        Assert.Greater(
            OverlapCheckerBase.IntersectionArea(checker, mostlyOver),
            OverlapCheckerBase.IntersectionArea(checker, barelyOver));
    }

    [Test]
    public void IntersectionArea_NoOverlap_IsZero_NotNegative()
    {
        var a = BoundsFrom(center: (0, 0), size: (1, 1));
        var b = BoundsFrom(center: (5, 5), size: (1, 1));
        Assert.AreEqual(0f, OverlapCheckerBase.IntersectionArea(a, b));
        // NOTE: the legacy code returns a *negative product* here that can compare
        // greater-than a true small overlap — this is the bug WS4.3 fixes.
    }
}
```

### Dependency system (already correct — keep it that way)

```csharp
[Test]
public void MultiDependent_OpensOnlyWhenAllSourcesTrue()
{
    var (a, b) = (new Observable<bool>(), new Observable<bool>());
    var door = TestMultiDependent.With(a, b);

    a.Value = true;
    Assert.IsFalse(door.Opened);
    b.Value = true;
    Assert.IsTrue(door.Opened);
}
```

---

## Quick Index: PRD requirement → example above

| PRD | Example | PRD | Example |
|---|---|---|---|
| WS1.1 | editor `using` guard/delete | WS5.1 | `Facing` utility |
| WS1.2 | 3D collision chain delete | WS5.2–5.3 | `DirectionalAnim.PlayAndWait` |
| WS1.3 | totals + null-safe events | WS6.1–6.3 | `DamageInfo` + explosion falloff |
| WS1.4 | `Singleton<T>` HUD | WS6.2 | `PlayerHealth : IDamageable` |
| WS1.5 | physics flag removal | WS7.1 | `PushBlockConfig` SO |
| WS2.2 | `destroyCancellationToken` BombTimer | WS7.2 | `KeyDefinition` SO |
| WS2.3 | de-fake `FindKeyPort` | WS8.1 | `Acquire` core |
| WS3.1/2.4 | `RunStateAction` + version guard | WS8.2 | `ItemBase.Finish()` |
| WS3.2 | `ExitState` clears fields | WS8.3 | throwable parenting |
| WS3.3 | dispatch dictionary | WS9.1 | asmdef JSON |
| WS4.2 | `DirectionalPlacement` | WS10 | inventory / characterization / dependency tests |
| WS4.3 | `IntersectionArea` fix | | |
| WS4.5 | `LayerMasks` presets | | |
