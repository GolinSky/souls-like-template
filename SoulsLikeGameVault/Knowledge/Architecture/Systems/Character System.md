---
title: Character System
type: architecture
domains:
  - character
  - combat
  - movement
status: needs-review
authority: advisory
verified: 2026-09-22
source_commit: 509f9d27
updated: 2026-09-22
context_keys:
  - character-architecture
aliases:
  - CHARACTER_SYSTEM_ARCHITECTURE
tags:
  - status/needs-review
---
# Character System Architecture & Runtime Guide

> [!warning] Validation boundary
> Updated for the authorized Character Clean Architecture refactor on 2026-09-22. Source and focused Edit Mode checks cover the structural changes; animation, root-motion, scene transitions, and full gameplay remain a separate review phase. See [[Work/Plans/Character Clean Architecture]].

## 1. Overview & Core Architectural Philosophy

Character keeps Unity effects and entity integration outside an engine-independent runtime. Source dependencies point inward: the local session and Unity adapters depend on runtime contracts; runtime code does not depend on Unity, VContainer, input services, or session services.

| Owner | Responsibility |
|---|---|
| `PlayerInputReader` | Recognize physical button edges and the 0.30-second sprint/roll gesture; return session-owned input. |
| `PlayerController` | Feed the local session and update the camera through a Character-agnostic snapshot. |
| `PlayerSessionCoordinator` | Translate local input, coordinate targeting/interaction and death/rest, and clear pending input when the binding ends. |
| `CharacterActorTick` | Consume one submitted semantic frame in VContainer's `IPostTickable` phase; enforce pause/death/control gates without local input/UI/camera dependencies. |
| `CharacterActionCoordinator` | Resolve semantic priority, submit actions, and report outer execution results to the existing state machine. |
| `CharacterActionStateMachine` | Sole owner of action state, one-slot buffering, queue windows, and chained-exit handling. |
| `Character` | Execute Unity-facing actions, route animation observations, coordinate rest, and expose aggregate operations. |
| Components | Own movement, animation playback, equipment, health, inventory, and combat state. |
| `CharacterProgressionRules` | Calculate levels, rune costs, and projected stat values shared by UI preview and commit. |
| `CharacterStaminaPolicy` | Calculate stamina admission thresholds and attack costs; adapters preserve physical acceptance and charging order. |

VContainer constructs the state machine, coordinator, actor tick, and local session in the existing character scope. The runtime assembly has `noEngineReferences: true`; its vectors use `System.Numerics`. Unity adapters convert vectors at the boundary. HealthComponent remains the only mutable resource owner. No networking, recovery framework, or second resource state store is introduced.


## 2. Entity Identity & Lifetime Management

### 2.1 Entity Boundary & Locator Registration

The player entity is composed of two coordinated layers:
- **`Character` (MonoBehaviour)**: The authoritative gameplay aggregate owning components, physics, combat orchestration, and public facade state.
- **`Entity` / `ViewEntity` (`IEntity`)**: The base entity system identity holding a unique generated 64-bit ID, `EntityType.Player`, and registered with `IEntityLocator`.

### 2.2 Factory & Lifetime Scope (`CharacterFactory.cs`)

When `CharacterFactory.CreateCharacter` is called:
1. Loads and validates the independent `Character` prefab via `IAssetService.LoadPrefab`.
2. Calls the parent scope's `CreateChildFromPrefab` with the inactive `CharacterScopeInstaller` prefab. VContainer clones the concrete scope and assigns its parent. The factory instantiates Character under it and calls `Character.StageSpawn` to apply the optional world-space position before activation.
3. Activates the hierarchy so `Awake` and `OnEnable` run, then builds the scope synchronously. `Character` configures its animator during injection, before entry-point initialization.
4. `CharacterScopeInstaller` finds required actor components under its own transform and registers:
   - Entity system (`RegisterEntitySystemExt`, commands: `InteractionCommand`, `GroundItemCollectionCommand`, `ApplyDamageCommand`, `ResolveMeleeHitCommand`, `TargetingCommand`).
   - Domain models, components, ScriptableObjects, and database catalogs (`ItemCatalog`, `WeaponDatabase`, `ShieldDatabase`, `ConsumableDatabase`).
   - UI Controllers (`PlayerHudUiController`, `LockOnUiController`, `InventoryUiController`, `EquipmentUiController`, `SystemUiController`, `PauseNavigationUiController`, `InteractionUiController`).
   - Player orchestration (`PlayerInputReader`, `InteractionController`, `PlayerSessionCoordinator`, `PlayerController`).
