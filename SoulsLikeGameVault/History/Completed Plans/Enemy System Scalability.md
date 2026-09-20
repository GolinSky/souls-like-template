---
title: Enemy System Scalability
type: plan
domains:
  - enemy
  - spawning
status: done
authority: advisory
updated: 2026-09-17
source_commit: b2c98dbe
aliases:
  - Enemy Service and Catalog
tags:
  - work/plan
  - status/done
---
# Enemy System Scalability

## Plan Contract

### Goal

Choose an enemy by **EnemyId**, place its spawner, and optionally assign patrol points. Configure that enemy's prefab, behaviour, moveset, and health once in a shared catalog. One **EnemyService per gameplay CoreScope** owns spawning, respawning, and cleanup.

Execution approved by the user's 2026-09-17 request. The reviewed implementation scope includes the additional sandbox scenes and the lifecycle and catalog-validation decisions below.

### Source Research and Decisions

Verified against source at `b2c98dbe` on 2026-09-17.

Currently, `EnemySpawnPoint` contains the prefab and all three data references. `EnemyEncounterSystem` passes that component through `EnemyFactory` into `EnemyScopeInstaller`. This couples enemy configuration to scene placement.

There is no existing enemy-variant enum. `EntityType.Enemy` identifies the general entity category; runtime entity IDs identify individual instances. Neither replaces the proposed `EnemyId`.

`DefaultLocation` runs both the main encounter and training-dummy encounter. They have separate combat pressure coordinators. Keep those groups independent when moving their execution into one service.

