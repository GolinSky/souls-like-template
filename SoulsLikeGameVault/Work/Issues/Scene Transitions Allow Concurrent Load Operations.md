---
title: Scene Transitions Allow Concurrent Load Operations
type: issue
domains:
  - scenes
  - lifecycle
status: open
authority: evidence
priority: medium
updated: 2026-09-08
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

**2026-09-08 partial remediation:** [[History/Implementation Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]] adds a single in-flight gate in `SceneService.LoadScene` and rollback of partial destination loads. Executable tests against the original source reproduced two overlapping Loading requests; the changed source rejects the second call and allows a later retry after cleanup. These are fake-backend control-flow tests, not a Unity runtime reproduction. This issue remains **open** because menu/travel callers still invoke `PrepareResume`/`PrepareGraceSpawn` before service admission, allowing a rejected request to overwrite the accepted transition's pending spawn intent. The following description records the original audit state.

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

Phase 6 execution authorized the bounded SceneService loading/cleanup experiment and service-level transition gate. That portion is implemented and isolated tests pass. Remaining remediation scope: align pending-spawn changes with accepted transition ownership across menu/travel callers, then validate native scene operations, retry behavior, and spawn intent. The broader cross-caller issue is not closed by the service gate alone.

### Acceptance Criteria

Two requests issued before completion produce one accepted transition, or a deterministic queue; no duplicate target loads occur and spawn data belongs to the accepted destination.

### Validation

Original audit: static call-path/source inspection only. Phase 6: two executable baseline reproductions and ten candidate source-linked control-flow checks passed; live C# diagnostics were clear. The harness substitutes .NET Task and fake Unity/Addressables operations, so native lifetime, gameplay/spawn behavior, and memory acceptance remain unverified. No Unity test run, Play Mode session, or build was started under the 1.9% commit-headroom blocker. Required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Work/Issues/DefaultLocation Memory and Rendering Issues#I-03 — All location dependencies load concurrently and stay resident]]. That performance audit also notes the missing transition guard; this note supplies the UI trigger and bounded acceptance criteria. Intentional parallel dependency loading within one transition is a separate concern.
