---
title: Character Owned Animation Sequencing and Recovery
type: plan
domains: [character, animation, movement, lifecycle]
status: done
authority: advisory
updated: 2026-09-09
source_commit: 2385b53122a52196675acb7e73b3c6a7f99d3468
aliases: []
tags: [work/plan, status/done]
---

# Character Owned Animation Sequencing and Recovery

## Plan Contract

### Goal

Phase 1 made Character choose exactly one presentation for character arrival without changing save, resume, or travel rules. The remaining work validates the Animator callback contract and changes runtime routing only when valid controller behavior proves a defect.

Keep the existing Animator state-machine event transport, action state machine, movement locks, progress thresholds, and domain owners. Do not introduce a generic animation workflow, playback token hierarchy, or runtime timeout system without a reproduced defect that requires one.

### Approved Feature Rules

1. A fresh/default or saved world-position arrival plays Spawn. Spawn Exit releases the existing startup restriction and the character becomes active.
2. A saved grace arrival or travel to any grace enters GraceRestIdle directly. Spawn is never requested. Grace protection remains until the existing grace-exit flow completes.
3. Core owns GameState and resolves the saved arrival intent. Character owns the animation command and its existing input/movement protection.
4. Grace, spawning, and enemy-system reload do not pause the game clock or time scale.
5. Primary weapons can be equipped only in the right hand. A left-hand-only primary-weapon loadout is unsupported; left-hand slots remain for supported off-hand item types such as shields.

### Verified Phase 1 Conflict — Resolved

1. Before Phase 1, `Character.Initialize` blocked gameplay and always called `AnimatorComponent.TriggerSpawn`.
2. `CoreGameOrchestrator.Start` then separately checked `_startsOnGrace` and called `Character.EnterGraceRestIdle`.
3. `AnimatorComponent.EnterGraceRestIdle` reset the pending Spawn trigger and played GraceRestIdle. That suppressed the symptom after both lifecycle paths had already issued competing commands.
4. Phase 1 replaced those competing commands with `Character.BeginArrival`, removed the Spawn reset, and added Character lock/arrival coverage.
5. `TwoHandedLayer` is synchronized to `OneHandedLayer` in `CharacterGreatSwordAnimator.controller`. The current source-layer playback is controller-specific but verified for that controller; do not replace it speculatively.

### Verified Later-Phase Comparison

1. `AnimatorStateMachine.OnStateUpdate` skips Progress and QueueCheck while its layer is transitioning. Required marker timing is therefore an Animator content contract, not something runtime should synthesize.
2. `AnimatorStateMachine` currently uses null-conditional receiver calls. An initialization failure can silently discard Enter, Progress, QueueCheck, or Exit instead of exposing the broken setup.
3. `CharacterActionStateMachine` uses `_pendingExitsToIgnore` for chained actions in the same broad category. It is correct for the currently tested callback order, but a missing older Exit can cause the current Exit to be ignored.
4. `AnimatorStateMachineDto` already carries `StateInfo` and `LayerIndex`. A playback identity/token abstraction is not justified by current evidence.
5. `AnimatorRootMotionRelay` checks current and next tags on every layer, including zero-weight layers. This is a static risk; no gameplay failure has been reproduced.
6. The receiver DTO is a struct delivered synchronously to one production observer. Its reuse is not a demonstrated defect.
7. Existing movement-lock bit ownership is already unified. Do not replace it as part of animation sequencing.
8. The default Character prefab uses `CharacterNoWeaponAnimator.overrideController`, whose base is `CharacterGreatSwordAnimator.controller`. The right-hand straight-sword profile also uses that controller.
9. `StraightSwordAnimationProfile.LeftHandController` points to missing GUID `006fe97f228a8b14389be20ea1c364cd`, and `InventoryEquipmentBootstrap` expects the absent `CharacterGreatSwordLeftHandAnimator.controller`. These are stale remnants of an unsupported left-hand-primary route, not a controller that should be restored.
10. `PauseNavigationUiController` still includes `ItemType.Weapon` for left-hand slots, while `EquipmentSlotCatalog` reduces both hand groups to broad `EquipmentGroup.Armament`. The current code therefore does not fully enforce the approved right-hand-only primary-weapon rule.

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

### Phase 2 — Callback and Controller Contracts

This phase is unconditional and produces evidence before any later production change.

- [x] Resolve the left-hand-only straight-sword decision: primary weapons are right-hand-only. Do not restore or substitute a left-hand primary controller.
- [x] Remove the stale left-hand-primary animation-profile/bootstrap route and verify that equipment assignment rejects primary weapons in left-hand slots before Animator profile selection. Keep this enforcement surgical and reuse the existing equipment compatibility path.
- [x] Validate the supported player controller routes: the Character default override, its `CharacterGreatSwordAnimator.controller` base, and right-hand controllers referenced by `AnimationProfile` assets.
- [x] Validate the required StateMachineName, Enter, Exit, QueueCheck, Progress flag/threshold, and authored transition path for lifecycle, attack, roll, equipment swap, item use, block-hit, parry, and critical states that drive gameplay.
- [x] Add one focused sequence fixture that records `(StateMachineName, event, shortNameHash, LayerIndex)` for different-state chaining, identical-state replay, hand-mode blends, and action-layer blends.
- [x] Make callback delivery fail visibly when an `AnimatorStateMachine` was not initialized. Use the required receiver directly instead of silently discarding callbacks through null-conditional calls.
- [x] Record whether valid controller execution actually emits duplicate, stale, wrong-layer, or inactive-layer callbacks/tags. No gameplay-impacting duplicate, stale, wrong-layer, or inactive-layer defect was reproduced; synchronized and action-source layer callbacks were confirmed as intentional controller contracts.