Preserve the current scope creation design from [[Work/Plans/Enemy Factory DI Refactor#Concrete Scope Prefab Revision — 2026-09-17]]. Its outstanding gameplay validation remains outstanding.

### Proposed Ownership

| Part | Owns |
|---|---|
| **EnemyCatalog** — one shared asset | Mapping from `EnemyId` to prefab, `EnemyBehaviourProfile`, `WeaponMovesetDefinition`, and `HealthData`. |
| **EnemySpawner** — simplified, renamed `EnemySpawnPoint` | One enemy ID, its Transform position/rotation, patrol points, and the existing seed offset. No prefab or enemy-data references. |
| **EnemySpawnGroup** — simplified, renamed `EnemyEncounterSystem` | Existing start/respawn and pressure settings. Enabled child spawners define membership; remove the manually maintained spawn-point array. |
| **EnemyService** | A `MonoBehaviour` on the scene root, registered once by `CoreScope`. It owns catalog lookup, all groups' active enemies, spawning, respawning, next-frame coroutines, coordinator ticking, subscriptions, and cleanup. One existing `EnemyGroupCoordinator` per group. Bind groups after DI and inspect their enabled state because `OnEnable` may precede binding. |
| **EnemyFactory** and enemy scope | Existing creation and per-instance dependency injection. Receive the resolved definition and spawn data instead of reading a scene spawner. |

The group remains only to preserve existing encounter settings and independent combat coordination. It performs no spawning, owns no actor list, and runs no respawn coroutine.

The flow is: **spawner ID → catalog definition → service → existing factory → enemy scope**.

### Assumptions and Non-Goals

- One spawner produces one enemy. Multiple enemies use multiple simple spawners.
- Patrol routes and seed offsets belong to placement. AI tuning remains in the behaviour profile.
- Catalog assets are shared configuration. Each spawned enemy keeps its own runtime health, AI state, and scope.
- This addresses adding and configuring enemy types. Pooling, waves, streaming, networking, and AI performance optimization are separate work.
- Keep current AI, combat, animation, and health implementations.

### Success Criteria

- Changing a catalog entry affects every spawner using that ID.
- Two IDs can use the same prefab with different health, behaviour, or movesets.
- Scene spawners contain none of the four enemy-definition references.
- Only the service coordinates enemy spawning and respawning; existing encounter groups retain independent pressure limits.
- Adding another enemy requires an enum value, a catalog entry, and its assets—not another runtime class.

## Execution Plan

- [x] **1. Centralize enemy definitions.** Add `EnemyId` and `EnemyCatalog`. Use one enum-keyed serialized mapping with a small nested serializable `Definition` containing the four references. Validate the serialized key-value entries directly before lookup because the existing dictionary builder silently drops duplicate keys. Reuse the project's existing serialized-dictionary support. No separate definition script, definition asset type, repository, or lookup service. Use stable explicit enum values with an unassigned zero value. Create entries for the distinct existing configurations, including the main enemy and both training variants. **Verify:** all current placements resolve to the same four assets they use today; duplicate/unassigned IDs and missing references produce clear authoring errors. Runtime lookup of an unknown ID fails rather than silently skipping a spawn.

- [x] **2. Make spawners simple and add the service.** Rename/simplify the existing point and encounter components as described above. Put the groups beneath one service-owned scene root and discover their child spawners during initialization, rather than maintaining per-point reference arrays. Move encounter execution into `EnemyService`; register it once through `CoreScope`. Group enable/disable notifications delegate to the service. Initialization must handle OnEnable occurring before DI; remove the old encounter auto-injection path so groups bind once. Preserve startup policy, disable/re-enable behavior, cancellation, and the next-frame respawn delay. Repeated requests must not create duplicate actors. The cheat reset delegates to the service and resets all its groups. **Verify:** both current groups are discovered once, preserve their settings, and never share pressure slots.

- [x] **3. Connect the existing creation path and migrate assets.** Have the service resolve the definition and construct `EnemySpawnData` from the spawner. The factory retains compatible NavMesh sampling and stages the sampled pose before activation. Pass the definition into `EnemyScopeInstaller` for the three data registrations. Preserve concrete scope prefabs, scoped runtime IDs, activation/build order, and despawn disposal. Migrate the scene and encounter prefabs before removing the old serialized fields; preserve script GUIDs during renames. Copy the old explicit selection into spawner enabled state: DefaultLocation selects only three of the main prefab's four points (seed offsets 0, 2, 1). Disable the excluded offset-3 spawner in that scene override, while preserving the prefab's default selection. ElevatorDemo currently overrides its spawn-point array to zero; disable all four inherited spawners in that scene to preserve its no-spawn behavior. Discovery must exclude disabled spawners. Update existing authoring validation and configuration tests. **Verify:** prefab/profile/moveset/health, pose, patrol, seed, and group membership match the pre-migration values.

## Risks and Rollback

The main risks are losing serialized references, merging unrelated combat groups, and changing initialization or respawn timing. Keep a before/after placement inventory and migrate code plus serialized assets together. Rollback must restore both; do not leave scenes depending on removed fields.

Pre-migration inventory (source and serialized assets, 2026-09-17): the main encounter prefab has four enabled spawners with offsets 0, 1, 2, 3 and one two-point patrol route; all four share the main definition. The training prefab has two enabled spawners with offsets 100 and 101 and distinct profile/moveset pairs but shared health. DefaultLocation's main encounter override selects only offsets 0, 1, 2. ElevatorDemo references the main encounter prefab but overrides its spawn-point array to size zero and disables spawn-on-start; all four prefab children must be disabled in that scene to preserve no-spawn behavior, including grace/game-ended resets. WorkShop has an empty encounter with startup/grace/game-ended spawn policies all disabled. CoreScope in all three scenes serializes an encounter reference. Preserve this inventory during migration.

Do not change the factory's existing exception-handling policy or redesign the recently refactored entity lifetime.

## Validation

For implementation, follow [[Knowledge/Guides/Testing/Unity Test Framework Test Flow]].

- Focused Edit Mode coverage: ID lookup and invalid configuration, two definitions with different health, excluded spawners staying excluded, repeated spawn/respawn requests, group isolation, and cleanup. Reuse existing enemy/scope fixtures and assembly boundaries.
- Extend `EnemyAuthoringValidator` to validate catalog entries, spawner IDs, unambiguous group membership, existing prefab/activation compatibility, and seed collisions. Preserve profile-plus-seed-offset uniqueness across active scene groups; different IDs can share a behaviour profile.
- Run only discovered, bounded Edit Mode selections through the official Unity CLI after clean-scene preflight. Run asynchronously with a caller-enforced time limit and inspect actual executed counts/results.
- After asset migration, import and save through Unity, check serialization/compilation errors, and leave no manual saving for the user.
- Separate bounded Play Mode follow-up: real NavMesh spawning, independent runtime health, patrol, trigger activation, grace/game-ended respawn, group/service disable and re-enable, and scope cleanup. Do not run gameplay tests during normal validation.
- The original planning pass used document/source checks only. Execution established compilation, authoring validation, and focused Edit Mode evidence; the separate Play Mode scenarios above remain unrun.

## Execution Outcome — 2026-09-17

Implemented the catalog, scene service, simple spawners/groups, factory and scope data flow, authoring validation, and catalog/configuration tests. Migrated both encounter prefabs, CoreScope prefab, and DefaultLocation, ElevatorDemo, and WorkShop through Unity. DefaultLocation runs three main and two training spawners; ElevatorDemo retains four inherited spawners disabled; WorkShop remains empty. Unity reloaded the saved assets, the authoring validator logged 0 errors, and the focused Edit Mode fixtures passed 11/11. Runtime NavMesh spawning, health independence, activation, grace/game-ended respawn, and disposal remain a separate Play Mode validation phase. The prior Enemy Factory DI Refactor's gameplay validation remains outstanding.

## Execution Handoff

**Script budget: three new production files** in `Assets/Scripts/Entities/Enemy/`: `EnemyId.cs`, `EnemyCatalog.cs`, and `EnemyService.cs`. Rename and simplify `EnemySpawnPoint.cs` and `EnemyEncounterSystem.cs`; reuse `EnemyFactory.cs`, `EnemySpawnData.cs`, and `EnemyGroupCoordinator.cs`. No new service interface or generic spawning framework.

Other integration points:

- `Assets/Scripts/Services/VContainer/EnemyScopeInstaller.cs` and `CoreScope.cs`.
- `Assets/Scripts/Ui/Cheats/CheatsUiController.cs` — service dependency/reset call only.
- `Assets/Scripts/Editor/EnemyAuthoringValidator.cs`, `Assets/Scripts/Editor/Tests/Configuration/WorldConfigurationTests.cs`, and relevant existing enemy/scope test fixtures.
- `Assets/Prefabs/Models/Enemy/EnemyEncounter.prefab`, `TrainingDummyEncounter.prefab`, `Assets/Prefabs/View/VContainer/CoreScope.prefab`, `Assets/Scenes/DefaultLocation/DefaultLocation.unity`, `Assets/Sandbox/Scenes/WorkShop/WorkShop.unity`, and `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity`. Preserve WorkShop's empty encounter and disabled spawn/respawn policy.
- New shared catalog under `Assets/Settings/Enemy/`; reuse the existing enemy data assets and concrete scope prefab.

Required contexts: `vault-usage`, `plan-workflow`, `work-routing`, and `unity-testing`. Apply the required UI contexts for the cheats-controller reference edit. Use the C# and Unity asset skills for their respective implementation work, with one writer per overlapping scope.

No unresolved blocker to reviewing this design. Keep status `draft` until reviewed; implementation needs explicit execution authorization.
