# Inventory UI/UX Architecture Guide

A detailed UX and technical C# architecture breakdown of the game's Inventory UI, view state controller, cell slot widgets, real-time stat delta engine, and VContainer integration.

---

## 1. Executive Overview & Design Philosophy

The game's inventory UI utilizes a **3-column diegetic panel layout** anchored over a semi-transparent dark backdrop.

### Core Design Goals
1. **High Stat Density without Visual Overwhelm:** Simultaneously presents item grid navigation, item metadata/art, scaling stats, and full character attributes.
2. **Diegetic Context & World Awareness:** The backdrop maintains semi-transparency and dark vignetting, allowing the player to remain aware of their in-game surroundings and lighting.
3. **Modal & Ergonomic Navigation:** Designed primarily for controller D-pad / bumper navigation and keyboard/mouse with immediate visual and stat feedback on item focus.
4. **Contextual View States:** Managed by `InventoryViewStateController`, allowing instant toggling between standard dual-panel view, extended narrative lore text, and simple/compact view for visual character inspection.
5. **Dual-Mode Operation:** Functions as both a standalone categorized inventory browser and a modal picker overlay (`IInventoryRoute.Open`) invoked by the Equipment system.

---

## 2. Spatial Layout & Screen Breakdown

The interface is structured around a three-column vertical core bounded by a persistent top header bar and a bottom keymap navigation footer.

```
+----------------------------------------------------------------------------------------------------+
|  [Header Bar] Title ("INVENTORY") | Primary Category Tabs | Sub-Category Icons                     |
+------------------------------------+----------------------------------+----------------------------+
|                                    |                                  |                            |
|  COLUMN 1: ITEM GRID               | COLUMN 2: ITEM DETAILS & LORE    | COLUMN 3: CHARACTER STATS  |
|  (~30% Width - 5xN Grid)           | (~40% Width - ItemDetails / Lore)| (~30% Width - Stats Sheet) |
|                                    |                                  |                            |
|  - Scrollable Grid (5 Columns)     | - High-Res Item Artwork          | - Base Attributes (8 stats)|
|  - Category / Subcategory Filtering| - Item Type / Weight / Skill     |   (Vig, Min, End, Str, Dex,|
|  - Equipped Status Badges (R1, L1) | - Attack Power Breakdown         |    Int, Fth, Arc)          |
|  - Stack Quantity Counters (x99)   | - Guard Boost                    | - Right Arm Attack & Delta |
|  - Unmet Requirement Overlays      | - Attribute Scaling (S..E)       | - Left Arm Attack          |
|  - Ash of War / Skill Badges       | - Stat Requirements Benchmarks   | - Equip Load (Cur / Max)   |
|                                    | - Lore Description Card (State 2)| - Poise                    |
|                                    |                                  |                            |
+------------------------------------+----------------------------------+----------------------------+
|  [Footer Bar] Legend: Select (Enter/A) | Back (Q/B) | Toggle Lore (R/Y) | Simple View (F/RS)       |
+----------------------------------------------------------------------------------------------------+
```

### Layout Specifications

| UI Region | Width Ratio | Primary Responsibilities | Unity Component / Hierarchy |
| :--- | :--- | :--- | :--- |
| **Top Header Bar** | 100% | Screen title (`screenTitleText`), primary category tabs (`primaryCategoryTabContainer`), sub-category icons (`subCategoryIconContainer`). | Header transform, horizontal layout group. |
| **Left Column (Item Grid)** | ~30% | Scrollable inventory cell grid (`gridScrollRect`), 5-column slot arrangement (`gridContentParent`), cell selection. | `CanvasGroup` (`gridColumnGroup`), `InventorySlotUI` instances. |
| **Middle Column (Item Details)** | ~40% | Full specs of currently focused item (artwork, weapon type, weight, attack power, scaling grades, requirements, skill name/cost). | `CanvasGroup` (`detailsColumnGroup`), `ItemDetailsUi`. |
| **Middle Column (Lore Card)** | ~40% | Full item narrative lore text and background card (swapped in Lore View state). | `CanvasGroup` (`loreCardGroup`), `LoreCardUi`. |
| **Right Column (Character Stats)**| ~30% | Live character attributes (Vigor through Arcane), real-time attack rating delta comparisons, current/max equip load, poise. | `CanvasGroup` (`statsColumnGroup`), `CharacterStatsUi`. |
| **Bottom Footer Bar** | 100% | Input action keymap legend (`legendSelectText`, `legendBackText`, `legendToggleLoreText`, `legendSimpleViewText`). | Footer transform, horizontal prompt pair. |

