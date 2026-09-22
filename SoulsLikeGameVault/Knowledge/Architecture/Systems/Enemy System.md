---
title: Enemy System
type: architecture
domains:
  - enemy
  - spawning
status: current
authority: advisory
verified: 2026-09-17
tags:
  - knowledge/architecture
---
# Enemy System

## Ownership

| Part | Current responsibility |
|---|---|
| `EnemyCatalog` | One shared ScriptableObject maps explicit `EnemyId` values to an enemy prefab, behaviour profile, moveset, and health data. It validates serialized entries before building a cached lookup. |
| `EnemySpawner` | Scene placement: ID, transform pose, patrol transforms, and random seed offset. It has no enemy-definition references. |
| `EnemySpawnGroup` | Authored startup/respawn and pressure settings. Enabled child spawners belong to the group. |
| `EnemyService` | One scene root per gameplay `CoreScope`. It discovers groups before registering for game-state notifications, owns each group's actors, next-frame respawn, subscriptions, cleanup, and separate `EnemyGroupCoordinator`. |
| `EnemyFactory` and `EnemyScopeInstaller` | The existing concrete scope prefab creation path. The factory samples NavMesh and stages pose before activation; the installer registers resolved definition data per instance. |

`CoreScope` registers its scene `EnemyService` once. The cheat reset calls the service so every active group is reset.

## Authoring and Runtime Flow

1. Add a stable nonzero `EnemyId` and a complete catalog entry.
2. Place an `EnemySpawner` under exactly one `EnemySpawnGroup`, below the scene's `EnemyService` root. Assign patrol points and seed offset on the spawner.
3. On service initialization, bind groups and game-state observation. Start policy runs after the service's Unity `Start`; grace/game-ended notifications and reset requests schedule next-frame respawn.
4. The service resolves the ID, then passes the definition and placement to the factory. The factory keeps one child scope per actor and the existing NavMesh sampling, staging, activation, and build order.
5. Each group has its own pressure coordinator. Disabled spawners are excluded, and nested groups do not claim one another's spawners.

An invalid catalog, unknown ID, or missing required definition fails visibly. The authoring validator checks catalog entries, prefab compatibility, group/service ownership, and profile-plus-seed uniqueness across active scene groups.

## Migrated Scene Selections

| Scene | Group selection |
|---|---|
| DefaultLocation | Three main enemy spawners (offsets 0, 1, 2); offset 3 disabled only in this scene. Two training variants remain enabled in a separate group. |
| ElevatorDemo | Main encounter retains four inherited prefab spawners, all disabled in this scene. Startup spawning remains off. |
| WorkShop | Empty encounter; startup, grace, and game-ended spawning remain off. |

The main encounter prefab itself retains four enabled spawners. The shared catalog resides at `Assets/Settings/Enemy/EnemyCatalog.asset`.

## Validation Boundary

On 2026-09-17 the catalog and world configuration Edit Mode fixtures passed 11/11, and the authoring validator reported 0 errors. Real NavMesh spawning, runtime health independence, patrol, trigger activation, grace/game-ended respawn, disable/re-enable, and scope disposal remain Play Mode follow-up validation.

See [[History/Completed Plans/Enemy System Scalability]] and [[History/Records/Enemy System Scalability Implementation]] for the execution scope and evidence.
