---
title: Character Factory DI Refactor
type: plan
domains:
  - character
  - dependency-injection
status: in-progress
authority: advisory
updated: 2026-09-17
source_commit: 4ddff2fd
aliases:
  - CharacterFactory DI Creation Plan
tags:
  - work/plan
  - status/in-progress
---
# Character Factory DI Refactor

## Plan Contract

### Goal

Refactor CharacterFactory to create a dedicated scope first, load the clean Character prefab separately, and build one player container through explicit composition. Preserve the Character facade, initial pose, animator setup, UI/input ownership and current respawn behavior.

This revision follows the shared decision in [[Work/Plans/Enemy Factory DI Refactor]]. **Do not add CharacterLifetimeScope or any other LifetimeScope to the player prefab.** Both factories reuse one infrastructure-only EntityLifetimeScope prefab asset; each runtime entity has its own instance.

### Source Research and Decisions

Observed on 2026-09-17 at commit `4ddff2fd`, including existing working-tree changes to CharacterFactory, EnemyFactory, BaseFactory and UiFactory. Preserve that baseline. This task changes documentation only; no runtime implementation or Unity execution has occurred.

The enemy plan is the canonical source for the shared host API, alternative comparison, teardown boundaries and VContainer 1.19.0 source evidence. This plan owns the Character-specific migration details.

#### Verified current behavior

| Source | Behavior to preserve |
|---|---|
| `Assets/Scripts/Entities/Character/CharacterFactory.cs:51` | CreateCharacter(Vector3? spawnPosition = null) loads the existing Character address. It currently instantiates the actor before creating a scope. |
| `CharacterFactory.cs:63` / `:94` | Applies optional position and calls AnimatorComponent.ConfigureCharacter before DI entry-point initialization. |
| `CharacterFactory.cs:71` | Adds ViewEntity if missing; the shipped Character prefab already contains it. Remove this repair only after auditing all supported prefabs. |
| `CharacterFactory.cs:97` / `:164` / `:169` | Creates one child scope, reparents the actor and later explicitly disposes the stored scope. Move ownership earlier without losing disposal. |
| `Assets/Scripts/Components/Animator/AnimatorComponent.cs:123` | ConfigureCharacter stores actor references, captures defaults, wires root motion, adds its observer and applies animation setup. Preserve once-per-creation setup. |
| `Assets/Scripts/Entities/Character/Character.cs:122` | Initialize subscribes to state, initializes movement, builds the loadout, applies presentation and blocks input. Required setup must already exist. |
| `EquipmentPresentation.cs:33` | Awake prepares the default fist runtime. An inactive-before-Build design cannot assume Character.Initialize is safe before Awake. |
| `AnimatorStateMachineReceiver.cs:14`; `AnimatorRootMotionRelay.cs:21` | OnEnable initializes state behaviours; Awake caches Animator for runtime root-motion callbacks. Account for both before player runtime initialization. |
| `MovementComponent.cs:80` | Initialize creates timers and probes ground physics; preserve pose and active-object ordering. |
| `Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs:58` / `:166` | Startup creates the player; normal respawn teleports the existing Character. It does not create another scope. |
| `Assets/Scripts/Services/Spawn/CharacterSpawnData.cs` | Existing mutable save/grace state; do not repurpose or duplicate this type name. |
| `Assets/AddressableAssetsData/AssetGroups/Default Local Group.asset:19` | Existing address is Character. Use the stable literal rather than coupling the asset key to a C# type rename. |
| `Assets/Prefabs/Models/Character/Character.prefab` | Character has the current gameplay/component graph and no LifetimeScope. Keep the prefab independently reusable. |

Current lifetime limit: duplicate CreateCharacter calls can overwrite the stored scope. Normal callers do not require multiple players. Make the current one-live-player contract explicit rather than implementing a coop ownership system prematurely.

#### Selected design