---

### Header Bar & Category Navigation
- **Primary Categories (`InventoryPrimaryCategory`):**
  - `Weapons` (0): Weapons, Shields, Ammunition
  - `Armor` (1): Head, Chest, Arms, Leg armor
  - `Talisman` (2): Talismans & accessories
  - `Consumables` (3): Consumables, Crafting Materials, Runes/Currency
  - `KeyItems` (4): Key items, quest items, unlocking tools
- **Sub-Categories (`InventorySubCategory`):**
  - `MeleeWeapon`, `RangedWeapon`, `Shield`, `HeadArmor`, `ChestArmor`, `ArmArmor`, `LegArmor`, `Talisman`, `CraftingMaterial`, `ConsumableItem`, `KeyItem`.

---

## 3. Visual Language, Typography & Color Palette

The visual style follows an authentic dark-fantasy parchment aesthetic using warm gold highlights, muted slate framing, high contrast, and crisp serif typography.

### Color Palette Reference

| Token Name | Hex / RGB Code | Visual Application & UX Context |
| :--- | :--- | :--- |
| **Background Dark Vignette** | `#0C0C0C` (85% Alpha) | Dark overlay shading out the center screen while keeping margins partially visible. |
| **Frame & Panel Fill** | `#141412` / `#1A1A18` | Container background fill for item cards, stat blocks, and column headers. |
| **Parchment Primary** | `#E6E1C5` / `(0.902, 0.882, 0.773)` | Primary text color for item titles, normal stats, and labels (`ColorParchmentPrimary`). |
| **Label / Divider Gray** | `#5C584E` / `#3D3A33` | Section borders, grid slot borders, subtle field dividing lines. |
| **Golden Focus Accent** | `#D4AF37` / `#C5A059` | Focus frame highlight around selected cell, active category tab border. |
| **Stat Buff / Improvement** | `#62B5F6` / `(0.384, 0.710, 0.965)` | Stat increases in character attack comparison (`ColorStatBuff`). |
| **Stat Nerf / Penalty** | `#EF5350` / `(0.937, 0.325, 0.314)` | Stat decreases in character attack comparison (`ColorStatNerf`). |
| **Unmet Requirement** | `#E53935` / `(0.898, 0.224, 0.208)` | Red stat requirement labels and cell overlay tint (`ColorUnmetRequirement`). |

### Typography & Styling Guidelines
- **Font Asset:** Cinzel / TextMeshPro serif tabular font asset.
- **Stat Values & Body Numbers:** Fixed numeric widths (tabular figures) to avoid layout jitter during grid navigation.

---

## 4. Cell UI Architecture (Item Grid Slots)

Each item grid cell is a self-contained interactive widget driven by `InventorySlotUI`.

```
+-----------------------------------+
| [R1]                     [!] (Red)|
|                                   |
|             [ ITEM ]              |
|             [ ICON ]              |
|                                   |
| [Ash]                        x99  |
+-----------------------------------+
  ^-- Golden Border Highlight when Focused
```

### Component Layer Hierarchy (`InventorySlotUI` - Back to Front)

1. **Background Box (`backgroundBox` - `MPImage`):**
   - Dark slate filled box with a subtle rounded border.
2. **Focus / Selection Frame (`focusFrame` - `MPImage`):**
   - Hidden by default. Enabled when the cell receives EventSystem selection (`OnSelect`).
   - Styled with golden highlight border (`#D4AF37`).
3. **Item Icon (`itemIcon` - `Image`):**
   - High-resolution item thumbnail sprite (`item.Icon`).
4. **Equipped Status Badge (`equippedBadgeBox` - `MPImage` & `equippedBadgeText` - `TMP_Text`):**
   - Top-Left alignment anchor.
   - Displays short equipment slot labels when assigned (e.g. `R1`, `R2`, `L1`, `L2`, `Head`, `Chest`, `Q1`).
5. **Unmet Requirement Overlay (`unmetRequirementOverlay` - `MPImage`):**
   - Semi-transparent dark red tint layer activated when `!item.MeetsRequirements`.
6. **Stack Quantity Counter (`quantityText` - `TMP_Text`):**
   - Bottom-Right alignment anchor.
   - Displays `x{Quantity}` when item is stackable and quantity $> 1$.
7. **Ash of War / Skill Badge (`ashOfWarIcon` - `Image`):**
   - Bottom-Left alignment anchor. Displays the weapon's skill/Ash of War sprite if present.