Verify: every supported controller route satisfies its gameplay callback contract; invalid setup fails clearly; the trace states whether later runtime correlation or filtering is necessary.

### Phase 3 — Repair Proven Content Defects

This phase is conditional on Phase 2 findings.

- [x] Fix only controller/profile defects reported by the contract validation. Four directional hit states were migrated from stale enum value 21 to `HitReaction` value 22; the obsolete left-hand-controller profile field was removed.
- [x] If a required Progress or QueueCheck marker is unreachable before an authored transition, correct that state's marker or transition without changing the intended gameplay timing. Validation proved the authored markers reachable, so no timing change was made.
- [x] Persist controller/profile changes through Unity, re-run import/serialization checks, and re-run the focused contract and sequence fixtures.
- [x] Do not manufacture Progress, QueueCheck, Enter, or Exit in runtime code.

Verify: supported assets satisfy the same contract without runtime fallback behavior.

### Phase 4 — Narrow Runtime Correlation or Filtering

This phase executes only when Phase 2 shows a defect with otherwise valid controller content. If the trace is correct, close this phase with no production change.

- [x] Keep the current same-state replay contract: each chained entry adds one older Exit to ignore, the broad action state remains active, and the final authored Exit completes it. The trace confirmed `Enter -> QueueCheck -> old Exit -> Enter -> QueueCheck -> final Exit`.
- [x] For legitimate stale or out-of-order different-state exits, match the current action to the concrete state hash already present in `AnimatorStateMachineDto.StateInfo`. No legitimate stale or out-of-order Exit was reproduced, so no production change was made.
- [x] Preserve current identical-state replay timing and its authored callback order. Identical hashes do not justify generations, delays, or duplicate Animator states.
- [x] If synchronized/action layers duplicate gameplay callbacks, filter once at the existing Animator-to-Character routing boundary using the authoritative layer proven by the fixture. No duplicate callback was reproduced, so no filter was added.
- [x] If zero-weight tagged layers actually interfere with movement/root motion, ignore only those inactive layers in `AnimatorRootMotionRelay` while preserving hand-mode blends and traversal. No root-motion interference was reproduced, so the relay was unchanged.
- [x] Add focused regression coverage for each reproduced defect and no broader cases. No Phase 4 runtime defect was reproduced; the Phase 2 trace is the regression coverage for the retained contract.

Verify: the reproduced valid-content defect is removed; current chaining, same-state replay, progress timing, hand modes, root motion, and traversal remain unchanged.

### Animator Callback Failure Policy

Missing Enter, Progress, QueueCheck, or Exit callbacks are authoring or architecture defects.

- Validate required states, layers, behaviours, semantic mappings, progress markers, and exit routes in Edit Mode.
- Fail clearly when invalid setup can be detected.
- Use existing cancellation for explicit caller cancellation or teardown and clean only the cancelling operation's existing state.
- Never synthesize Progress/Exit, silently unlock, grant an effect, or report animation success.
- Fix the missing callback at its Animator setup or routing source; do not recover by completing gameplay at a different boundary.

For this plan, no missing-callback gameplay recovery is defined. A missing required callback remains a visible setup/architecture failure until its source is fixed.

### Separate Work

`Locked Roll State Is Cleared Before Root Motion` is a real locomotion defect in the current source, but it is not an animation callback sequencing phase. Keep its validation and repair in a separate task. Fade cancellation and scene/spawn transaction issues are also outside this plan.

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

Completed Phase 1 evidence: `assert_test_ready` returned `TEST_READY` with `ElevatorDemo` clean. The four focused Edit Mode fixtures completed asynchronously with 8 passed, 0 failed, 0 skipped. Both runtime and Editor C# assemblies built with 0 errors. Independent review found no material runtime defects; full Play Mode travel remains intentionally outside normal validation.

Phases 2–4 evidence: Unity `assert_test_ready` returned `TEST_READY` with `ElevatorDemo` clean. The focused callback trace passed 1/1, the broad Edit Mode Animation suite passed 43/43, and `CharacterActionStateMachineTests` passed 14/14. The trace covered authored lifecycle, action chaining/replay, hand-mode, and action-layer routes and reproduced no duplicate, stale, wrong-layer, or gameplay-impacting inactive-layer callback defect. Controller/profile assets were saved and force-reserialized through Unity with no import or serialization errors. Play Mode remained outside normal validation.

## Execution Handoff

Phases 1–4 are complete. See [[History/Records/Character Owned Animation Sequencing and Recovery Phases 2-4|the implementation record]] for the executed later-phase scope and validation evidence.
