# Equipment UI/UX Architecture Guide

This guide breaks down the structure, spatial layout, UX interaction states, visual design specifications, and C# Unity implementation details for the Souls-like Equipment UI.

---

## 1. UI/UX Design Philosophy & Architectural Overview

The equipment interface follows FromSoftware's dark fantasy minimalist aesthetic:
- **Low Clutter, High Information Density:** Complex RPG calculations and equipment slots are neatly organized into modular panels that update dynamically without obscuring gameplay context.
- **Immediate Feedback Loop:** Every hover, selection, or slot assignment instantly updates inspector cards, candidate comparisons, and global character stats (Equip Load, Weight, Attack Ratings).
- **Diegetic Medieval Palette:** Dark slate/stone container backgrounds (`#121417`, `#1A1A18`) with subtle borders (`#3A342B`), framed by warm gold focus accents (`#C5A059`) and parchment typography (`#E6DFD3` / `#E6E1C5`).
- **Gamepad-First Spatial Navigation:** Grid-based multi-row navigation explicitly configured for D-pad / WASD movement with clear active selection borders and seamless mouse/pointer hover support.
- **Decoupled MVP / Controller Pattern:** Built on `EquipmentUi` (View), `EquipmentUiController` (Controller / Presenter), `EquipmentSlotUI` (Slot View), and `CharacterStatsUi` (Shared Stats View), resolved and injected via VContainer.

---

## 2. Spatial UI Breakdown (What is Located Where)

The Equipment Screen is divided into **four main visual zones** plus an **Inventory Picker Overlay modal** rendered over a dimmed live game world.

```
+-----------------------------------------------------------------------------------------------+
| ZONE 1: TOP HEADER (Title: "EQUIPMENT", Player Summary: "Runes 45,210")                       |
+-------------------------------------------------------------+---------------------------------+
| ZONE 2: EQUIPMENT SLOTS GRID (Left Side - 28 Slots)         | ZONE 4: CHARACTER STATUS        |
|                                                             |         & CALCULATIONS PANEL    |
| [R-Arm 1]   [R-Arm 2]   [R-Arm 3]                           | (Right Side: CharacterStatsUi)  |
| [L-Arm 1]   [L-Arm 2]   [L-Arm 3]                           | - Base Attributes (8 stats)     |
| [Arrow 1]   [Arrow 2]   [Bolt 1]   [Bolt 2]                 |   (Vig, Min, End, Str, Dex,     |
| [Head]      [Chest]     [Arms]     [Legs]                   |    Int, Fth, Arc)               |
| [Talisman1] [Talisman2] [Talisman3] [Talisman4]             | - Right Armament Attack Power   |
| [Quick 1]   [Quick 2]   [Quick 3]  [Quick 4]  [Quick 5]     | - Left Armament Attack Power    |
| [Quick 6]   [Quick 7]   [Quick 8]  [Quick 9]  [Quick 10]    | - Equip Load (Current / Max)    |
|                                                             | - Poise                         |
| +---------------------------------------------------------+ |                                 |
| | ZONE 3: ITEM INSPECTOR CARD (Middle / Lower Left)       | |                                 |
| | Icon, Name, Category, Skill, FP Cost, Physical Attack,  | |                                 |
| | Requirements (Str/Dex/Int/Fth/Arc), Scaling, Weight     | |                                 |
| +---------------------------------------------------------+ |                                 |
+-------------------------------------------------------------+---------------------------------+
| ZONE 5: BOTTOM ACTION BAR (Select, Back, Remove, Switch Display)                              |
+-----------------------------------------------------------------------------------------------+
```

---

### Zone 1: Top Navigation Bar & Header
- **Location:** Top edge of the screen (Full Width).
- **Elements:**
  - **Screen Title (`screenTitleText`):** Fixed label displaying `"EQUIPMENT"`.
  - **Player Summary (`playerSummaryText`):** Bound via `DisplayPlayerSummary(Character character)` displaying held currency: `Runes {character.HeldCurrency:N0}`.