---

## 5. Information & Stat Calculation Engine

### Dynamic Stat Comparison (Hover Feedback)
When navigating across cells in Column 1:
1. **Candidate Resolution:** `InventoryUiController.OnItemFocused(entryId)` resolves `InventoryItemViewData`.
2. **Inspector Update:** `ItemDetailsUi.Display(item, attributes)` and `LoreCardUi.Display(item)` update with item metadata, damage types, guard boost, scaling, and requirements.
3. **Attack Power Delta Computation:**
   - Evaluates candidate physical attack against currently active right-hand weapon:
     $$\Delta \text{Attack} = \text{Candidate.PhysicalAttack} - \text{ActiveRight.PhysicalAttack}$$
   - `CharacterStatsUi.UpdateRightAttackComparison(currentAttack, candidateAttack)` formats the text:
     - $\Delta > 0$: Displays `{candidateAttack} (+{delta})` in **Soft Blue** (`ColorStatBuff` `#62B5F6`).
     - $\Delta < 0$: Displays `{candidateAttack} ({delta})` in **Soft Red** (`ColorStatNerf` `#EF5350`).
     - $\Delta = 0$: Displays `{candidateAttack}` in **Parchment Primary** (`ColorParchmentPrimary` `#E6E1C5`).

### Attribute Requirement Evaluation
Evaluates all 5 character attributes against `ItemStatSnapshot.Requirements`:
- `Strength >= RequiredStrength`
- `Dexterity >= RequiredDexterity`
- `Intelligence >= RequiredIntelligence`
- `Faith >= RequiredFaith`
- `Arcane >= RequiredArcane`
- If any requirement fails:
  - `InventoryItemViewData.MeetsRequirements` evaluates to `false`.
  - Failing requirement labels turn **Solid Red** (`ColorUnmetRequirement`).
  - Cell's `unmetRequirementOverlay` activates.

---

## 6. UI/UX View State Machine

Managed by `InventoryViewStateController` manipulating `CanvasGroup` visibility and interaction:

```
                     +---------------------------+
                     |  STATE 0: DUAL-PANEL VIEW |
                     |  (Grid + Details + Stats) |
                     +-------------+-------------+
                                   |
                ToggleLore (R/Y)   | ToggleLore (R/Y)
                                   v
                     +-------------+-------------+
                     |  STATE 1: LORE VIEW       |
                     |  (Grid + Lore Text Card)  |
                     +-------------+-------------+
                                   |
             ToggleSimple (F/RS)   | ToggleSimple (F/RS)
                                   v
                     +-------------+-------------+
                     |  STATE 2: SIMPLE VIEW     |
                     |  (Grid Only / Compact)    |
                     +---------------------------+
```

### State Descriptions & CanvasGroup Orchestration

| View State (`InventoryViewState`) | `gridColumnGroup` | `detailsColumnGroup` | `loreCardGroup` | `statsColumnGroup` | UX Purpose |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`DualPanel` (0 - Default)** | **Active** | **Active** | Inactive | **Active** | Fast gear swapping, stat comparison, and attribute inspection. |
| **`LoreView` (1)** | **Active** | Inactive | **Active** | Inactive | Reading full item narrative lore descriptions and background flavor text. |
| **`SimpleView` (2)** | **Active** | Inactive | Inactive | Inactive | Minimal grid overlay for unobstructed visual inspection of character model in game world. |

---

## 7. Navigation, Focus Management & Input Mapping

### Grid Navigation Topology
- Fixed 5-column grid layout (`GRID_COLUMN_COUNT = 5`).
- `ConfigureGridNavigation()` links 2D directional navigation:
  - `Up`: `index - 5` (if $\ge 5$)
  - `Down`: `index + 5` (if $< \text{count}$)
  - `Left`: `index - 1` (if $\text{index} \pmod 5 > 0$)
  - `Right`: `index + 1` (if $\text{index} \pmod 5 < 4$ and $\text{index} + 1 < \text{count}$)
- First slot is auto-selected on show (`SelectFirstSlot()`).

### Input Mapping Reference

