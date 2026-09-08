---
title: Animation Completion Is Not Correlated To The Owning Action
type: issue
domains: [character, animation, lifecycle]
status: open
authority: evidence
priority: medium
updated: 2026-09-08
source_commit: 30e5f953d4fe5824f3923c5628b3dc8748e10a1a
verification: static source; runtime not reproduced
aliases: []
tags: [work/issue, status/open, audit/architecture]
---

# Animation Completion Is Not Correlated To The Owning Action

## Issue Contract

### Observed Behavior

Animator DTOs carry state/layer data but no execution identity. Character lifecycle handlers accept category/event kind, and chained CharacterActionStateMachine actions count obsolete exits using _pendingExitsToIgnore. Loss or duplication changes which execution appears complete.

### Expected Behavior

Only an event belonging to the active execution may complete it. Repeated execution of the same state, stale callbacks, and duplicates must be distinguishable without relying on the expected number of exits.

### Reproduction

Unexecuted deterministic case: start attack A; open its queue window; report attack B executed; omit A's exit; deliver B's exit. The current counter consumes B's completion as the obsolete exit and leaves Attack active. Also exercise duplicate A exits and an exit from another layer/state.

### Impact and Priority

MEDIUM — lifecycle correctness/availability. Evidence is static; failure-injection and gameplay outcomes remain untested.

### Evidence

- `Assets/Scripts/Components/Animations/AnimatorStateMachineDto.cs:5–12` — no request identity.
- `Assets/Scripts/Components/Animations/AnimatorStateMachineReceiver.cs:26–78` — category/layer events forwarded to observers.
- `Assets/Scripts/Entities/Character/Character.cs:455–481` — lifecycle handlers use category/event kind.
- `Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs:95–121,193–235` — category filtering and exit-count heuristic.
- `Assets/Scripts/Tests/CharacterRuntime/CharacterActionStateMachineTests.cs:82–89,105–140` — existing contradictory-category and expected-count coverage.

### Hypotheses

Counter behavior is source-backed; frequency under the actual Animator is unmeasured. Category filtering and normal chained-action handling already exist, so this is not a claim that every interrupted action is broken.

### Open Questions

Review generation capture at the request/state-entry boundary and controller-replacement invalidation. A hash/layer alone cannot distinguish repeated executions of the same state. Attaching the current generation on delivery would incorrectly legitimize stale callbacks.

## Resolution Handoff

### Approved Fix Scope

Proposed local implementation: correlate completion in the narrow character animation/action boundary and preserve existing buffering/queue semantics. Do not replace the whole Animator or action system.

### Acceptance Criteria

Missing old exits cannot suppress the current completion; duplicate/stale/wrong-layer events cannot release the new operation; current contradictory-event and chained-action tests still pass; repeated same-state entry and controller replacement are covered.

### Validation

Source trace and existing-test inspection completed in the worktree. No Unity tests, compilation, asset mutation, or runtime reproduction performed: the only connected Editor targets the main checkout. Use [[Work/Plans/Architecture Lifecycle Remediation and Local Validation]] for preflight, bounded Edit Mode fixtures, and the separate gameplay follow-up.

Audit: [[Research/Architecture and Systems Audit 2026-09-08]]. When resolved, mark done and link an implementation record and updated architecture guidance.

