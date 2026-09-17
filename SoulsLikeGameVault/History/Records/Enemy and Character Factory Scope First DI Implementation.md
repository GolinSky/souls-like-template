---
title: Enemy and Character Factory Scope First DI Implementation
type: implementation-record
domains:
  - enemy
  - character
  - dependency-injection
status: done
authority: historical
updated: 2026-09-17
aliases: []
tags:
  - history/change
---

# Enemy and Character Factory Scope First DI Implementation

## Implementation Record Contract

### Outcome

Implemented the selected factory → scope → clean entity prefab → activation → DI build → actor flow in [[Work/Plans/Enemy Factory DI Refactor]] and [[Work/Plans/Character Factory DI Refactor]]. Both plans remain in progress because runtime gameplay validation is pending.

### Why

One reusable infrastructure scope prefab gives each enemy or local player a separate child container without putting LifetimeScope or scope references on gameplay prefabs. Factories own creation and cleanup; concrete installers own registrations.

### Changed Files and Assets

- Added `EntityLifetimeScope`, `EnemyScopeInstaller`, `CharacterScopeInstaller` and `EnemyDespawnHandler` under `Assets/Scripts/Services/VContainer/`; updated `CoreScope` and the Enemy/Character factories.
- Added `EnemySpawnData`, `IEnemyDespawnHandler` and `CharacterCreationData`; removed EnemyActor's lifetime-root attachment API.
- Created `Assets/Prefabs/View/VContainer/EntityLifetimeScope.prefab`; assigned it in `CoreScope.prefab`, `DefaultLocation.unity`, `WorkShop.unity` and `ElevatorDemo.unity` through Unity. Actor prefabs remained unchanged.
- Updated focused EditMode tests and the Character System factory/lifetime documentation.

### Decisions and Tradeoffs

The shared scope prefab is inactive with `autoRun=false`. Each factory stages the actor beneath it, sets world pose, activates for Unity callbacks and builds once before publishing the actor. Initialization exceptions propagate to factory rollback. Normal enemy despawn retains deferred scope-root destruction; failed construction and CharacterFactory disposal use explicit scope disposal. Local-player UI/input registrations remain in the current player scope. No networking or pooling behavior was added.

### Subsequent Scope Component Revision — 2026-09-17

The user directed a narrower runtime handoff after the first implementation. The reusable prefab asset remains the same inactive host but now contains only a Transform. Each factory adds one concrete `LifetimeScope` component to its clone while inactive: `CharacterScopeInstaller` or `EnemyScopeInstaller`, both derived from `EntityLifetimeScope`. The concrete scope uses `RegisterComponentInHierarchy<T>().UnderTransform(transform)` for actor components and receives only identity and domain/spawn inputs. `CoreScope` now references the prefab root `GameObject`. Factories keep explicit partial-creation cleanup in `finally` and contain no `catch` blocks; enemy despawn immediately disposes the child container and schedules host destruction. The four CoreScope prefab/scene references were migrated to the same host root GUID/file ID and persisted through Unity. The earlier validation evidence below describes the first implementation; the revised results follow here.

Revision validation: Unity imported and saved the Transform-only host and all four CoreScope references, each resolving GUID `18ab38a77b568924da8b47cc9cb3048b` and root file ID `527184280236412325`. Production and Editor C# assemblies compiled. The revised `BuildOnce` EditMode test passed 1/1, and the corrected enemy despawn EditMode test passed 1/1 after an initial fixture setup failure caused by its obsolete abstract scope reference. The final Editor state was ready with one clean ElevatorDemo scene and no active test run. Gameplay/PlayMode integration remains open. Per-player UI scope/view ownership for independent coop player replacement is a future boundary; the existing shared UI factory behavior predates this refactor.

### Factory Exception-Block Removal — 2026-09-17

At the user's direction, `CharacterFactory` and `EnemyFactory` no longer use `try`, `catch`, or `finally`. Their normal creation order and successful-lifetime ownership remain the same. Errors after host allocation propagate without factory cleanup of the partial host, which can persist until the parent hierarchy is destroyed.

### Concrete Scope Prefab Revision — 2026-09-17

The user then directed separate concrete Enemy and Character scope prefabs and VContainer `CreateChildFromPrefab`. The factories now request the appropriate inactive prefab through the parent scope, stage independent actor prefabs beneath it, activate, build, and return the actor. Character prefab lookup is validated before scope allocation. `EnemyActor.StageSpawn` owns pose and spawn metadata; `Character.StageSpawn` owns initial placement and its injected configuration prepares the animator. A scoped `long` registration generates one ID through `IUniqueIdGenerator` per child scope. The enemy scope creates its own despawn handler. The obsolete shared-host architecture above records the earlier implementation and is superseded by this revision.

Unity saved `CharacterScope.prefab` and `EnemyScope.prefab` as inactive, actor-free concrete scopes with `autoRun=false`, rewired CoreScope.prefab and the three direct scene instances, then deleted the unreferenced old host prefab. Production and Editor C# assemblies build. After clean-scene preflight, three bounded asynchronous EditMode tests passed: scoped ID reuse 1/1, scope failure/single-build behavior 1/1, and enemy despawn idempotence 1/1. The Editor finished idle with ElevatorDemo clean and no active test runner. Real gameplay spawning and actor callback order remain PlayMode validation gaps.

### Validation Evidence

- Unity 6000.3.11f1 imported and saved the scope prefab and all four CoreScope references. Disk YAML contains the same nonzero prefab component GUID/file ID at each site. The original ElevatorDemo scene was restored clean; no new import or compilation errors followed final asset mutation.
- Independent C# review found no material correctness finding and confirmed registration parity against the previous factories.
- Bounded EditMode tests passed: EnemyActor despawn notification/idempotence 1/1; EntityLifetimeScope initializer failure propagation and single-build rule 1/1. Tests ran asynchronously after clean-scene preflight; no test remained active.
- Real NavMesh creation, multi-entity isolation, Unity callback timing, deferred destruction after a frame, Character startup/UI/input, movement and respawn were not exercised. These remain explicit runtime validation gates in both plans.

### Documentation Updated

Updated the two plans with implementation progress and the Character System factory/lifetime section.

### Follow-Up

Assign bounded runtime validation for the open gates to `unity_test_runner` as a separate Play Mode follow-up under the project test safety policy. Close the plans only on recorded runtime evidence or a later explicit user decision.
