---
title: Settings Apply Hides Persistence Failures
type: issue
domains:
  - settings
  - persistence
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: error propagation defect
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Settings Apply Hides Persistence Failures

## Issue Contract

### Observed Behavior

SaveService logs an I/O write failure and returns normally. SettingsService.Commit updates Current and clears the edit session regardless; Apply returns Applied and the settings UI closes. The user loses the draft and has no indication in the settings flow that the change was not saved.

Evidence classification: **error propagation defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

A failed persistence attempt must remain distinguishable from a successful apply, with a recoverable edit session or explicit unsaved state.

### Reproduction

In an isolated save fixture, make the settings write fail (for example, a fake save backend or a non-writable temporary target). Apply a setting. Verify that current code clears the draft and returns Applied despite failure. Do not change the real player save permissions.

### Impact and Priority

**MEDIUM** — error propagation defect. No actual disk failure was induced. This finding concerns the verified failure-handling path; Steam Cloud is disabled in the current code.

### Evidence

- Assets/Scripts/Services/Save/SaveService.cs:65-94 — Save ignores TryWriteLocal failure beyond logging and returning.
- Assets/Scripts/Services/Settings/SettingsService.cs:141-160,250-262 — Commit unconditionally updates Current and clears edit state.
- Assets/Scripts/Ui/Settings/SettingsUiController.cs:163-175 — non-confirmation Apply result closes the UI.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

No actual disk failure was induced. This finding concerns the verified failure-handling path; Steam Cloud is disabled in the current code.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Expose the persistence outcome to settings commit and preserve a retryable state; define whether runtime preview remains applied after a failed disk write.

### Acceptance Criteria

Injected write failure cannot produce an indistinguishable successful apply; draft values remain recoverable and retry succeeds; successful apply still persists and closes normally.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Knowledge/Architecture/Systems/Settings System]].
