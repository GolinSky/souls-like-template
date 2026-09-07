---
title: Stance Recovery Is Gated By Poise Delay
type: issue
domains:
  - combat
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: code and architecture mismatch
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Stance Recovery Is Gated By Poise Delay

## Issue Contract

### Observed Behavior

TickRecovery returns while the poise delay is positive, before reaching stance regeneration. A hit that damages poise therefore also delays stance recovery.

Evidence classification: **code and architecture mismatch**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

The documented combat contract gives stance independent recovery with no poise delay, except while a critical opportunity is active.

### Reproduction

Apply a nonlethal hit that damages both meters without breaking stance. Advance recovery by a positive delta smaller than the poise delay. The stance value remains unchanged although stanceRecoveryPerSecond is positive.

### Impact and Priority

**MEDIUM** — code and architecture mismatch. The note is advisory. If shared recovery delay is intentional, update the documented rule instead; the code/documentation discrepancy itself is confirmed.

### Evidence

- Assets/Scripts/Entities/Combat/CombatDefenseComponent.cs:200-243 — the poise-delay return precedes the stance-recovery block.
- SoulsLikeGameVault/Architecture/Systems/Hitbox System.md:240-256 — stance is documented as having no delay timer.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

The note is advisory. If shared recovery delay is intentional, update the documented rule instead; the code/documentation discrepancy itself is confirmed.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Confirm the combat rule, then gate only poise regeneration on the poise timer; keep critical-window behavior intact.

### Acceptance Criteria

With positive poise delay and no critical opportunity, stance increases by its configured rate while poise remains unchanged; critical opportunities still suppress stance recovery.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Architecture/Systems/Hitbox System]].
