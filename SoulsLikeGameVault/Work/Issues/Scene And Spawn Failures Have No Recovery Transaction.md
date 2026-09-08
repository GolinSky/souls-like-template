---
title: Scene And Spawn Failures Have No Recovery Transaction
type: issue
domains: [scenes, spawn, lifecycle]
status: open
authority: evidence
priority: medium
updated: 2026-09-08
source_commit: 30e5f953d4fe5824f3923c5628b3dc8748e10a1a
verification: static source; runtime not reproduced
aliases: []
tags: [work/issue, status/open, audit/architecture]
---

# Scene And Spawn Failures Have No Recovery Transaction

## Issue Contract

### Observed Behavior

SceneService loads Loading in Single mode before target lookup/load and has no failure recovery branch. Spawn intent is a separate mutable singleton: grace resolution saves the destination before character startup succeeds, and TryConsumeSpawn clears the pending request before CharacterFactory returns.

### Expected Behavior

One accepted transition owns destination intent, loading, initialization, outcome, and cleanup. Failure leaves a deliberate recoverable scene and coherent save/pending-spawn state. A later request cannot consume or overwrite another operation's intent.

### Reproduction

Unexecuted local fault injection: fail destination lookup/load after Loading succeeds; then separately fail CharacterFactory initialization after grace resolution. Capture active scene, pending spawn, persisted checkpoint, and subsequent retry. Add a second request during loading to cover the existing concurrency issue.

### Impact and Priority

MEDIUM — lifecycle correctness/availability. Evidence is static; failure-injection and gameplay outcomes remain untested.

### Evidence

- `Assets/Scripts/Services/Scenes/SceneService.cs:45–48,69–122` — destructive loading precedes target validation; success path only.
- `Assets/Scripts/Services/Travel/TravelService.cs:20–21` — prepare and load are separate operations.
- `Assets/Scripts/Services/Spawn/CharacterSpawnService.cs:59–63,78–113` — mutable pending data, early persistence and consume.
- `Assets/Scripts/Interactions/GraceSystem.cs:64–67` — resolution in construction.
- `Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs:55–64,149–179` — consume before creation; separate same-scene respawn/failure cleanup.

### Hypotheses

Ordering and absent failure ownership are source-confirmed. Actual load/DI failure outcomes, leaked handles, and retry behavior require local tests. This extends failure handling rather than duplicating [[Work/Issues/Scene Transitions Allow Concurrent Load Operations]] or [[Work/Issues/Respawn Assumes The Last Grace Is In The Current Scene]].

### Open Questions

Review admission policy (proposed reject while busy), safe recovery destination, checkpoint commit point, and cross-scene respawn. Admission must occur before mutating pending spawn data.

## Resolution Handoff

### Approved Fix Scope

Proposed local implementation: one transition owner using existing scene/spawn boundaries, explicit terminal outcome, cleanup of only operation-owned resources, and save commit after successful domain initialization. Preserve prior issue scopes and deliberate additive residency.

### Acceptance Criteria

Injected destination/initialization failure reaches a recoverable state; no pending intent is silently reused by an unrelated request; last-good save is preserved according to the persistence contract; retry and cross-scene respawn pass; ordinary successful travel remains correct.

### Validation

Source trace and existing-test inspection completed in the worktree. No Unity tests, compilation, asset mutation, or runtime reproduction performed: the only connected Editor targets the main checkout. Use [[Work/Plans/Architecture Lifecycle Remediation and Local Validation]] for preflight, bounded Edit Mode fixtures, and the separate gameplay follow-up.

Audit: [[Research/Architecture and Systems Audit 2026-09-08]]. When resolved, mark done and link an implementation record and updated architecture guidance.

