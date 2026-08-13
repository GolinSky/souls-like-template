# Equipment UI/UX Architecture & Unity Implementation Guide

This guide breaks down the structure, spatial layout, UX interaction states, and technical implementation details for recreating an Souls-like style Equipment UI in **Unity3D**.

---

## 1. UI/UX Design Philosophy

The game's equipment interface follows FromSoftware's dark fantasy minimalist aesthetic:
- **Low Clutter, High Information Density:** Complex RPG calculations are neatly organized into modular panels that collapse or expand without obscuring context.
- **Immediate Feedback Loop:** Every hover, equip, or swap instantly updates global character attributes (Attack Power, Defense Negation, Equip Load, Weight Class).
- **Diegetic Medieval Palette:** Dark slate/stone backgrounds with subtle parchment textures, framed by warm desaturated gold accents (`#C5A059`) and serif typography.
- **Gamepad-First Focus:** Grid-based spatial navigation designed for fast D-pad or WASD movement with clear active selection borders.

---

## 2. Spatial UI Breakdown (What is Located Where)

The Equipment Screen is divided into **four main visual zones** rendered over a dimmed live game world.

```
+-----------------------------------------------------------------------------------------------+
| ZONE 1: TOP HEADER (Title, Character Level, Held Runes/Currency)                             |
+-------------------------------------------------------------+---------------------------------+
| ZONE 2: EQUIPMENT SLOTS GRID (Left Side)                   | ZONE 4: CHARACTER STATUS        |
|                                                             |         & CALCULATIONS PANEL    |
| [R-Arm 1]   [R-Arm 2]   [R-Arm 3]                           | (Right Side)                    |
| [L-Arm 1]   [L-Arm 2]   [L-Arm 3]                           | - Base Attributes               |
| [Arrow 1]   [Arrow 2]   [Bolt 1]   [Bolt 2]                 | - Vitals (HP, FP, Stamina)      |
| [Head]      [Chest]     [Arms]     [Legs]                   | - Equip Load & Weight Class     |
| [Talisman1] [Talisman2] [Talisman3] [Talisman4]             | - Attack Power Breakdown        |
| [Item 1]    [Item 2]    [Item 3]   [Item 4]   [Item 5]      | - Damage Negation (%)           |
| [Item 6]    [Item 7]    [Item 8]   [Item 9]   [Item 10]     | - Resistances                   |
|                                                             |                                 |
| +---------------------------------------------------------+ |                                 |
| | ZONE 3: ITEM INSPECTOR CARD (Middle / Hover Details)    | |                                 |
| | Icon, Name, Type, Skill/Ash of War, Requirements, etc.  | |                                 |
| +---------------------------------------------------------+ |                                 |
+-------------------------------------------------------------+---------------------------------+
| ZONE 5: BOTTOM ACTION BAR / CONTROLLER LEGEND (Select, Back, Remove, Toggle View, Help)      |
+-----------------------------------------------------------------------------------------------+
```

---

### Zone 1: Top Navigation Bar & Header
- **Location:** Top edge of the screen (Full Width).
- **Elements:**
  - **Screen Title (Left):** "EQUIPMENT" heading.
  - **Player Summary (Right):**
    - Character Name
    - Current Level (e.g., `Lvl 120`)
    - Held Runes / Currency (e.g., `45,210`)

---

### Zone 2: Equipment Grid Panel (Left Side)
Organized into 6 logical horizontal rows. Each slot contains a square frame showing the currently equipped item sprite, quantity (if consumable), or an empty slot icon.

1. **Right-Hand Armaments (Row 1 - 3 Slots):** Weapons/Shields held in Right Hand (`R-Armament 1`, `R-Armament 2`, `R-Armament 3`).
2. **Left-Hand Armaments (Row 2 - 3 Slots):** Weapons/Shields/Seals held in Left Hand (`L-Armament 1`, `L-Armament 2`, `L-Armament 3`).
3. **Ammunition (Row 3 - 4 Slots):**
   - Arrows 1 & 2 (Used with Bows)
   - Bolts 1 & 2 (Used with Crossbows)
4. **Apparel / Armor (Row 4 - 4 Slots):**
   - Head (Helm)
   - Chest (Armor)
   - Arms (Gauntlets)
   - Legs (Greaves)
