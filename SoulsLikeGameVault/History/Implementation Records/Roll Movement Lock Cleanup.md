---
title: Roll Movement Lock Cleanup
type: implementation-record
domains:
  - movement
  - roll
status: done
authority: historical
updated: 2026-09-09
aliases: []
tags:
  - history/change
---

# Roll Movement Lock Cleanup

## Implementation Record Contract

### Outcome

`MovementComponent.SetMovementBlocked` clears active roll target/direction only on a blocked-to-unblocked transition. Repeated blocked calls retain their grounded horizontal-velocity reset.

### Why

Rechecked [[Work/Issues/Locked Roll State Is Cleared Before Root Motion]] after the user reported rolling works in-game. The original audit omitted the synchronous `TriggerRoll` / `TriggerBackStep` → `BeginRootMotionAction` → `BeginRootMotionContract` path. It already enables the animation movement lock before the later character synchronization, preserving roll data. The path also exists in the original audit commit. This change is user-authorized cleanup, not proof of a repaired gameplay defect.

### Changed Files and Assets

- `Assets/Scripts/Components/Movement/MovementComponent.cs`: one previous-state local and a narrowed cleanup condition.
- `Assets/Scripts/Editor/Tests/Animation/MovementComponentRollContractTests.cs` and Unity-generated metadata: four focused Edit Mode tests using a real movement component and locked-roll startup.

### Decisions and Tradeoffs

Preserved the current movement-contract lifecycle instead of adding dedicated roll completion APIs. No current caller was found to require repeated false assignments for cleanup. Tests check movement-state behavior; they do not execute the full Animator or gameplay loop.

### Validation Evidence

- Baseline: four tests completed in 0.43s; three passed and repeated-unblocked preservation failed as expected.
- After cleanup: the same four tests passed in 0.34s, covering repeated false preservation, repeated true preservation, true-to-false cleanup, and repeated true grounded velocity reset.
- Unity 6000.3.11f1 compilation completed without errors. Test preflight reported the ElevatorDemo scene clean. Asynchronous Edit Mode run completed with no test left active.
- Scoped `git diff --check` passed.
- Independent review found no regression in the setter change. Its efficacy/coverage concerns are addressed by the corrected diagnosis and explicit classification as state-setter cleanup, not a demonstrated root-motion fix or integration test.

### Documentation Updated

Corrected the originating issue, updated [[Work/Work Queue]], and documented the contract in [[Architecture/Systems/Jump and Roll System]].

### Follow-Up

Play Mode gameplay validation was skipped under project policy. A separate `unity_test_runner` gameplay phase remains required for any claim that locked-roll appearance, orbit, completion, sprint interruption, and chained-roll behavior are runtime-verified. No such runtime claim is made here.
