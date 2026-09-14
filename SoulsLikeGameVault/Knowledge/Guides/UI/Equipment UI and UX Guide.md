---
title: Equipment UI and UX Guide
type: guide
domains:
  - ui
  - equipment
status: needs-review
authority: advisory
updated: 2026-09-14
context_keys:
  - equipment-ui
aliases:
  - Equipment UI-UX Architecture & Unity Implementation Guide
tags:
  - status/needs-review
---
# Equipment UI/UX Architecture Guide

This guide breaks down the structure, spatial layout, UX interaction states, visual design specifications, and C# Unity implementation details for the Souls-like Equipment UI.

---

## 1. UI/UX Design Philosophy & Architectural Overview

The equipment interface follows FromSoftware's dark fantasy minimalist aesthetic:
- **Low Clutter, High Information Density:** Complex RPG calculations and equipment slots are neatly organized into modular panels that update dynamically without obscuring gameplay context.
- **Immediate Feedback Loop:** Every hover, selection, or slot assignment instantly updates inspector cards, candidate comparisons, and global character stats (Equip Load, Weight, Attack Ratings).
- **Diegetic Medieval Palette:** Charcoal olive backgrounds (`#171916`, `#20231D`), muted control borders (`#858B76`), warm gold focus (`#C9B67C`), and parchment text (`#E6E1CE`).
- **Gamepad-First Spatial Navigation:** Grid-based multi-row navigation explicitly configured for D-pad / WASD movement with clear active selection borders and seamless mouse/pointer hover support.
- **Decoupled MVP / Controller Pattern:** Built on `EquipmentUi` (View), `EquipmentUiController` (Controller / Presenter), `EquipmentSlotUI` (Slot View), and `CharacterStatsUi` (Shared Stats View), resolved and injected via VContainer.

---

## 2. Spatial UI Breakdown (What is Located Where)

The equipment view uses the approved Penpot design on a 1920 × 1080 logical canvas. The main content starts at (64, 136), measures 1792 × 800, and contains three columns with 48-unit gaps.

| Zone | Current presentation |
|---|---|
| Header | Inventory-style frame, equipment emblem, and EQUIPMENT title. |
| Equipment grid | 584-unit left column with selected slot/name and equipped count, followed by 28 slots. Category row labels are omitted. |
| Lore | 648-unit center column using the linked inventory `LoreCard.prefab` and `LoreCardUi`. |
| Character status | 464-unit right column using the linked inventory `CharacterStats.prefab` and existing controller updates. |
| Footer | Current actions: Select, Back, Unequip. No unsupported Switch Display action is advertised. |

### Zone 2: Equipment Grid Panel (Left Side)

The six logical equipment groups retain seven navigation rows:

1. RightHand1–3.
2. LeftHand1–3.
3. Arrow1–2 and Bolt1–2.
4. Head, Chest, Arms, Legs.
5. Talisman1–4.
6. QuickItem1–5.
7. QuickItem6–10.

Each logical slot is 88 × 88 with a 64 × 64 icon and 12-unit spacing. Empty category sprites come from `Assets/Art/Textures/EquipmentUI/EmptySlots/`. Right-hand slots use the weapon ghost, left-hand slots the shield ghost, ammo and armor use matching ghosts, talismans use a ring, and quick items use a flask. These visual cues do not change equipment compatibility.

### Zone 3: Shared Inventory Lore Card

`EquipmentUi.DisplaySlot` updates `selectedSlotText`, `selectedItemNameText`, and the linked `loreCardUi`.

- Occupied slots use the same `LoreCardUi.Display(InventoryItemViewData)` path as inventory, showing the item name, artwork, description, and lore.
- Empty slots call `DisplayEmpty(slotName, slotIcon)`, replacing stale artwork/text with the category icon and an equipment-selection instruction.
- `RefreshSlots` updates the equipped count and all 28 slot presentations.
- Character statistics remain owned by the existing `CharacterStatsUi` and equipment controller.

### Inventory Selection and Retained Inspector

The active change-equipment flow uses `EquipmentUiController.SubmitSlot` → `InventoryRequested` to open the shared inventory route. Returning to equipment restores the previously selected slot.

The older detailed inspector is retained under `InventoryPickerOverlay/PickerModalWindow/ComparisonViewPanel/ItemInspectorPanel` so its serialized bindings and comparison methods remain intact. `PickerGridContent` remains a direct child of `PickerModalWindow`. This retained overlay is separate from the active shared-inventory route.

---

## 3. Interactive UX States & Navigation Flow

```
[ Primary Equipment Screen ]
       |
       |-- (D-Pad / WASD / Arrow Keys) -> Move cursor across 28 slots (ConfigureSlotNavigation)
       |-- (Pointer Enter / Hover) ------> Immediate slot focus & inspector card update
       |-- (Press Delete / Gamepad X) ---> Unequip selected slot (EquipmentUiController.UnequipSelectedSlot)
       |-- (Press Q / Escape / B) -------> Close equipment screen & return to pause / gameplay
       |
       v  (Press Enter / Gamepad A / Click)
[ Shared Inventory Selection Route ]
       |
       |-- Populates filtered candidate items (EquipmentGroup compatibility)
       |-- (Navigate Candidate Grid) ----> Existing inventory item inspection
       |-- (Press Enter / Gamepad A) ----> Assign item to slot & refresh loadout
       |-- (Press Q / Escape / B) -------> Cancel picker & restore focused slot
```

