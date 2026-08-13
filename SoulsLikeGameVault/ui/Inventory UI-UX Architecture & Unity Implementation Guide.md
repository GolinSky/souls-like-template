# Inventory UI/UX Architecture & Unity Implementation Guide

A detailed UX breakdown of The game's inventory, equipment, and stat screens. This document provides spatial layout specifications, color palettes, grid slot prefab component hierarchies, stat delta logic, view state transitions, and Unity implementation guidelines for game developers.

---

## 1. Executive Overview & Design Philosophy

The game's inventory and equipment UI utilizes a **3-column diegetic panel layout** anchored over a semi-transparent dark vignette backdrop. 

### Core Design Goals
1. **High Stat Density without Visual Overwhelm:** Simultaneously presents item grid navigation, item metadata/art, scaling stats, and full character attributes.
2. **Diegetic Context & World Awareness:** The backdrop maintains ~15% transparency and peripheral dark vignetting, allowing the player to remain aware of their in-game surroundings and lighting.
3. **Modal & Ergonomic Navigation:** Designed primarily for controller DPAD / bumper navigation with immediate visual and stat feedback on item focus.
4. **Contextual View States:** Allows instant toggling between standard dual-panel view, extended narrative lore text, and simple/compact view for visual character inspection.

---

## 2. Spatial Layout & Screen Breakdown

The interface is structured around a three-column vertical core bounded by a persistent top header bar and a bottom keymap navigation footer.

```
+----------------------------------------------------------------------------------------------------+
|  [Header Bar] Category Tabs (LB/RB) | Sub-Category Icons (D-Pad Left/Right)                       |
+------------------------------------+----------------------------------+----------------------------+
|                                    |                                  |                            |
|  COLUMN 1: ITEM GRID               | COLUMN 2: ITEM DETAILS           | COLUMN 3: CHARACTER STATS  |
|  (~30% Width)                      | (~40% Width)                     | (~30% Width)               |
|                                    |                                  |                            |
|  - Scrollable Grid (5xN or 6xN)    | - High-Res Item Artwork          | - Base Character Attributes|
|  - Category Sorting                | - Item Type / Weight / Skill     | - Equipped Gear Stats      |
|  - Equipped Status Badges          | - Attack Power & Scaling         | - Defenses & Resistance    |
|  - Quantity Counters               | - Requirement Benchmarks         | - Equip Load & Poise       |
|  - Unmet Requirement Overlays      | - Passive Effects / Ashes of War | - Real-time Stat Deltas    |
|                                    |                                  |                            |
+------------------------------------+----------------------------------+----------------------------+
|  [Footer Bar] Input Action Legend: (A) Select  (B) Back  (Y) Toggle Lore  (RS) Simple View             |
+----------------------------------------------------------------------------------------------------+
```

### Layout Specifications

| UI Region | Width Ratio | Primary Responsibilities | Anchor / Flex Behavior |
| :--- | :--- | :--- | :--- |
| **Top Header Bar** | 100% | Screen title, primary category tab bar (`LB`/`RB`), sub-category icon row (`DPAD Left/Right`). | Fixed top anchor, horizontal layout group. |
| **Left Column (Item Grid)** | ~30% | Scrollable inventory cell grid, cell focusing, item filtering, equipment assignment badges. | Left-anchored vertical panel with fixed cell grid constraints. |
| **Middle Column (Item Details)** | ~40% | Full specs of currently focused item (3D preview image, weapon type, weight, physical/magical damage, scaling grades, skill). | Center-anchored vertical scroll/stat block. |
| **Right Column (Character Stats)** | ~30% | Live character stats (Vigor, Mind, Endurance, etc.), total defense values, current/max equip load capacity, poise. | Right-anchored vertical stat sheet. |
| **Bottom Footer Bar** | 100% | Input action keymap legend, contextual tooltips (`Select`, `Back`, `Switch View`, `Sort`). | Fixed bottom anchor, horizontal icon-text pairs. |

---

## 3. Visual Language, Typography & Color Palette

The visual style follows an authentic dark-fantasy parchment aesthetic using warm gold highlights, muted slate framing, high contrast, and crisp serif typography.

### Color Palette Reference

