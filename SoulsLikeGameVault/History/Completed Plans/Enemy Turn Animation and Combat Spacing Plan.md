---
title: "Enemy Turn Animation and Combat Spacing Plan"
type: plan
domains:
  - enemy
  - animation
status: done
authority: advisory
updated: "2026-09-20"
aliases:
  - Enemy_Turn_Animation_and_Combat_Spacing_Plan
tags:
  - work/plan
  - status/done
---

# Enemy Turn Animation and Combat Spacing Plan

## Plan Contract

### Goal
Eliminate instantaneous orientation snapping (`FaceImmediately`), integrate in-place turn animations for enemies (`DoubleL` FBX `Turn_B` clips), and replace frantic backward sprinting (`Retreat`) with controlled combat walking backward and strafing (`WalkBack`, `CircleLeft`, `CircleRight`) at dedicated combat walk speeds with state commitment.

### Source Research and Decisions
- `EnemyController.DecideCombat` previously called `FaceImmediately(combatTarget)` on attack start, snapping rotation by up to 180° in 0 seconds. This violated Souls-like telegraphing and positioning rules.
- `ErikaGreatSwordEnemy.controller` locomotion was driven by a 2D blend tree (`MoveX`, `MoveY`) with no turn states and no dampening.
- `Assets/ThirdParty/DoubleL/FBX_Animations/One Hand Up/Movement/Idle/Turn_B/InPlace/` contains matching in-place turn animations: `L45`, `R45`, `L90`, `R90`, `180`.
- In `DecideCombat`, `distance < PreferredRangeMin` previously triggered `Retreat` which, because `NavMeshAgent.speed` was 3.5 m/s, played backward running (`Run_A_B_InPlace`) instead of walking.
- Circling and retreat previously flapped directions rapidly every 0.18s due to uncommitted random selection in `ResolveCombatMovement`.

### Assumptions and Non-Goals
- Existing attack actions and movesets remain unchanged; only pre-attack facing requirements and orientation alignment were introduced.
- Non-combat leashing and returning home continue to use sprint speed.
- Player character turning is unaffected.

### Success Criteria
1. Enemy aligns toward player with turn animation before attacking when angle > facing tolerance (20°); `FaceImmediately` is eliminated.
2. In-place turn animations (`TurnL45`, `TurnR45`, `TurnL90`, `TurnR90`, `Turn180`) play when the stationary enemy turns to track or align.
3. Close-range retreat walks backwards (`Walk_A_B_InPlace`) at walk speed (1.3 m/s) with locked facing towards the player, not running away.
4. Combat strafing and spacing commit to direction for 1.2s (`spacingCommitmentSeconds`), eliminating rapid 0.18s dragging/glitching.

## Execution Plan

- [x] Phase 1 — C# Domain Logic & State Machine: Added `WalkBack` to `EnemyCombatMovement`, added speed control (`SetSpeed` / `ResetSpeed`) to `EnemyNavigationMotor`, added pacing duration commitment (`spacingCommitmentSeconds`) and pre-attack facing alignment to `EnemyController`, and added dampening (0.12s) to `EnemyActionExecutor.SetLocomotion`.
- [x] Phase 2 — Animator Controller & Clips: Added turn states (`TurnL45`, `TurnR45`, `TurnL90`, `TurnR90`, `Turn180`) to `ErikaGreatSwordEnemy.controller` adhering to `SoulsLikeGameVault/Knowledge/Guides/Animation/Animator Sub-State Machine Guide.md` and persisted via Unity AssetDatabase.
- [x] Phase 3 — ScriptableObject & Prefab Tuning: Updated `ErikaMeleeBehaviour.asset` with new speed and commitment fields (`combatWalkSpeed = 1.3`, `combatRunSpeed = 3.5`, `spacingCommitmentSeconds = 1.2`, `attackFacingAngle = 20`, `turnInPlaceAngleThreshold = 30`).
- [x] Phase 4 — Validation & UTF Tests: Updated `EnemyActionSelectorTests.cs`, `EnemyMultiHitActionTests.cs`, `EnemyCriticalLifecycleTests.cs`, and `EnemyTimingTests.cs`, and executed EditMode UTF tests via Unity CLI (81 passed, 0 failed).

## Risks and Rollback
- Risk: Too high an attack facing requirement might make the enemy struggle to attack moving players. Mitigation: Set `ATTACK_FACING_TOLERANCE` to 20° and retained dynamic tracking window during windup.
- Rollback: Revert changes to `EnemyController.cs`, `EnemyCombatMovement.cs`, `ErikaGreatSwordEnemy.controller`, and `ErikaMeleeBehaviour.asset`.

## Validation
- Unity UTF Tests: All 81 EditMode tests under `Enemy*` passed cleanly (`EnemyActionSelectorTests`, `EnemyMultiHitActionTests`, `EnemyCriticalLifecycleTests`, `EnemyTimingTests`, `EnemyExecutionModeTests`, etc.).
- Unity AssetDatabase: Persisted and reserialized `ErikaGreatSwordEnemy.controller` and `ErikaMeleeBehaviour.asset`.
- Unity Console: 0 compilation errors, 0 asset import/serialization warnings or errors.

## Execution Handoff
- Target files:
  - [`Assets/Scripts/Entities/Enemy/EnemyCombatMovement.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyCombatMovement.cs)
  - [`Assets/Scripts/Entities/Enemy/EnemyExecutionMode.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyExecutionMode.cs)
  - [`Assets/Scripts/Entities/Enemy/EnemyBehaviourProfile.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyBehaviourProfile.cs)
  - [`Assets/Scripts/Entities/Enemy/EnemyNavigationMotor.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyNavigationMotor.cs)
  - [`Assets/Scripts/Entities/Enemy/EnemyActionExecutor.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyActionExecutor.cs)
  - [`Assets/Scripts/Entities/Enemy/EnemyController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyController.cs)
  - [`Assets/Art/Animation/Enemy/ErikaGreatSwordEnemy.controller`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Animation/Enemy/ErikaGreatSwordEnemy.controller)
  - [`Assets/Settings/Enemy/ErikaMeleeBehaviour.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Enemy/ErikaMeleeBehaviour.asset)
  - [`Assets/Scripts/Tests/EnemyRuntime/EnemyActionSelectorTests.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Tests/EnemyRuntime/EnemyActionSelectorTests.cs)
  - [`Assets/Scripts/Tests/EnemyRuntime/EnemyMultiHitActionTests.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Tests/EnemyRuntime/EnemyMultiHitActionTests.cs)
  - [`Assets/Scripts/Tests/EnemyRuntime/EnemyTimingTests.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Tests/EnemyRuntime/EnemyTimingTests.cs)
- Status: Completed and verified.