---

### Zone 2: Equipment Grid Panel (Left Side)
Organized into 6 logical equipment groups across 7 navigation rows (28 total slots defined by `EquipmentSlotId`). Each slot is an `EquipmentSlotUI` component showing the equipped item sprite, stack quantity, lock overlay, or empty slot placeholder.

1. **Right-Hand Armaments (Row 1 - 3 Slots):** `RightHand1`, `RightHand2`, `RightHand3` (Weapons/Catalysts/Shields in Right Hand).
2. **Left-Hand Armaments (Row 2 - 3 Slots):** `LeftHand1`, `LeftHand2`, `LeftHand3` (Shields/Weapons/Catalysts in Left Hand).
3. **Ammunition (Row 3 - 4 Slots):** `Arrow1`, `Arrow2`, `Bolt1`, `Bolt2` (Projectiles for Bows & Crossbows).
4. **Apparel / Armor (Row 4 - 4 Slots):** `Head`, `Chest`, `Arms`, `Legs`.
5. **Talismans (Row 5 - 4 Slots):** `Talisman1`, `Talisman2`, `Talisman3`, `Talisman4`.
6. **Quick Items / Belt (Rows 6 & 7 - 10 Slots in 2x5 Grid):** `QuickItem1` through `QuickItem5` (Row 6) and `QuickItem6` through `QuickItem10` (Row 7).

---

### Zone 3: Item Inspector Card (Middle / Lower Left)
Displays detailed specifications of the **currently highlighted slot or candidate item** (bound via `EquipmentUi.DisplaySlot()`):
- **Item Graphic (`inspectorItemIcon`):** Item sprite thumbnail (disabled when slot is empty).
- **Item Title (`inspectorItemName`):** Full display name, or placeholder `[Empty {SlotDisplayName}]` when unequipped.
- **Category (`inspectorItemCategory`):** Item type label (`item.ItemType.ToString()`).
- **Weapon Skill & FP Cost (`inspectorSkillName`, `inspectorSkillFpCost`):** Equipped skill name and focus point cost (`FP {stats.SkillFocusCost}`).
- **Attack Rating Summary (`inspectorAttackSummary`):** Physical attack power (`Physical {stats.PhysicalAttack}`).
- **Stat Requirements (`inspectorReqStr`, `inspectorReqDex`, `inspectorReqInt`, `inspectorReqFth`, `inspectorReqArc`):** Required attribute thresholds. Rendered in **Red** (`ColorUnmetRequirement` `#E53935`) if the character's base attribute is below the required value, otherwise rendered in **Parchment Primary** (`ColorParchmentPrimary` `#E6DFD3`).
- **Attribute Scaling (`inspectorScalingText`):** Formatted scaling grades (`STR {grade}  DEX {grade}`).
- **Item Weight (`inspectorWeightText`):** Numerical weight value (`Weight {item.Weight:F1}`).
- **Live Comparison Delta (`EquipmentUi.UpdateComparison`):** When previewing candidate gear, modifies attack and weight strings:
  - Attack: `Physical {candidateAttack} ({attackDelta:+#;-#;0})`
  - Weight: `Weight Δ {weightDelta:+0.0;-0.0;0.0}`

---

### Zone 4: Character Status & Calculations Panel (Right Side)
Rendered by the reusable `CharacterStatsUi` component, updating in real time on loadout changes:
- **Character Attributes (8 Stats):**
  - `vigorText`: Vigor
  - `mindText`: Mind
  - `enduranceText`: Endurance
  - `strengthText`: Strength
  - `dexterityText`: Dexterity
  - `intelligenceText`: Intelligence
  - `faithText`: Faith
  - `arcaneText`: Arcane
- **Attack Ratings:**
  - `rightAttackText`: Right Armament Attack Power (supports live delta comparison)
  - `leftAttackText`: Left Armament Attack Power
