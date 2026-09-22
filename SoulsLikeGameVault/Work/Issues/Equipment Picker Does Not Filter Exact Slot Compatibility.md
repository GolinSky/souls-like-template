---
title: Equipment Picker Does Not Filter Exact Slot Compatibility
type: issue
domains:
  - ui
  - equipment
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: conditional content integration defect
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Equipment Picker Does Not Filter Exact Slot Compatibility

## Issue Contract

### Observed Behavior

All armor slots use ItemType.Armor and arrow/bolt slots use ItemType.Ammunition as the candidate filter, but EquipmentComponent.Assign rejects incompatible EquipmentGroup values. Adding normal distinct armor or ammunition content therefore makes invalid candidates selectable and causes assignment exceptions.

Evidence classification: **conditional content integration defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

Every visible selectable candidate is compatible with the exact destination slot.

### Reproduction

Conditional content fixture: put head and chest armor in the inventory, open the Head picker, and select chest armor. Repeat with arrows and bolts. The current shipped catalog has no armor/arrow/bolt definitions, so this is not an observed current-content failure.

### Impact and Priority

**MEDIUM** — conditional content integration defect. Requires armor/ammunition content or test fixtures; treat as an integration gate before those content types ship.

### Evidence

- Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs:175-180,231-246 — exact slot is reduced to broad item-type filters.
- Assets/Scripts/Ui/Inventory/InventoryUiController.cs:173-200 — selection-mode filtering uses item type.
- Assets/Scripts/Components/Equipment/EquipmentComponent.cs:49-58 — Assign throws if CanEquipIn(GetCompatibilityGroup(slotId)) is false.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

Requires armor/ammunition content or test fixtures; treat as an integration gate before those content types ship.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Keep the target slot in the picker contract and filter using the same canonical compatibility rule used by assignment.

### Acceptance Criteria

Every displayed candidate passes CanEquipIn for the target slot; head/chest and arrow/bolt fixtures exclude each other's incompatible items; hand/shield behavior remains valid.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Knowledge/Architecture/UI/Pause Navigation]].
