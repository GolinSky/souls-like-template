---
title: Inventory Category Controls Wiring
type: implementation-record
domains:
  - ui
  - inventory
status: done
authority: historical
updated: 2026-09-09
aliases: []
tags:
  - history/change
---

# Inventory Category Controls Wiring

## Implementation Record Contract

### Outcome

Standalone inventory now exposes working primary-category and subcategory toggles. Selecting a category updates the existing inventory filter, and directional input can move from the top grid row into the category controls. Equipment-picker mode keeps its separate item-type filtering and hides the standalone controls.

### Why

The controller already implemented category filtering, but the view never invoked its presenter methods. Both serialized container fields also referenced the same empty prefab object, leaving the inventory permanently on its default Weapons category.

### Changed Files and Assets

- `Assets/Scripts/Ui/Inventory/InventoryUi.cs` — binds and manages serialized category toggles, visibility, state, and navigation.
- `Assets/Scripts/Ui/Inventory/InventoryUiController.cs` — shows controls for standalone inventory and hides them for equipment selection.
- `Assets/Scripts/Ui/Inventory/InventorySlotUI.cs` — supports an upward category navigation target for top-row slots.
- `Assets/Prefabs/Ui/Inventory/InventoryUi.prefab` — replaces the empty shared template with distinct containers and wired primary/subcategory toggles.

### Decisions and Tradeoffs

- Reused `CustomButtonToggle` and the existing presenter/filter API instead of introducing another controller layer.
- Kept subcategory deselection meaningful: it returns to the active primary-category filter.
- Suppressed presenter callbacks only during programmatic synchronization while still allowing toggle visual-state updates.
- Left equipment-picker route filtering unchanged.

### Validation Evidence

- Unity Test Safety preflight passed with `ElevatorDemo.unity` clean.
- Unity script compilation passed with no errors.
- Prefab inspection confirmed distinct containers, two `ToggleGroup`s, 5 primary controls, 11 subcategory controls, and enum-ordered serialized arrays.
- Unity reported no new console errors after import and compilation.
- Independent review found no remaining material issue after the toggle synchronization correction.
- No relevant inventory Edit Mode tests exist. Play Mode mouse, keyboard, and gamepad verification was intentionally deferred.

### Documentation Updated

- [[History/Closed Issues/Inventory Category Controls Are Not Connected]]
- [[Work/Work Queue]]

The advisory [[Knowledge/Guides/UI/Inventory UI and UX Guide]] already describes the intended category layout, so no architecture text changed.

### Follow-Up

Perform an interactive Play Mode pass for mouse, keyboard, and gamepad focus movement when that validation phase is scheduled.

Originating issue: [[History/Closed Issues/Inventory Category Controls Are Not Connected]].