5. **Talismans (Row 5 - Up to 4 Slots):**
   - Slots unlock dynamically via key items (Talisman Pouches). Locked slots render a padlock graphic.
6. **Quick Items / Belt (Row 6 & 7 - 10 Slots in 2x5 Grid):**
   - Consumables assigned to D-pad Down scrolling (Flasks, Pots, Grease, Ashes, Lantern).

---

### Zone 3: Item Inspector Card (Middle / Lower Left)
Displays detailed specifications of the **currently highlighted slot or item**:
- **Item Graphic & Name:** Large item icon, full item title.
- **Weapon Skill / Ash of War:** Name of equipped skill + FP cost (e.g., `Unsheathe ( - / 15)`).
- **Attack Rating / Defense Values:** Base physical/elemental stats of the item.
- **Stat Requirements:** Required Attributes (Strength, Dexterity, Intelligence, Faith, Arcane). Displays in **Red** if player stats are below requirement.
- **Attribute Scaling:** Scaling grades (`S`, `A`, `B`, `C`, `D`, `E`, or `-`).
- **Passive Effects:** e.g., `Causes Blood Loss Accumulation (50)`.
- **Item Weight:** Numerical weight value (e.g., `Weight 5.5`).

---

### Zone 4: Character Status & Calculations Panel (Right Side)
A persistent status panel showing character stats updated in real-time when hovering over or equipping new gear:
- **Character Vitals:** Max HP, Max FP, Max Stamina.
- **Equip Load & Roll Speed:**
  - `Equip Load: 42.5 / 70.0`
  - **Weight Class Badge:** Light Load (<30%), Medium Load (30%-70%), Heavy Load / Fat Roll (70%-100%), Overencumbered (>100%).
- **Total Attack Power:**
  - `R-Armament 1`, `2`, `3` Attack Ratings
  - `L-Armament 1`, `2`, `3` Attack Ratings
- **Damage Negation (%):** Physical, VS Strike, VS Slash, VS Pierce, Magic, Fire, Lightning, Holy.
- **Resistances:** Immunity (Poison/Rot), Robustness (Bleed/Frost), Focus (Sleep/Madness), Vitality (Death Blight), Poise, Discovery.

---

### Zone 5: Bottom Action Bar (Controller Legend)
- **Location:** Bottom of the screen.
- **Elements:** Button prompts mapping input actions based on active state:
  - `[A / Enter]` Select / Equip
  - `[X / Delete]` Remove / Unequip
  - `[Y / F]` Switch Display (Simple View vs Detailed Comparison View)
  - `[Select / H]` Help / Explanation Tooltips
  - `[B / Escape]` Back / Close

---

## 3. Interactive UX States & Navigation Flow

```
[ Primary Equipment Screen ]
       |
       |-- (Navigate D-Pad / WASD) --> Move Cursor between slots
       |-- (Press X / Delete) --------> Unequip selected slot
       |-- (Press Y / F) -------------> Toggle Detailed Character Stats
       |
       v  (Press A / Select Slot)
[ Inventory Selection Overlay ]
       |
       |-- Highlights candidate items for selected slot category
       |-- Shows STAT COMPARISON (Current Equipped vs Hovered Item)
       |   - Increases shown in BLUE (+5)
       |   - Decreases shown in RED (-12)
       |-- Press A -> Confirm Equip & Return
       |-- Press B -> Cancel & Return
```

### State 1: Primary Equipment Navigation
- User navigates the 6xN grid using D-Pad, WASD, or Mouse click.
- As cursor shifts, Zone 3 (Item Inspector) updates instantly.
- Pressing `A` transitions to **State 2 (Inventory Picker)**.

### State 2: Inventory Selection Modal & Comparison View
- The grid dims slightly, and an **Inventory Picker Overlay** opens.
- Filters candidate items matching the slot type (e.g., clicking Head slot shows only Head armors).
- **Comparison Logic:**
  - Hovering an item in the list compares its stats with the currently equipped item.
  - **Blue Text / Positive Delta (`+`):** Improvement.
  - **Red Text / Negative Delta (`-`):** Downgrade.
  - **Equip Load recalculation preview:** Displays potential weight class change before confirming.

