---
title: Scene Transitions Allow Concurrent Load Operations
type: issue
domains:
  - scenes
  - lifecycle
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: code defect
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Scene Transitions Allow Concurrent Load Operations

## Issue Contract

### Observed Behavior

Each Play invocation starts a new asynchronous scene transition. The UI and orchestrators provide no in-flight guard, and SceneService starts a fresh Single-mode Loading scene before awaiting completion. Two invocations before the first load completes can overlap transitions and compete over active scene, loading-scene lifetime, and shared pending spawn state.

Evidence classification: **code defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

One transition owns scene loading and pending spawn state at a time; repeated UI requests are rejected, coalesced, or queued explicitly.

### Reproduction

Invoke PlayGame twice before the first Loading-scene operation completes, for example rapid activation during a slow load. Trace two simultaneous LoadSceneAsync calls. The exact engine-visible failure requires a controlled runtime reproduction.

### Impact and Priority

**MEDIUM** — code defect. Overlap is statically reachable; duplicate scenes, exceptions, or loading-screen stalls are possible outcomes, not runtime observations from this audit.

### Evidence

- Assets/Scripts/Ui/MainMenu/MainMenuUi.cs:29-35 — Play callback remains bound with no disable-on-start.
- Assets/Scripts/Ui/MainMenu/MainMenuUiController.cs:32-35 and Assets/Scripts/Orchestrators/MainMenu/MainMenuOrchestrator.cs:32-35 — repeated calls are forwarded directly.
- Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs:30-33 and Assets/Scripts/Services/Scenes/SceneService.cs:45-48,61-122 — no transition gate or ownership token.
- Assets/Scripts/Services/Spawn/CharacterSpawnService.cs:40-66 — PrepareResume/PrepareGraceSpawn mutate shared pending state.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

Overlap is statically reachable; duplicate scenes, exceptions, or loading-screen stalls are possible outcomes, not runtime observations from this audit.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Give scene transitions a single owner and define repeated-request behavior across all callers; align pending-spawn changes with the accepted transition.

### Acceptance Criteria

Two requests issued before completion produce one accepted transition, or a deterministic queue; no duplicate target loads occur and spawn data belongs to the accepted destination.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Work/Issues/DefaultLocation Memory and Rendering Issues#I-03 — All location dependencies load concurrently and stay resident]]. That performance audit also notes the missing transition guard; this note supplies the UI trigger and bounded acceptance criteria. Intentional parallel dependency loading within one transition is a separate concern.

