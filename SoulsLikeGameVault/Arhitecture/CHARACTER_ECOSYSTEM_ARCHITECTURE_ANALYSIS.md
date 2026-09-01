# Character Ecosystem: Raw Architecture Inventory and Simplification Analysis

> Audience: GPT-5.6 Pro or another architecture-focused reviewer.
>
> Repository snapshot: branch `main`, commit `4e7ba1ddb681`, inspected 2026-09-01.
>
> Scope: the player `Character` as a gameplay entity, its prefab and VContainer composition, components, input pipeline, action state machine, animation feedback, facade surface, and mediator responsibilities.
>
> This is an investigation document, not an implementation plan. Part I records source-backed facts. Part II analyzes the facts and identifies simplification targets. No production code or Unity assets were changed. Tests were not executed, in accordance with repository instructions.

## Method and evidence rules

The investigation used the existing `graphify-out/graph.json` first, then verified important relationships against the live C# source with Serena symbol queries and exact-text searches. The Character prefab YAML was inspected read-only to confirm its serialized composition.

The Graphify query was expanded from graph vocabulary to:

`character player input state machine mediator controller movement action command animation entity`

Graphify is useful for breadth, but its snapshot contains duplicate nodes and at least one stale or source-less node (`IComponentMediator`). Therefore:

- live source is authoritative;
- all line references below are 1-based;
- a statement labeled **fact** is directly supported by the cited source;
- a statement labeled **inference** is architectural interpretation;
- a statement labeled **risk** is a plausible failure mode that still needs a reproduction or explicit behavioral decision;
- a statement labeled **defect** has a concrete contradiction or unused contract in the current source.

---

# Part I — Raw information

## 1. What “the Character entity” currently means

There are two parallel concepts named like an entity.

### 1.1 Gameplay Character

`SoulsLike.Entities.Character.Character` is a `MonoBehaviour`. It is the player gameplay facade and the central mediator for movement, animation, actions, equipment, health, inventory, audio, grace, death, currency, and lock-on.

Source: `Assets/Scripts/Entities/Character/Character.cs:24-724`.

### 1.2 Base entity-system Entity

`SoulsLike.Entities.BaseEntity.Entity` implements `IEntity`. It owns an ID, an `EntityType`, an `IEntityLocator` registration, and a list of `IEntityComponent` command-like components.

Sources:

- `Assets/Scripts/Entities/BaseEntity/Entity.cs:8-13`
- `Assets/Scripts/Entities/BaseEntity/Entity.cs:15-72`
- `Assets/Scripts/Entities/BaseEntity/EntityRegistrationExt.cs:7-12`

`Character` does **not** implement `IEntity`. `CharacterFactory` registers a separate `Entity` for the player scope and also registers a `ViewEntity` MonoBehaviour containing the same generated ID and `EntityType.Player`.

Sources:

- `Assets/Scripts/Entities/Character/CharacterFactory.cs:81-94`
- `Assets/Scripts/Entities/BaseEntity/ViewEntity.cs:15-26`

The practical result is:

```text
player scope
├── Entity                 // locator identity + IEntityComponent lookup
├── ViewEntity             // Unity view identity
└── Character              // actual player gameplay aggregate/facade
```

The “entity boundary” is therefore split between identity/commands (`Entity`) and gameplay state/behavior (`Character`).

## 2. Character prefab composition

The root `Character` GameObject is on layer 6, tagged `Player`, and contains 12 components including its Transform.

Root components confirmed in `Assets/Prefabs/Character/Character.prefab:825-1034`:

1. `Transform`
2. Unity `CharacterController`
3. `Character`
4. `MovementComponent`
5. `AnimatorComponent`
6. `EquipmentComponent`
7. `HealthComponent`
8. `InventoryComponent`
9. `AttackComponent`
10. `EquipmentPresentation`
11. `ViewEntity`
12. `PlayerMeleeCombatRelay`

Serialized references on `Character` point to:

- root `MovementComponent`;
- root `AnimatorComponent`;
- child `CharacterAudioComponent`;
- root `EquipmentComponent`;
- root `HealthComponent`;
- root `InventoryComponent`;
- root `EquipmentPresentation`;
- child `PlayerCameraRoot` Transform.

Source: `Assets/Prefabs/Character/Character.prefab:891-914`.

Additional character hierarchy objects visible in the prefab include:

- `PlayerCameraRoot`;
- `AimTarget`;
- `TargetLockNode`;
- `Geometry` / armature / mesh hierarchy;
- right/left hand anchors and a fist runtime;
- `AnimatorStateMachineReceiver` and `AnimatorRootMotionRelay` on the animated hierarchy;
- `CharacterAudioComponent` on a child.

`Character` still serializes `aimTargetDistance` and `aimLayerMask`, but neither field is read by `Character.cs`.

Sources:

- `Assets/Scripts/Entities/Character/Character.cs:52-54`
- exact-use search across `Assets/Scripts` found no reads.

## 3. Dynamic composition and lifetime scope

`CharacterFactory.CreateCharacter` does all of the following:

1. loads the `Character` Addressables prefab;
2. instantiates it;
3. applies the spawn position before child-scope construction;
4. gets or adds `ViewEntity`;
5. validates required prefab components;
6. generates an entity ID;
7. creates a child `LifetimeScope`;
8. registers the entity system, components, models, databases, commands, controllers, UI controllers, runtime objects, gesture resolvers, and adapters;
9. reparents the instance beneath the generated scope.

Source: `Assets/Scripts/Entities/Character/CharacterFactory.cs:42-150`.

The method contains 52 `builder.Register...` calls at `CharacterFactory.cs:85-144`. The scope includes:

### Entity and commands

- `Entity` / `IEntity` through `RegisterEntitySystemExt`;
- `ViewEntity`;
- `TargetLockNode`;
- `InteractionCommand`;
- `GroundItemCollectionCommand`;
- `ApplyDamageCommand`;
- `TargetingCommand`.

### Gameplay components and models

- `Character` and `CharacterData`;
- `AnimatorModel`, `AnimatorComponent`;
- `CharacterAudioData`, `CharacterAudioComponent`;
- `AttackComponent`, `PlayerMeleeCombatRelay`;
- `MovementModel`, `MovementData`, `MovementComponent`;
- `EquipmentModel`, `EquipmentComponent`, `EquipmentPresentation`;
- `InventoryData`, four item databases, `ItemCatalog`, `InventoryModel`, `InventoryComponent`;
- `HealthData`, `CharacterHealthData`, `HealthModel`, `HealthComponent`.

### Player/runtime/input objects

- `UnityCharacterClock`;
- `MovementGate`;
- `CharacterCommandBuffer`;
- `CharacterActionStateMachine`;
- `CharacterRuntime`;
- `CharacterAnimationAdapter`;
- `EquipmentSwapCoordinator`;
- `SprintRollGestureResolver`;
- `HeavyAttackGestureResolver`;
- `PlayerCharacterInputAdapter`;
- `InteractionController`;
- `PlayerController`.

### UI registered inside the character scope

- `PlayerHudUiController`;
- `LockOnUiController`;
- `InventoryUiController`;
- `EquipmentUiController`;
- `SystemUiController`;
- `PauseNavigationUiController`;
- `InteractionUiController`.

`CharacterFactory.Dispose` disposes the whole generated character scope.

Source: `Assets/Scripts/Entities/Character/CharacterFactory.cs:152-155`.

## 4. Size and responsibility map

Line counts are physical source lines at the inspected snapshot.

