---
title: "Enemy Spawning Cheat Implementation"
type: implementation-record
domains:
  - cheats
  - enemy
  - ui
status: done
authority: historical
updated: "2026-09-20"
aliases: []
tags:
  - history/change
---

# Enemy Spawning Cheat Implementation

## Implementation Record Contract

### Outcome
Implemented a cheat to spawn any configured enemy in front of the player (facing the player) on the nearest valid NavMesh position. Added enemy type/ID parameter selection to the Cheats IMGUI panel and programmatic overloads in `CheatsUiController`.

### Why
Requested by developer to easily spawn and test specific enemy variants in front of the player during runtime gameplay.

### Changed Files and Assets
- [`Assets/Scripts/Entities/Enemy/EnemyFactory.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyFactory.cs): Added position/rotation-based `CreateEnemy` overload.
- [`Assets/Scripts/Entities/Enemy/EnemyService.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyService.cs): Added dynamic enemy spawning (`SpawnEnemy`), dynamic group coordinator, and despawn cleanup on Grace rest / enemy respawn.
- [`Assets/Scripts/Ui/Cheats/ICheatsPresenter.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Cheats/ICheatsPresenter.cs): Added `AvailableEnemyIds` and `SpawnEnemy(EnemyId)`.
- [`Assets/Scripts/Ui/Cheats/CheatsUiController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Cheats/CheatsUiController.cs): Implemented `AvailableEnemyIds`, `SpawnEnemy(EnemyId)` using player `TargetingCommand`, and `SpawnEnemy(string)` / `SpawnEnemy(int)` overloads.
- [`Assets/Scripts/Ui/Cheats/CheatsUi.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Cheats/CheatsUi.cs): Resized window height to 280f, added `< [Enemy Type] >` cycling control and "Spawn Enemy In Front" button in "Enemies" tab.
- [`Assets/Scripts/Editor/Tests/Ui/CheatsEnemySpawnTests.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/Tests/Ui/CheatsEnemySpawnTests.cs): Added EditMode unit tests.

### Decisions and Tradeoffs
- Spawn position calculation uses player `TargetingCommand.Read()`, offsetting 3 meters along the horizontal heading `forward` and facing the enemy towards `-forward` (back at the player).
- `EnemyFactory` handles NavMesh sampling to ensure dynamic spawns snap onto valid walkable geometry.
- Dynamically spawned enemies share a dynamic coordinator so their AI state ticks and pressure slots expire cleanly without requiring an authored `EnemySpawnGroup` in the scene.

### Validation Evidence
- EditMode tests in `SoulsLike.Editor.Tests.Ui.CheatsEnemySpawnTests`: 9/9 passed.
- Regression tests in `SoulsLike.Editor.Tests.Configuration.EnemyCatalogTests`: 3/3 passed.
- Unity Console: 0 errors, 0 warnings.
