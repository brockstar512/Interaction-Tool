# PRD — Codebase Refactor: Cleaner, Scalable, Best-Practice

| | |
|---|---|
| **Status** | Draft for review |
| **Date** | 2026-06-11 |
| **Scope** | `Assets/Scripts/` (~125 files, ~6.7k lines) — no gameplay redesign, no art/content work |
| **Companion docs** | `ARCHITECTURE.md` (current-state reference) · `TODO.md` (feature backlog — partially superseded by this doc) |

---

## 1. Background

The project recently completed a structural reorganization (namespaces under `IT.*`, folders per system, renamed animation classes). The architecture is genuinely good: a state-pattern player, polymorphic `Interactable`s, a context facade, and an observer-based puzzle system. See `ARCHITECTURE.md` for the full picture.

What's left is the layer *underneath* the architecture: async patterns that swallow or mishandle failure, duplicated geometry/direction code, hand-maintained magic numbers, a half-finished damage loop, two scripts that will fail a player build, and no tests guarding any of it. Each new feature (enemy AI, sword/whip, health) will be built **on top of** these foundations, so the cost of fixing them grows with every feature shipped first.

## 2. Problem Statement

> A solo developer adding a new interactable, item, or enemy today must: hand-copy a four-way direction `if`-chain, hand-copy an AABB overlap helper, hand-time animation clips in a dictionary, pick async patterns with no convention, and hope nothing throws inside an `async void` — with zero test coverage and two latent build breakers. The refactor's job is to make the *second* enemy, the *tenth* item, and the *fiftieth* puzzle cheap and safe to add.

## 3. Goals

1. **Correctness first** — eliminate build breakers, silent exception sinks, and state-machine race windows.
2. **One way to do each thing** — one direction utility, one overlap base, one async convention, one "play animation and wait" helper.
3. **Data-driven tuning** — designers (future-you) tune speeds/distances/timers in assets, not constants in code.
4. **Scalability of content** — adding an item, interactable, key type, or enemy touches *new files only*, never a switch statement in core code.
5. **Safety net** — the pure logic (inventory, dependency system, overlap math) is unit-tested before and after being touched.

## 4. Non-Goals

- No new gameplay features (enemy AI, sword/whip mechanics, health pickups stay in `TODO.md`).
- No ECS/DOTS migration, no third-party DI framework, no networking-readiness work.
- No scene/prefab re-authoring beyond what renamed/moved scripts force.
- The Apple prototype FSM is deleted, not modernized.

## 5. Success Metrics

| Metric | Today | Target |
|---|---|---|
| Player build compiles | ❌ (editor-only APIs in runtime code) | ✅ on every commit |
| `async void` methods outside Unity event entry points | ~12 | 0 (all routed through a guarded runner) |
| Four-way direction `if`-chains | 8+ copies (~400 lines) | 1 utility |
| Overlap AABB implementations | 4 (`OverlapCheckerBase`, `DamageOverlap`, `MovementOverlap`, `RopeObstructionOverlap`) | 1 base + thin subclasses |
| Hand-maintained animation duration tables | 1 per anim class | 0 (durations read from clips) |
| EditMode test count | 0 | ≥ 25 covering inventory, dependency, overlap math |
| Adding a new key/lock type | edit `KeyTypes` enum + every match site | create one ScriptableObject asset |

---

## 6. Workstreams

Requirements are tagged **P0** (correctness/build — do first), **P1** (architecture — the core of this PRD), **P2** (polish/guardrails).

### WS1 — Build & Correctness Blockers (P0)

Latent bugs found in the current code; all are small, none are risky to fix.

