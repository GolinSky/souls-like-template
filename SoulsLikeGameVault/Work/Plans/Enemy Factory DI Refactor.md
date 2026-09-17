---
title: Enemy Factory DI Refactor
type: plan
domains:
  - enemy
  - dependency-injection
status: in-progress
authority: advisory
updated: 2026-09-17
source_commit: 4ddff2fd
aliases:
  - EnemyFactory DI Creation Plan
tags:
  - work/plan
  - status/in-progress
---
# Enemy Factory DI Refactor

## Plan Contract

### Goal

Refactor enemy and player creation around **one reusable, dedicated scope prefab asset and separately authored, clean entity prefabs**. Each spawned entity still receives its own scope instance and child container. Factories own creation; composition installers own registrations; gameplay actors do not own or expose VContainer objects.

This plan selects the separate-scope approach requested on 2026-09-17. It replaces the earlier recommendation to put EnemyLifetimeScope on the enemy root. [[Work/Plans/Character Factory DI Refactor]] applies the same convention to CharacterFactory. The implementation evidence is recorded below.

### Source Research and Decisions

Baseline: commit `4ddff2fd`, inspected on 2026-09-17 with existing uncommitted changes in EnemyFactory, CharacterFactory, BaseFactory and UiFactory. Preserve those edits when implementing.

Evidence: existing Graphify relationships, live C# / serialized prefab inspection, Context7 VContainer documentation, and installed VContainer 1.19.0 source under `Library/PackageCache/jp.hadashikick.vcontainer@dcf16cda6a65/Runtime/`. Graphify and advisory architecture notes do not override live source.

#### Current behavior to preserve

| Source | Verified behavior / implication |
|---|---|
| `Assets/Scripts/Services/VContainer/CoreScope.cs:56` | Registers both factories. This is their current parent scope; do not call it the global root. |
| `Assets/Scripts/Entities/Enemy/EnemyFactory.cs:33` | Samples NavMesh using the enemy agent type, area mask and radius; falls back to nearest position and fails if unavailable. Preserve that logic and snapshot the sampled position. |
| `EnemyFactory.cs:65` / `:69` | Resolves the ID generator and creates one child scope per enemy. Replace the hidden lookup with constructor injection; preserve the scope cardinality. |
| `EnemyFactory.cs:71` / `:76` | Resolves actor/scope to discover transforms. Replace with known scope and actor references. |
| `EnemyFactory.cs:77`; `EnemyActor.cs:47` | Four raw spawn parameters feed ConfigureSpawn. Replace with one EnemySpawnData value. |
| `EnemyFactory.cs:161`; `EnemyActor.cs:60` | Attaches the separate scope root through AttachLifetimeRoot. Replace the gameplay-facing root attachment with a required domain despawn operation. |
| `EnemyActor.cs:72` | Despawn is idempotent, emits Despawned, and schedules destruction in finally. Preserve these semantics, including cleanup if a subscriber throws. |
| `EnemyEncounterSystem.cs:115`; `EnemyController.cs:98` | Encounter stores actors and despawns on reset/disable; controller initialization/disposal owns coordinator membership and subscriptions. |
| `Assets/Scripts/Entities/BaseEntity/Entity.cs:22` | Entity initialization/disposal owns IEntityLocator registration/removal. Scope cleanup must preserve this behavior. |
| `Assets/Prefabs/Models/Enemy/ErikaMeleeEnemy.prefab`; `Assets/Prefabs/Models/Character/Character.prefab` | Both currently have no LifetimeScope component. Keep them that way; do not introduce nested scope prefabs inside them. |
| `EnemySpawnPoint.cs:9`; `CharacterFactory.cs:51` | Enemy prefab comes from the spawn point; Character uses the existing Character asset address. Preserve these acquisition paths. |

#### Design selection after validation