| Type/file | Lines | Main responsibilities visible in source |
|---|---:|---|
| `Character.cs` | 725 | facade, action executor, tick orchestration, movement/animation/equipment sinks, grace/death, consumables, stats/currency, lock-on |
| `MovementComponent.cs` | 898 | controller movement, ground probing, slopes, collision resolution, gravity, landing, jumping, rolling, crouch, facing, lock-on movement, animation blend output, debug gizmos |
| `AnimatorComponent.cs` | 537 | animator parameters/triggers, action triggers, layer weights, controller/profile swaps, hand modes, state observation, root-motion relay setup, smoothing |
| `CharacterRuntime.cs` | 496 | 23 top-level runtime protocol/state types plus a thin runtime wrapper |
| `CharacterActionStateMachine.cs` | 267 | four-state action policy, queue window, one-command buffer use, equipment companion rule, roll-to-sprint interrupt, stale-exit suppression |
| `PlayerController.cs` | 174 | game-state gating, tick ownership, camera, lock-on, interaction tick, death/respawn bridge |
| `PlayerCharacterInputAdapter.cs` | 149 | raw input reads, gesture state, implicit action priority, command creation, two-command batching |
| `EquipmentSwapCoordinator.cs` | 139 | equipment swap phase state and animation-progress/exit handling |
| `CharacterAudioComponent.cs` | 120 | audio settings observation and movement/combat audio playback |
| `CharacterAnimationAdapter.cs` | 62 | Animator state-name to generic character-action-state conversion |

Graphify’s degree ranking reported `Character` as the highest-degree source-backed node in the character area with 89 edges, followed by `AnimatorComponent` with 64 and `MovementComponent` with 57. These numbers are graph heuristics, not compiler dependency counts, but they correctly identify the three main hubs.

## 5. Character’s declared roles

`Character` declares seven non-Unity roles in addition to being a `MonoBehaviour`:

```csharp
IInitializable
ICharacterActionExecutor
IMovementPresentationSink
IAnimationStateSink
IRootMotionSink
IEquipmentLoadoutSink
IDisposable
```

Source: `Assets/Scripts/Entities/Character/Character.cs:24-30`.

### 5.1 Serialized dependencies

`Character.cs:43-54`:

- `MovementComponent`;
- `AnimatorComponent`;
- `CharacterAudioComponent`;
- `EquipmentComponent`;
- `HealthComponent`;
- `InventoryComponent`;
- `EquipmentPresentation`;
- camera target;
- unused aim settings.

### 5.2 Injected dependencies

`Character.ConfigureRuntime`, `Character.cs:81-103`:

- `AttackComponent`;
- `CharacterRuntime`;
- `CharacterAnimationAdapter`;
- `EquipmentSwapCoordinator`;
- `EquipmentPresentation` again;
- `ItemCatalog`;
- `IEntityLocator`;
- `ICombatStateNotifier`;
- `CharacterData`.

The component acquisition style is mixed: most required Unity components are serialized, `AttackComponent` is injected, and `EquipmentPresentation` is both serialized and injected, with the injected value overwriting the serialized field.

### 5.3 Public facade state

`Character.cs:68-79` exposes:

- camera target;
- grounded state and vertical velocity;
- the concrete `InventoryComponent`;
- mutable `HealthStats`;
- held currency;
- attribute stats;
- input-block status;
- current action state;
- equipment action status;
- death-animation completion event.

As a `MonoBehaviour`, callers also use `Character.transform` directly.

### 5.4 Public behavior surface used externally

The main external uses are:

- `PlayerController`: tick, camera data, lock-on, input-block state, death and death-animation completion;
- `CoreGameOrchestrator`: grace transitions, death completion, respawn position, current position;
- UI: attributes and character summary data;
- `GroundItemCollectionCommand`: currency grant;
- `CharacterHealthData`: attributes for derived max health;
- `InteractionController`: character Transform.

Sources:

- `Assets/Scripts/Entities/Character/PlayerController.cs:51-172`
- `Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs:52-183`
- `Assets/Scripts/Ui/Inventory/InventoryUiController.cs:23-48,102,201-226`
- `Assets/Scripts/Ui/Equipment/EquipmentUiController.cs:24-45,85,117,180-202`
- `Assets/Scripts/Entities/BaseEntity/EntityCommands/GroundItemCollectionCommand.cs:42`
- `Assets/Scripts/Entities/Character/CharacterHealthData.cs:6-33`
- `Assets/Scripts/Interactions/InteractionController.cs:40-49,109`

`GroundItemCollectionCommand` aliases `SoulsLike.Entities.Character.Character` as `PlayerCharacter`; it is not a different abstraction.

Source: `Assets/Scripts/Entities/BaseEntity/EntityCommands/GroundItemCollectionCommand.cs:4`.

## 6. Character initialization, tick, and disposal

### 6.1 Initialize

`Character.Initialize`, `Character.cs:105-113`:

1. subscribes to `HealthModel.OnDamageApplied`;
2. pushes current equipment hand mode into `AnimatorComponent`;
3. builds and applies the initial loadout;
4. locks the cursor;
5. blocks input through `CharacterRuntime`;
6. triggers the spawn animation.

### 6.2 Tick

`Character.Tick`, `Character.cs:120-178`, executes one cross-domain frame in this order:

1. informs `AttackComponent` whether strong attack is held;
2. restores normal charged-attack speed when not held;
3. ticks `CharacterRuntime`, passing `this` as the action executor;
4. consumes any runtime request to interrupt roll for sprint;
5. resolves a `MovementPolicy`;
6. rebuilds the equipment loadout;
7. classifies guard as shield-block or weapon-block through `ItemCatalog`;
8. calculates combat sprint stamina drain and start threshold;
9. sets `MovementComponent`’s movement-blocked boolean;
10. moves the character;
11. ticks movement audio;
12. consumes sprint stamina if applicable;
13. writes animator block booleans;
14. ticks stamina recovery.

### 6.3 Dispose

`Character.Dispose`, `Character.cs:115-118`, only removes the health damage subscription. Other object lifetimes are owned by their VContainer registrations, MonoBehaviour destruction, or `PlayerController`.

## 7. Complete player input path

The normal per-frame path is:

```text
Unity Input System
  -> InputService.CharacterActions
  -> PlayerController.Tick
  -> PlayerCharacterInputAdapter.Read(current action state)
  -> CharacterInputBatch
       ├── CharacterControlFrame (continuous held input)
       ├── FirstCommand?
       └── SecondCommand?
  -> Character.Tick
  -> CharacterRuntime.Tick(batch, Character)
  -> CharacterActionStateMachine.Tick(batch, Character)
  -> CharacterCommand.TryExecute(ICharacterActionExecutor)
  -> Character.TryStartAttack/Roll/Jump/EquipmentAction
  -> concrete component mutation + Animator trigger
  -> Animator StateMachineBehaviour callback
  -> AnimatorStateMachineReceiver
  -> AnimatorComponent.UpdateState
  -> Character.OnAnimationStateChanged
  -> CharacterAnimationAdapter
  -> CharacterRuntime.HandleAnimation
  -> CharacterActionStateMachine.HandleAnimation
```

### 7.1 InputService

`InputService` owns generated `ProjectInputActions`, exposes the `Character` action map, creates additional menu actions, and enables/disposes the input asset.

Source: `Assets/Scripts/Services/Input/InputService.cs:24-112`.

### 7.2 PlayerController tick gates

`PlayerController.Tick`, `PlayerController.cs:87-107`:

- clears interaction and returns when dead;
- clears interaction and returns when `Character.IsInputBlocked`;
- clears interaction and returns outside `Idle` or `Paused` game states;
- during `Paused`, clears interaction but still calls `Character.Tick(ReadMovementOnly())`;
- during `Idle`, processes lock-on, ticks interaction, then ticks Character with the full input batch.

Lock-on input is processed directly by `PlayerController`, outside the character command/state-machine path.

Interaction input is processed directly by `InteractionController`, also outside the character command/state-machine path.

Sources:

- `Assets/Scripts/Entities/Character/PlayerController.cs:87-107,122-162`
- `Assets/Scripts/Interactions/InteractionController.cs:64-74`

### 7.3 Gesture resolution

`SprintRollGestureResolver` uses a 0.3-second hold threshold:

- qualified hold -> sprint;
- unqualified release -> roll request.

Source: `Assets/Scripts/Entities/Character/Runtime/Input/SprintRollGestureResolver.cs:3-41`.

`HeavyAttackGestureResolver` tracks strong-input activity and suppresses light attack until light input is released.

Source: `Assets/Scripts/Entities/Character/Runtime/Input/HeavyAttackGestureResolver.cs:3-26`.

Both are separately registered singletons but only consumed by `PlayerCharacterInputAdapter`.

### 7.4 Input priority and two-command batch

`PlayerCharacterInputAdapter.Read`, `PlayerCharacterInputAdapter.cs:40-147`, creates at most two commands.

Priority is implicit in `if / else if` order:

```text
equipment group first:
  SwitchRightWeapon
  > SwitchLeftWeapon
  > SwitchQuickItem
  > UseQuickItem

TwoHanded may be retained as the only special second command.

if no equipment command exists:
  HeavyAttack
  > SpecialAbility
  > LightAttack
  > Guard-press encoded as left-hand LightAttack
  > Roll
  > Jump
```

Additional same-frame presses are discarded. The only preserved pair is “equipment command + toggle hand mode,” described in source as a legacy behavior.

Continuous frame data contains:

- move input;
- camera yaw;
- qualified sprint active: the sprint/roll resolver has crossed its 0.3-second hold threshold and movement input is nonzero;
- crouch held;
- guard held;
- strong attack held.

Sources:

- `Assets/Scripts/Entities/Character/Input/PlayerCharacterInputAdapter.cs:60-147`
- `Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs:6-47`

## 8. Runtime protocol inventory

`Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs` defines 23 public top-level types in one 496-line file:

1. `CharacterControlFrame`
2. `CharacterInputBatch`
3. `CharacterCommandKind`
4. `AttackIntent`
5. `CharacterCommandExecutionStatus`
6. `CharacterCommandDisposition`
7. `CharacterActionStateId`
8. `CharacterAnimationSignalKind`
9. `MovementGateReason`
10. `CharacterCommandExecutionResult`
11. `AttackRequest`
12. `RollRequest`
13. `JumpRequest`
14. `EquipmentActionRequest`
15. `EquipmentActionKind`
16. `ICharacterActionExecutor`
17. `CharacterCommand`
18. `ICharacterClock`
19. `CharacterCommandBuffer`
20. `MovementPolicy`
21. `MovementGate`
22. `CharacterAnimationSignal`
23. `CharacterRuntime`

The file is in a separate zero-reference assembly, `SoulsLike.Character.Runtime.asmdef`.

Production usage search shows:

- `CharacterRuntime` is registered by `CharacterFactory` and injected only into `Character`;
- `CharacterActionStateMachine` is registered by `CharacterFactory` and consumed only by `CharacterRuntime`;
- `CharacterCommandBuffer` is registered by `CharacterFactory` and consumed only by the state machine;
- `ICharacterActionExecutor` has only one production implementation: `Character`;
- `CharacterInputBatch` is produced only by the player input adapter and consumed through Character/runtime/state machine;
- enemies use their own `EnemyBrain` / `EnemyAnimationController` path, not this runtime.

`CharacterFactory` stores only one `_characterScope` field and overwrites it on creation, so its lifetime design assumes one live player character.

Source: `Assets/Scripts/Entities/Character/CharacterFactory.cs:36,83,152-155`.

The test named `FakeAiCanSubmitAttackDataWithoutInputSystem` proves protocol-level possibility, but no live enemy uses that route.

Source: `Assets/Tests/CharacterRuntime/CharacterRuntimeTests.cs:117-130`.

## 9. Character action state machine

### 9.1 State

The state enum has four values:

```text
Neutral
Attack
Roll
EquipmentSwap
```

Source: `CharacterRuntime.cs:79-85`.

Internal state held by `CharacterActionStateMachine`:

- current state;
- input-blocked boolean;
- queue-window boolean;
- ignore-next-action-exit boolean;
- qualified-sprint-during-roll boolean, despite the field name `_sprintHeldDuringRoll`;
- accept-equipment-companion boolean;
- roll-sprint-interrupt-requested boolean;
- reference to a one-slot command buffer.

Source: `CharacterActionStateMachine.cs:5-25`.

### 9.2 Command result vocabulary

Execution result status:

- `Executed`;
- `TemporarilyBlocked`;
- `Invalid`.

Submission disposition:

- `Executed`;
- `Buffered`;
- `Rejected`;
- `Ignored`.

Source: `CharacterRuntime.cs:64-77`.

### 9.3 Submission behavior

`CharacterActionStateMachine.Submit`, `CharacterActionStateMachine.cs:37-71`:

- ignores all commands when the single input-block boolean is true;
- calls `CanExecute` based on current generic state;
- asks `CharacterCommand` to execute against `ICharacterActionExecutor`;
- transitions to the returned started state when executed;
- stores temporarily blocked commands only when `CanBuffer` is true;
- equipment commands cannot buffer;
- invalid commands become rejected;
- other blocked commands become ignored.

### 9.4 State admission rules

`CanExecute`, `CharacterActionStateMachine.cs:125-140`:

| Current state | Allowed command |
|---|---|
| `Neutral` | any |
| `Attack` | non-equipment, only while queue window is open |
| `Roll` | non-equipment, only while queue window is open |
| `EquipmentSwap` | exactly one equipment companion while the special flag is open |

### 9.5 Tick order

`CharacterActionStateMachine.Tick`, `CharacterActionStateMachine.cs:73-94`:

1. handles per-state continuous tick logic;
2. submits first fresh command;
3. submits second fresh command;
4. clears an expired buffer only if current state is `Neutral`;
5. tries to execute the buffered command.

Fresh commands are therefore considered before the previous buffered command on each frame.

### 9.6 Buffer behavior

`CharacterCommandBuffer`, `CharacterRuntime.cs:325-358`:

- one slot only;
- new storage overwrites the previous command;
- fixed one-second expiration;
- no command-specific duration;
- exposes peek and clear;
- expiration is checked by the state machine only while neutral.

The existing test contract explicitly expects an expired attack-state command to remain until the queue window opens.

Source: `Assets/Tests/CharacterRuntime/CharacterRuntimeTests.cs:91-114`.

### 9.7 Queue window and animation exit

Animation callbacks drive transition timing:

- attack enter closes the queue;
- queue check opens the queue for attack or roll;
- queue check immediately retries the buffered command;
- exit normally enters neutral;
- a chained same-state action sets `_ignoreNextActionExit` so the previous animation’s later exit does not cancel the new action;
- equipment swap exit is not handled as a normal state exit; its coordinator phase controls completion.

Source: `CharacterActionStateMachine.cs:96-123,179-256`.

### 9.8 Roll-to-sprint interrupt

During roll:

- the frame's qualified sprint value is sampled, not the raw button-held value; it becomes true only after the resolver's 0.3-second threshold and while movement input is nonzero;
- if the queue window is open, the state machine requests a roll interrupt and enters neutral;
- `Character` later consumes that request and calls `AnimatorComponent.InterruptRollForSprint`.

Sources:

- `CharacterActionStateMachine.cs:155-177,179-193,258-265`
- `Character.cs:480-485`

### 9.9 Equipment companion rule

When an equipment action starts a real animated swap, entering `EquipmentSwap` opens `_acceptEquipmentCompanion`. This permits the adapter’s same-frame second equipment command. At the start of the next state-machine tick, the flag is closed.

Source: `CharacterActionStateMachine.cs:168-177,212-233`.

The rule depends on the exact ordering of `FirstCommand` and `SecondCommand` within one `CharacterInputBatch`.

## 10. Command execution is split across layers

`CharacterCommand.TryExecute`, `CharacterRuntime.cs:288-317`, maps a command kind to `ICharacterActionExecutor` calls.

The only production executor is `Character`.

### 10.1 Attack execution

`Character.TryStartAttack`, `Character.cs:268-331`, performs:

- grounded and movement-gate checks;
- special-attack restriction during roll;
- shield special -> parry selection;
- equipment loadout construction;
- left/right weapon resolution;
- item/weapon database lookup;
- stamina cost and start-threshold calculation;
- stamina validation and consumption;
- `AttackComponent` resolution;
- charged animation speed;
- facing toward input;
- attack animation trigger.

### 10.2 Roll execution

`Character.TryStartRoll`, `Character.cs:333-356`:

- derives interrupt permission from current action state;
- validates stamina;
- asks `MovementComponent` to start the roll;
- consumes stamina.

`MovementComponent.TryStartRoll`, `MovementComponent.cs:218-284`, then owns:

- landing recovery before roll;
- movement-block/cooldown/grounded checks;
- backstep selection;
- free versus lock-on direction;
- transform rotation;
- roll target and direction;
- roll timer;
- presentation callback that triggers roll/backstep animation through Character.

### 10.3 Jump execution

`Character.TryStartJump`, `Character.cs:358-376`, validates/consumes stamina and delegates physical jump start to `MovementComponent`. Jump returns `Neutral` as its action state.

### 10.4 Equipment and quick-item execution

`Character.TryStartEquipmentAction`, `Character.cs:378-417`, handles:

- animated right/left weapon swap;
- immediate quick-item slot switch;
- active quick-item use;
- hand-mode toggle with movement-gate checks.

`Character.TryUseActiveQuickItem`, `Character.cs:637-672`, combines:

- equipment loadout lookup;
- item catalog classification;
- consumable definition lookup;
- healing or currency mutation;
- active weapon runtime infusion;
- inventory consumption.

## 11. Animation feedback path

### 11.1 Animator StateMachineBehaviour

Each configured `AnimatorStateMachine` behavior can report:

- enter;
- exit;
- optional progress at a serialized normalized time, default 0.5;
- optional queue check at a serialized normalized time, default 0.55.

Source: `Assets/Scripts/Components/Animations/AnimatorStateMachine.cs:5-77`.

### 11.2 Receiver

`AnimatorStateMachineReceiver` initializes all Animator behaviors, reuses one mutable `AnimatorStateMachineDto`, and synchronously notifies registered observers.

Source: `Assets/Scripts/Components/Animations/AnimatorStateMachineReceiver.cs:6-98`.

### 11.3 AnimatorComponent forwarding

`AnimatorComponent` injects `IAnimationStateSink` and `IRootMotionSink`; both resolve to `Character`. It subscribes itself to the receiver, then forwards every state update to the sink.

Sources:

- `Assets/Scripts/Components/Animator/AnimatorComponent.cs:77-115`
- `Assets/Scripts/Components/Animator/AnimatorComponent.cs:232-235`

### 11.4 Character as animation event router

`Character.OnAnimationStateChanged`, `Character.cs:424-478`, synchronously handles:

1. `AttackComponent` animator state;
2. active equipment swap coordinator phase;
3. spawn input block/unblock;
4. grace phase transitions;
5. death completion event;
6. parry lock/unlock;
7. heavy-attack speed reset;
8. generic animation-to-action adaptation;
9. state-machine animation handling;
10. pending runtime animation requests.

### 11.5 Generic animation adapter

`CharacterAnimationAdapter`, `CharacterAnimationAdapter.cs:6-61`, maps detailed Animator names into only three generic action states:

- light/heavy/roll/backstep/run/special/parry attacks -> `Attack`;
- roll/backstep -> `Roll`;
- equipment swap out/in -> `EquipmentSwap`.

Other animation names are ignored by this adapter but may still be handled directly in `Character.OnAnimationStateChanged`.

## 12. Root motion and movement blocking

`AnimatorRootMotionRelay` reads tags from all active/current/next Animator layers:

- `RootMotion`;
- `MovementBlocked`.

It stores local `_movementBlocked` and `_usesRootMotion` booleans, sends contract changes to `IRootMotionSink` (`Character`), and sends Animator delta position/rotation every `OnAnimatorMove`.

Source: `Assets/Scripts/Components/Animator/AnimatorRootMotionRelay.cs:7-100`.

Character then:

- stores the animation block reason and root-motion flag in `CharacterRuntime`;
- copies runtime blocked state into `MovementComponent`;
- forwards root-motion deltas to `MovementComponent` only when runtime permits.

Source: `Character.cs:535-553`.

`CharacterRuntime` holds:

- `MovementGate` reason mask;
- a separate `_animationRootMotionEnabled` boolean;
- access to state-machine input-block state.

Source: `CharacterRuntime.cs:430-495`.

`MovementComponent` holds another `_movementBlocked` boolean.

Source: `MovementComponent.cs:46,105-119`.

## 13. Movement presentation mediation

`MovementComponent` injects `IMovementPresentationSink`, whose only live implementation is `Character`.

Sources:

- `MovementComponent.cs:25,81-85`
- `Character.cs:25,555-565`

The interface contains nine callbacks:

- locomotion;
- turn;
- grounded;
- land notification;
- airborne motion;
- jump;
- roll;
- backstep;
- crouch.

`MovementComponent.Move`, `MovementComponent.cs:171-202`, updates physical movement and calls the presentation sink. Character forwards most calls one line later to `AnimatorComponent`; landing goes to `CharacterAudioComponent`.

## 14. Equipment mediation

`EquipmentComponent` directly depends on `InventoryComponent` and `ItemCatalog` and injects `IEquipmentLoadoutSink`, whose only live implementation is `Character`.

Sources:

- `EquipmentComponent.cs:12-30,112-116`
- `Character.cs:29,608-635`

On every loadout change, Equipment:

1. normalizes hand mode;
2. rebuilds loadout;
3. publishes `LoadoutChanged`;
4. calls `Character.ApplyEquipmentLoadout`.

Source: `EquipmentComponent.cs:203-209`.

Character then:

1. applies visual equipment presentation;
2. resolves active left/right weapons;
3. chooses/reset/applies an animation profile;
4. transitions Animator hand mode;
5. updates `AttackComponent` weapon runtimes.

Source: `Character.cs:608-635`.

This relationship is bidirectional:

```text
Character -> EquipmentComponent
EquipmentComponent -> IEquipmentLoadoutSink -> Character
```

## 15. Equipment swap state machine

`EquipmentSwapCoordinator` has a second, specialized phase machine:

```text
None
SwapOut
SwapOutHidden
SwapIn
```

Source: `Assets/Scripts/Entities/Character/Adapters/EquipmentSwapCoordinator.cs:8-20`.

It receives `EquipmentComponent`, `AnimatorComponent`, and `EquipmentPresentation` at every operation boundary. It reacts to animation progress and exit, hides/reveals armaments, switches the slot, and triggers swap-in/out animations.

Source: `EquipmentSwapCoordinator.cs:22-137`.

The generic `CharacterActionStateMachine` also has `EquipmentSwap`, so equipment swap state exists at both generic-action and specialized-phase levels.

## 16. Attack state

`AttackComponent`, `AttackComponent.cs:43-228`, owns additional detailed combat state:

- active Animator state name;
- contextual state;
- one-second contextual attack timer;
- strong-input active flag;
- right/left weapon IDs and runtimes;
- active hand mode through current execution context.

It resolves:

- heavy attacks;
- left-hand attacks;
- alternating light attacks;
- rolling/backstep/run contextual attacks;
- charged attack speed.

It also receives every Animator state from `Character.OnAnimationStateChanged`.

## 17. Health, stamina, and character data

`HealthComponent`, `HealthComponent.cs:7-284`, owns:

- current `HealthStats`;
- damage/heal/revive calculations;
- focus/stamina consumption and restoration;
- stamina debt, delay, and recovery;
- invulnerability.

`Character` nevertheless owns policy around:

- attack stamina cost selection;
- roll/jump stamina costs from `MovementModel`;
- combat sprint stamina drain;
- stamina recovery tick timing and block multiplier;
- grace invulnerability;
- damage -> audio/hit animation response.

`CharacterHealthData` wraps `HealthData` but injects the whole `Character` only to read `Character.Attributes.Vigor` for max health.

Source: `Assets/Scripts/Entities/Character/CharacterHealthData.cs:6-33`.

This makes health construction depend back on the gameplay facade.

## 18. Interaction, lock-on, death, and grace are parallel orchestration paths

### Interaction

`InteractionController` scans nearby colliders, selects the best interactable, reads the Interact input directly, and executes `InteractionCommand` asynchronously. It uses `Character.transform` as the actor position.

Source: `Assets/Scripts/Interactions/InteractionController.cs:14-248`.

It does not submit a character action and does not participate in `CharacterActionStateMachine`.

### Lock-on

`PlayerController` reads LockOn directly, calls `TargetingService`, then calls both `Character.SetLockOnTarget` and `CameraService`.

Source: `PlayerController.cs:122-162`.

Lock-on does not use the character command/state-machine path.

### Death

Death ownership is split:

- `HealthModel.OnDied` is observed by `PlayerController`;
- `PlayerController` calls `Character.PlayDeath`;
- `Character` blocks input and triggers death animation;
- animation exit causes `Character.OnDeathAnimationCompleted`;
- `PlayerController` calls `CoreGameOrchestrator.RespawnAtLastGrace`;
- the orchestrator later calls `Character.CompleteDeathAnimation` and `SetPosition`.

Sources:

- `PlayerController.cs:51-65,164-172`
- `Character.cs:183-195,443-450`
- `CoreGameOrchestrator.cs:146-177`

### Grace

`CoreGameOrchestrator` drives Character’s async grace-unblock, rest-start, idle, cancel, and rest-end methods. Character maintains its own `GracePhase`, task completion source, invulnerability, input blocking, and animation callback matching.

Sources:

- `CoreGameOrchestrator.cs:63-73,97-143`
- `Character.cs:33-41,197-266,488-533`

## 19. Existing runtime behavior tests

The character runtime test file names the intended behavior contracts:

- latest command overwrites buffer and expires after one second;
- attack state retains an expired command until queue window;
- a fake AI can submit without Input System;
- input block rejects without buffering;
- neutral prunes an expired blocked command;
- attack queue allows guard through animation block only;
- contradictory animation signal does not alter current state;
- equipment cannot execute or buffer in attack queue;
- equipment swap accepts exactly one same-frame companion;
- chained attack ignores the previous animation’s exit callback;
- roll during roll executes at queue window;
- jump leaves generic action state neutral;
- movement gate keeps independent block reasons;
- sprint tap rolls, qualified hold does not;
- the test named “sprint held during roll” verifies interruption only when the queue window opens; the runtime frame value is actually qualified by the 0.3-second hold threshold and nonzero movement;
- heavy press suppresses light until light release.

Source: `Assets/Tests/CharacterRuntime/CharacterRuntimeTests.cs:66-384`.

These tests were inspected but not run.

---

# Part II — Analysis

## 20. Core diagnosis

**Inference:** the central problem is not simply “too many components” or “Character is large.” The problem is that responsibilities belonging to one cohesive character action are separated into many low-coupled protocol layers, then reassembled synchronously inside one high-degree `Character` mediator.

The current architecture gets the costs of both styles:

- many interfaces, DTOs, commands, requests, statuses, adapters, and registered objects;
- a central concrete class that still knows almost every component and cross-domain rule.

The abstraction flow for a roll illustrates the problem:

```text
raw input
-> gesture resolver
-> input adapter
-> nullable command slot
-> command union struct
-> runtime wrapper
-> generic state machine
-> executor interface
-> Character action method
-> MovementComponent
-> Character presentation sink
-> AnimatorComponent
-> Animator StateMachineBehaviour
-> receiver
-> AnimatorComponent observer
-> Character animation sink
-> animation adapter
-> runtime wrapper
-> generic state machine
```

This is low compile-time coupling between steps, but low change locality and low readability for the character action as a whole.

## 21. Character is simultaneously facade, mediator, executor, presenter, and lifecycle controller

**Fact:** Character implements the external facade plus four callback ports and the action executor.

**Inference:** these roles pull the class in different directions:

- a facade should present a small, stable character API;
- a mediator needs knowledge of component interactions;
- an action executor owns gameplay use-case policy;
- a presentation sink translates domain state to visuals/audio;
- a lifecycle controller owns spawn/death/grace transitions.

The class therefore changes for unrelated reasons. A new stamina rule, equipment animation, grace state, lock-on behavior, consumable effect, or movement presentation signal can all require editing `Character.cs`.

This is the main cohesion failure.

## 22. Interfaces hide rather than remove the central coupling

The four callback ports are narrow, but every live path resolves back to the same concrete Character:

```text
MovementComponent -> IMovementPresentationSink -> Character
AnimatorComponent -> IAnimationStateSink        -> Character
AnimatorComponent -> IRootMotionSink            -> Character
EquipmentComponent -> IEquipmentLoadoutSink     -> Character
```

**Inference:** this is dependency inversion mechanically, but not architectural independence. The components remain temporally and semantically coupled through Character. Reading a component alone does not explain behavior; the reader must follow the port to Character and then to another component.

The one-line forwarding methods (`SetTurn`, `SetGrounded`, `PlayJump`, and similar) add navigation without adding policy. The policy-heavy callbacks (`OnAnimationStateChanged`, `ApplyEquipmentLoadout`) make Character a hub.

## 23. One gameplay action has fragmented ownership

Attack ownership is split across:

- adapter input priority and gesture suppression;
- generic state-machine admission and buffering;
- `CharacterCommand` dispatch;
- `Character.TryStartAttack` validation and stamina;
- `AttackComponent` attack selection/context;
- `MovementComponent` facing;
- `AnimatorComponent` playback;
- Animator state behavior timing;
- melee behavior/relay/hitbox activation;
- animation callbacks returning through Character.