5. Generates one scoped entity ID through `IUniqueIdGenerator` and returns `Character` only after the build succeeds. `CharacterFactory.Dispose` disposes its one owned local-player scope. Creation failures propagate without factory cleanup of the partial scope.

The `Character` prefab has no `LifetimeScope`. Normal respawn reuses the existing actor, while later coop-specific creation and removal remain at the composition boundary.

`PlayerSessionCoordinator` uses `.AsSelf().AsImplementedInterfaces()` to expose the same registration through its concrete type, `IPlayerSession`, and lifecycle interfaces. Do not also add `.As<IPlayerSession>()`: the installed VContainer version appends implemented interfaces without deduplication, causing scope construction to fail.

---

## 3. Input Pipeline & Semantic Control Translation

The path is `PlayerInputReader -> PlayerController -> IPlayerSession -> PlayerSessionCoordinator -> CharacterActionCoordinator -> CharacterActorTick -> Character`. Neither the controller nor the reader references Character components, character commands, or action states.

`PlayerSessionInput` carries axes, button edges, and held values. The session translates it into `CharacterInputFrame`; the coordinator produces `CharacterInput` with at most two semantic actions. `PlayerSessionSnapshot` exposes only camera and control information to the local controller.

### 3.1 PlayerInputReader (`Assets/Scripts/Entities/Character/Input/PlayerInputReader.cs`)

The reader owns the 0.30-second sprint hold and short-release roll gesture. It resets physical gesture state when control is disabled. The runtime coordinator owns state-dependent arbitration: heavy/special suppression during rolls, heavy/light suppression, and ItemUse restrictions.

Priority remains right equipment, left equipment, quick-item selection, item use, then hand-mode toggle. Hand mode is the only companion to an equipment action. With no equipment action, priority is heavy attack, special attack, light attack, guard-as-left-hand attack, roll, then jump. A heavy press during a roll does not activate later light-attack suppression.

### 3.2 CharacterInput & CharacterAction Structs

`CharacterInputFrame` contains control intent before priority selection. `CharacterInput` contains movement and holds plus the selected first/second action. `CharacterAction` contains only runtime values and semantic action identifiers. `CharacterActionExecution` returns acceptance and the started state through `ICharacterActionExecutor`; Character implements that real Unity execution boundary.


## 4. Action State Machine & Action Lifecycle

`CharacterActionStateMachine` owns the states `Neutral`, `Attack`, `Roll`, `EquipmentSwap`, `Critical`, `ItemUse`, and `BlockHit`. `CharacterActionCoordinator` uses that same injected instance; it does not maintain another action state or buffer.

### 4.1 State Hierarchy & Transitions

Neutral permits immediate dispatch. Attack, Roll, ItemUse, and BlockHit follow the existing state-specific admission and animation queue rules. EquipmentSwap admits the existing companion equipment action. Critical rejects ordinary actions until completion. Unity execution can report Executed, TemporarilyBlocked, or Invalid; the coordinator reports that result to the state machine without charging resources or forcing success itself.

### 4.2 Buffering & Execution Semantics

The buffer remains one slot with latest-input replacement and a one-second timeout. Expiration is pruned only in Neutral. QueueCheck permits buffered execution, including commands retained through a long action. Chained exits and roll-to-sprint interruption retain their existing state-machine ownership. Animation callbacks route observations to that state machine and ask the coordinator to execute any newly permitted buffered action.


## 5. Unified Capability Gating (`MovementLockReason`)

