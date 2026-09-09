---
title: Character Owned Animation Sequencing and Recovery
type: plan
domains: [character, animation, movement, lifecycle]
status: done
authority: advisory
updated: 2026-09-09
source_commit: 7e7b1ebe4894c36df12c22666f00b7e7b8c93fc5
aliases: []
tags: [work/plan, status/done]
---

# Character Owned Animation Sequencing and Recovery

## Plan Contract

### Goal

Make Character choose exactly one presentation for character arrival without changing save, resume, or travel rules. Remove the current Spawn-versus-Grace command conflict at its source instead of resetting one animation command from another.

Keep the existing Animator state-machine event transport, action state machine, movement locks, progress thresholds, and domain owners. Do not introduce a generic animation workflow, playback token hierarchy, or runtime timeout system without a reproduced defect that requires one.

### Approved Feature Rules

1. A fresh/default or saved world-position arrival plays Spawn. Spawn Exit releases the existing startup restriction and the character becomes active.
2. A saved grace arrival or travel to any grace enters GraceRestIdle directly. Spawn is never requested. Grace protection remains until the existing grace-exit flow completes.
3. Core owns GameState and resolves the saved arrival intent. Character owns the animation command and its existing input/movement protection.
4. Grace, spawning, and enemy-system reload do not pause the game clock or time scale.

### Verified Current Conflict

1. `Character.Initialize` currently blocks gameplay and always calls `AnimatorComponent.TriggerSpawn`.
2. `CoreGameOrchestrator.Start` separately checks `_startsOnGrace` and calls `Character.EnterGraceRestIdle`.
3. `AnimatorComponent.EnterGraceRestIdle` resets the pending Spawn trigger and plays GraceRestIdle. This suppresses the symptom after both lifecycle paths have already issued competing commands.
4. The current Animator test proves direct state playback for the representative controller and both hand modes. It does not prove Character lock ownership or the complete arrival decision.
5. `TwoHandedLayer` is synchronized to `OneHandedLayer` in `CharacterGreatSwordAnimator.controller`. The current source-layer playback is controller-specific but verified for that controller; do not replace it speculatively.

### Non-Goals

- No Spawn-then-Grace sequence.
- No general save migration. One backward-compatible `ResumesAtGrace` flag records the feature's existing grace-versus-world resume intent.
- No new async arrival workflow or typed terminal-result hierarchy.
- No runtime start/finish watchdogs, synthetic animation events, occupancy reconciliation, controller epochs, or universal playback bindings.
- No same-state timing change, duplicated Animator states, or new replay delay.
- No root-motion/layer-policy change without a reproduced failure.
- No Animator Controller, prefab, CharacterData, CharacterFactory, PlayerController, combat, equipment, or inventory mutation unless focused validation proves it necessary.

### Success Criteria

1. Each arrival issues exactly one presentation command from Character.
2. World-position arrival issues Spawn and unlocks only through the existing Spawn Exit handling.
3. Grace arrival issues no Spawn and no Spawn reset, enters GraceRestIdle, and stays protected until the existing grace-exit flow.
4. Core no longer directly chooses an Animator command after Character has already selected another presentation.
5. Both arrival paths work for the active hand modes supported by the referenced controller.
6. Invalid required Animator setup is caught by focused Edit Mode validation and reported clearly; runtime code does not manufacture success.
7. Existing save, travel, grace, action-buffer, progress, movement, and game-state behavior remains unchanged outside this arrival boundary.
8. Saved grace rest persists across returning to the menu; saving a normal world position clears that intent.

## Execution Plan

### Phase 1 — One Character-Owned Arrival Decision

Owned production scope: `Character.Initialize`, one small Character arrival entry point, `CoreGameOrchestrator.Start/QuitGame`, `AnimatorComponent.EnterGraceRestIdle`, and the minimal saved arrival intent in `CharacterSpawnData` / `CharacterSpawnService`.

