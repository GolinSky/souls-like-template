---
title: "Character Respawn Dead Loop Pose Fix"
type: plan
domains:
  - animation
  - character
status: done
authority: advisory
updated: "2026-09-21"
aliases:
  - character-respawn-dead-loop-fix
tags:
  - work/plan
  - status/done
  - domain/animation
  - domain/character
---

# Character Respawn Dead Loop Pose Fix

## Plan Contract

### Goal
Fix the issue where the player character respawns at grace stuck in the dead looping animation pose (`DeathIdle`) after dying.

### Source Research and Decisions
1. In `AnimatorComponent.cs`, `OneHandedFreeLocomotionState` was defined as `Animator.StringToHash("OneHandedLayer.FreeLocomotion")`.
2. In `CharacterGreatSwordAnimator.controller`, `FreeLocomotion` is inside the sub-state machine `Locomotion`.
3. Per `SoulsLikeGameVault/Knowledge/Guides/Animation/Animator Sub-State Machine Guide.md` (Rule 4: Runtime CrossFade Resolution Compatibility):
   > "Unity expects either the full path (`"Base Layer.Attack.LightAttack1"`) or the short state name (`"LightAttack1"`). Always trigger states by their short name or short name hash... Never concatenate layer prefixes (`"Base Layer." + stateName`) when targeting states inside sub-state machines."
4. Live Unity `animator.HasState()` evaluation confirmed that `animator.HasState(layerIndex, Animator.StringToHash("OneHandedLayer.FreeLocomotion"))` evaluates to `false`, while `animator.HasState(layerIndex, Animator.StringToHash("FreeLocomotion"))` evaluates to `true`.
5. Because `OneHandedLayer.FreeLocomotion` did not resolve to a valid state, `animator.Play(OneHandedFreeLocomotionState, ...)` in `CompleteDeathAnimation()` failed silently, leaving the animator indefinitely in `DeathIdle`.
6. Live evaluation and test execution confirmed that when `animator.Play(Animator.StringToHash("FreeLocomotion"), layerIndex, 0f)` is called on `OneHandedLayer`, both `OneHandedLayer` and its synchronized layer `TwoHandedLayer` correctly exit `DeathIdle` and enter `FreeLocomotion`.

### Assumptions and Non-Goals
- Non-goals: Modifying Animator Controller asset geometry or adding new states to `CharacterGreatSwordAnimator.controller`. The controller structure is correct; the bug was purely in runtime state hash resolution and lifecycle cleanup.
- Assumptions: Player death and respawn orchestration (`CoreGameOrchestrator.RespawnAtLastGrace`) calls `_character.CompleteDeathAnimation()`.

### Success Criteria
- [x] Character reliably transitions out of `DeathIdle` into `FreeLocomotion` upon respawn in both `OneHanded` and `TwoHanded` modes.
- [x] Lingering `Death` trigger is reset upon death completion.
- [x] Character combat defense and action state machine are clean upon respawn.
- [x] EditMode tests in `DeathAnimationTests.cs` pass with 0 failures.

## Execution Plan

- [x] Phase 1 — C# runtime fix in `AnimatorComponent.cs` & `Character.cs`; verify: compilation and state hash resolution.
- [x] Phase 2 — EditMode unit test authoring in `DeathAnimationTests.cs`; verify: running UTF test runner via Unity CLI bridge.
- [x] Phase 3 — Full validation and test pass; verify: 0 test failures, 0 console errors.

## Risks and Rollback
- Risk: Low. Changing state hash from invalid `OneHandedLayer.FreeLocomotion` to canonical `FreeLocomotion` aligns with existing pattern used by `GraceRestIdleState` and `GraceAnimationTests`.
- Rollback: Revert `AnimatorComponent.cs`, `Character.cs`, and delete `DeathAnimationTests.cs`.

## Validation
- Executed `DeathAnimationTests` (3 passed), `GraceAnimationTests` (2 passed), `AnimatorCallbackContractTests` (27 passed), and `CharacterArrivalAnimationContractTests` (1 passed) in EditMode.
- 0 console errors, 0 compilation errors.

## Execution Handoff
- Files:
  - `Assets/Scripts/Components/Animator/AnimatorComponent.cs`
  - `Assets/Scripts/Entities/Character/Character.cs`
  - `Assets/Scripts/Editor/Tests/Animation/DeathAnimationTests.cs`
- Required context: `animation-code`.