---

## 4. Unity Data Architecture

### Data Models (ScriptableObjects)

```csharp
using UnityEngine;

public enum ItemType { Weapon, Armor, Talisman, Consumable, Ammunition }
public enum ArmorSlotType { Head, Chest, Arms, Legs }
public enum WeaponSlotType { RightHand, LeftHand }
public enum WeightClass { Light, Medium, Heavy, Overencumbered }

[CreateAssetMenu(fileName = "NewItem", menuName = "ItemData")]
public class ItemDataSO : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public float weight;
    [TextArea] public string description;
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "WeaponData")]
public class WeaponDataSO : ItemDataSO
{
    public WeaponSlotType slotType;
    public string skillName;
    public int fpCost;
    
    [Header("Base Attack Power")]
    public int physicalAttack;
    public int magicAttack;
    public int fireAttack;
    public int lightningAttack;
    public int holyAttack;
    public int critical;

    [Header("Requirements")]
    public int reqStrength;
    public int reqDexterity;
    public int reqIntelligence;
    public int reqFaith;
    public int reqArcane;
}

[CreateAssetMenu(fileName = "NewArmor", menuName = "ArmorData")]
public class ArmorDataSO : ItemDataSO
{
    public ArmorSlotType armorSlot;

    [Header("Damage Negation (%)")]
    public float physicalNegation;
    public float strikeNegation;
    public float slashNegation;
    public float pierceNegation;
    public float magicNegation;
    public float fireNegation;
    
    [Header("Resistances")]
    public int immunity;
    public int robustness;
    public int focus;
    public int vitality;
    public int poise;
}
```

---

## 5. Unity Implementation Architecture (UGUI / C#)

### UI Layout Structure (Canvas Hierarchy)
```
[Canvas] (Screen Space - Overlay / Camera)
 ├── [HeaderPanel]
 │    ├── [TMP_TitleText] ("EQUIPMENT")
 │    └── [TMP_PlayerSummaryText] ("Lvl 120 | Runes: 45,210")
 ├── [MainContentPanel]
 │    ├── [EquipmentGridPanel] (Vertical Layout Group)
 │    │    ├── [Row_RightHand] (Horizontal Layout Group -> 3 SlotViews)
 │    │    ├── [Row_LeftHand]  (Horizontal Layout Group -> 3 SlotViews)
 │    │    ├── [Row_Ammo]      (Horizontal Layout Group -> 4 SlotViews)
 │    │    ├── [Row_Armor]     (Horizontal Layout Group -> 4 SlotViews)
 │    │    ├── [Row_Talismans] (Horizontal Layout Group -> 4 SlotViews)
 │    │    └── [Row_QuickItems] (Grid Layout Group -> 10 SlotViews)
 │    ├── [ItemInspectorPanel]
 │    │    ├── [Image_ItemIcon]
 │    │    ├── [TMP_ItemName]
 │    │    ├── [TMP_SkillText]
 │    │    └── [StatsDetailsContainer]
 │    └── [CharacterStatusPanel]
 │         ├── [TMP_Vitals] (HP, FP, Stamina)
 │         ├── [TMP_EquipLoadText] ("42.5 / 70.0 (Medium)")
 │         └── [TMP_NegationStats]
 ├── [InventoryPickerOverlay] (Disabled by Default)
 │    ├── [ScrollRect_InventoryList]
 │    └── [ComparisonViewPanel]
 └── [BottomActionBar]
      └── [TMP_ActionPrompts]
```

---

### Core UI Controller Scripts

#### 1. Equipment Slot View (`EquipmentSlotView.cs`)
```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class EquipmentSlotView : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectionHighlight;
    [SerializeField] private Image lockOverlay;

    public string SlotId { get; private set; }
    public ItemDataSO CurrentItem { get; private set; }
    public bool IsLocked { get; private set; }

    public event Action<EquipmentSlotView> OnSlotSelected;
    public event Action<EquipmentSlotView> OnSlotClicked;

    public void SetupSlot(string id, ItemDataSO item, bool isLocked = false)
    {
        SlotId = id;
        CurrentItem = item;
        IsLocked = isLocked;

        if (IsLocked)
        {
            lockOverlay.gameObject.SetActive(true);
            iconImage.enabled = false;
        }
        else
        {
            lockOverlay.gameObject.SetActive(false);
            if (item != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
        selectionHighlight.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        selectionHighlight.gameObject.SetActive(true);
        OnSlotSelected?.Invoke(this);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selectionHighlight.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(this);
    }
}
```

