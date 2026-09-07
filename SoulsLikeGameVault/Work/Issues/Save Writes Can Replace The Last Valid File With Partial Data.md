---
title: Save Writes Can Replace The Last Valid File With Partial Data
type: issue
domains:
  - persistence
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: resilience gap
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Save Writes Can Replace The Last Valid File With Partial Data

## Issue Contract

### Observed Behavior

SaveService and StorageRegistry write directly over the live JSON file, without a temporary-file commit or a retained last-good copy. An interrupted or failing write can destroy previously valid data. On a subsequent failed deserialize, SaveStore.LoadOrCreate constructs fresh state.

Evidence classification: **resilience gap**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

A failed save should leave the previous committed file readable; corrupted data should not silently become the basis for overwriting the only recoverable progress record.

### Reproduction

Use a disposable fixture containing a known valid save. Inject an interruption after the destination has been truncated but before a full JSON write, then reload. This is a proposed failure-injection test, not an operation performed on user saves.

### Impact and Priority

**MEDIUM** — resilience gap. No crash, corrupt save, or lost player data was observed. This is a confirmed missing resilience mechanism under real external I/O failure conditions.

### Evidence

- Assets/Scripts/Services/Save/SaveService.cs:77-94,122-136 — direct File.WriteAllText and default-return deserialization error path.
- Assets/Scripts/Services/Storage/StorageRegistry.cs:21-32 — grace/ladder storage also overwrites the destination directly.
- Assets/Scripts/Services/Save/SaveStore.cs:24-25 — unreadable saves become new T() via LoadOrCreate.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

No crash, corrupt save, or lost player data was observed. This is a confirmed missing resilience mechanism under real external I/O failure conditions.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Use a durable commit strategy appropriate to the supported platforms for both active persistence paths, and retain or quarantine the last-good data on load failure.

### Acceptance Criteria

Failed/interrupted write preserves a readable previously committed save; recovery is explicit; successful writes remain compatible with existing JSON.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

