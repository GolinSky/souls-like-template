---
title: Respawn Assumes The Last Grace Is In The Current Scene
type: issue
domains:
  - spawn
  - scenes
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: conditional code defect
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Respawn Assumes The Last Grace Is In The Current Scene

## Issue Contract

### Observed Behavior

Respawn fades to black and enters Ended, then GetLastGracePosition throws if the stored grace belongs to another scene. The finally block only clears _isRespawning; it does not fade out or restore gameplay. Fresh save data always defaults LastGraceId to WorkshopGrace01.

Evidence classification: **conditional code defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

Starting a supported non-Workshop scene without a matching saved grace must provide a valid spawn fallback or load the stored grace's scene without leaving a black screen.

### Reproduction

Conditional: start DefaultLocation or ElevatorDemo directly with fresh save data, before resting at that scene's grace, then die. Alternatively use a valid saved-current-scene record whose last grace is in another scene. Normal fresh MainMenu Play currently defaults to Workshop and does not by itself trigger this path.

### Impact and Priority

**MEDIUM** — conditional code defect. Conditional on direct scene entry or a cross-scene saved checkpoint; not claimed as a failure of the default Workshop path.

### Evidence

- Assets/Scripts/Services/Spawn/CharacterSpawnData.cs:9-12 — default current scene and last grace are Workshop.
- Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs:55-64,149-181 — fresh direct spawn is allowed; respawn has no cross-scene branch or failure fade cleanup.
- Assets/Scripts/Services/Spawn/CharacterSpawnService.cs:137-154 — explicit different-scene exception.
- Assets/Scripts/Entities/Character/PlayerController.cs:176-183 — death completion invokes RespawnAtLastGrace.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

Conditional on direct scene entry or a cross-scene saved checkpoint; not claimed as a failure of the default Workshop path.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Define first-death behavior for direct scene entry and cross-scene saved grace; use the available scene/spawn service boundary and ensure fade completion on failure.

### Acceptance Criteria

Fresh direct entry into each supported gameplay scene can die and recover without a stuck fade; a cross-scene grace loads or resolves deterministically; Workshop behavior remains correct.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