To eliminate conflicting boolean flags and prevent race conditions between overlapping blocking lifecycles, `Character.cs` manages movement and control locks using a single bitmask enum:

```csharp
[Flags]
private enum MovementLockReason
{
    None      = 0,
    Manual    = 1 << 0,  // External / script block
    Animation = 1 << 1,  // Root motion / animation block tag
    Spawn     = 1 << 2,  // Initial spawn sequence
    Parry     = 1 << 3,  // Active parry animation window
    Critical  = 1 << 4   // Synchronized critical attack sequence
}
```

### Derived Capability Rules

- **Movement Blocked**: `movementComponent.SetMovementBlocked(_movementLockReasons != MovementLockReason.None)`
- **Input Blocked**: `_actionStateMachine.IsInputBlocked` (synchronized with `Spawn`, `Parry`, `Critical`, or `Grace` transitions).
- **Guard Permission**:
  ```csharp
  private bool CanGuard() => 
      _movementLockReasons == MovementLockReason.None || 
      (_movementLockReasons == MovementLockReason.Animation && _actionStateMachine.CanGuardDuringAnimationBlock);
  ```
  Guard is permitted during animation movement block specifically when `_actionStateMachine.CurrentState == State.Attack && _queueWindowOpen`.

---

## 6. Component Responsibilities & Boundaries

Character coordinates workflows spanning components. Direct single-component operations remain appropriate where they already express ownership clearly; do not introduce a facade wrapper for every method. Cross-entity communication still uses `IEntityLocator -> IEntity -> target-owned command`.

### 6.1 `MovementComponent` (`Assets/Scripts/Components/Movement/MovementComponent.cs`)

Owns physics, ground probing, gravity, and jump/roll trajectories. Character converts pure runtime vectors to Unity vectors and applies the resulting movement presentation.

### 6.2 `AnimatorComponent` (`Assets/Scripts/Components/Animator/AnimatorComponent.cs`)

Owns Animator playback and observed timing. Its existing callback into Character is an outer Unity collaboration. Short state names/hashes, sub-state machines, root-motion ownership, and callback correlation remain unchanged.

### 6.3 `AttackComponent` (`Assets/Scripts/Components/Attack/AttackComponent.cs`)

Owns weapon/combo context and attack resolution. `ResolveAttack(in CharacterAction)` reads its own context; callers no longer fetch that context only to pass it back.

### 6.4 `EquipmentComponent` (`Assets/Scripts/Components/Equipment/EquipmentComponent.cs`)

Owns slots, active selections, and swap timing. It publishes `SlotChanged` followed by `LoadoutChanged`. Character subscribes to loadout changes and applies presentation/weapon updates once, including one explicit initial application. Equipment does not inject or call Character. Character removes its subscription on disposal.

### 6.5 `CombatDefenseComponent` & `CriticalAttackController` (`Assets/Scripts/Entities/Combat/`)

Keep defense, hit reactions, and synchronized critical execution ownership. Critical eligibility must be updated after state-machine ticking and before current-frame action submission.

`BaseComponent<TModel>.Model` remains an injection/standalone-setup contract because existing component tests construct models explicitly. Runtime model replacement is not a gameplay operation. HealthComponent owns mutable health, focus, stamina, and invulnerability; rest applies authoritative stats once and then refills the existing flask supply.


## 7. Frame Execution & Update Order

VContainer phases establish order without depending on registration order within a phase:

1. `PlayerController.ITickable`: read local input, then session targeting/interaction and semantic arbitration; submit a frame.
2. `CharacterActorTick.IPostTickable`: consume that frame once; apply existing death/input-block/pause/ladder gates; call Character.Tick.
3. `PlayerController.ILateTickable`: update camera follow, then the existing Idle camera rotation/tracking path.

Within Character.Tick, preserve strong-attack hold, state-machine advancement, critical eligibility, semantic submissions and buffer processing, guard/block decisions, movement/stamina charging, audio/recovery, then movement presentation. Animation callbacks may independently open a queue window and process the buffered action.