Roll and equipment swaps have similar cross-file chains.

**Inference:** this is the concrete manifestation of low cohesion. A developer cannot understand or modify one action by reading one cohesive place. The architecture is optimized for substitutable steps that are not actually substituted in production.

## 24. Generic runtime abstraction has only one live consumer

**Fact:** the runtime assembly is deliberately Unity-independent, and a test demonstrates fake-AI submission. However, live enemy code uses separate enemy state/animation systems. Only player Character uses `CharacterRuntime`, `CharacterActionStateMachine`, `CharacterCommand`, and `ICharacterActionExecutor` in production.

**Inference:** the architecture pays for a hypothetical shared character runtime without receiving shared behavior. The executor interface and protocol types split player behavior from its only implementation.

This does not mean pure runtime tests are bad. It means purity has become a design driver stronger than production cohesion.

## 25. Too much protocol for four generic states

The runtime models four generic states using:

- two status enums;
- command kind enum;
- action-state enum;
- animation-signal enum;
- movement-gate enum;
- equipment-action enum;
- five request/result structs;
- command union struct;
- executor interface;
- clock interface;
- command buffer class;
- movement policy struct;
- movement gate class;
- animation signal struct;
- state machine;
- runtime wrapper;
- animation adapter.

**Inference:** many of these types are individually reasonable, but together they exceed the conceptual complexity of the behavior they model. `CharacterRuntime` is especially thin: almost every method forwards to the state machine or movement gate.

The 23 public top-level types in one file also directly violate this repository’s current “one top-level type per C# file” organization rule and make navigation harder.

## 26. State is duplicated across multiple machines

The same action exists simultaneously in several representations:

| Concern | State representation |
|---|---|
| generic action admission | `CharacterActionStateId` |
| detailed active animation | `StateMachineName` / Animator state |
| attack context | `AttackComponent._activeState` and `_contextualState` |
| equipment sequencing | `EquipmentSwapCoordinator.SwapPhase` |
| movement action | roll/jump timers, grounded/landing state, movement mode |
| lifecycle | `Character.GracePhase`, death boolean |
| movement control | `MovementGateReason`, runtime root-motion bool, relay booleans, MovementComponent blocked bool |

Some duplication is necessary because animation, physics, and domain policy are different concerns. The issue is that ownership and synchronization are implicit and distributed.

`Character.OnAnimationStateChanged` is the synchronization junction. A mismatched generic animation signal is only logged and ignored, which preserves state but leaves the reader to reason about which representation is authoritative.

Source: `Character.cs:466-476`.

## 27. Animation is a cross-domain event bus

**Fact:** one animation callback handles attack, equipment, spawn, grace, death, parry, heavy charge, generic action queueing, and roll interruption.

**Inference:** animation timing is not merely presentation here; it is the hidden scheduler for multiple gameplay domains. This makes controller assets, StateMachineBehaviour configuration, Character routing, and state-machine logic one distributed transaction.

The synchronous mutable DTO observer path further obscures ownership:

```text
AnimatorStateMachine
-> AnimatorStateMachineReceiver mutable DTO
-> AnimatorComponent observer
-> Character
-> several domain handlers
```

Adding a global event bus would worsen this. The simplification should make animation callbacks go to the cohesive owner of the action/lifecycle, not broadcast them more broadly.

## 28. Hidden temporal behavior in input and buffering

The current behavior depends on source-code order rather than explicit data:

- input priority is the `else if` order in the adapter;
- only two command slots exist;
- only one exact equipment/two-hand pair survives;
- buffer is one-slot/latest-wins;
- fresh commands are processed before buffered retry;
- expired commands are pruned only in neutral;
- an expired attack command may intentionally execute at a later queue window;
- same-state chaining uses a single “ignore next exit” boolean;
- equipment companion acceptance exists only for part of one tick.

**Inference:** these are valid game-design rules only if intentionally named and visible. In the current form they are hard to discover and fragile under reordering.

## 29. Concrete defect: RollRequest interrupt policy is dead

**Defect:** `RollRequest` carries `CanInterrupt`.

Sources:

- declaration: `CharacterRuntime.cs:149-160`;
- construction: `CharacterRuntime.cs:258-270`;
- adapter always passes `true`: `PlayerCharacterInputAdapter.cs:132-136`.

`Character.TryStartRoll` never reads `request.CanInterrupt`. It recomputes:

```csharp
bool canInterrupt = _runtime.ActionState != CharacterActionStateId.Neutral;
```

Source: `Character.cs:333-350`.

The request contract and actual policy disagree. Either the field is obsolete or current callers cannot control the behavior it claims to represent.

## 30. Correctness risk: input-block ownership is not reason-safe

`MovementGate` correctly stores independent bit reasons. The state machine stores only one `_inputBlocked` boolean.

`CharacterRuntime.SetInputBlocked(bool)` does both:

```text
stateMachine.SetInputBlocked(blocked)
MovementGate.Set(Spawn, blocked)
```

`SetParryLocked(bool)` also writes the same state-machine boolean while independently toggling the `Parry` gate.

Source: `CharacterRuntime.cs:468-478`.

Callers use `SetInputBlocked` for spawn, death, and grace, despite it always using the `Spawn` movement-gate reason.

Sources:

- spawn: `Character.cs:105-113,436-441`;
- death: `Character.cs:183-195`;
- grace: `Character.cs:529-533`.

**Risk:** if two blocking lifecycles overlap, the first one to clear the boolean can make `Character.IsInputBlocked` false while another movement-gate reason remains active. PlayerController gates its whole tick from the boolean, not from the reason mask.

This is a state-ownership problem caused by having a reason-aware gate and a separate reason-unaware boolean.

## 31. Concrete dead or incomplete contracts

Source-use search found:

- `MovementPolicy.RotationBlocked` is always assigned but never consumed by production code;
- `MovementPolicy.UseRootMotion` is returned but never consumed; Character separately reads `CharacterRuntime.CanApplyRootMotion`;
- `MovementGateReason.Stagger` is declared but never set;
- `CharacterAnimationSignalKind.Progressed` is produced for Animator `Progress` and `Loop`, but `CharacterActionStateMachine.HandleAnimation` has no `Progressed` case;
- `AnimatorStateMachine._currentLoopIndex` is reset but never advanced and `AnimatorStateMachineReceiver.OnLoop` has no live caller in this behavior;
- `HeavyAttackGestureResolver.StrongAttackHeld` is never read;
- `CharacterCommandBuffer.HasCommand` and `MovementGate.Reasons` are only used by tests;
- `Character.aimTargetDistance` and `aimLayerMask` are serialized but unused;
- `Character.Submit(CharacterCommand)` has no production caller; the runtime is reached through `Character.Tick`.

These are small individually, but collectively show that the protocol has accumulated abandoned or parallel concepts.

Two enum types cross serialized asset boundaries and must not be casually reordered during cleanup:

- `StateMachineName`, serialized by Animator state behaviors;
- `CharacterActionId`, serialized by combat/enemy action assets.

Sources:

- `Assets/Scripts/Components/Animations/StateMachineName.cs:3-27`
- `Assets/Scripts/Entities/Character/Runtime/CharacterActionId.cs:3-12`

## 32. Facade boundary leaks implementation details

Character exposes concrete mutable internals:

- `InventoryComponent`;
- mutable `HealthStats`;
- `Transform` through MonoBehaviour;
- attribute data used directly by UI and health construction.

UI controllers often inject both `Character` and concrete inventory/equipment components. The facade is therefore neither the single boundary nor a narrow query surface.

