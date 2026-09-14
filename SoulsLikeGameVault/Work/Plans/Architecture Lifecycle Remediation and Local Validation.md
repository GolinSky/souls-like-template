---
title: Architecture Lifecycle Remediation and Local Validation
type: plan
domains: [architecture, character, animation, spawn, scenes, lifecycle]
status: draft
authority: advisory
updated: 2026-09-14
source_commit: 30e5f953d4fe5824f3923c5628b3dc8748e10a1a
aliases: []
tags: [work/plan, status/draft]
---

# Architecture Lifecycle Remediation and Local Validation

## Plan Contract

### Goal

Review and scope the remaining fade and scene/spawn ownership work. This draft does not authorize implementation.

### Source Research and Decisions

- [[Research/Architecture and Systems Audit 2026-09-08]] is the historical starting point.
- On 2026-09-14 the user completed the roll, missing-notification lifecycle, and animation-correlation issues. They are in [[History/Implementation History]], and are excluded from this plan. Do not add tokens, watchdogs, or new recovery machinery on the basis of the superseded checklist.
- [[History/Records/Character Owned Animation Sequencing and Recovery Phases 2-4]] records configuration repairs and supported callback traces without production correlation changes.
- [[History/Records/Scene Loading Model State and Fail Fast Policy]] establishes concurrent dependencies, destination last, and failure propagation without rollback or automatic retry.
- Remaining issue scope: [[Work/Issues/Interrupted Fades Leave Lifecycle Awaiters Unsettled]], [[Work/Issues/Rejected Scene Transition Can Overwrite Pending Spawn Intent]], [[Work/Issues/Scene And Spawn Failures Have No Recovery Transaction]], and [[Work/Issues/Respawn Assumes The Last Grace Is In The Current Scene]].

### Assumptions and Non-Goals

- Recheck the actual checkout and connected Editor before implementation or validation.
- Preserve unrelated work, entity-locator boundaries, scope ownership, and accepted loader semantics.
- Closed issue scenarios are historical limitations, not prerequisites for reopening or completing this plan.
- No changes to scene rollback, automatic retry, or completed animation/roll work are included.

### Success Criteria

- An interrupted/replaced fade settles its originating awaiter under a reviewed ownership contract.
- A rejected scene request cannot overwrite the accepted request's spawn intent.
- Spawn consumption/persistence ordering and cross-scene respawn have a reviewed policy compatible with fail-fast loading.
- Validation evidence distinguishes deterministic checks from native Unity and gameplay behavior.

## Execution Plan

- [ ] Recheck the four remaining issues against current source and agree on the exact ownership/commit contracts.
- [ ] Assign one bounded writer per overlapping scope after this plan is reviewed and explicitly authorized for execution.
- [ ] Implement and validate each accepted slice independently, preserving completed work and current loader policy.
- [ ] Record implementation evidence, deferred gameplay checks, and the resulting issue disposition.

## Risks and Rollback

Replacing a fade can abandon an awaiter; admitting a scene after spawn mutation can mix requests; committing before initialization can leave inconsistent state. A gate confined to SceneService does not fix caller ordering. Rollback here means reverting the bounded code change, not adding runtime scene recovery.

## Validation

This is a reconciled draft. No new Unity run was performed. Future execution must use the registered testing workflow, clean-scene preflight, explicit bounded asynchronous Edit Mode selection, caller-side timeout enforcement, and confirmation that no test remains active. Normal validation excludes Play Mode. Persist any explicitly assigned asset changes through Unity.

## Execution Handoff

Read `plan-workflow`, `issue-workflow`, `work-routing`, and the domain keys required by the selected remaining slice. Review failure outcomes, scope disposal, admission, and spawn commit timing before marking this plan ready. Do not infer approval from older audit proposals. Each implementation and validation handoff remains bounded to the chosen open issue.