| # | Problem | Evidence | Requirement |
|---|---|---|---|
| 1.1 | Editor-only API in runtime assembly — **player builds will fail** | `PlayerAnimator.cs` (`using UnityEditor.Animations`), `GridSnapper.cs` (`using UnityEditor`, `EditorApplication.isPlaying`) | Delete `PlayerAnimator` (vestigial). Wrap `GridSnapper` body in `#if UNITY_EDITOR` or move it to `Scripts/Editor/`. |
| 1.2 | 3D physics callback in a 2D game — **never fires** | `PlayerStateMachine.OnCollisionEnter(Collision)`; `PlayerStateBase.OnCollisionEnter(... Collision ...)` | Switch to `OnCollisionEnter2D(Collision2D)` or delete the hook entirely (no state uses it today — prefer delete). |
| 1.3 | HUD events broken | `PlayerStatus.UpdateHealth` invokes `HealthChange.Invoke(HP)` — **null deref if no HUD**, and passes the *delta*, while `PlayerStatusHUD.UpdateHealth` writes it to `Image.fillAmount` (expects 0–1 total) | Null-safe invoke (`?.Invoke`), pass current totals, normalize health to 0–1 at the HUD boundary. |
| 1.4 | Singleton race | `PlayerStatusManager.Init` dereferences `HUDManager.instance` with no null guard; init order depends on scene | Use `Core.Utilities.Singleton<T>` (already written, unused) or a null-checked lookup with a clear error. |
| 1.5 | Global physics flag mutation | `SlidableBase.ClosestContactPointHelper.SetColliderHit` sets `Physics2D.queriesStartInColliders = false` globally and never restores it | Set once in project Physics2D settings; remove the runtime write. |

**Acceptance criteria:** a Mac/standalone player build compiles; HUD updates correctly with and without damage wired; no runtime writes to global `Physics2D` state.

### WS2 — Async & Lifetime Safety (P0/P1)

The codebase mixes `async void`, `Task`, `Task.FromResult` fake-async, raw `Task.Delay` (continues after object death), and Unity `Awaitable`. There is no convention, and exceptions inside `async void` bodies tear down nothing — they just log (or worse, vanish before the `try` is entered).

**Requirements:**

- **2.1 (P0)** Adopt one rule: *Unity lifecycle/event entry points may be `async void`; everything they call returns `Awaitable`/`Task` and is awaited.* Document it in `CLAUDE.md`/`CONTRIBUTING`.
- **2.2 (P0)** Every awaited delay ties to object lifetime: replace `Task.Delay(...)` with `Awaitable.WaitForSecondsAsync(t, destroyCancellationToken)` in `BombTimer`, `CandleItem`, `SlidableBase.SlideItem` (the `Task.Delay(AnimationDelayMs)`). Unity's `MonoBehaviour.destroyCancellationToken` removes all the hand-rolled `CancellationTokenSource` bookkeeping in `BombTimer`/`CandleItem`.
- **2.3 (P1)** Remove fake async: `KeyPortOverlap.FindKeyPort`/`IsOnKeyPort` and `GrappleSocketOverlap.GetMostOverlappedHookStartCol` do no awaiting — make them synchronous and delete the `await Task.FromResult` noise. Callers simplify accordingly (`PushBlock.CleanUp`, `SlidableBase.CleanUp`, `GrappleProjectile.CheckForStartPin`).
- **2.4 (P1)** Player-state async actions (`PlayerThrowState.EnterState`, `PlayerSlideState.Action`, `PlayerEquipState.EnterState`) route through one helper on `PlayerStateMachine` — e.g. `RunStateAction(Func<Awaitable>)` — that owns the try/catch, the "return to idle on failure" recovery, and the stale-continuation guard from WS3. Today each state hand-rolls (or forgets) this.

**Acceptance criteria:** zero `async void` outside Unity entry points; destroying any object mid-action (bomb, candle, sliding block, grapple) produces no errors and no orphaned tasks; grep for `Task.FromResult` returns nothing.

### WS3 — State Machine Hardening (P1)

The state pattern is right; its edges are sharp.