**Inference:** external code can choose between Character and its internal components, so ownership is ambiguous and invariants are difficult to centralize.

## 33. Reverse dependency in health construction

`CharacterHealthData` depends on the whole Character only to calculate max health from Vigor.

```text
Character -> CharacterData.Attributes
HealthModel -> IHealthData -> CharacterHealthData -> Character
```

This is a composition-level cycle disguised by interfaces and factory ordering. The required information is `CharacterAttributeStats`, not the entire facade.

## 34. Composition boundary is too broad

The dynamic character child scope owns the gameplay entity **and** player input/controller **and** interaction **and** seven UI controllers.

**Inference:** “character lifetime,” “local player lifetime,” and “player UI lifetime” are treated as one boundary. This makes a future non-player character, player replacement, respawn strategy, UI persistence change, or local multiplayer split harder than necessary.

The answer is not to hide the 52 registrations behind more extension methods. The ownership boundaries themselves need to be explicit.

### 34.1 Repeated creation loses ownership of the previous scope

`CharacterFactory.CreateCharacter` assigns every newly created child scope to the same `_characterScope` field. It does not reject a second creation and does not dispose the previous scope before overwriting the field. `Dispose` can therefore dispose only the most recently assigned scope.

**Risk:** if creation can occur more than once during one factory lifetime, the previous entity, controllers, and registrations remain alive without an owner. Either enforce and document single creation, or give replacement an explicit dispose-before-create lifecycle.

Source: `Assets/Scripts/Entities/Character/CharacterFactory.cs:36,42-152`.

### 34.2 Addressables load ownership is not represented

`CharacterFactory` calls `IAssetService.LoadPrefab` for every creation. `AddressableAssetService.LoadPrefab` calls `Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion()` and returns only the loaded prefab object, discarding the operation handle. No matching `Addressables.Release` or `Addressables.ReleaseInstance` exists in the character/asset-service path; the only project search result is an unrelated scene-loading operation release.

**Risk:** repeated character creation can retain Addressables reference counts because no owner has enough information to release the load. The factory or asset service needs explicit handle ownership, or the creation path should use an Addressables instantiate/release pair. The exact repair depends on whether the prefab is intentionally cached for the whole project lifetime.

Sources:

- `Assets/Scripts/Entities/Character/CharacterFactory.cs:42-52`
- `Assets/Scripts/Services/AssetService/AddressableAssetService.cs:14-29`
- `Assets/Scripts/Services/Scenes/SceneService.cs:114` — unrelated release found by project search

### 34.3 Respawn can outlive the character binding that started it

`PlayerController.HandleDeathAnimationCompleted` starts `RespawnAtLastGrace().Forget()`. The orchestrator then awaits fade callbacks and a frame before repeatedly using its `_character` reference. The call has no lifecycle cancellation token, and fire-and-forget errors are not observed at this call site.

**Risk:** a scene/scope change or character replacement during either fade wait can let the continuation act on a destroyed or obsolete character. This should first be characterized against scene unload and replacement behavior. If the lifetime can end mid-respawn, the operation needs a lifecycle-owned cancellation token and an observed failure path.

Sources:

- `Assets/Scripts/Entities/Character/PlayerController.cs:169-172`
- `Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs:146-177`

## 35. Movement and Animator are also internal god components

Character is the largest relationship hub, but simplification cannot stop there.

`MovementComponent` is 898 lines and combines:

- motor physics;
- ground sensing;
- locomotion state;
- roll/jump action rules;
- animation blend production;
- lock-on facing;
- crouch capsule changes;
- debug drawing.

`AnimatorComponent` is 537 lines and combines:

- low-level Animator parameter writes;
- action selection triggers;
- weapon animation profile/controller changes;
- layer-weight policy;
- state observation;
- root-motion relay setup;
- smoothing state.

**Inference:** moving more orchestration from Character into these existing classes without a clear cohesive owner would only relocate the problem.

## 36. What is essential behavior versus accidental machinery

### Essential Souls-like behavior that must be preserved

- continuous movement, camera-relative direction, lock-on movement;
- sprint/roll gesture semantics;
- stamina costs and start thresholds;
- action buffering and animation-timed queue windows;
- same-action chaining without stale exit cancellation;
- roll interruption at the configured queue point;
- attack contextual selection and hitbox windows;
- equipment swap visibility and animation phases;
- root motion and movement blocking from animation tags;
- guard/parry rules;
- spawn/death/grace protection and animation synchronization;
- lock-on, interaction, quick items, and equipment UI behavior.

### Accidental machinery that can change while behavior stays identical

- `CharacterRuntime` as a forwarding wrapper;
- separate one-consumer `CharacterCommandBuffer` class;
- public 23-type protocol file;
- executor indirection with one live implementation;
- separate animation adapter for a single switch mapping;
- separately registered one-consumer gesture resolver objects;
- single-purpose sink forwarding through Character when no policy is applied;
- duplicate movement/root-motion/input-block state;
- two nullable command slots as an implicit priority model;
- UI registrations inside the entity scope;
- health data depending on the full Character facade.

## 37. Recommended simplification direction

This section is a target direction, not a request to implement all of it at once.

### 37.1 Preserve Character as the entity facade

Do not remove the useful idea that external systems talk to a Character entity. Make it true:

- Character should expose a small stable facade for lifecycle, position/targeting, character state snapshots, and player actions;
- external UI should receive read-only character/equipment/inventory state or dedicated presenters, not raw mutable components;
- the base `Entity` identity and Character gameplay aggregate should have one explicit relationship.

High internal cohesion inside the Character ecosystem is preferable to dozens of tiny cross-entity abstractions.

### 37.2 Make one cohesive subsystem for action execution and timing

The first safe simplification should remove forwarding and split ownership while preserving the one seam that already has meaningful pure-runtime tests.

Recommended initial shape:

```text
CharacterActionStateMachine
  owns sequencing, queue windows, and buffer policy
        |
        v
ICharacterActionExecutor
        |
        v
CharacterActionExecutor
  owns complete attack/roll/jump/equipment execution
  and direct character-local component dependencies
```

Then:

- delete the thin `CharacterRuntime` forwarding wrapper;
- make `CharacterAnimationAdapter` static/internal or fold its switch into the action callback path;
- internalize `CharacterCommandBuffer` inside the state machine unless its separate test surface is still useful;
- move `Character.TryStartAttack/Roll/Jump/Equipment` together into the concrete action executor;
- keep the pure `ICharacterActionExecutor` boundary initially because the separate runtime assembly and existing EditMode tests provide real value;
- reconsider removing that interface only if the pure-runtime boundary still adds more navigation than value after extraction.

The resulting character-action subsystem should own together:

- current action state;
- queue-window state;
- the one buffered action and its expiration semantics;
- action admission;
- action execution result;
- animation queue/exit callbacks;
- stale-exit handling;
- equipment companion rule if that rule remains required.

The concrete executor may directly coordinate character-local `Movement`, `Attack`, `Equipment`, `Health`, and animation dependencies. That is intentional high cohesion around the “perform a character action” use case, not an application-wide service.

It must not become a replacement god object. Ownership remains specific:

- the state machine owns admission, buffering, queue timing, and stale-exit suppression;
- the concrete executor owns action-level orchestration and resource checks;
- the motor owns physics and root-motion application;
- equipment owns swap phase and presentation visibility;
- combat owns attack resolution and hit windows.

The important constraint is that the rule and its side effects become readable in one place.

### 37.3 Keep player input outside the entity but make the intent explicit