| Token Name | Hex Code | Visual Application & UX Context |
| :--- | :--- | :--- |
| **Background Vignette** | `#0C0C0C` (85% Alpha) | Dark overlay shading out the center screen while keeping margins partially visible. |
| **Frame & Panel Fill** | `#141412` (92% Alpha) | Container background fill for item cards, stat blocks, and column headers. |
| **Parchment Primary** | `#E6E1C5` | Primary text color for item titles, active menu headers, and key numeric values. |
| **Parchment Subdued** | `#9E9885` | Secondary text color for labels, categories, and unselected tab headers. |
| **Label / Divider Gray**| `#5C584E` | Section borders, grid slot borders, subtle field dividing lines. |
| **Golden Focus Accent** | `#D4AF37` / `#F0C048` | Glow ring around focused cell, active category tab border, highlighted menu options. |
| **Stat Buff / Improvement**| `#62B5F6` / Soft Blue | Stat increases in the right column when hovering over a superior candidate item. |
| **Stat Nerf / Penalty** | `#EF5350` / Soft Red | Stat decreases or equip load weight increases when hovering over inferior items. |
| **Unmet Requirement** | `#E53935` / Solid Red | Red tint over cell or red stat label when player lacks required Str/Dex/Int/Fth/Arc. |

### Typography Guidelines
* **Primary Headers & Names:** Serif font with slight tracking (e.g., Trajan Pro, Georgia, or custom serif).
* **Stat Values & Body Numbers:** Clean semi-serif or legible Sans-Serif numbers with fixed numeric widths (tabular figures) to avoid layout jitter during navigation.

---

## 4. Cell UI Architecture (Item Grid Slots)

Each item grid cell is a self-contained interactive widget. In Unity UGUI or UI Toolkit, structure the component hierarchy layered from back to front:

### Prefab Layer Hierarchy (Back to Front)

1. **Background Box (`Image`)**
   - Dark slate filled box (`#1A1A18`) with a faint 1px border (`#3D3A33`).
2. **Focus / Selection Frame (`Image` / Glow Overlay)**
   - Hidden by default. Enables when the item cell receives focus.
   - Soft golden radial gradient outline (`#D4AF37`) with slight pulse/glow animation.
3. **Item Icon (`Image`)**
   - High-resolution item thumbnail sprite. Aspect ratio set to `Preserve Aspect`.
4. **Equipped Status Badge (`Image` + `Text`)**
   - Top-Left alignment anchor.
   - Shows equipment slot markers (e.g., `R1`, `R2`, `L1`, `L2`, `1`, `2`, or checkmark) if the item is currently equipped.
5. **Unmet Requirement Overlay (`Image` + `Icon`)**
   - Semi-transparent dark red tint layer (`#E53935` at 35% alpha) plus a red warning cross icon in the top-right corner if character requirements are unmet.
6. **Stack Quantity Counter (`Text` / `TextMeshPro`)**
   - Bottom-Right alignment anchor.
   - Text format: `x99`, `x1` (hidden if non-stackable or quantity is 1).
7. **Quick-Item / Ash of War Badge (`Image`)**
   - Bottom-Left alignment anchor. Indicates attached Ash of War or affinity icon.

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

---

## 5. Information & Stat Calculation Engine

### Dynamic Stat Comparison (Hover Feedback)
When the user navigates across the grid cells in Column 1:
1. **Candidate Resolution:** The UI system queries the currently focused item (`ItemData`).
2. **Active Comparison:** The system identifies the item currently equipped in the target slot.
3. **Delta Computation:**
   $$\Delta 	ext{Stat} = 	ext{Candidate.StatValue} - 	ext{Equipped.StatValue}$$
4. **Visual Indicator Rules:**
   - If $\Delta 	ext{Stat} > 0$: Display value in **Soft Blue** (`#62B5F6`) with an upward arrow indicator (`↑`).
   - If $\Delta 	ext{Stat} < 0$: Display value in **Soft Red** (`#EF5350`) with a downward arrow indicator (`↓`).
   - If $\Delta 	ext{Stat} = 0$: Display value in **Parchment Primary** (`#E6E1C5`).

### Unmet Attribute Warning
- Items check character base attributes vs item requirements:
  - `Character.Strength < Item.RequiredStrength`
  - `Character.Dexterity < Item.RequiredDexterity`
  - `Character.Intelligence < Item.RequiredIntelligence`
  - `Character.Faith < Item.RequiredFaith`
  - `Character.Arcane < Item.RequiredArcane`
- Any failing metric turns red on the Middle Column stat card, and triggers the cell's red warning overlay.

---

## 6. UI/UX View State Machine

uses a multi-state view switcher to prevent visual clutter while providing deep lore and model inspection.

