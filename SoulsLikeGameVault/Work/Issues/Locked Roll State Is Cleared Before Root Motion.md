---
title: Locked Roll State Is Cleared Before Root Motion
type: issue
domains:
  - movement
  - combat
status: open
authority: evidence
priority: high
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: code defect
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Locked Roll State Is Cleared Before Root Motion

## Issue Contract

### Observed Behavior

Starting a roll from unlocked movement records its target and direction, then the same Character.Tick calls SetMovementBlocked(false), which clears both values before the Animator root-motion callback consumes them. A locked lateral roll consequently loses the special orbit/target-facing path.

Evidence classification: **code defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

Roll target and direction remain owned by the active roll until it completes or is interrupted. Repeating the current movement-lock value must not end that roll.

### Reproduction

Static trace: begin grounded, locked on, and neutral with no movement locks; submit a lateral roll. Inspect the active roll fields immediately after TryStartRoll and after Character.Tick line 190. Then inspect ApplyAnimationMovement. Runtime appearance has not been reproduced in this audit.

### Impact and Priority

**HIGH** — code defect. Exact visual severity and Animator callback timing need Play Mode validation. This is a different finding from the existing chained-roll interruption report.

### Evidence

- Assets/Scripts/Entities/Character/Character.cs:162-190 — action submission precedes per-frame SetMovementBlocked.
- Assets/Scripts/Entities/Character/Character.cs:370-401,903-937 — StartRoll executes before reporting the action state; it does not establish a movement lock.
- Assets/Scripts/Components/Movement/MovementComponent.cs:142-150,158-180,252-317 — roll metadata is created and then cleared by any false movement-block assignment.
- Assets/Scripts/Components/Animator/AnimatorComponent.cs:391-396 — TriggerRoll sets Animator parameters; it does not synchronously restore movement metadata.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

Exact visual severity and Animator callback timing need Play Mode validation. This is a different finding from the existing chained-roll interruption report.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Separate roll termination from routine movement-lock synchronization; keep the fix within roll lifecycle and movement state.

### Acceptance Criteria

An ordinary SetMovementBlocked(false) during roll startup preserves roll metadata; a locked lateral roll reaches CalculateLockedRollDelta until completion; cancellation clears metadata exactly when the roll ends.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Architecture/Systems/Character System]], [[Architecture/Systems/Jump and Roll System]], [[Work/Issues/Roll Interruption Issue]].