Earlier A/B options with a scope on the player root are superseded. Reuse `Assets/Prefabs/View/VContainer/EntityLifetimeScope.prefab`, authored inactive with autoRun=false, empty autoInjectGameObjects and no actor children or references.

Factory dependencies are explicit: parent LifetimeScope, shared EntityLifetimeScope prefab reference, IUniqueIdGenerator and IAssetService. Remove BaseFactory inheritance while retaining IDisposable and the owned player scope. CoreScope explicitly supplies the prefab asset to both factories; it must not replace the live parent LifetimeScope binding.

Use a concrete `CharacterScopeInstaller : IInstaller` for the existing registration list, and the shared host's small `InstallAndBuild(IInstaller)` seam. The concrete installer keeps player composition readable without a generic registration framework or actor-type branches in the host. An ActionInstaller in the factory is viable, but would retain the registration wall that the existing Character refactor intends to remove.

Creation sequence:

1. Reject a second live CreateCharacter call before allocating another scope or loading a prefab. Preserve the existing Character held by orchestration.
2. Create the inactive, unbuilt shared scope instance and retain it locally for rollback.
3. Load the Character prefab through the existing asset service and stable `CHARACTER_PREFAB_KEY = "Character"` address. Instantiate it as a child of that inactive scope.
4. Apply the initial pose from `readonly struct CharacterCreationData`, containing only `Vector3? SpawnPosition`. Preserve omitted-position, prefab rotation and scale semantics; do not assume every parent transform is identity.
5. Activate the hierarchy while autoRun=false. Allow required Awake/OnEnable work to complete, then call the existing AnimatorComponent.ConfigureCharacter with known Character and MovementComponent references.
6. Construct CharacterScopeInstaller with those direct references, identity and creation inputs. Call InstallAndBuild once in the same synchronous creation operation, with no frame yield between activation and completed build.
7. Store the successfully built scope as the factory's owned player scope and return the direct Character. On a propagated loading/composition/initialization failure, explicitly dispose the partial scope, destroy the owned actor hierarchy and preserve the failure.

This selects **activation before DI initialization** because live source shows Awake-dependent setup. Audit supported Awake/OnEnable callbacks for injected-dependency usage; do not claim injection happens before them. Do not move setup until after Build, since Character.Initialize and UI/input entry points may already have run.

CharacterCreationData is a composition input, separate from persistent CharacterSpawnData. Do not register an unused gameplay copy merely to resemble enemy spawn injection. If a real consumer is introduced, bind the same value once. Normal respawn continues using Character.SetPosition/MovementComponent.SetPosition.

#### Composition inventory

Use known component references or lookups bounded to the cloned Character root; never discover another entity through an unbounded hierarchy search. Register components through RegisterComponent or explicitly bounded component bindings so injection is preserved. Avoid duplicate auto-injection.

Retain aliases and lifetimes unless a specific behavioral requirement justifies changing them:

| Section | Registrations / setup |
|---|---|
| Identity and components | EntityRegistrationExt; Character; ViewEntity; TargetLockComponent; AnimatorComponent; CharacterAudioComponent; AttackComponent; PlayerMeleeCombatRelay; CriticalAttackController; MovementComponent; EquipmentComponent; EquipmentPresentation; InventoryComponent; HealthComponent; CombatDefenseComponent; LadderClimber. |
| Configuration / databases | CharacterData, CharacterAudioData, MovementData as IMovementData, InventoryData, ItemDatabase, WeaponDatabase, ShieldDatabase, ConsumableDatabase, HealthData. |
| Runtime models | AnimatorModel, MovementModel, EquipmentModel, InventoryModel, ItemCatalog, CharacterHealthData as IHealthData, HealthModel. |
| Entity commands | InteractionCommand, GroundItemCollectionCommand, ApplyDamageCommand, ResolveMeleeHitCommand, TargetingCommand, PlatformRideCommand. |
| Presentation / UI | PlayerHudUiController, LockOnUiController, InventoryUiController, EquipmentUiController, StatusUiController, SystemUiController, PauseNavigationUiController, LevelUpUiController, GraceUiController, InteractionUiController. |
| Player orchestration | PlayerInputReader, InteractionController, PlayerController. |