```
                     +---------------------------+
                     |  STATE 1: DUAL-PANEL VIEW |
                     |  (Grid + Item + Stats)    |
                     +-------------+-------------+
                                   |
                     Toggle (Y) /  | Toggle (Y) /
                     View Button   | View Button
                                   v
                     +-------------+-------------+
                     |  STATE 2: LORE VIEW       |
                     |  (Grid + Lore Text Card)  |
                     +-------------+-------------+
                                   |
                     RS Click /    | RS Click /
                     Simple Key    | Simple Key
                                   v
                     +-------------+-------------+
                     |  STATE 3: SIMPLE VIEW     |
                     |  (Grid Only / Compact)    |
                     +---------------------------+
```

### State Descriptions

1. **State 1: Dual-Panel View (Default)**
   - **Visible:** Left (Grid), Middle (Item Stats), Right (Character Attributes).
   - **Purpose:** Fast gear swapping and stat optimization.

2. **State 2: Lore / Narrative View**
   - **Visible:** Left (Grid), Middle/Center (Full Flavor Text & Lore Card). Right column hidden/faded out.
   - **Purpose:** Reading item background, item effect descriptions, and game lore.

3. **State 3: Simple / Compact View**
   - **Visible:** Left (Grid minimal overlay). Middle and Right columns completely hidden.
   - **Purpose:** Unobstructed visual inspection of weapon/armor models on the live player character in the game world.

---

## 7. Navigation, Focus Management & Input Mapping

To achieve authentic controller-first feel in Unity:

### Input Mapping Table

| Gamepad Input | Keyboard/Mouse Input | Contextual Function |
| :--- | :--- | :--- |
| **`LB` / `RB`** | `Q` / `E` or Top Tabs | Switch Primary Item Category (Weapons, Armor, Talismans, Consumables, Key Items). |
| **`DPAD Left / Right`** | `A` / `D` or Arrow Keys | Switch Sub-Category Filter (e.g., Melee Weapons vs Ranged Weapons vs Shields). |
| **`DPAD Up / Down`** | `W` / `S` or Arrow Keys | Navigate grid cell rows vertically. |
| **`A` / Cross** | Left Click / `Enter` | Select item / Open equipment assignment sub-menu. |
| **`B` / Circle** | Right Click / `Escape` | Close current sub-menu / Exit inventory. |
| **`Y` / Triangle** | `R` / Secondary Key | Toggle Lore Description Card View (State 1 ↔ State 2). |
| **`RS Click`** | `F` / Toggle Key | Toggle Simple/Compact UI View (State 1/2 ↔ State 3). |
| **`X` / Square** | `X` | Open Item Context Action Menu (Use, Discard, Leave, Organize). |

### Unity Event System & Focus Handling Rules
- Always maintain explicit explicit navigation anchors (`Selectable.FindSelectableOnDown()`) or rely on dynamic grid positioning (`GridLayoutGroup`).
- When switching tabs (`LB`/`RB`), programmatically reset focus to the first valid cell in the grid using `EventSystem.current.SetSelectedGameObject(firstCell)`.
- Ensure scroll views automatically scroll to keep the focused cell centered in view (`ScrollRect.ScrollToCell()`).

---

## 8. Recommended Unity Implementation Architecture

For building this UI architecture in Unity, a modular MVP (Model-View-Presenter) or Signal/Event-driven approach is recommended.

```
  [ ItemData / CharacterStats ScriptableObjects ]
                       |
                       v
         [ InventoryManager / Controller ]
            /          |                      v           v            v
  [ GridView ]   [ DetailsView ]   [ CharacterStatsView ]
     (Cells)      (Middle Card)       (Right Panel)
```

### Essential C# Script Structure Overview

1. **`InventoryItemSO.cs` (ScriptableObject):**
   - Data container holding item ID, localized name, icon sprite, type, weight, attack stats, scaling grades, requirements, and lore string.

2. **`InventorySlotUI.cs` (MonoBehaviour on Cell Prefab):**
   - Manages UI references (`Image icon`, `Text quantity`, `GameObject equipBadge`, `GameObject unmetOverlay`, `Outline glowFrame`).
   - Listens for `ISelectHandler` and `IDeselectHandler` events to trigger state feedback.

3. **`InventoryGridController.cs` (MonoBehaviour):**
   - Instantiates cell prefabs inside a `GridLayoutGroup` or `UI Toolkit ScrollView`.
   - Populates items based on active category filter.

4. **`InventoryDetailsPresenter.cs` (MonoBehaviour):**
   - Listens for focus change events from grid cells.
   - Updates the Middle Details panel and triggers delta computations in the Right Stats panel.

5. **`InventoryViewStateController.cs` (MonoBehaviour):**
   - Manages canvas group alpha / visibility toggles between Standard, Lore, and Simple view states.

---

*End of Inventory UI/UX Architecture Specification Guide.*