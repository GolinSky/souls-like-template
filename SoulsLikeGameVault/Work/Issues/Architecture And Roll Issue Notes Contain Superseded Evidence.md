---
title: Architecture And Roll Issue Notes Contain Superseded Evidence
type: issue
domains:
  - documentation
  - architecture
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: documentation defect
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Architecture And Roll Issue Notes Contain Superseded Evidence

## Issue Contract

### Observed Behavior

The interaction research still labels pre-migration code as current, while current interaction discovery uses IEntityLocator and target-owned commands. The existing roll issue includes proposed changes that are already present in Character.StartRoll. The entity-locator contract also lists GetEntity(ulong), while the live API uses long. Reusing these notes without checking source can produce duplicate or incorrect remediation.

Evidence classification: **documentation defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

Current behavior, historical evidence, outstanding defects, and already-implemented recommendations are clearly separated, with verified source references.

### Reproduction

Compare the cited note sections against the current source at the audit commit. The registry already flags some research as stale; that flag must be respected, and the remaining obsolete instructions need reconciliation.

### Impact and Priority

**MEDIUM** — documentation defect. The old roll issue's complete runtime resolution was not checked. Stale UI/settings guidance is acknowledged by the registry and was not used to invent missing-feature bugs.

### Evidence

- SoulsLikeGameVault/Research/Interaction System Audit.md:31-48,54-82 — claims locator discovery is missing.
- Assets/Scripts/Interactions/InteractionController.cs:128-173 — current locator and IInteractableCommand discovery.
- Assets/Scripts/Items/GroundItemSystem.cs:49-70 and Assets/Scripts/Interactions/GraceSystem.cs:70-88 — entities and commands are registered.
- SoulsLikeGameVault/Work/Issues/Roll Interruption Issue.md — Fix 3 and Fix 4 propose out-of-combat stamina gating and animation-lock interruption support.
- Assets/Scripts/Entities/Character/Character.cs:370-401 — those two proposed changes are already implemented.
- SoulsLikeGameVault/Architecture/Systems/Entity Locator System.md:37-48 vs Assets/Scripts/Entities/BaseEntity/IEntityLocator.cs — documented unsigned ID differs from current signed ID API.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

The old roll issue's complete runtime resolution was not checked. Stale UI/settings guidance is acknowledged by the registry and was not used to invent missing-feature bugs.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Reconcile affected note sections against live code, retain historical evidence, and distinguish remaining runtime validation from changes already implemented. Do not close the existing roll issue solely from static inspection.

### Acceptance Criteria

Interaction research clearly distinguishes historical from current paths; the roll issue no longer proposes already-present changes as pending work; entity-locator signatures match source; links to the new roll finding preserve its separate scope.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Agent Guide/Agent Context Registry]], [[Research/Interaction System Audit]], [[Work/Issues/Roll Interruption Issue]], [[Architecture/Systems/Entity Locator System]].