Keep section comments in the concrete installer and preserve child audio/target lookup. Existing UI controllers remain in the current local-player scope for this refactor. Do not move them to a new session scope or duplicate them for hypothetical remote players.

#### Ownership and future coop

CharacterFactory owns its one player scope. Preserve scope.Dispose for immediate container cleanup followed by scheduled object destruction; destroying only the actor or only the container is insufficient. Cover never-created, failed-create, successful-create and repeated-dispose cases with explicit ownership. Do not add a player Despawn API or a scope reference to Character.

The shared host installs a per-entity entry-point exception handler before registrations so initialization failure can propagate to factory rollback. Test this: VContainer's default handler logs initialization errors, so Build returning alone is not sufficient evidence of success. Unity Awake/OnEnable exceptions also require Console inspection.

Clean prefabs and explicit installer inputs prepare a composition boundary for later network-created player instances. This plan does not implement remote/local player roles, network authority, net IDs, a spawn transport or multiple player ownership. The present local player's input/HUD registrations must not be blindly reused for every future remote player. A later coop plan must separate role-specific composition and pair external network despawn with scope disposal, including SDK-specific root-object/parenting constraints.

### Assumptions and Non-Goals

- Current supported behavior remains one live local player per owning CoreScope; ordinary respawn reuses it.
- Player/enemy prefab assets remain free of LifetimeScope and serialized scope references.
- Keep existing synchronous asset loading. IAssetService currently has no handle-release API; asset lease ownership is separate work.
- No networking implementation, pooling, async Addressables redesign, save migration, input/UI redesign, animation-controller editing or package upgrade.
- BaseFactory, UiFactory and static VContainerExt asset-loading behavior remain outside the implementation scope.

### Success Criteria

- CharacterFactory has explicit dependencies and delegates the registration inventory to a concrete composition installer.
- The shared infrastructure scope is created before the Character instance; one scope owns one current local player.
- Character prefab stays free of lifetime infrastructure; no post-build wrapper reparenting or actor-to-scope attachment is required.
- Initial world pose, Awake/OnEnable setup and animator preparation precede Character.Initialize in the validated sequence.
- All original bindings, aliases and UI/input ownership remain present with exactly-once setup/injection.
- Supported prefabs need no runtime ViewEntity repair.
- Duplicate creation cannot orphan a player; failure/disposal removes owned subscriptions, locator registration and hierarchy.
- Existing respawn/fast travel semantics continue to reuse the Character.

## Execution Plan

- [ ] **Phase 1 — Verify the shared contract and player callbacks.** Capture dirty baseline, actual supported prefabs, registration parity and Awake/OnEnable requirements. Verify: no callback needs DI before Build and fist/animator setup ordering is explicit.
- [ ] **Phase 2 — Add CharacterCreationData and CharacterScopeInstaller.** Share EntityLifetimeScope with the enemy plan; move existing registrations with section comments. Verify: no actor-prefab scope component, resolver access, duplicate injection or lost alias.
- [ ] **Phase 3 — Migrate CharacterFactory creation and ownership.** Create scope before loading/instantiating Character, set pose, activate, prepare animator, build, publish. Verify: public signature remains valid; duplicate creation and failure cannot leak ownership.
- [ ] **Phase 4 — Wire and persist infrastructure.** Assign the shared scope prefab on supported CoreScope assets/instances through Unity. Audit Character and variants; change an actor asset only for a confirmed missing required gameplay component. Verify: all changes imported/saved, no errors and no manual user save.
- [ ] **Phase 5 — Review and validate.** Run focused safe Editor composition tests and independent review; schedule runtime activation/player/UI validation separately and retain the gap until executed.
- [ ] **Phase 6 — Record implementation outcome.** Update relevant Character System ownership/factory documentation and an implementation record after validation, then close the plan on its actual evidence basis.

