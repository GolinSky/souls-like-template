---
title: Interrupted Fades Leave Lifecycle Awaiters Unsettled
type: issue
domains: [ui, lifecycle, spawn]
status: open
authority: evidence
priority: medium
updated: 2026-09-08
source_commit: 30e5f953d4fe5824f3923c5628b3dc8748e10a1a
verification: static source; runtime not reproduced
aliases: []
tags: [work/issue, status/open, audit/architecture]
---

# Interrupted Fades Leave Lifecycle Awaiters Unsettled

## Issue Contract

### Observed Behavior

FadeUi replaces its single tween by Kill and also kills on destruction. Caller notification exists only in OnComplete. Respawn awaits completion sources populated only by these success callbacks, without a cancellation token or other terminal outcome.

### Expected Behavior

Every fade operation settles its caller exactly once on success, replacement, destruction, cancellation, or failure. Interrupted presentation must not masquerade as successful gameplay progression.

### Reproduction

Unexecuted local fixture: start a FadeIn with a waiting caller, immediately replace it with FadeOut/FadeInOut, and assert the old wait settles. Repeat by destroying the FadeUi. Then exercise cancellation of the owning respawn scope. Gameplay overlap is a conditional integration scenario.

### Impact and Priority

MEDIUM — lifecycle correctness/availability. Evidence is static; failure-injection and gameplay outcomes remain untested.

### Evidence

- `Assets/Scripts/Ui/Fade/FadeUi.cs:15–63` — replacement/destruction kills and completion-only callbacks.
- `Assets/Scripts/Services/Fade/FadeService.cs:6–35` — callback-only API shared by callers.
- `Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs:159–179` — waits with no cancellation; finally is not reached while pending.
- `Assets/Scripts/Ui/Grace/GraceUiController.cs:108` — another shared fade caller.
- [DOTween upstream Kill contract](https://github.com/Demigiant/dotween/blob/develop/_autodocs/02-tween-extensions.md) — `complete = false` by default, retrieved with Context7.

### Hypotheses

The completion-only ownership gap is verified in source. No normal gameplay race was reproduced. Confirm the installed DOTween behavior in the local fixture before claiming a particular visible deadlock.

### Open Questions

Define whether a new fade cancels or supersedes its predecessor; proposed explicit cancellation. Reconcile the caller's game state on cancellation without force-completing interrupted gameplay.

## Resolution Handoff

### Approved Fix Scope

Proposed local implementation: an operation result/cancellation contract in the existing fade service/view and matching scope-owned respawn cleanup. Do not globally use Kill(true) as a success substitute.

### Acceptance Criteria

Replacement/destruction always settles the old caller; successful completion fires once; a late callback cannot advance an obsolete respawn; scope disposal leaves no pending wait or permanent respawning flag.

### Validation

Source trace and existing-test inspection completed in the worktree. No Unity tests, compilation, asset mutation, or runtime reproduction performed: the only connected Editor targets the main checkout. Use [[Work/Plans/Architecture Lifecycle Remediation and Local Validation]] for preflight, bounded Edit Mode fixtures, and the separate gameplay follow-up.

Audit: [[Research/Architecture and Systems Audit 2026-09-08]]. When resolved, mark done and link an implementation record and updated architecture guidance.