- **Equip Load (`equipLoadText`):**
  - Displays `{equipWeight:F1} / {maxEquipWeight:F1}`
  - Maximum load formula: `maxEquipWeight = 45.0f + (character.Attributes.Endurance * 1.5f)`
- **Poise (`poiseText`):**
  - Poise rating (currently initialized to `0`).

---

### Zone 5: Bottom Action Bar (Controller Legend)
- **Location:** Bottom of the screen (`actionPromptsText`).
- **Text:** `"Select   Back   Remove   Switch Display"`.
- **Action Bindings:**
  - `[Enter / Gamepad A / Left Click]`: Select / Open item picker for focused slot.
  - `[Delete / Gamepad X]`: Unequip item from selected slot (`UnequipAction`).
  - `[Q / Escape / Gamepad B]`: Back / Close screen (`UiBackAction` / `PauseNavigationUiController`).
  - `[F / Gamepad RS]`: Switch display / Toggle simple view.

---

### Inventory Picker Overlay & Stat Comparison Modal
- **Container (`inventoryPickerOverlay`):** Modal window embedded within `EquipmentUi` (or routed via `PauseNavigationUiController` to `InventoryUiController.Open`).
- **Grid Container (`inventoryPickerGridContainer`):** 5-column layout populated with candidate `InventorySlotUI` instances matching the target slot's `EquipmentGroup` compatibility.
- **Comparison Panel (`comparisonPanel`):** Displays side-by-side attack power and weight differences when focusing candidate items before confirming equipment.

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
[ Inventory Selection Modal / Picker ]
       |
       |-- Populates filtered candidate items (EquipmentGroup compatibility)
       |-- (Navigate Candidate Grid) ----> Live hover stat comparison (UpdateComparison)
       |                                   - Attack delta: (+5) in Blue / (-12) in Red
       |                                   - Weight delta: Δ +2.5
       |-- (Press Enter / Gamepad A) ----> Assign item to slot & refresh loadout
       |-- (Press Q / Escape / B) -------> Cancel picker & restore focused slot