### State 1: Primary Equipment Navigation & Inspection
- The user navigates the 28 equipment slots using D-Pad, WASD, Arrow keys, or Mouse Hover.
- `ConfigureSlotNavigation()` establishes explicit 2D neighbor relationships (`_up`, `_down`, `_left`, `_right`) between rows of varying widths (3, 3, 4, 4, 4, 5, 5).
- On focus (`OnSelect` / `OnPointerEnter`), `EquipmentSlotUI` fires `SlotFocused`, calling `EquipmentUiController.FocusSlot(slotId)`.
- Zone 3 (shared LoreCard) refreshes with the focused item; Character Stats refreshes through the existing loadout flow.

### State 2: Shared Inventory Selection
- Pressing `Enter` / Gamepad `A` / clicking an unlocked slot invokes `SubmitSlot(slotId)`.
- Opens candidate items filtered by `EquipmentSlotCatalog.GetCompatibilityGroup(slotId)`:
  - `RightHand1..3` & `LeftHand1..3` $\rightarrow$ Armaments (Weapons / Shields)
  - `Arrow1..2` $\rightarrow$ Arrows
  - `Bolt1..2` $\rightarrow$ Bolts
  - `Head`, `Chest`, `Arms`, `Legs` $\rightarrow$ Corresponding Armor types
  - `Talisman1..4` $\rightarrow$ Talismans
  - `QuickItem1..10` $\rightarrow$ Consumables
- `SubmitSlot` raises `InventoryRequested`; the navigation route opens the existing inventory UI for the selected equipment slot. Candidate inspection uses the inventory presentation.
- Submitting a candidate calls `EquipmentUiController.SelectItem(entryId)` $\rightarrow$ `EquipmentComponent.Assign(slotId, entryId)`, updating character attributes, weapon models, and UI slots.
- `FocusCandidate`, `UpdateComparison`, and the modal picker remain available to the retained legacy picker. They do not describe the active shared-inventory route.

### State 3: Unequipping & Slot Clearing
- While focusing an assigned slot, pressing `Delete` (Keyboard) or `Gamepad X` triggers `UnequipAction`.
- `EquipmentUiController.UnequipSelectedSlot()` invokes `EquipmentComponent.Unequip(slotId)`.
- Fires `EquipmentComponent.SlotChanged`, clearing the slot visual and refreshing loadout calculations.

---

## 4. Visual UI Layout Hierarchy

### Prefab GameObject & CanvasGroup Structure (`EquipmentUi.prefab`)

- Root: existing `EquipmentUi`, `RectTransform`, and `CanvasGroup`.
- Background layers: inventory vignette and center fade.
- `HeaderBar`: inventory frame, equipment emblem, and editable TMP title.
- `MainContentPanel`: equipment grid, linked `LoreCard.prefab`, and linked `CharacterStats.prefab`.
- `InventoryPickerOverlay/PickerModalWindow`: preserved `PickerGridContent` and comparison inspector.
- `BottomActionBar`: current action prompts.

The shared inventory prefab source assets are unchanged. Equipment layout and presentation overrides are stored on their nested instances. The equipment prefab GUID and its existing `EquipmentUi` Addressables mapping are preserved.

### Component Layer Hierarchy (`EquipmentSlotUI`)

1. Logical 88-unit body and pointer target.
2. Independent background and border; focus uses the gold selection boundary.
3. Non-interactive 120-unit selection effect, independent of logical bounds.
4. 64-unit item/empty icon. Empty icons use alpha 0.45; occupied icons use full opacity.
5. Independent equipped badge, visible for an occupied unlocked slot.
6. Quantity text, visible for an unlocked stackable item with quantity greater than one.
7. Existing lock overlay; locked slots suppress item/ghost, badge, and quantity.

Focus state is retained when `Bind` refreshes an item. `EquipmentUi.Show` restores the previously selected slot, using RightHand1 only for the initial selection.

---

## 5. Visual Language, Typography & Color Palette

### Color Palette Reference

| Token Name | Hex Code | Visual Application & UX Context |
| :--- | :--- | :--- |
| **Background / Panel** | `#171916` / `#20231D` | Screen backdrop and main container panels. |
| **Slot / Control Border** | `#272B23` / `#858B76` | Slot fill and default unselected boundary. |
| **Active Focus Gold** | `#C9B67C` | Active selection border and focus glow. |
| **Parchment Primary / Secondary** | `#E6E1CE` / `#ADAFA0` | Titles and body text / supporting text. |
| **Stat Buff / Improvement** | `#62B5F6` / Soft Blue | Positive attack comparison deltas (`ColorStatBuff`). |
| **Stat Nerf / Penalty** | `#EF5350` / Soft Red | Negative attack comparison deltas (`ColorStatNerf`). |
| **Unmet Requirement** | `#E53935` / Solid Red | Stat requirement text when player stats are insufficient (`ColorUnmetRequirement`). |

