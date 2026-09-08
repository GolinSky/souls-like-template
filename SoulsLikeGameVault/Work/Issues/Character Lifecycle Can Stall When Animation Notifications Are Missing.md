---
title: Character Lifecycle Can Stall When Animation Notifications Are Missing
type: issue
domains: [character, spawn, animation, lifecycle]
status: open
authority: evidence
priority: high
updated: 2026-09-08
source_commit: 30e5f953d4fe5824f3923c5628b3dc8748e10a1a
verification: static source; runtime not reproduced
aliases: []
tags: [work/issue, status/open, audit/architecture]
---

# Character Lifecycle Can Stall When Animation Notifications Are Missing

## Issue Contract

### Observed Behavior

Fresh spawn installs input/movement protection before TriggerSpawn and releases it on Spawn/Exit. Death requests respawn only after Death/Exit. Grace transitions have cancellation cleanup but no missing-notification failure deadline. A live character can therefore remain blocked indefinitely if the expected event never arrives.

### Expected Behavior

An owned lifecycle operation must reach success, cancellation, or explicit failure even if its animation never starts or ends. Recovery must run while gameplay input ticking is blocked and must release only the operation's own protection.

### Reproduction

Local fault-injection cases, not executed: suppress Spawn entry/exit or disable the Animator before startup; omit Death/Exit after health reaches zero; omit GraceRestIdle/Enter during grace entry while its cancellation token remains live. Observe operation state, input/movement locks, invulnerability, checkpoint writes, and respawn count.

### Impact and Priority

HIGH — lifecycle correctness/availability. Evidence is static; failure-injection and gameplay outcomes remain untested.

### Evidence

- `Assets/Scripts/Entities/Character/Character.cs:118–129,219–303,455–475,568–612,950–954` — request, protection, notification and cancellation paths.
- `Assets/Scripts/Entities/Character/PlayerController.cs:91–100,176–183` — blocked/dead early return and death-to-respawn bridge.
- `Assets/Scripts/Components/Animator/AnimatorComponent.cs:328–350` — trigger-only spawn versus explicit grace-idle initialization.
- `Assets/Scripts/Editor/Tests/Animation/GraceAnimationTests.cs:17–78` — existing grace happy-path coverage.

### Hypotheses

The missing-event stall follows from source. This explains the reported class of bugs, but the user's three individual incidents have not been reproduced. Receiver initialization races and wrong-controller content are candidate triggers, not established causes.

### Open Questions

Review exact spawn recovery and death presentation timing. Proposed grace failure cancels the interaction without opening/saving the grace; do not treat a timeout as successful gameplay completion.

## Resolution Handoff

### Approved Fix Scope

Audit and local follow-up planning are complete. Proposed local implementation: one lifecycle owner for startup/death/grace completion and cancellation, using explicit operation outcomes and diagnostics. Start with spawn/death; extend grace after those contracts pass. No runtime changes were made here.

### Acceptance Criteria

Missing-start/missing-end cases terminate within the reviewed bound; spawn becomes playable or reports a recoverable failure; death respawns exactly once; failed grace entry grants no checkpoint/reward; unrelated locks survive cleanup. The owner continues operating when PlayerController.Tick returns early.

### Validation

Source trace and existing-test inspection completed in the worktree. No Unity tests, compilation, asset mutation, or runtime reproduction performed: the only connected Editor targets the main checkout. Use [[Work/Plans/Architecture Lifecycle Remediation and Local Validation]] for preflight, bounded Edit Mode fixtures, and the separate gameplay follow-up.

Audit: [[Research/Architecture and Systems Audit 2026-09-08]]. When resolved, mark done and link an implementation record and updated architecture guidance.