## Risks and Rollback

- Pre-build Awake/OnEnable cannot depend on injected services. If a variant violates this, revise the staged integration rather than adding a scope to the prefab.
- Loadout, animation and ground probing are ordering-sensitive. Source inspection identifies the boundary but does not prove runtime correctness.
- CoreScope parent transform changes can alter omitted-position/default rotation/scale; preserve the old instantiate-then-reparent world result.
- Early initialization can register partial locator/UI state before a later failure; rollback must dispose the created container and object hierarchy.
- Parent container disposal alone does not recursively dispose all child scopes. Retain explicit CharacterFactory ownership and verify parent teardown.
- Revert only implementation-owned scripts and infrastructure asset edits together, preserving earlier uncommitted factory changes.

## Validation

This planning task used live source/API and serialized-text inspection only. No Editor, UTF or gameplay execution was performed.

- Resolve `unity-testing` and follow [[Knowledge/Guides/Testing/Unity Test Framework Test Flow]].
- Add/reuse focused safe Editor coverage for registration parity, explicit host inputs, optional pose, duplicate-create ownership, initializer failure propagation, partial cleanup and immediate locator/controller cleanup on Dispose.
- `Assets/Scripts/Tests/CharacterRuntime/CharacterRuntimeTests.asmdef` references the Character runtime assembly; factory/composition types may be outside it. Use a compatible existing Editor seam or minimal fixture without broad assembly migration.
- Before any test command, inspect all open scenes; no test run with dirty scenes. Discover the live schema and intended fixtures using list_tests --mode editor. Run explicit editor mode/filter/async/timeout with a caller-enforced budget, initially 120 seconds per batch.
- Poll test_status and inspect actual nested executed/pass/fail/cancelled outcomes. On timeout inspect/cancel and confirm no active test remains.
- Through Unity, verify shared host activeSelf=false / autoRun=false / no actor references; verify CoreScope assignments and clean Character prefab/variants. Save/import changed assets and inspect errors.
- Defer actual Awake/OnEnable ordering, default-fist initialization, animator callbacks, NavMesh-adjacent integration, initial grounding, player startup, movement, root motion, combat, equipment, inventory/HUD, menus, interaction, death/respawn and location transitions to a separately assigned bounded Play Mode follow-up. Editor tests cannot close these runtime gates.

## Execution Handoff

Implementation is `in-progress` under the user's explicit 2026-09-17 execution request. The separate-scope topology is implemented; Character runtime validation remains open.

| Owner | Exact scope |
|---|---|
| Same C# writer as shared enemy infrastructure | `Assets/Scripts/Entities/Character/CharacterFactory.cs`; new `CharacterCreationData.cs`; new `Assets/Scripts/Services/VContainer/CharacterScopeInstaller.cs`; shared EntityLifetimeScope/CoreScope wiring and focused compatible Editor tests. |
| Unity asset writer after compilation | Shared EntityLifetimeScope prefab and CoreScope references listed in the enemy plan. Read-only audit of `Assets/Prefabs/Models/Character/Character.prefab` and confirmed variants; edit only for a verified missing gameplay component. |
| Read-only integration checks | CoreGameOrchestrator; Character.Configure/Initialize/SetPosition; AnimatorComponent.ConfigureCharacter; AnimatorRootMotionRelay; AnimatorStateMachineReceiver; EquipmentPresentation; MovementComponent; EntityRegistrationExt; existing CharacterSpawnData and Character address. |
| Independent validation | unity_reviewer and unity_test_runner after integration, followed by the separately assigned runtime validation phase. |

Default design does not change Character, AnimatorComponent, MovementComponent, BaseFactory, UiFactory, CoreGameOrchestrator or persistent CharacterSpawnData. Expand only for a verified ordering/integration defect.