### Typography & Styling
- **Fonts:** Cinzel for display and headings, EB Garamond for body and stat rows, Inter for slot utilities and counters; all remain editable TMP text.
- **Numbers & Counters:** Fixed numeric widths (tabular figures) to eliminate jitter when updating real-time stats.

---

## 6. Technical C# Implementation & DI Wiring

### Core Classes & Architecture Map

| Class / Interface | Namespace | Role & Responsibilities |
| :--- | :--- | :--- |
| [`EquipmentUi`](../../../../Assets/Scripts/Ui/Equipment/EquipmentUi.cs) | `SoulsLike.Ui.Equipment` | Root View component (inherits `BaseUi`). Manages 28 slot bindings, inspector updates, picker overlay, and navigation graphs. |
| [`EquipmentUiController`](../../../../Assets/Scripts/Ui/Equipment/EquipmentUiController.cs) | `SoulsLike.Ui.Equipment` | Controller / Presenter. Handles user input, slot focus/selection, item assignment/unequipping, and character stat calculations. |
| [`IEquipmentPresenter`](../../../../Assets/Scripts/Ui/Equipment/IEquipmentPresenter.cs) | `SoulsLike.Ui.Equipment` | Presenter contract defining `FocusSlot`, `SubmitSlot`, `FocusCandidate`, `SubmitCandidate`, `UnequipSelectedSlot`, `CancelPicker`, and `CloseEquipment`. |
| [`IEquipmentRoute`](../../../../Assets/Scripts/Ui/Equipment/IEquipmentRoute.cs) | `SoulsLike.Ui.Equipment` | Pause navigation route interface (inherits `IPauseNavigationRoute`). Exposes `InventoryRequested` event and `SelectItem` method. |
| [`EquipmentSlotUI`](../../../../Assets/Scripts/Ui/Equipment/EquipmentSlotUI.cs) | `SoulsLike.Ui.Equipment` | Interactive slot view component handling Unity EventSystem events (`ISelectHandler`, `IDeselectHandler`, `IPointerClickHandler`, `ISubmitHandler`, `IMoveHandler`). |
| [`CharacterStatsUi`](../../../../Assets/Scripts/Ui/Inventory/CharacterStatsUi.cs) | `SoulsLike.Ui.Inventory` | Reusable character attribute and combat stat panel shared between Equipment and Inventory screens. |
| [`EquipmentComponent`](../../../../Assets/Scripts/Components/Equipment/EquipmentComponent.cs) | `SoulsLike.Entities.Character.Components.Equipment` | Domain component managing equipped inventory entries, slot assignments, active weapon cycling, and hand modes. |
| [`EquipmentSlotCatalog`](../../../../Assets/Scripts/Components/Equipment/EquipmentSlots.cs) | `SoulsLike.Entities.Character.Components.Equipment` | Static catalog defining slot groups, compatibility groups, cyclability, and display names for all 28 slots. |

### VContainer DI Registration & Lifecycle
`EquipmentUiController` is registered as a Singleton in `CharacterFactory.cs` under the player's `CharacterScope`:

```csharp
// Registered in CharacterFactory.cs
builder.Register<EquipmentUiController>(Lifetime.Singleton)
       .AsSelf()
       .AsImplementedInterfaces();
```

- **Instantiation:** Created lazily or on initialize via `_view = CreateUi<EquipmentUi>()` through `IUiService`.
- **Addressables:** Prefab is registered in Addressables group `Ui` with address `"EquipmentUi"` and mapped in `AssetMappingData.asset`.
- **Event Synchronization:** Subscribes to `_equipment.SlotChanged`, `_equipment.LoadoutChanged`, and `_inventory.Model.Changed` to automatically synchronize UI state with runtime domain changes.

### Input Mapping Reference

| Input Action | Primary Keyboard Binding | Gamepad Binding | Handler |
| :--- | :--- | :--- | :--- |
| **Open Equipment** | `<Keyboard>/o` | `<Gamepad>/start` | `PauseNavigationUiController.Tick()` |
| **Unequip Slot** | `<Keyboard>/delete` | `<Gamepad>/buttonWest` (`X`) | `EquipmentUiController.Tick()` $\rightarrow$ `UnequipSelectedSlot()` |
| **UI Back / Cancel** | `<Keyboard>/q` / `<Keyboard>/escape` | `<Gamepad>/buttonEast` (`B`) | `PauseNavigationUiController.HandleUiBack()` |
| **Slot Navigation** | Arrow Keys / WASD | D-Pad / Left Stick | `EquipmentSlotUI.OnMove()` |
| **Select / Confirm** | `Enter` / Left Click | `<Gamepad>/buttonSouth` (`A`) | `EquipmentSlotUI.OnSubmit()` / `OnPointerClick()` |

---

*End of Equipment UI/UX Architecture Guide.*
