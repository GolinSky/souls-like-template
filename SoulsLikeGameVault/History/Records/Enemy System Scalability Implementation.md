---
title: Enemy System Scalability Implementation
type: implementation-record
domains:
  - enemy
  - spawning
status: done
authority: historical
updated: 2026-09-17
tags:
  - history/change
---
# Enemy System Scalability Implementation

## Implementation Record Contract

### Outcome

Implemented [[History/Completed Plans/Enemy System Scalability]]: shared ID keyed enemy catalog, scene-owned service, simplified spawners/groups, and migrated Unity assets. The main and training enemy configurations now live in one catalog; groups retain independent pressure settings.

### Why

The previous scene spawners each held prefab, behaviour, moveset, and health references, and each encounter component owned its own spawn loop. Centralizing definitions and orchestration reduces repeated authoring while preserving scene placement and group behavior.

### Changed Files and Assets

- Runtime: `Assets/Scripts/Entities/Enemy/EnemyId.cs`, `EnemyCatalog.cs`, `EnemyService.cs`, renamed `EnemySpawner.cs` and `EnemySpawnGroup.cs`, `EnemyFactory.cs`, `CoreScope.cs`, `EnemyScopeInstaller.cs`, and `CheatsUiController.cs`.
- Authoring: `EnemyAuthoringValidator.cs`, `WorldConfigurationTests.cs`, and `EnemyCatalogTests.cs`.
- Assets: `Assets/Settings/Enemy/EnemyCatalog.asset`, `EnemyEncounter.prefab`, `TrainingDummyEncounter.prefab`, `CoreScope.prefab`, and the DefaultLocation, ElevatorDemo, and WorkShop scenes.

The renamed scripts kept their original `.meta` GUIDs. Unity imported and saved the catalog, prefabs, and scenes; ElevatorDemo was restored as the clean active scene.

### Decisions and Tradeoffs

`EnemyService` is a scene `MonoBehaviour` so it can own Unity coroutines and ticks. Each group receives its own coordinator. The catalog validates serialized entries directly and caches the validated lookup. The factory's concrete scope and exception policy were preserved.

DefaultLocation's former three-point override became one disabled offset-3 spawner. ElevatorDemo's former zero-length spawn array became four disabled inherited spawners; WorkShop remains empty. This preserves the scenes' effective selections.

### Validation Evidence

- Unity 6000.3.11f1; runtime and Editor C# builds: 0 errors each.
- Edit Mode: `EnemyCatalogTests` 3/3 passed and `WorldConfigurationTests` 8/8 passed. An initial catalog run was cancelled without a result; the runner was confirmed inactive before a successful bounded retry.
- The enemy authoring validator logged: 1 enemy prefab, 3 movesets, 3 behaviour profiles, 2 encounter prefabs, 4 scene encounters, 16 spawn points, 0 errors. The CLI request hit the Pipeline five-second response limit, but the validator completed and the Editor returned ready with a clean scene.
- Unity reloaded the persisted catalog and scenes; all three service catalog references and `CoreScope` links remained assigned. No new import or serialization errors were reported.

### Documentation Updated

Created [[Knowledge/Architecture/Systems/Enemy System]], registered `enemy-architecture` in [[Meta/Agent Context Registry]], and completed the originating plan.

### Follow-Up

A separate bounded Play Mode pass is still needed for real NavMesh spawning, independent health, patrol, trigger activation, grace/game-ended respawn, group/service disable and re-enable, and scope cleanup. The preceding [[Work/Plans/Enemy Factory DI Refactor]] also retains its stated gameplay validation gap. No Play Mode tests were run in this implementation pass.