Use soulslike-csharp-change, soulslike-unity-assets, soulslike-change-review and soulslike-validation as appropriate. Carry soulslike-ui-workflow for preserved presentation composition and context keys character-architecture, entity-locator, ui-code, ui-style, ui-asset-layout and unity-testing. Resolve animation-code before any actual animation runtime/controller mutation.

## Final Refactor Note — 2026-09-17

CharacterFactory follows the same **dedicated scope prefab + clean entity prefab** convention selected for enemies. Create the scope first, load and stage Character, set the pose, activate for Unity lifecycle setup, prepare the animator, then install/build and return Character. The previous prefab-root scope recommendation is superseded.

Keep future coop-specific spawn/despawn ownership outside the Character prefab. This creates a reusable composition boundary; it does not yet make the local-player runtime network-ready.

## Implementation Progress — 2026-09-17

- CharacterFactory now creates the shared scope before loading and staging the independent Character prefab, applies the optional world position, activates it for Unity callbacks, configures the animator, then builds through CharacterScopeInstaller before returning the actor.
- The shared scope prefab and all four CoreScope references were imported and saved through Unity. The Character prefab remains free of LifetimeScope. Independent review found registration parity and no material C# regression; the focused shared-host failure/one-build EditMode test passed (1/1).
- The actual Character startup, animator/default-fist ordering, grounding, UI/input, equipment, respawn, duplicate-create ownership and disposal paths still require bounded runtime validation. Assign the separate bounded Play Mode follow-up phase to `unity_test_runner` under the project test safety policy.

## Scope Component Revision — 2026-09-17

The user's subsequent direction supersedes the plain `IInstaller`/`InstallAndBuild` design. `CharacterScopeInstaller` now inherits `EntityLifetimeScope`, itself a `LifetimeScope`, so it receives MonoBehaviour callbacks and configures its own child container. The shared prefab asset is an inactive bare host; `CharacterFactory` adds the concrete scope component while the clone is inactive, sets the parent, stages Character beneath it, activates for Unity callbacks, prepares the animator, and calls `BuildOnce`. Character component bindings use `RegisterComponentInHierarchy<T>().UnderTransform(transform)` rather than passing components to the scope; its configuration receives only the generated identity. `CoreScope` holds the host as a `GameObject` reference.

Unity persisted the host and all CoreScope references. The revised scope initialization failure/single-build EditMode test passed (1/1). Character startup, UI/input and later independent coop player removal remain runtime and future ownership gates; the preexisting shared UI factory lifetime model was not changed by this scope revision.

The subsequent user instruction removed `try`, `catch`, and `finally` from both factories. Character creation errors still propagate, but a failure after host allocation can leave a partial host until the parent hierarchy is destroyed. `CharacterFactory.Dispose` still owns a successfully built local-player scope.

## Concrete Scope Prefab Revision — 2026-09-17

The user's latest direction replaces dynamic scope-component creation with a separate inactive `CharacterScopeInstaller` prefab. `CharacterFactory` first loads and validates the independent clean Character prefab, then uses the parent scope's `CreateChildFromPrefab`, stages Character, asks `Character.StageSpawn` to apply any initial position before activation, activates, builds, and returns Character. Character's injected `Configure` prepares its animator before entry-point initialization. The scope registers one ID per child container through `IUniqueIdGenerator`, so the factory no longer takes the generator or calls `ConfigureCharacter(entityId)`. The earlier shared-host and factory animator/pose steps are superseded. Factory exception blocks remain absent, as explicitly requested.

Unity persisted the concrete Character scope prefab and its CoreScope references. Production and Editor C# assemblies build; focused EditMode tests passed for scoped ID reuse (1/1) and scope failure/single-build behavior (1/1). Character startup, movement grounding, animation callbacks, UI/input and respawn still need separate runtime validation.
