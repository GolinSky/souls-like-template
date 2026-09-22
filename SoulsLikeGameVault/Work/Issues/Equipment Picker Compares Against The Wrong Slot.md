---
title: Equipment Picker Compares Against The Wrong Slot
type: issue
domains:
  - ui
  - equipment
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
# Equipment Picker Compares Against The Wrong Slot

## Issue Contract

### Observed Behavior

The equipment route opens Inventory with item types and a selection callback, but no target-slot context. Focusing any candidate updates right-hand attack comparison, even when selecting a left-hand shield. The separate slot-aware FocusCandidate implementation is not used by this route.

Evidence classification: **code defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

Candidate comparison reflects the selected equipment slot, its current item, and relevant statistics.

### Reproduction

Open a left-hand equipment slot and focus the existing Wooden Shield candidate while a weapon is assigned to the right hand. The inventory comparison uses that right-hand weapon as its baseline.

### Impact and Priority

**MEDIUM** — code defect. Exact presentation for non-weapon slots should follow the chosen UI design; the incorrect current right-hand baseline is independently verified.

### Evidence

- Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs:175-180 — target slot is reduced to broad item types before opening the inventory route.
- Assets/Scripts/Ui/Inventory/InventoryUiController.cs:99-113 — every focused item calls UpdateRightAttackComparison.
- Assets/Scripts/Ui/Equipment/EquipmentUiController.cs:105-121 — slot-specific comparison exists in a separate, unused candidate path.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

Exact presentation for non-weapon slots should follow the chosen UI design; the incorrect current right-hand baseline is independently verified.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Carry target-slot comparison context into the displayed picker or route focus through equivalent slot-aware presentation.

### Acceptance Criteria

Left-hand selection compares left-hand values; right-hand selection uses the selected right-hand slot; candidate weight changes compare against the item being replaced; ordinary inventory does not imply an unrelated equipment replacement.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Knowledge/Architecture/UI/Pause Navigation]].