```

### State 1: Primary Equipment Navigation & Inspection
- The user navigates the 28 equipment slots using D-Pad, WASD, Arrow keys, or Mouse Hover.
- `ConfigureSlotNavigation()` establishes explicit 2D neighbor relationships (`_up`, `_down`, `_left`, `_right`) between rows of varying widths (3, 3, 4, 4, 4, 5, 5).
- On focus (`OnSelect` / `OnPointerEnter`), `EquipmentSlotUI` fires `SlotFocused`, calling `EquipmentUiController.FocusSlot(slotId)`.
- Zone 3 (Item Inspector) and Zone 4 (Character Stats) refresh immediately with the slot's current item details.

### State 2: Inventory Selection Modal & Live Stat Comparison
- Pressing `Enter` / Gamepad `A` / clicking an unlocked slot invokes `SubmitSlot(slotId)`.
- Opens candidate items filtered by `EquipmentSlotCatalog.GetCompatibilityGroup(slotId)`:
  - `RightHand1..3` & `LeftHand1..3` $\rightarrow$ Armaments (Weapons / Shields)
  - `Arrow1..2` $\rightarrow$ Arrows
  - `Bolt1..2` $\rightarrow$ Bolts
  - `Head`, `Chest`, `Arms`, `Legs` $\rightarrow$ Corresponding Armor types
  - `Talisman1..4` $\rightarrow$ Talismans
  - `QuickItem1..10` $\rightarrow$ Consumables
- Focusing a candidate item triggers `EquipmentUiController.FocusCandidate(entryId)`, calculating deltas:
  $$\Delta \text{Attack} = \text{Candidate.PhysicalAttack} - \text{Current.PhysicalAttack}$$
  $$\Delta \text{Weight} = \text{Candidate.Weight} - \text{Current.Weight}$$
- Submitting a candidate calls `EquipmentUiController.SelectItem(entryId)` $\rightarrow$ `EquipmentComponent.Assign(slotId, entryId)`, updating character attributes, weapon models, and UI slots.

### State 3: Unequipping & Slot Clearing
- While focusing an assigned slot, pressing `Delete` (Keyboard) or `Gamepad X` triggers `UnequipAction`.
- `EquipmentUiController.UnequipSelectedSlot()` invokes `EquipmentComponent.Unequip(slotId)`.
- Fires `EquipmentComponent.SlotChanged`, clearing the slot visual and refreshing loadout calculations.

---

## 4. Visual UI Layout Hierarchy

### Prefab GameObject & CanvasGroup Structure (`EquipmentUi.prefab`)

```
[EquipmentUi] (Root: RectTransform, CanvasGroup, EquipmentUi)
 ├── [HeaderPanel]
 │    ├── TitleText ("EQUIPMENT")
 │    └── PlayerSummaryText ("Runes 45,210")
 ├── [MainContentPanel]
 │    ├── [EquipmentGridPanel] (Transform: equipmentGridContainer)
 │    │    ├── Row 1 (RightHandSlots: 3x EquipmentSlotUI)
 │    │    ├── Row 2 (LeftHandSlots: 3x EquipmentSlotUI)
 │    │    ├── Row 3 (AmmoSlots: 4x EquipmentSlotUI)
 │    │    ├── Row 4 (ArmorSlots: 4x EquipmentSlotUI)
 │    │    ├── Row 5 (TalismanSlots: 4x EquipmentSlotUI)
 │    │    ├── Row 6 (QuickItemSlots 1..5: 5x EquipmentSlotUI)
 │    │    └── Row 7 (QuickItemSlots 6..10: 5x EquipmentSlotUI)
 │    ├── [ItemInspectorPanel]
 │    │    ├── InspectorItemIcon (Image)
 │    │    ├── InspectorItemName (TMP_Text)
 │    │    ├── InspectorItemCategory (TMP_Text)
 │    │    ├── InspectorSkillName & InspectorSkillFpCost (TMP_Text)
 │    │    ├── InspectorAttackSummary (TMP_Text)
 │    │    ├── InspectorRequirementsContainer (5x TMP_Text: Str, Dex, Int, Fth, Arc)
 │    │    ├── InspectorScalingText (TMP_Text)
 │    │    └── InspectorWeightText (TMP_Text)
 │    └── [CharacterStatsPanel] (CharacterStatsUi component)
 │         ├── AttributeValuesContainer (8x TMP_Text: Vig, Min, End, Str, Dex, Int, Fth, Arc)
 │         ├── RightAttackText (TMP_Text)
 │         ├── LeftAttackText (TMP_Text)
 │         ├── EquipLoadText (TMP_Text)
 │         └── PoiseText (TMP_Text)
 ├── [InventoryPickerOverlay] (GameObject: inventoryPickerOverlay)
 │    ├── [InventoryPickerGridContainer] (5-column Grid: Transform)
 │    └── [ComparisonPanel] (GameObject: comparisonPanel)
 └── [BottomActionBar]
      └── ActionPromptsText (TMP_Text)
```

### Component Layer Hierarchy (`EquipmentSlotUI`)
Each equipment slot widget is built with layered MPUIKit and TextMeshPro components:
1. **`borderImage` (`MPImage`):** Outer styled frame (`normalBorderColor` `#1A1A18`).
2. **`selectionHighlight` (`MPImage`):** Golden focus highlight border (`#C5A059`), enabled on focus.
3. **`iconImage` (`Image`):** High-resolution item icon sprite.
4. **`quantityText` (`TMP_Text`):** Stack counter (active when item is stackable and quantity > 1).
5. **`lockOverlay` (`GameObject`):** Padlock graphic displayed if the slot is locked.

---

