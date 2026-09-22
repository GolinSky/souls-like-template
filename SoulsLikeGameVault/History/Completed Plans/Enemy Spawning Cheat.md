---
title: "Enemy Spawning Cheat"
type: plan
domains:
  - cheats
  - enemy
  - ui
status: completed
authority: historical
updated: "2026-09-20"
aliases:
  - Cheat Panel Enemy Spawning
tags:
  - work/plan
  - status/completed
---

# Enemy Spawning Cheat

## Plan Contract

### Goal
Locate the cheat panel scripts and add a cheat function to spawn an enemy in front of the player, supporting an enemy type/ID parameter in both the Cheats IMGUI view and the controller/presenter API.

### Source Research and Decisions
- Cheat UI and Presenter components reside in `Assets/Scripts/Ui/Cheats/`:
  - `CheatsUi.cs`: IMGUI-based window panel with "Player" and "Enemies" tabs.
  - `ICheatsPresenter.cs`: Interface consumed by `CheatsUi`.
  - `CheatsUiController.cs`: Controller implementing `ICheatsPresenter`, registered in `CoreScope`.
- Key binding: F12 via `ToggleCheatsAction` in `InputService.cs`.
- Enemy creation pipeline:
  - `EnemyFactory.cs`: Samples NavMesh and instantiates enemy with dedicated `EnemyScopeInstaller`.
  - `EnemyService.cs`: Coordinates scene enemy groups and catalogues.
  - `EnemyCatalog.cs` / `EnemyCatalog.asset`: Maps `EnemyId` (`ErikaMelee`, `BackstabDummy`, `RiposteDummy`) to prefab, behavior, moveset, and health data.
  - `TargetingCommand.cs`: Registered on player entity, provides player root position and forward direction via `TargetingSnapshot`.

### Assumptions and Non-Goals
- Spawning takes place on the nearest valid NavMesh position ~3m ahead of the player, facing the player.
- Cheat-spawned enemies are assigned a dedicated `EnemyGroupCoordinator` so their AI pressure slots and alerts tick properly.
- Cheat-spawned enemies are cleaned up upon Grace rest or cheat "Respawn Enemies".

### Success Criteria
- Cheats panel "Enemies" tab has an enemy type/ID selector and a "Spawn Enemy In Front" button.
- `ICheatsPresenter` and `CheatsUiController` expose `SpawnEnemy(EnemyId)` as well as string/int overloads.
- EditMode tests verify ID resolution, offset calculations, and parameter parsing.

## Execution Plan

- [x] Phase 1 — Enemy Spawning Pipeline:
  - Add position-based `CreateEnemy` overload to `EnemyFactory.cs`.
  - Add `SpawnEnemy(EnemyId, Vector3, Quaternion)` to `EnemyService.cs` with dynamic coordinator and cleanup.
- [x] Phase 2 — Cheats Presenter & Controller:
  - Update `ICheatsPresenter.cs` with `AvailableEnemyIds` and `SpawnEnemy(EnemyId)`.
  - Implement methods in `CheatsUiController.cs` calculating spawn transform from player's `TargetingCommand`.
- [x] Phase 3 — Cheats UI View:
  - Update `CheatsUi.cs` with selector and spawn button, resizing window height to 280f.
- [x] Phase 4 — Validation:
  - Add EditMode unit tests in `CheatsEnemySpawnTests.cs`.
  - Verify with UTF CLI (9/9 passed in `CheatsEnemySpawnTests`, 3/3 passed in `EnemyCatalogTests`).

## Validation Evidence
- EditMode test fixture `SoulsLike.Editor.Tests.Ui.CheatsEnemySpawnTests` (9 passed, 0 failed).
- Regression test fixture `SoulsLike.Editor.Tests.Configuration.EnemyCatalogTests` (3 passed, 0 failed).
- Unity console: 0 errors, 0 warnings.
- Open scenes: all 10 open scenes remain clean (`isDirty=false`).