| Candidate | Decision |
|---|---|
| Previous A/B: LifetimeScope on the actor prefab root | **Rejected by the user’s modularity requirement**, even if easier to destroy. Do not retain this as a fallback. |
| One infrastructure-only scope prefab reused by both factories; concrete enemy/player installers | **Selected.** One prefab asset, many scope instances; entity prefabs remain independent. |
| Separate EnemyLifetimeScope and CharacterLifetimeScope prefab assets | Feasible fallback only if a concrete incompatible host requirement is found. Different registration lists alone do not justify duplicate host assets. |
| Build and initialize while the entire actor hierarchy is inactive | **Not selected.** Source inspection exposes active-object / Awake dependencies described below. |
| One shared runtime container for all enemies/players | Rejected: loses per-entity state and independent disposal. |
| Stock scope plus a callback capturing actor variables assigned later | Feasible but less explicit. Prefer a small host accepting an already populated IInstaller after actor creation. No static/global installer queue. |

The selected host is a small `EntityLifetimeScope : LifetimeScope`, used only by the composition layer. Its single per-instance installation seam accepts VContainer's existing `IInstaller`, stores it for Configure, and builds once. Proposed method: `InstallAndBuild(IInstaller installer)`. Concrete `EnemyScopeInstaller` and `CharacterScopeInstaller` contain their respective registrations. No actor-kind switch, generic ActorFactory, installer inheritance framework, or gameplay service bag is needed.

#### Verified API and lifecycle constraints

- `LifetimeScope.cs:259`: CreateChildFromPrefab sets the cloned scope's parent reference. An inactive source scope prefab stays inactive on creation.
- `LifetimeScope.cs:135`: Awake only builds automatically when autoRun is true. Author the dedicated scope prefab with **activeSelf=false and autoRun=false**. Activation must not trigger a second build.
- `LifetimeScope.cs:189` / `:288`: Configure runs before local installers, scope self-registration, injection and entry-point dispatch. Required actor/data references must exist before explicit Build.
- `ContainerBuilderUnityExtensions.RegisterComponent` forces component injection during build. Use this for known actor components; RegisterInstance is suitable for spawn values and authored data, not a substitute for component injection.
- `FindComponentProvider` searches scene roots unless explicitly bounded. Use known components or UnderTransform(actor.transform), including inactive children; never use the scope wrapper as an excuse for scene-wide lookup.
- `EnemyNavigationMotor.cs:26`: Initialize calls NavMeshAgent.Warp and throws when it fails. Building an entirely inactive enemy cannot be accepted without changing that runtime contract.
- `EquipmentPresentation.cs:33`; `Character.cs:122`: Awake prepares the default fist runtime, while Character.Initialize builds the loadout. Awake must precede this initialization.
- `AnimatorStateMachineReceiver.cs:14`: OnEnable initializes animation behaviours. Preserve player preparation before Character.Initialize, but after the required Unity callbacks.
- `EntryPointDispatcher.cs:21`: initialization exceptions are caught and normally logged. Build returning does not alone prove successful initialization. Install a per-entity fail-fast entry-point exception handler before installer registrations; prove propagation and rollback with a focused failure test. Do not change the global handler.
- `LifetimeScope.cs:169`: Dispose disposes the container immediately and schedules scope GameObject destruction; OnDestroy also disposes. Container disposal alone is not GameObject destruction.
- `Container.cs`: parent container disposal does not recursively own arbitrary child scope objects. Use explicit entity owners and the current scope hierarchy; do not claim parent-container-only disposal cleans children.