- **3.1 — Stale-continuation guard.** `PlayerThrowState.EnterState` awaits a pick-up animation, then calls `stateManager.item.Interact(...)`. If the state changed while awaiting (death, another input), the continuation still runs against the wrong state. Add a transition counter/token to `PlayerStateMachine.SwitchState`; the WS2 runner checks it after every `await` and aborts silently if stale.
- **3.2 — Kill mutable cross-call state in shared state instances.** States are pre-allocated singletons, yet `PlayerPullState` caches `_stateManager`, `_pullable`, `_axis`; `PlayerUseState` caches `_buttonUp`; `PlayerThrowState` caches `_currentAnimation`. Either (a) pass context into every call and keep states stateless, or (b) accept per-state fields but **clear them in `ExitState`** — today `ExitState` is empty in all three. Choose (b) as the minimal fix; document it.
- **3.3 — Dispatch without the switch.** `PlayerIdleState.Action`'s `switch (item.Kind)` means every new `InteractionType` edits idle-state code. Replace with a `Dictionary<InteractionType, PlayerStateBase>` built in `PlayerStateMachine.Awake`. (Keeping the enum is fine — the goal is that *dispatch* is data, not control flow.)
- **3.4 — Delete the false abstractions.** `IPlayerState` (nested empty classes, implemented by nothing), `IStateMachine` (states take `PlayerStateMachine` concretely anyway), `PlayerContext` (empty), `PlayerMover` (movement actually lives in `PlayerStateBase.Move`). Keeping unused seams is a cost, not an investment — re-introduce an interface when a second consumer exists.
- **3.5 — Implement or quarantine `PlayerDeathState`.** Six `NotImplementedException`s on a reachable-by-design state is a crash waiting for WS6 to wire damage. Minimum viable: stop input, play nothing, log. Full version arrives with WS6.

**Acceptance criteria:** spamming interact/use/switch during any animation cannot leave the player stuck or acting on a stale target; no `NotImplementedException` anywhere; `Interactables`/`Items` compile without referencing `PlayerStateMachine` (context only).

### WS4 — Overlap System Consolidation (P1)

`OverlapCheckerBase` was a good extraction; finish the job.

- **4.1** Fold `DamageOverlap` and `MovementOverlap` onto `OverlapCheckerBase`. Both re-implement the AABB-from-sprite-bounds code and a private `OverlapMoveCheckHelper` with near-identical logic but different magic offsets (`(0,-0.82)` vs `(0,-0.7)`…).
- **4.2** Replace the three hard-coded helper classes (`OverlapCheckHelper` + two private copies) with **serialized per-direction placement data** (`[SerializeField] DirectionalPlacement` struct: offset + scale per facing) so prefabs own their tuning and the code owns only the mechanism.
- **4.3** **Test, then fix, the intersection math.** `OverlapCheckerBase.GetOverlappingArea` mixes corners (`Mathf.Max(_areaBottomLeftCornerAABB.x, overlappingTopRightCornerAABB.x)` uses the wrong corner; the `y` term similarly) and `KeyPortOverlap.GetPercentOfOverlap` multiplies *A's width by B's height* for the denominator. These "work" today because callers only compare relative magnitudes or tuned thresholds — write characterization tests first, then replace both with `Bounds`-based intersection (`Vector3.Min/Max` as `KeyPortOverlap` already half-does), re-tune the 60 % seat threshold if needed.
- **4.4** Strip dead weight: `DamageOverlap`'s commented-out emergency-stop/layer blocks, `RopeObstructionOverlap`'s unused `GetAABBCorners`, `MovementOverlap`'s empty `AddDetectionLayers`.
- **4.5** Layer-mask presets: classes assemble masks bit-by-bit in `Awake` (`SlidableBase`, `PushBlock`, etc.). Add composed masks to `LayerIndex` (e.g. `LayerIndex.ObstructionMask = SlidableObstruction | Interactable | Locked`) so "what blocks movement" is defined once.

**Acceptance criteria:** one AABB implementation; `Overlap/` line count roughly halves; characterization tests pass before and after; checker prefabs expose their direction placement in the Inspector.

### WS5 — Direction & Animation Unification (P1)

The single largest source of duplication.