- [x] Remove unconditional `TriggerSpawn` from `Character.Initialize`. Initialization still prepares components and blocks gameplay.
- [x] Add one explicit Character arrival choice with two values: world position and grace rest.
- [x] World-position arrival calls the existing `TriggerSpawn` path.
- [x] Grace arrival enters the existing RestIdle phase/protection and calls the existing GraceRestIdle adapter. It never creates or cancels a Spawn request.
- [x] Make `CoreGameOrchestrator.Start` pass the resolved arrival choice once. Core continues to own `GameState` and preserves the current direct-grace feature outcome.
- [x] Remove `ResetTrigger(Spawn)` from `AnimatorComponent.EnterGraceRestIdle`.
- [x] Keep the verified synchronized-layer route; controller validation found no asset change was needed.
- [x] Persist grace-versus-world resume intent with a backward-compatible boolean. Preserve it when quitting from `OnGraceSit`; clear it when saving a normal position.
- [x] Add focused Edit Mode tests for world-position and grace arrivals, active hand modes, command exclusivity, arrival-owned lock behavior, and resume persistence.
- [x] Validate the required GraceRestIdle/Spawn states, layers, Spawn callback, and authored transition out in the referenced controller. Invalid setup fails the test clearly.

Verify: saved/default position -> Spawn -> Active; saved grace/travel -> direct GraceRestIdle -> OnGraceSit; no competing trigger; no premature unlock; existing grace exit restores normal control.

### Deferred — Action Exit Correlation

`CharacterActionStateMachine` currently counts exits to ignore when broad action categories chain. If an old Exit is missing, a newer Exit can consume that count. This is separate from Phase 1.

When a failing gameplay case is established, prefer matching the concrete expected animation state/hash for different-state chains. Do not change identical-state replay timing, duplicate authored states, or add playback generations without evidence that the existing content requires them.

### Deferred — Layer and Root-Motion Filtering

`AnimatorRootMotionRelay` currently observes tags on every current/next layer, including layers whose weight may be zero. This is not part of the arrival defect.

First validate active controllers and synchronized layers. If an inactive-layer defect is reproduced, apply the smallest active-layer/weight filter that preserves valid hand-mode, reaction, traversal, and blend behavior. Do not add an operation-token layer system speculatively.

### Animator Callback Failure Policy

Missing Enter, Progress, QueueCheck, or Exit callbacks are authoring or architecture defects.

- Validate required states, layers, behaviours, semantic mappings, progress markers, and exit routes in Edit Mode.
- Fail clearly when invalid setup can be detected.
- Use existing cancellation for explicit caller cancellation or teardown and clean only the cancelling operation's existing state.
- Never synthesize Progress/Exit, silently unlock, grant an effect, or report animation success.
- Add runtime recovery only for a separately reproduced failure whose correct domain outcome is defined.

## Risks and Rollback

- Calling arrival before Character initialization would be an ordering defect. Verify the existing VContainer child-scope lifecycle; change `CharacterFactory` only if the fixture proves the current ordering insufficient.
- Grace playback currently targets the synchronized controller's source layer. Keep the verified adapter behavior until another referenced controller demonstrates a different requirement.
- Removing the Spawn reset is safe only after the grace path is proven not to request Spawn. Test command exclusivity directly.
- Roll back this bounded arrival slice without restoring two independent presentation owners.

## Validation

- Static diff inspection for one presentation owner and no duplicate startup path.
- Focused Edit Mode Character arrival tests plus static Core lifecycle review.
- Existing `GraceAnimationTests` for both hand modes, updated to test direct GraceRestIdle rather than suppression of a pending Spawn trigger.
- Focused Animator contract validation for required states/layers/behaviours and callback paths.
- Unity Test Safety preflight before asynchronous Edit Mode tests. No Play Mode tests during normal validation.

Completed evidence: `assert_test_ready` returned `TEST_READY` with `ElevatorDemo` clean. The four focused Edit Mode fixtures completed asynchronously with 8 passed, 0 failed, 0 skipped. Both runtime and Editor C# assemblies built with 0 errors. Independent review found no material runtime defects; full Play Mode travel remains intentionally outside normal validation.

## Execution Handoff

One `csharp_worker` owns all overlapping production and test changes in Character, CoreGameOrchestrator, AnimatorComponent, and focused arrival/Animator tests using `$soulslike-csharp-change`, `$soulslike-context` keys `animation-code` and `character-architecture`, and `$soulslike-animation-workflow`.

After implementation, `unity_reviewer` and `unity_test_runner` independently review and validate the same bounded arrival slice. Animator assets are changed only if the tests prove a content defect; any asset mutation requires Unity persistence and serialization verification.