The public API is documented in [VContainer child scope creation](https://vcontainer.hadashikick.jp/scoping/generate-child-with-code-first) and [component registration](https://vcontainer.hadashikick.jp/registering/register-monobehaviour). Exact sequencing above is grounded in the installed 1.19.0 source, not inferred from current online examples.

#### Selected creation flow

One infrastructure scope prefab asset is reused, while each runtime scope owns only one entity:

```text
CoreScope
  EntityLifetimeScope instance A
    Clean enemy prefab instance
  EntityLifetimeScope instance B
    Clean enemy prefab instance
  EntityLifetimeScope instance C
    Clean Character prefab instance
```

1. Validate the supported prefab contract, snapshot spawn inputs and allocate the entity ID. Enemy NavMesh preprocessing remains before allocation of runtime objects.
2. Factory clones the dedicated inactive scope prefab under its injected parent scope. Keep a direct reference immediately so later failure can release it.
3. Factory obtains the entity prefab separately: current enemy spawn-point reference, or Character asset-service load. Instantiate beneath the inactive scope. Do not modify source prefab active state.
4. Apply the initial world pose while inactive, using the same spawn snapshot that will be injected. Preserve Character's optional-position and prefab-rotation behavior and verify non-identity parent transforms. Build is still deferred.
5. Activate the scope hierarchy with autoRun=false. The owning parent must be active and the supported actor root must become activeInHierarchy; preserve intentionally inactive child objects. Actor Awake/OnEnable complete before DI entry-point initialization. **These callbacks must not require injected dependencies.** Audit supported components/variants before accepting this contract.
6. For Character, run its existing AnimatorComponent.ConfigureCharacter setup after Awake and before Character.Initialize. Construct the concrete installer with direct actor/component references and all per-spawn data.
7. Call InstallAndBuild once on the main thread, synchronously in the same creation call. Register components/data, inject the typed spawn value, and run entry points while the actor is active. No await, frame yield or gameplay publication is allowed between activation and completed build.
8. Publish/return the direct actor reference only after successful construction. On a propagated load, registration, injection or initialization failure, explicitly dispose the partial scope and destroy its owned hierarchy, then preserve the exception.

This sequence deliberately permits Awake/OnEnable before injection; it does **not** promise injection before every Unity callback. If a supported component requires DI in those callbacks, the runtime gate fails. Rework only that proven ordering dependency or the staging sequence; do not add LifetimeScope back to the actor prefab. Unity callback exceptions may be logged rather than propagate to the factory, so validation must inspect Console as well as return values.

#### Registration and payload ownership

EnemyFactory depends explicitly on parent LifetimeScope, the dedicated scope-prefab reference, INavMeshService and IUniqueIdGenerator. Remove its BaseFactory inheritance. CharacterFactory additionally needs IAssetService. Leave BaseFactory and UiFactory outside the implementation scope.

CoreScope holds one serialized `entityScopePrefab` reference and passes that same asset explicitly to both factory registrations. Keep it distinct from the injected parent LifetimeScope; do not register the prefab globally as LifetimeScope and shadow the live parent. Prefer this existing serialized composition route over adding an Addressable key for the scope.

EnemyScopeInstaller receives known actor components, entity identity, EnemySpawnData, health data, behaviour profile, moveset, group coordinator and the required despawn handler. The host receives only the installer. Keep gameplay spawn data free of scope/resolver/factory references.

`readonly struct EnemySpawnData` carries sampled Position, Rotation, a fresh patrol-position snapshot and RandomSeedOffset. Register it once and inject it into EnemyActor.ConfigureSpawn as one argument. Position is also applied for safe pre-activation placement; ConfigureSpawn preserves HomePosition, patrol and random-seed behavior. A readonly struct containing an array is not deeply immutable: do not share a writable patrol snapshot between spawns.

Move the existing registration inventory without changing aliases or lifetimes for appearance:

| Section | Preserve |
|---|---|
| Actor and scene components | EnemyActor, ViewEntity, TargetLockComponent, HealthComponent, CombatDefenseComponent, VisibilityComponent, EnemyNavigationMotor, LadderClimber, EnemyActionExecutor, MeleeHitboxController; zero/one optional EnemyActivationTrigger. |
| Models and data | HealthModel, WeaponDatabase, per-spawn assets and snapshot. |
| Entity commands | ApplyDamageCommand, ResolveMeleeHitCommand, CriticalTargetCommand, TargetingCommand, PlatformRideCommand and entity registration. |
| AI and orchestration | EnemyPerception, EnemyRandomStreams, EnemyActionSelector, EnemyController. |
| Presentation | EnemyHealthUiComponent and its current interface/lifetime bindings. |

Use section comments in each concrete installer. Do not create one helper per registration. Preserve child-local Singleton/Scoped choices unless a behavior change is justified. Composition may inspect its own actor subtree; cross-entity gameplay still uses IEntityLocator and target-owned commands.

#### Teardown and future networking boundary

EnemyActor retains Despawn and the optional Despawned notification. Remove AttachLifetimeRoot, _lifetimeRoot and _hasLifetimeRoot. Inject a required domain contract such as `IEnemyDespawnHandler`; its infrastructure implementation knows the scope, while the actor does not.

Despawn guards repeat calls, notifies once, then invokes the required handler in finally. The local handler schedules destruction of the **scope root**, preserving existing deferred destruction semantics. Scope OnDestroy releases the container, locator identity, coordinator membership and subscriptions. Do not replace this with an optional event subscriber or silently change normal despawn to immediate disposal.

Failed creation uses explicit scope.Dispose even if the scope was never activated; do not rely on OnDestroy for an inactive partial construction. CharacterFactory retains explicit disposal of its owned player scope, including immediate container cleanup and scheduled hierarchy destruction. Verify never-created and repeated-dispose cases.

Hierarchy destruction of CoreScope reaches its scope children. Any code path disposing only a parent container needs explicit owner-driven child disposal; verify actual teardown callers rather than assuming recursive container ownership.

For future coop, keep scope infrastructure out of the entity prefab, keep concrete actor instances as installer inputs, and keep entity creation/removal under composition ownership. A later network integration can supply network-created instances and coordinate despawn at this boundary. This draft does not choose a networking SDK, promise nested network-object support, implement remote players, or equate local entity IDs with network IDs. If a future network API requires root objects, the owner must pair separately parented entity and scope instances and handle external network despawn explicitly. Destroying only the child actor currently does not imply destruction of its scope.

### Assumptions and Non-Goals

- Planning only; no C#, assets, Editor state, packages or Obsidian configuration changed.
- “Single prefab” means one reusable infrastructure prefab asset, not one scope instance for the entire population and not an enemy nested into a scope prefab asset.
- Keep enemy direct prefab references and current synchronous Character loading. The current IAssetService exposes no release/lease API; do not invent ReleaseInstance calls for ordinary Instantiate objects or claim this refactor fixes asset-loading ownership.
- Preserve AI, combat, NavMesh sampling, health presentation, entity communication, public CreateEnemy/Despawn contracts and ordinary Character respawn.
- No pooling, networking implementation, async loading migration, global ID redesign, UI redesign or animation-controller changes.

### Success Criteria

- Supported enemy/player prefabs contain no LifetimeScope, scope-reference fields or embedded scope prefab.
- Both factories use one shared infrastructure prefab asset; each entity has its own scope instance and container.
- Factories and installers have no container Resolve calls, hidden inherited resolver, resolver-valued transform callbacks or raw enemy spawn WithParameter chain.
- Scope is created before the entity instance; initial pose and Awake/OnEnable ordering satisfy the validated activation contract.
- Build/injection/initialization occurs once before factory return; missing mandatory dependencies fail visibly and partial creation is cleaned up.
- Components bind to their own actor, even for multiple identical prefabs; disposing/despawning one leaves siblings intact.
- Actor gameplay exposes no VContainer root attachment; teardown preserves current event, locator and coordinator semantics.
- Registration inventory, UI/input ownership and Character setup/respawn behavior remain intact.

## Execution Plan

- [ ] **Phase 1 — Lock baseline and validate staging contract.** Record dirty changes, prefab variants, all registrations/aliases, Awake/OnEnable dependencies and teardown callers. Verify: the selected active-before-Build sequence supports the actual components; record any blocker before production migration.
- [ ] **Phase 2 — Implement shared composition infrastructure.** Add EntityLifetimeScope and the concrete installers; add the explicit CoreScope prefab reference and constructor dependencies. Verify: fully formed installers reach one builder, parent binding remains the live scope, and initialization failures propagate.
- [ ] **Phase 3 — Migrate enemy creation and removal atomically.** Add EnemySpawnData and domain despawn contract/adapter; replace factory resolver wiring and AttachLifetimeRoot together. Verify: NavMesh input parity, single spawn injection, deferred teardown, isolation and failure rollback.
- [ ] **Phase 4 — Align CharacterFactory.** Execute the coordinated steps in [[Work/Plans/Character Factory DI Refactor]] using the same host prefab. Verify: scope-first creation, Awake-dependent setup, singular UI/input ownership and existing respawn behavior.
- [ ] **Phase 5 — Persist infrastructure assets through Unity.** Create the shared scope prefab, assign all applicable CoreScope references, validate clean actor prefabs and overrides. Verify: imported/saved assets and no serialization errors; no manual save required.
- [ ] **Phase 6 — Review, validate and record.** Run bounded safe Editor tests and independent review; report runtime gates as deferred until explicitly validated. Update architecture/history only after implementation and close on its recorded evidence basis.

## Risks and Rollback

- Activation before Build is required by inspected code but exposes Awake/OnEnable before injection. Missing callback inspection or a later variant can invalidate staging.
- Non-identity parent transforms can change spawn pose/scale. Verify supported CoreScope transforms and preserve world-space semantics explicitly.
- Registration order is not dependency initialization order. Prove spawn configuration and player preparation precede their dependent entry points.
- A successful CLI call or Build return cannot replace runtime outcome checks, especially with Unity callback logging.
- Normal enemy deferred destruction and Character immediate container disposal are different existing contracts; keep both explicit.
- Roll back only implementation-owned scripts and asset changes together. Restore factory/actor teardown wiring atomically and preserve preexisting dirty edits.

## Validation

Planning evidence: source/API and serialized-text inspection only. **No Unity Editor run, UTF execution, prefab mutation or runtime reproduction was performed.**

| Gate | Planning result | Implementation acceptance evidence |
|---|---|---|
| Reusable host API | Supported by installed source | Two enemies and a Character use one prefab asset with distinct runtime child containers. |
| Entirely inactive Build | Rejected as the default by source dependencies | Active NavMeshAgent and completed Awake-dependent player setup before entry points. |
| Active-before-Build staging | Selected, runtime proof pending | Correct pose; no DI-dependent pre-build callback; no intervening tick; one build; no initialization errors. |
| Isolation | Design defined, not executed | Distinct IDs/models/components; correct actor subtree; independent locator/coordinator membership. |
| Cleanup | Required owner paths identified, not executed | Despawn twice, throwing notification, failed load/build/initializer, parent hierarchy destruction and player Dispose leave no owned residue or sibling damage. |
| Future coop | Architectural boundary only | Later network-specific authority, callback, root-parenting and external-despawn tests remain separate work. |

Resolve `unity-testing` and follow [[Knowledge/Guides/Testing/Unity Test Framework Test Flow]] for implementation validation.

- Replace obsolete attachment tests in `Assets/Scripts/Tests/EnemyRuntime/EnemyActorLifetimeTests.cs`. Reuse relevant enemy health lifetime coverage in `Assets/Scripts/Editor/Tests/EnemyHealth/EnemyHealthUiLifetimeTests.cs` where its fixture is safe and applicable.
- Add focused composition coverage for typed snapshot delivery, aliases, scope isolation, initialization failure propagation, partial cleanup and idempotent removal. Respect existing asmdef/reflection boundaries; do not migrate gameplay assemblies solely for tests.
- Discover live CLI schemas. Before any test command, inspect every scene using assert_test_ready/list_open_scenes; no dirty-scene test run. Confirm the intended fixture with list_tests --mode editor.
- Run only bounded safe Editor tests with explicit editor mode, filter, async_tests=true, timeout and a caller-enforced wall-clock budget (initial target: 120 seconds per batch). Poll test_status and validate nested executed/pass/fail/cancelled counts. On timeout inspect/cancel and confirm no active run remains.
- Inspect the shared prefab's inactive root, autoRun=false, empty autoInjectGameObjects, no actor children and no actor references. Verify CoreScope assignments and absence of scope components in player/enemy variants through Unity; save/import every changed asset.
- Defer real NavMesh spawning, Awake/OnEnable sequencing, animator/fist setup, deferred Destroy across frames, death/reset, triggers, player/UI gameplay and scene teardown to a separately assigned bounded Play Mode follow-up. These remain validation gaps until executed, and the full runtime acceptance criteria are not met by Editor tests alone.

## Execution Handoff

Implementation is `in-progress` under the user's explicit 2026-09-17 execution request. The clean-prefab and scope-first topology is implemented; the runtime validation gates below remain open.

| Owner | Assigned scope |
|---|---|
| One C# writer, coordinated across both plans | Existing EnemyFactory, EnemyActor, CharacterFactory and CoreScope; new `Assets/Scripts/Entities/Enemy/EnemySpawnData.cs`, `IEnemyDespawnHandler.cs`; new `Assets/Scripts/Services/VContainer/EntityLifetimeScope.cs`, `EnemyScopeInstaller.cs`, `CharacterScopeInstaller.cs`, `EnemyDespawnHandler.cs`; CharacterCreationData and focused compatible tests. One top-level type per file. |
| Unity asset writer, after scripts compile | New `Assets/Prefabs/View/VContainer/EntityLifetimeScope.prefab`; `Assets/Prefabs/View/VContainer/CoreScope.prefab`; confirmed CoreScope scene instances/overrides. Audit found CoreScope references in `Assets/Scenes/DefaultLocation/DefaultLocation.unity`, `Assets/Sandbox/Scenes/WorkShop/WorkShop.unity`, `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity`; distinguish inherited references from actual overrides before editing. |
| Read-only integration review | EnemySpawnPoint/encounter prefabs and variants, EnemyEncounterSystem, EnemyController, EnemyNavigationMotor, Entity/EntityRegistrationExt, CoreGameOrchestrator, actor component callbacks and supported actor prefabs. Expand edits only for a proven staging defect. |
| Independent validation | unity_reviewer and unity_test_runner after integration; later bounded runtime validation assigned separately. |

Required skills: soulslike-csharp-change / soulslike-unity-assets for writers; soulslike-change-review / soulslike-validation for reviewers and validators; unity-cli for Editor operations. Context keys: entity-locator, unity-testing and relevant character-architecture headings. Carry soulslike-ui-workflow with ui-code, ui-style and ui-asset-layout for preserved UI registrations; no visual redesign. Resolve animation-code only if animation runtime/controller changes are actually added.

Remaining technical gates are callback ordering, failure cleanup and prefab/reference coverage. If a gate fails, revise the implementation detail while preserving the separate clean entity prefab requirement.

## Final Refactor Note — 2026-09-17

Use **one dedicated EntityLifetimeScope prefab asset**, instantiated once per enemy or player, and load/instantiate the clean gameplay prefab separately. The flow is **factory -> create scope -> obtain and stage entity prefab -> set pose -> activate for Awake/OnEnable -> install and build -> return actor**.

Do not put LifetimeScope on EnemyActor, Character or another gameplay entity prefab. The old A/B recommendations are superseded. Separate scope ownership is the selected modular boundary for later coop integration; networking itself is outside this refactor. Static validation supports this design, but runtime activation and cleanup evidence is still required before calling the implementation complete.

## Implementation Progress — 2026-09-17

- Added one inactive, `autoRun=false` EntityLifetimeScope prefab and assigned it to the CoreScope prefab and all three direct CoreScope scene instances. Unity imported and saved each asset; all four references have the same prefab component GUID/file ID.
- Migrated both factories to scope-first staging, active-before-Build initialization, concrete installers and direct actor return. EnemyActor now uses a domain despawn handler; failed creation disposes its partial scope. The player factory owns one scope and rejects duplicate creation.
- Independent C# review found no material regression. Focused EditMode tests passed for throwing enemy despawn notification/idempotence (1/1) and fail-fast initializer propagation/single-build behavior (1/1). Unity reported no new import or compilation errors after the final asset mutation.
- Runtime validation remains open for real NavMesh spawning, Awake/OnEnable ordering, multiple-entity isolation, deferred cleanup after a frame, and Character startup, UI/input, movement and respawn. Assign the separate bounded Play Mode follow-up phase to `unity_test_runner` under the project test safety policy.

## Scope Component Revision — 2026-09-17

The user's subsequent direction supersedes the `IInstaller`/`InstallAndBuild` design and the scope component previously stored on the reusable prefab. The same inactive prefab asset is now a bare scope host. Each factory clones it, adds exactly one concrete `LifetimeScope` component (`EnemyScopeInstaller` or `CharacterScopeInstaller`) while inactive, sets its parent reference, stages the clean actor prefab, activates the host, calls `BuildOnce`, and returns the actor. The concrete scope registers actor components with `RegisterComponentInHierarchy<T>().UnderTransform(transform)`; only identity and spawn/domain data are passed into its configuration method. Enemy despawn disposes its child container immediately and schedules host destruction. `CoreScope` stores the host as a `GameObject` prefab reference. Enemy and Character actor prefabs remain free of `LifetimeScope`.

Unity imported and saved the Transform-only host and all four CoreScope references. After this revision, bounded EditMode tests passed for scope initialization failure/single-build behavior (1/1) and enemy despawn notification/idempotence (1/1). The clean ElevatorDemo scene remained open. Real gameplay spawning and multi-entity isolation still require the separate runtime validation phase.

The subsequent user instruction removed `try`, `catch`, and `finally` from both factories. Creation errors still propagate, but a failure after host allocation can leave a partial host until the parent hierarchy is destroyed. Successful enemy despawn and player factory disposal remain owned as described above.

## Concrete Scope Prefab Revision — 2026-09-17

The user's latest direction replaces the bare shared host and dynamic `AddComponent` path with separate inactive `EnemyScopeInstaller` and `CharacterScopeInstaller` prefab assets. Each contains its concrete `LifetimeScope` component and no actor child; clean actor prefabs remain independent. The factory calls its parent scope's `CreateChildFromPrefab`, stages the actor under the inactive clone, calls the actor's `StageSpawn`, activates, builds, and returns it. `EnemyActor.StageSpawn` owns pose, home, patrol and seed before NavMeshAgent activation. The enemy scope receives only the spawn point and group coordinator, creates its despawn handler, and registers the spawn point's domain assets. Entity ID generation is a single scoped DI registration backed by `IUniqueIdGenerator`; factories no longer request IDs. Factory exception blocks remain absent, as explicitly requested.

Unity persisted the two concrete scope prefabs and both typed references in CoreScope.prefab and its three direct scene instances; the obsolete shared host has no remaining Asset references and was deleted. Production and Editor C# assemblies build. Bounded EditMode tests passed for one scoped ID across repeated resolves (1/1), scope failure/single-build behavior (1/1), and enemy despawn idempotence (1/1). Real Character/Enemy spawning, activation order and multi-entity isolation remain PlayMode validation gates.