- **5.1 — `Direction` utility.** A `Facing` enum (`Up/Down/Left/Right`) with extensions: `FromVector(Vector2)`, `ToVector()`, `IsHorizontal()`. Replaces the four-branch `if (dir == Vector2.up) …` chains in `PlayerStateBase.UpdateLookDirection`, `GrappleProjectile.SetHookSprite`, `MoveAnimState`, `CarryAnimState`, `KickAnimState`, `PushPullAnimState`, and the overlap placement helpers.
- **5.2 — `DirectionalClipSet`.** A small struct/class holding four animator-state hashes (one per facing) with `Play(Animator, Facing)` and `PlayAndWait(Animator, Facing, CancellationToken)`. Each anim-state class (`KickAnimState` etc., currently 60–220 lines) collapses to declaring its clip names.
- **5.3 — Kill the hand-kept TimeSheets.** Clip durations live in `Dictionary<int, float>` literals that silently drift when clips are re-timed. `PlayAndWait` should read the duration from the entered state (`animator.GetCurrentAnimatorStateInfo(0).length` after `animator.Update(0)`), or — simpler and robust — from a serialized `AnimationClip` reference (`clip.length`). Hand-typed seconds are acceptable only as an explicit override field.
- **5.4 — Delete empty bases** `AnimClipBase`, `AnimStateBase` (they declare nothing), or give `AnimStateBase` the real shared API (`DirectionalClipSet` + `PlayAndWait`) so it earns its place. Prefer the latter.
- **5.5 — Sprite-direction data.** `GrappleProjectile`'s four serialized sprites + if-chain becomes a `DirectionalSpriteSet` reusing 5.1.

**Acceptance criteria:** `Animation/Player/States/` shrinks ~60 %; re-timing a clip in the editor changes await durations with no code edit; no four-way direction `if`-chain outside the `Facing` utility.

### WS6 — Damage & Status Completion (P1)

Currently a marker interface with no payload, no player wiring, and one consumer that self-destructs.

- **6.1** Give damage a payload: `ApplyDamage(in DamageInfo)` where `DamageInfo` carries amount, source position, and kind (replaces the empty `IDamage` marker — sources construct a `DamageInfo` instead of *being* the damage).
- **6.2** Player becomes `IDamageable`: routes to `PlayerStatus.UpdateHealth`, triggers `CharacterFlash` (already written, unused by the player), i-frames window, and `PlayerDeathState` at zero (WS3.5).
- **6.3** Fix `ExplosionDamageArea`: it computes `damagePercent` and discards it, uses hard-coded range/damage, and re-queries `OverlapCircleAll` per trigger entry. Use the computed falloff in `DamageInfo.amount`; serialize range/damage.
- **6.4** `EnemyDummy` stops calling `Destroy(gameObject)` on any hit; it takes `DamageInfo` and dies through the same status pattern as the player (sets up the enemy work in `TODO.md`).

**Acceptance criteria:** bomb explosion damages the player, HUD bar drops, flash plays, death state entered at 0 HP; one damage code path shared by player and enemies.

### WS7 — Data-Driven Configuration (P1/P2)

- **7.1 (P1)** ScriptableObject configs for tuning constants now buried in code: `PlayerConfig` (move/pull/carry speeds — currently `Speed` overrides scattered across states), `PushBlockConfig` (speed, wiggle), `SlideConfig` (speed, delay, wall epsilon), `GrappleConfig` (range, speed), `BombConfig` (fuse, bounce decay), `CandleConfig` (fuel seconds). One asset per archetype; prefabs reference assets; variants (a faster bomb) become new assets, not new subclasses.
- **7.2 (P2)** Key/lock identity as assets: replace the `GameUtilities.KeyTypes` enum with a `KeyDefinition` ScriptableObject referenced by `KeyItem`, `OpenableBase`, `KeyPortBase`, and the sliding-block family. Matching becomes reference-equality; `SymbolKeyPort`'s string compare folds into it. Adding key type #7 = creating an asset. (This is the "touch new files only" goal applied to the lock system.)
- **7.3 (P2)** Move layer-name constants out of `GameUtilities` into `LayerIndex` (one home, not two) and delete what remains of `GameUtilities` (`CreateObjectFromClass` is unused; `GridDirection`/`GetDirectionFromTwoPoints` move to the `Facing` utility).

**Acceptance criteria:** zero gameplay-tuning `const float`s in `Interactables/`/`Items/`; a new key/lock pair requires no enum edit.