| Input Action | Primary Keyboard | Gamepad Binding | Handler |
| :--- | :--- | :--- | :--- |
| **Open Inventory** | `<Keyboard>/i` | `<Gamepad>/select` | `PauseNavigationUiController.Tick()` |
| **Toggle Lore View** | `<Keyboard>/r` | `<Gamepad>/buttonNorth` (`Y`) | `InventoryUiController.Tick()` $\rightarrow$ `ToggleLoreView()` |
| **Toggle Simple View** | `<Keyboard>/f` | `<Gamepad>/rightStickPress` (`RS`) | `InventoryUiController.Tick()` $\rightarrow$ `ToggleSimpleView()` |
| **UI Back / Cancel** | `<Keyboard>/q` / `<Keyboard>/escape` | `<Gamepad>/buttonEast` (`B`) | `PauseNavigationUiController.HandleUiBack()` |
| **Grid Navigation** | Arrow Keys / WASD | D-Pad / Left Stick | `InventorySlotUI.OnMove()` |
| **Select / Submit** | `Enter` / Left Click | `<Gamepad>/buttonSouth` (`A`) | `InventorySlotUI.OnSubmit()` / `OnPointerClick()` |

---

## 8. Technical C# Implementation & DI Wiring

### Core Classes & Architecture Map

| Class / Interface | Namespace | Role & Responsibilities |
| :--- | :--- | :--- |
| [`InventoryUi`](../../Assets/Scripts/Ui/Inventory/InventoryUi.cs) | `SoulsLike.Ui.Inventory` | Root View component (inherits `BaseUi`). Manages grid instantiation, column sub-views, and 5-column navigation. |
| [`InventoryUiController`](../../Assets/Scripts/Ui/Inventory/InventoryUiController.cs) | `SoulsLike.Ui.Inventory` | Controller / Presenter. Handles category filtering, item focus/submission, stat calculations, and view state actions. |
| [`IInventoryPresenter`](../../Assets/Scripts/Ui/Inventory/IInventoryPresenter.cs) | `SoulsLike.Ui.Inventory` | Presenter contract defining `SelectPrimaryCategory`, `SelectSubCategory`, `OnItemFocused`, `OnItemSubmitted`, `CloseInventory`, `ToggleLoreView`, `ToggleSimpleView`. |
| [`IInventoryRoute`](../../Assets/Scripts/Ui/Inventory/IInventoryRoute.cs) | `SoulsLike.Ui.Inventory` | Pause navigation route interface. Exposes `Open(itemTypes, onSelected)` for modal equipment item selection. |
| [`InventoryViewStateController`](../../Assets/Scripts/Ui/Inventory/InventoryViewStateController.cs) | `SoulsLike.Ui.Inventory` | View state switcher orchestrating `CanvasGroup` visibility for DualPanel, LoreView, and SimpleView. |
| [`InventorySlotUI`](../../Assets/Scripts/Ui/Inventory/InventorySlotUI.cs) | `SoulsLike.Ui.Inventory` | Grid slot widget handling icons, badges, stack quantities, unmet overlays, and EventSystem focus. |
| [`ItemDetailsUi`](../../Assets/Scripts/Ui/Inventory/ItemDetailsUi.cs) | `SoulsLike.Ui.Inventory` | Detailed item specs card (damage ratings, scaling grades, requirements, weapon skill). |
| [`LoreCardUi`](../../Assets/Scripts/Ui/Inventory/LoreCardUi.cs) | `SoulsLike.Ui.Inventory` | Lore text card displaying item artwork, title, effect summary, and extended narrative text. |
| [`CharacterStatsUi`](../../Assets/Scripts/Ui/Inventory/CharacterStatsUi.cs) | `SoulsLike.Ui.Inventory` | Character attributes and attack comparison panel. |
| [`InventoryItemViewData`](../../Assets/Scripts/Ui/Inventory/Data/InventoryItemViewData.cs) | `SoulsLike.Ui.Inventory.Data` | UI presentation data model computed from `InventoryEntry`, `ItemCatalog`, `EquipmentComponent`, and `CharacterAttributeStats`. |

### VContainer DI Registration & Lifecycle
`InventoryUiController` is registered as a Singleton in `CharacterFactory.cs` under the player's `CharacterScope`:

```csharp
// Registered in CharacterFactory.cs
builder.Register<InventoryUiController>(Lifetime.Singleton)
       .AsSelf()
       .AsImplementedInterfaces();
```

- **Instantiation:** Created lazily or on initialize via `_view = CreateUi<InventoryUi>()` through `IUiService`.
- **Addressables:** Prefab is registered in Addressables group `Ui` with address `"InventoryUi"` and mapped in `AssetMappingData.asset`.
- **Event Synchronization:** Subscribes to `_inventory.Model.Changed` and `_equipment.SlotChanged` to automatically refresh item counts, badges, and attributes.

---

*End of Inventory UI/UX Architecture Guide.*

