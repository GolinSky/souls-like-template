---
title: Rejected Scene Transition Can Overwrite Pending Spawn Intent
type: issue
domains:
  - scenes
  - lifecycle
status: open
authority: evidence
priority: medium
updated: 2026-09-14
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: code defect
aliases:
  - Scene Transitions Allow Concurrent Load Operations
tags:
  - work/issue
  - status/open
  - audit/architecture
---

# Rejected Scene Transition Can Overwrite Pending Spawn Intent

## Issue Contract

### Observed Behavior

`SceneService.LoadScene` rejects overlap through `SceneModel.IsLoadingScene`. Menu and travel callers prepare shared spawn intent before attempting admission, so a rejected request can replace the accepted transition's pending spawn data.

This is the remaining caller-ownership defect. Concurrent dependency loading inside one accepted request and the absence of rollback are deliberate decisions in [[History/Records/Scene Loading Model State and Fail Fast Policy]].

### Expected Behavior

Reject a busy transition before mutating its pending spawn intent. The accepted scene and its spawn data must remain paired.

### Reproduction

With one transition pending, request another destination. Compare the pending spawn data before and after service rejection. This source-backed scenario has not been newly reproduced in Unity during the documentation cleanup.

### Impact and Priority

Medium: a rejected request can change the destination spawn intent consumed by the accepted request.

### Evidence

- `Assets/Scripts/Services/Scenes/SceneService.cs:35-60`: model-owned overlap gate.
- `Assets/Scripts/Orchestrators/MainMenu/MainMenuOrchestrator.cs:32-35`: prepare resume before load admission.
- `Assets/Scripts/Services/Travel/TravelService.cs:20-21`: prepare grace spawn before load admission.
- `Assets/Scripts/Services/Spawn/CharacterSpawnService.cs:40-69`: shared pending intent mutation.
- Original audit: [[Research/Architecture and Systems Audit 2026-09-07]]. The former sequential-loading/rollback experiment is [[History/Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]].

### Open Questions

Choose the smallest admission boundary that includes intent preparation. Post-failure retry/recovery remains outside the accepted fail-fast loader policy.

## Resolution Handoff

### Approved Fix Scope

This note scopes a future caller-ownership correction; the current request authorized documentation cleanup, not production implementation.

### Acceptance Criteria

- A rejected request leaves the accepted pending spawn data unchanged.
- One accepted transition consumes its own intended spawn.
- Concurrent dependency loading and fail-fast exceptions remain unchanged.

### Validation

2026-09-14: current-source reconciliation only. Historical fake-backend checks do not establish native Unity or gameplay behavior. No tests were run for this documentation change.