## 5. Visual Language, Typography & Color Palette

### Color Palette Reference

| Token Name | Hex Code | Visual Application & UX Context |
| :--- | :--- | :--- |
| **Slate Background** | `#121417` | Screen backdrop and main container panels. |
| **Slot Frame Border** | `#1A1A18` / `#3A342B` | Default unselected slot borders (`normalBorderColor`). |
| **Active Focus Gold** | `#C5A059` / `#D4AF37` | Active selection border and focus glow (`selectedBorderColor`). |
| **Parchment Primary** | `#E6DFD3` / `#E6E1C5` | Primary text for item titles, normal stats, and labels (`ColorParchmentPrimary`). |
| **Stat Buff / Improvement** | `#62B5F6` / Soft Blue | Positive attack comparison deltas (`ColorStatBuff`). |
| **Stat Nerf / Penalty** | `#EF5350` / Soft Red | Negative attack comparison deltas (`ColorStatNerf`). |
| **Unmet Requirement** | `#E53935` / Solid Red | Stat requirement text when player stats are insufficient (`ColorUnmetRequirement`). |

### Typography & Styling
- **Font Asset:** Cinzel / TextMeshPro serif tabular font asset.
- **Numbers & Counters:** Fixed numeric widths (tabular figures) to eliminate jitter when updating real-time stats.

---

## 6. Technical C# Implementation & DI Wiring

### Core Classes & Architecture Map

| Class / Interface | Namespace | Role & Responsibilities |
| :--- | :--- | :--- |
| [`EquipmentUi`](../../Assets/Scripts/Ui/Equipment/EquipmentUi.cs) | `SoulsLike.Ui.Equipment` | Root View component (inherits `BaseUi`). Manages 28 slot bindings, inspector updates, picker overlay, and navigation graphs. |
| [`EquipmentUiController`](../../Assets/Scripts/Ui/Equipment/EquipmentUiController.cs) | `SoulsLike.Ui.Equipment` | Controller / Presenter. Handles user input, slot focus/selection, item assignment/unequipping, and character stat calculations. |
| [`IEquipmentPresenter`](../../Assets/Scripts/Ui/Equipment/IEquipmentPresenter.cs) | `SoulsLike.Ui.Equipment` | Presenter contract defining `FocusSlot`, `SubmitSlot`, `FocusCandidate`, `SubmitCandidate`, `UnequipSelectedSlot`, `CancelPicker`, and `CloseEquipment`. |
| [`IEquipmentRoute`](../../Assets/Scripts/Ui/Equipment/IEquipmentRoute.cs) | `SoulsLike.Ui.Equipment` | Pause navigation route interface (inherits `IPauseNavigationRoute`). Exposes `InventoryRequested` event and `SelectItem` method. |
| [`EquipmentSlotUI`](../../Assets/Scripts/Ui/Equipment/EquipmentSlotUI.cs) | `SoulsLike.Ui.Equipment` | Interactive slot view component handling Unity EventSystem events (`ISelectHandler`, `IDeselectHandler`, `IPointerClickHandler`, `ISubmitHandler`, `IMoveHandler`). |
| [`CharacterStatsUi`](../../Assets/Scripts/Ui/Inventory/CharacterStatsUi.cs) | `SoulsLike.Ui.Inventory` | Reusable character attribute and combat stat panel shared between Equipment and Inventory screens. |
| [`EquipmentComponent`](../../Assets/Scripts/Components/Equipment/EquipmentComponent.cs) | `SoulsLike.Entities.Character.Components.Equipment` | Domain component managing equipped inventory entries, slot assignments, active weapon cycling, and hand modes. |
| [`EquipmentSlotCatalog`](../../Assets/Scripts/Components/Equipment/EquipmentSlots.cs) | `SoulsLike.Entities.Character.Components.Equipment` | Static catalog defining slot groups, compatibility groups, cyclability, and display names for all 28 slots. |

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
