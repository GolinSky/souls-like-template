---
title: Inventory Category Controls Are Not Connected
type: issue
domains:
  - ui
  - inventory
status: done
authority: evidence
priority: high
updated: 2026-09-09
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: Unity compile and prefab validation passed
aliases: []
tags:
  - work/issue
  - status/done
  - audit/architecture
---
# Inventory Category Controls Are Not Connected

## Issue Contract

### Observed Behavior

Standalone inventory starts on Weapons and cannot switch category: category presenter methods have no view/input callers, and the two category-container fields reference the same empty prefab template. Existing consumables therefore cannot be reached through standalone inventory browsing.

Evidence classification: **code and prefab defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

The inventory exposes functional primary-category and subcategory controls, including access to the consumables already present in the starting inventory.

### Reproduction

Open standalone Inventory using the inventory hotkey or pause route. Attempt to select Consumables and find the Crimson Flask, Lightning Grease, or Golden Rune. The equipment selection route uses separate item-type filtering and should not be confused with standalone inventory.

### Impact and Priority

**HIGH** — code and prefab defect. Keyboard/gamepad focus behavior and final layout need interactive validation. Prefab evidence was read from disk; no asset was modified.

### Evidence

- Assets/Scripts/Ui/Inventory/InventoryUiController.cs:26-29,85-97,173-200 — default category and filtering depend on unreachable presenter actions.
- Assets/Scripts/Ui/Inventory/InventoryUi.cs:19-21,103-122 — container references are only validated; no category controls are bound.
- Assets/Prefabs/Ui/Inventory/InventoryUi.prefab:1373-1374,1822-1840 — both fields point to an empty template RectTransform.
- Assets/Settings/Data/InventoryData.asset:15-25 — current inventory includes consumables.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

Keyboard/gamepad focus behavior and final layout need interactive validation. Prefab evidence was read from disk; no asset was modified.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Connect the existing presenter category API to visible controls and persist the corrected prefab through Unity.

### Acceptance Criteria

Each primary category and subcategory is reachable by the supported input methods; selecting Consumables displays the current consumable entries; standalone and equipment-picker filtering both continue to work.

### Validation

Resolved on 2026-09-09. `InventoryUi` now binds primary and subcategory toggles to the existing presenter API, maintains toggle state and category-specific visibility, and connects top-row grid navigation to the category controls. Standalone inventory shows the controls; equipment-picker mode hides them and retains its separate item-type filter.

The `InventoryUi` prefab now contains distinct primary and subcategory containers with 5 and 11 serialized toggles in enum order. Unity compilation, prefab persistence/serialization inspection, clean-scene preflight, and console checks passed. Independent review found no remaining material issue. No matching Edit Mode tests exist; Play Mode mouse, keyboard, and gamepad verification was intentionally deferred under Unity Test Safety.

Implementation: [[History/Implementation Records/Inventory Category Controls Wiring]].

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

Related: [[Guides/UI/UI Code Build Guide]].