Scripted control can submit semantic CharacterInput directly to CharacterActorTick without resolving the local controller or session. The actor adapter consumes each submitted frame once; a continuous scripted controller submits each frame. The existing factory still creates the complete local-player composition. Its topology, prefab GUIDs, and serialized component fields are unchanged.


## 8. Animation Feedback & Routing Matrix

`Character.OnAnimationStateChanged(AnimatorStateMachineDto state)` dispatches incoming animation callbacks to exact subsystem owners:

| Animator State Machine Name | Signal State | Target Owner / Action |
|---|---|---|
| `LightAttack`, `HeavyAttack`, `RollAttack`, `RunAttack`, `BackStepAttack`, `SpecialAttack`, `Parry` | `Enter` / `QueueCheck` / `Exit` | `AttackComponent.HandleAnimatorState`<br/>`CharacterActionStateMachine` (enters Attack, opens queue, exits to Neutral) |
| `Roll`, `BackStep` | `Enter` / `QueueCheck` / `Exit` | `CharacterActionStateMachine` (enters Roll, opens queue, triggers sprint interrupt or exits) |
| `EquipmentSwapOut`, `EquipmentSwapIn` | `Enter` / `Progress` / `Exit` | `EquipmentComponent.HandleAnimationState`<br/>`CharacterActionStateMachine` |
| `Spawn` | `Enter` / `Exit` | Sets/Clears `MovementLockReason.Spawn` and State Machine input block |
| `Death` | `Exit` | Clears `_isDeathAnimationPlaying`, fires `OnDeathAnimationCompleted` |
| `Parry` | `Enter` / `Exit` | Sets/Clears `MovementLockReason.Parry` and input block |
| `HitReaction` | `Enter` / `Exit` | `CombatDefenseComponent.SetHitReaction(true / false)` |
| `ParryStun` | `Enter` / `Exit` | `CombatDefenseComponent.SetParryStunned(true / false)` |
| `GraceUnblock`, `GraceRestStart`, `GraceRestEnd` | `Enter` / `Exit` | `Character.HandleGraceAnimationState` (advances `GracePhase` and resolves `UniTaskCompletionSource`) |

---

## 9. Lifecycle Systems: Spawn, Death, Grace

### 9.1 Spawn

Character initialization sets up components, subscribes to their events, applies the initial loadout, and blocks input. The factory stages the arrival before beginning the existing spawn/grace animation flow. Cursor locking belongs to local PlayerController initialization.

### 9.2 Death & Respawn

PlayerSessionCoordinator observes health death, calls Character.PlayDeath, then requests the existing core respawn operation when the death animation completes. It unregisters its game-state and death subscriptions on disposal and clears pending input. Factory/scope lifetime ownership remains unchanged.

### 9.3 Grace Rest Transitions

Character retains existing animation-driven grace transitions and completion sources. The session invokes Character.RestoreAtGrace on the same game-state transitions as before. That use case restores existing resource values/alive state through HealthComponent and refills five Crimson Flasks. No identity-only restoration class or parallel resource model is needed.


## 10. Rules of the Character System (Durable Invariants)

1. Keep runtime policy independent of Unity, VContainer, local input, UI, camera, and session services. VContainer belongs in the outer composition root.
2. Keep action state/buffering in the existing state machine, and health storage in HealthComponent.
3. Preserve the one-slot/one-second buffer, Neutral-only expiry pruning, queue windows, interruption cleanup, and physical rejection/resource-charge timing.
4. Keep reason-aware movement locks; one workflow must not unlock another.
5. Keep physical gestures in the input adapter and state-dependent action choices in the runtime coordinator.
6. Apply component snapshots at presentation boundaries. Cross-entity work uses the existing locator/command path.
7. Keep scoped subscriptions paired with disposal, and clear pending input on disable/unbind.
8. Add only meaningful responsibilities and real boundary contracts, with one top-level type per new file and a short class summary. Avoid global event buses, speculative wrappers, or new recovery machinery.