Player-specific Input System reads should remain outside the character so AI/replay can drive the same entity. However:

- keep one `PlayerCharacterInput` object;
- internalize the two tiny gesture state machines unless they gain another consumer;
- return an explicit intent/event collection rather than `FirstCommand?` and `SecondCommand?`;
- name priority and same-frame coexistence rules rather than encoding them only as `else if` order.

This preserves input independence without carrying a large generic command protocol through the entire entity.

### 37.4 Use one reason-aware capability gate, while keeping root motion orthogonal

Replace the state-machine input boolean plus duplicated movement/rotation/guard blocking with one reason-aware character capability gate.

It should derive, from the same source of truth:

- accepts actions;
- accepts movement;
- accepts rotation;
- guard allowed.

Spawn, death, grace, parry, stagger, manual, and animation must have distinct reasons. Clearing one reason must not clear another.

Do **not** make root-motion mode another value derived from that gate. `AnimatorRootMotionRelay` deliberately reports two independent facts: whether animation blocks ordinary movement and whether the active animation supplies root motion. Keep a separate typed animation-motion contract such as `{ MovementBlocked, UsesRootMotion }`, with explicit precedence at the motor boundary. This prevents control admission, animation state, and motor mechanics from collapsing into a new policy god object.

Source: `Assets/Scripts/Components/Animator/AnimatorRootMotionRelay.cs:56-59,80-90`.

### 37.5 Route animation callbacks to the cohesive owner

Avoid a generic event bus. Route by ownership:

- action queue/exit -> character-action subsystem, specifically its state machine;
- equipment visibility/swap phase -> equipment action owner;
- death/grace/spawn completion -> character lifecycle owner;
- root motion -> motor/control owner;
- hitbox windows -> combat owner.

Character may remain the explicit wiring point, but it should not contain all handler logic in one method.

### 37.6 Separate character lifecycle from per-frame action logic

Spawn/death/grace form one cohesive lifecycle concern. Their async completion, protection, and animation phase state can move together. This removes a second state machine from the main Character action/tick file without scattering it into many services.

### 37.7 Clarify presentation ownership

Movement-to-animation forwarding can be simplified in either of two coherent ways:

1. a character-local presentation component receives movement snapshots and owns animator/audio updates; or
2. Movement directly owns its pure locomotion presentation output while Character only mediates rules that cross domains.

Choose one. The current mixture—nine callbacks through Character, some one-line and some policy-heavy—is the hard-to-read option.

### 37.8 Split lifetime scopes by real ownership without relocating global services

Suggested ownership boundary:

```text
ProjectScope (existing application lifetime)
├── InputService
└── global asset/settings services

CoreScope (existing scene/core lifetime)
├── CameraService / targeting coordination
├── CoreGameOrchestrator
└── scene-level UI coordination

player binding lifetime
├── PlayerController
├── interaction controller/input binding
└── character-bound HUD/inventory/equipment bindings

character entity scope
├── Character facade + identity relation
├── CharacterActions
├── Movement/motor
├── combat/health
├── equipment/inventory
└── presentation/lifecycle
```

`InputService` is already a `ProjectScope` singleton, and `CameraService` is already a `CoreScope` component. They should not be moved into a short-lived player or character scope. The change is to separate only the registrations whose dependencies and lifetime are actually bound to the current playable character.

Sources:

- `Assets/Scripts/Services/VContainer/ProjectScope.cs:24-69`
- `Assets/Scripts/Services/VContainer/CoreScope.cs:19-39`

This is a boundary correction, not a request for more interfaces.

## 38. Minimal migration order

The lowest-risk order is:

1. **Freeze behavior and lifetime contracts.** Preserve the existing runtime tests and add characterization only when implementation work is explicitly requested. Include input priority, paused movement-only behavior, interaction bypass, overlapping control locks, repeated-create semantics, Addressables ownership, and respawn cancellation across scene/scope changes.
2. **Unify control blocking first.** It is a correctness boundary and removes duplicated state before larger moves.
3. **Remove dead protocol.** Resolve `RollRequest.CanInterrupt`, unused movement policy fields, unused aim settings, unused resolver property, and unused gate reason intentionally.
4. **Extract the cohesive action executor.** Move attack/roll/jump/equipment action orchestration from Character to one concrete action executor one action at a time; preserve the pure state-machine interface and tests.
5. **Collapse forwarding layers without changing behavior.** Remove `CharacterRuntime`; internalize or make static the buffer/animation adapter only where that reduces navigation without losing a useful test seam.
6. **Separate lifecycle routing.** Move spawn/death/grace state and animation callbacks together.
7. **Clarify presentation path.** Replace one-line Character sink forwarding with one consistent owner.
8. **Narrow the facade.** Update external callers and UI after internal ownership is stable.
9. **Split composition scope.** Move player/UI/session objects out of the character entity lifetime last, once dependencies are explicit.

Each step should preserve serialized prefab references and animator behavior. Do not simultaneously rewrite `MovementComponent`, `AnimatorComponent`, and Character action flow.

## 39. What not to do

- Do not split every Character method into a new service.
- Do not add an event bus to replace explicit synchronous calls.
- Do not add one interface per concrete component merely to reduce compile-time references.
- Do not keep a generic AI/player action protocol unless a live second consumer actually uses it.
- Do not make the Character facade a passive service locator for raw components.
- Do not hide the 52 registrations behind registration helpers while leaving lifetime ownership unchanged.
- Do not treat Animator state as the only gameplay source of truth; animation reports timing, while action ownership should remain explicit.
- Do not rewrite serialized assets as part of the first simplification step.
- Do not reorder or renumber serialized `StateMachineName` or `CharacterActionId` values during a mechanical file split.

## 40. Questions GPT-5.6 Pro should answer before proposing code

1. Is the target Character shared by player and enemies, or is the current player-only runtime acceptable?
2. Must action buffering retain the current “expired during attack may execute at queue” behavior?
3. Must the exact two-command equipment + hand-mode pairing survive?
4. Is character movement during `GameState.Paused` intentional?
5. Should interaction be an interruptible character action, or intentionally remain parallel?
6. Should jump stay outside the generic action state?
7. Is `Character` intended to be the authoritative gameplay entity, and if so, should it explicitly relate to/implement the base `IEntity` identity?
8. Should UI live beyond character respawns, or should it really be destroyed with the character child scope?
9. Which animation timings are contractual controller data and which should move into action definitions?
10. Are multiple local players, character replacement, possession, replay, or AI-driving-the-player real requirements or only hypothetical flexibility?
11. Is `CharacterFactory` contractually single-create, or must replacement dispose the previous scope and Addressables ownership?
12. Can scene/core lifetime end while respawn awaits a fade, and what owns cancellation in that case?

## 41. Final assessment

The character ecosystem is overcomplicated because it emphasizes low coupling between internal steps more than cohesion of complete character behaviors. The result is a large facade that still knows everything, surrounded by many protocol objects that each know too little.

The simplification goal should be:

```text
fewer runtime layers
+ one owner per complete behavior
+ one reason-aware capability gate
+ a separate explicit animation-motion contract
+ explicit animation timing routes
+ a genuinely narrow Character facade
+ lifetime scopes matching entity vs player-session ownership
```

The likely highest-value first architectural move is to make action admission, buffering, concrete execution, and animation queue/exit handling one cohesive character-local subsystem, while keeping its pure sequencing seam and raw player input outside it. The likely highest-value correctness move is to unify action/movement/rotation/guard blocking under one reason-aware capability gate while leaving the animation-motion contract explicit and orthogonal.