### WS8 — Items & Inventory Contract (P1)

- **8.1** Unify `PlayerInventory.PickUpItem(IItemHolder)` and `PickUpItem(IItem)` — they duplicate the full-inventory swap logic with different displacement behavior (swap-into-holder vs. drop). Extract one `AcquireItem(IItem, displacedSink)` core; the two entry points become thin adapters. Add the missing `ItemSwitch` invoke symmetry and index handling tests.
- **8.2** Codify the item lifecycle in `ItemBase`: `Use → (work) → PutAway → ItemFinishedCallback`. Today every item re-implements the callback dance slightly differently (`BellItem` invokes in `finally`, `CandleItem` from `ButtonUp`, `GrapplingHook` may pass an item). Provide `protected void Finish(Interactable next = null)` in `ItemBase` and make subclasses call only that.
- **8.3** Resolve the `ThrowableBase.PickUp` parenting quirk: `SetParent` is commented out but `localPosition = Vector3.zero` remains — with no parent change this teleports the object to world origin unless scene setup compensates. Decide the intended model (parent to `ItemAnchorPoint`), implement it, and delete the comment archaeology.
- **8.4** Stub items (`SwordItem`, `WhipItem`, `PlankItem`) get a shared `NoOpItem` pattern or an explicit `// STUB:` banner + tracking entry, so a reader can tell designed-minimal from unfinished. (Implementing them is `TODO.md` feature work, not refactor scope.)

**Acceptance criteria:** inventory behavior covered by EditMode tests (pickup, swap-when-full, drop-when-full, dispose, switch, sprite events); all items end interactions through one base-class path.

### WS9 — Project Structure & Hygiene (P2)

- **9.1 — Assembly definitions:** `IT.Runtime` (everything), `IT.Editor` (existing `Scripts/Editor/`), `IT.Tests.EditMode` / `IT.Tests.PlayMode`. Benefits: editor-API leaks like WS1.1 become compile errors at the source, faster iteration compiles, and a place to hang test assemblies.
- **9.2 — Deletions** (after WS3.4): `Prototype/Apple*` + `Core/StateMachine/StateBase` (note: `Core` currently references `IT.Prototype` — a core→prototype dependency that asmdefs would forbid; deleting both resolves it), `IStateMachine`/`IPlayerState`/`PlayerContext`/`PlayerMover`/`PlayerAnimator`, empty `HUDManager` methods, dead `using` directives (`Unity.VisualScripting` in `HUDManager`/`BombFlash` are accidental IDE imports).
- **9.3 — Conventions pass:** public fields → `[SerializeField] private` (e.g. `DamageOverlap._areaTopRightCornerAABB` is `public` with an underscore name); property naming (`getState`, `hookConnectorStartPin` → PascalCase); file-header path comments that no longer match real paths (e.g. `// Assets/Scripts/OverlapScripts/…`) deleted — git is the source of history.
- **9.4 — `HUDManager` on `Singleton<T>`** (or deleted in favor of direct reference wiring): one singleton idiom in the codebase, not two.

**Acceptance criteria:** asmdefs in place and compiling; `Prototype/` gone; no `public` mutable fields outside data containers; one singleton pattern.

### WS10 — Test Safety Net (P1 for targets of WS4/WS8, P2 breadth)