#### 2. Master Equipment UI Controller (`EquipmentUIController.cs`)
```csharp
using UnityEngine;
using TMPro;

public class EquipmentUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject equipmentMainPanel;
    [SerializeField] private GameObject inventoryPickerOverlay;

    [Header("Inspector References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemIconImage;

    [Header("Status References")]
    [SerializeField] private TextMeshProUGUI equipLoadText;
    [SerializeField] private TextMeshProUGUI weightClassText;

    private EquipmentSlotView currentlySelectedSlot;

    public void OnSlotHighlighted(EquipmentSlotView slot)
    {
        currentlySelectedSlot = slot;
        UpdateInspector(slot.CurrentItem);
    }

    private void UpdateInspector(ItemDataSO item)
    {
        if (item == null)
        {
            itemNameText.text = "Empty";
            itemDescriptionText.text = "";
            itemIconImage.enabled = false;
            return;
        }

        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemIconImage.sprite = item.itemIcon;
        itemIconImage.enabled = true;
    }

    public void OpenInventoryPicker()
    {
        if (currentlySelectedSlot == null || currentlySelectedSlot.IsLocked) return;
        
        inventoryPickerOverlay.SetActive(true);
        // Populate inventory picker based on currentlySelectedSlot type
    }

    public void CloseInventoryPicker()
    {
        inventoryPickerOverlay.SetActive(false);
    }
}
```

#### 3. Stat Comparison Formatting Logic (`StatComparisonUtility.cs`)
```csharp
public static class StatComparisonUtility
{
    public static string FormatStatDiff(int currentVal, int newVal)
    {
        int diff = newVal - currentVal;
        if (diff > 0)
        {
            return $"{newVal} <color=#55AAFF>(+{diff})</color>"; // Blue for improvement
        }
        else if (diff < 0)
        {
            return $"{newVal} <color=#FF5555>({diff})</color>";  // Red for downgrade
        }
        return $"{newVal}";
    }
}
```

---

## 6. Visual & Audio Polish Guidelines

### Visual Style
- **Color Palette:**
  - Slate Background: `#121417`
  - Slot Frame Border: `#3A342B`
  - Active Highlight Border: `#C5A059` (Gold)
  - Text Primary: `#E6DFD3` (Warm White)
  - Stat Improvement Delta: `#4A90E2` (Soft Blue)
  - Stat Penalty Delta: `#D0021B` (Muted Red)
- **Typography:**
  - Headings & Titles: *Cinzel* or *Trajan Pro* (Serif, All Caps)
  - Numbers & Stats: *Garamond* or *Cinzel Decorative*

### Audio Trigger Events
- `SFX_UI_Move`: Soft leather/pact rustle or metallic tap when moving selection.
- `SFX_UI_Select`: Distinct metallic chime when clicking a slot.
- `SFX_UI_Equip`: Heavy metallic thud when equipping armor/weapon.
- `SFX_UI_Unequip`: Quick cloth slide sound when clearing a slot.
- `SFX_UI_Back`: Muted slate click when closing menus.

---

## 7. Summary Checklist for Unity Implementation

1. **[ ] Data Setup:** Create `ItemDataSO`, `WeaponDataSO`, and `ArmorDataSO` ScriptableObject templates.
2. **[ ] UI Layout:** Construct the 6-row Equipment Grid using Unity's `VerticalLayoutGroup` and `HorizontalLayoutGroup`.
3. **[ ] Navigation setup:** Ensure Unity EventSystem explicit navigation links work seamlessly across D-Pad, WASD, and Mouse.
4. **[ ] Dynamic Inspector:** Wire `OnSelect` actions from `EquipmentSlotView` to update the Item Inspector Card and Character Status.
5. **[ ] Selection Modal:** Implement the Inventory Picker Overlay with side-by-side stat comparison (`+` Blue / `-` Red diff text).