- **10.1** EditMode (pure C#, fast): `PlayerInventory` (8–10 cases), `Observable`/`Dependent`/`MultiDependent` (5–6 cases incl. unsubscribe on disable), overlap intersection math (WS4.3 characterization first), `Facing` conversions, `LatchedLever`/`RetractingLever` release logic (pure enough once DOTween is behind a seam).
- **10.2** PlayMode smoke tests (a handful, not exhaustive): interact→open chest spawns item; kick→block slides and seats; pull→door scrubs; full-inventory pickup drops holder.
- **10.3** Run order rule: characterization tests land **before** the workstream that touches the code (especially WS4).

**Acceptance criteria:** Test Runner green locally; WS4/WS8 diffs reviewed against passing characterization tests.

---

## 7. Phasing & Sequencing

Dependencies, not dates (solo project — each milestone is a mergeable, game-still-works checkpoint):

| Milestone | Contents | Why this order |
|---|---|---|
| **M1 — Stop the bleeding** | WS1 (all), WS9.2 deletions, WS2.1–2.2 | Small, independent, de-risks everything after; deletions shrink the surface the rest must cover |
| **M2 — Foundations** | WS10.1 characterization tests → WS4, then WS2.3–2.4, WS3 | Overlap + async + state machine are the load-bearing walls; tests go in first |
| **M3 — Systems** | WS5, WS6, WS8, WS7.1 | Each consumes M2's primitives (`Facing`, runner, guarded transitions); WS6 unblocks the enemy/health items in `TODO.md` |
| **M4 — Scale & guardrails** | WS7.2–7.3, WS9.1/9.3/9.4, WS10.2 | Structural polish once behavior is stable; asmdefs last so file moves don't fight earlier diffs |

Rule of thumb per PR: one workstream slice, game boots, relevant tests pass, `ARCHITECTURE.md` updated if a diagram changed.

## 8. Risks & Mitigations

| Risk | Likelihood | Mitigation |
|---|---|---|
| Fixing the overlap math changes puzzle feel (seat threshold, slide stop points) | High | WS4.3 characterization tests + manual playtest checklist per block type; re-tune thresholds, don't chase old bugs |
| Scene/prefab references break when scripts are deleted/renamed | Medium | Delete scripts only after confirming no scene references (Unity's "find references in scene"); keep `.meta` GUIDs when renaming |
| `KeyTypes` enum → SO migration (WS7.2) touches serialized prefab data | Medium | Do it last (M4); migrate one lock family at a time; keep the enum until all consumers are converted |
| DOTween callbacks (`onComplete`) outliving destroyed objects during refactor | Medium | Standardize `.SetLink(gameObject)` (already used in places — make it a checklist item) |
| Scope creep into feature work (enemy AI, sword) | High | Hard line: if it adds behavior players can see, it's `TODO.md`, not this PRD |

## 9. Open Questions

1. **WS3.2** — stateless states (pass context per call) vs. clear-on-exit fields: clear-on-exit is the cheap fix; is statelessness worth it before a second player (co-op `MaxPlayers = 2` hints yes someday)?
2. **WS7.2** — is the `KeyTypes` enum actually painful yet, or is 6 values fine for the game's planned size? (Defer-able without harm; that's why it's M4.)
3. **WS5.3** — serialized `AnimationClip` references vs. runtime state-info length: clip references are simpler and editor-visible but add prefab wiring; pick after prototyping one anim class.
4. Does anything outside the editor use `DepthSorter`'s every-frame `LateUpdate` on static props? (If yes, add a `static` flag that sorts once — micro-perf, only if profiling says so.)

---

## Appendix A — File Disposition (deletions/moves)

| File | Action | Reason |
|---|---|---|
| `Player/StateMachine/PlayerContext.cs` | Delete | Empty template |
| `Player/Movement/PlayerMover.cs` | Delete | Movement lives in `PlayerStateBase.Move` |
| `Animation/Player/PlayerAnimator.cs` | Delete | Vestigial + editor-API build breaker |
| `Player/StateMachine/IPlayerState.cs` | Delete | Empty nested-class sketch, no implementers |
| `Core/StateMachine/IStateMachine.cs` | Delete | States use the concrete class; re-add when a second machine exists |
| `Core/StateMachine/StateBase.cs` + `Prototype/Apple*.cs` | Delete | Learning prototype; creates a Core→Prototype dependency |
| `Animation/AnimClipBase.cs` | Delete | Empty |
| `Animation/AnimStateBase.cs` | Repurpose | Becomes the home of `DirectionalClipSet` + `PlayAndWait` (WS5) |
| `Core/Utilities/GridSnapper.cs` | Move to `Scripts/Editor/` | Editor-only tool |
| `Core/Utilities/GameUtilities.cs` | Dissolve | Layers → `LayerIndex`, direction helpers → `Facing`, unused helpers deleted (WS7.3) |
