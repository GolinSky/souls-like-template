---
title: Approved Inventory UI Integration
type: implementation-record
domains: [ui, inventory]
status: done
authority: historical
updated: "2026-09-12"
aliases: []
tags: [history/change, ui/inventory]
---

# Approved Inventory UI Integration

## Implementation Record Contract

### Outcome

The reviewed inventory design is on Penpot's `02 Approved` page. Its Main,
Lore, Simple, and design-notes boards retain their IDs and six prototype links.
Named version: `Approved Inventory UI — before Unity integration — 2026-09-12`.

The existing `Assets/Prefabs/Ui/Inventory/InventoryUi.prefab` carries the new
layout, icons, typography, translucent surfaces, and soft borders. Its GUID
remains `04c9c316a7440544085b0c87e72a777b`, preserving asset references.

### Why

The user approved the design and requested integration with a recoverable
backup, specifically retaining the nested CharacterStats prefab approach.
Design context: [[2026-09-12 Inventory UI Structure and Opacity]].

### Changed Files and Assets

- `InventoryUi.prefab`: inventory-local overrides on nested CharacterStats,
  ItemDetails, LoreCard, and category-toggle instances.
- `InventorySlotInventory.prefab`: an inventory-only variant of InventorySlot.
- `Assets/Art/Fonts/EBGaramond/` and `Inter/`: licensed font sources and persistent
  TMP material/atlas subassets; existing Cinzel is reused. Unity populated the
  Cinzel SDF glyph/atlas cache while rendering the new text.
- `Character.cs`: currency-change notification after a successful grant.
- `InventoryUi.cs` and `InventoryUiController.cs`: live held currency, selected
  item/category/count labels, and readable filter captions.
- `InventoryViewStateController.cs`: CharacterStats remains visible in Lore.
- `InventoryRuntimeBindingTests.cs`: currency event and three view states.

### Decisions and Tradeoffs

- CharacterStats, ItemDetails, LoreCard, and InventorySlot shared source files
  match their backup byte-for-byte. Equipment UI retains those shared styles.
- The reference canvas is 1920×1080, with 584 / 648 / 464 pixel columns and
  48 pixel gaps; the inventory retains five 104 pixel columns with 16 pixel gaps.
- Panel alpha belongs to separate background graphics. Details and Lore each
  own their background inside their visibility group; Simple hides both.
- Fixed single-line text permits glyph overflow inside the containing viewport:
  TMP ellipsis otherwise removed entire lines when font metrics exceeded the
  logical row height. Long descriptions remain clipped or scrollable.
- There is no character level model. The Level field shows an em dash; no
  progression data was invented. Existing stat calculations remain unchanged.
- Editor previews use illustrative temporary data and project artwork. They
  are visual layout evidence, not a capture of live gameplay.

### Validation Evidence

- Independent runtime and saved-prefab reviews found no actionable defects.
- Root GUID, nested prefab links, all new required references, five primary
  and eleven subcategory toggles, and the slot variant linkage were checked.
- The Items-level clip is disabled; the scroll viewport retains clipping.
- Shared source prefab hashes match the saved baseline.
- Main visual review passed with project artwork, readable text, and a gold
  focus edge. Lore's reading area uses the full content width; Simple hides
  the middle and status surfaces. Captures use temporary illustrative data.
- UTF Editor fixture `SoulsLike.Editor.Tests.Inventory.InventoryRuntimeBindingTests`:
  2 executed, 2 passed, 0 failed/skipped/inconclusive, 0.41 seconds. Run through
  the official Unity CLI with explicit Editor mode, test-name filter, async
  execution, and a 120 second caller budget.
- Unity 6000.3.11f1 and ElevatorDemo were idle/clean before and after validation.
  Earlier corrected task-local evaluator/render-probe errors remain in the
  Console; validation produced no compilation, import, or test failures.
- C# whitespace checks passed. Unity's serialized YAML retains its normal
  trailing spaces for empty values; it was not reformatted outside the Editor.

### Documentation Updated

Recovery instructions: `Backups/InventoryUi/2026-09-12-pre-penpot/RESTORE.md`.
The backup contains both the original disk state and the saved dirty Prefab
Stage baseline, with matching scripts and `.meta` files. An importable stage
snapshot is also retained under the inventory prefab's `Backups/` directory.

Design specification, Unity API edit scripts, audits, and previews are in
`output/inventory-integration/`.

### Follow-Up

Completed visual follow-up: added `InventoryUi/BackgroundCenterFade` immediately
after `BackgroundVignette`. It initially reused `FadeBackground.png` as a full-screen,
non-raycasting dark Image, with component alpha 0.50 and the texture's soft edge
falloff. The existing vignette and UI layout are preserved. Unity save/reload
verified alpha, sibling order, and anchors; the updated preview was reviewed,
the Console reported no errors, and ElevatorDemo remained clean. Pre-change
prefab and meta are in `Backups/InventoryUi/2026-09-12-pre-center-fade/`.

Coverage correction: the original Simple Image scaled the source's wide alpha
falloff with the screen, keeping the stronger fill concentrated inward. Changed
only this Image to the existing `FadeSquareBackgroundSliced.png`, Sliced with
Fill Center enabled and Pixels Per Unit Multiplier 2. The opaque source center
now stretches independently of the soft perimeter (roughly 100 UI units of
falloff), while component alpha remains 0.50. Full-stretch anchors, zero offsets,
non-raycasting behavior, sibling order, and nested prefabs are preserved.
Unity save/reload and rendered mesh checks passed at 1280×720, 1920×1080, and
3440×1440; each background mesh matches its full canvas dimensions. Reviewed
`inventory-preview-center-fade-fullscreen-1920x1080.png`. The task-local coverage
probe was corrected for this Unity version's parameterless `GetMesh()` API;
the successful audit is `output/inventory-integration/center-fade-coverage-audit.json`.

The sliced background still left more transparent space than requested. The final
follow-up removes the sprite from `BackgroundCenterFade` entirely: a plain,
full-stretch uGUI Image now provides the 0.50 dimmer, while `BackgroundVignette`
retains the atmosphere. All four rendered corner alphas are 128/255, with no
texture alpha or transparent padding, at 1280×720, 1920×1080, and 3440×1440.
Evidence: `output/inventory-integration/fullscreen-dimmer-audit.json`.

Added `InventoryEmptySlot.prefab`, a decorative Image with a narrow soft border,
and 25 nested instances under the inventory's scrolling grid Content. Their
104×104 cells and 16-unit gaps use the existing five-column GridLayoutGroup.
`InventoryUi.emptySlotBackgrounds` references these instances. Population keeps
25 total visible cells for short/empty lists and completes the final row for
longer lists. Empty cells remain outside item data, counts, navigation, and
selection; they have no Selectable or InventorySlotUI and do not raycast.
Old item objects are deactivated before deferred destruction so an immediate
refresh cannot include them in the new grid layout. Shared nested prefab sources
and the inventory-specific real item slot variant remain unchanged.

Recovery snapshot: `Backups/InventoryUi/2026-09-12-pre-fullscreen-dimmer-empty-cells/`.
Visual preview: `output/inventory-integration/inventory-preview-empty-cells-1920x1080.png`.
The bounded production review found no material issues. Compilation succeeded,
and UTF discovered all six `InventoryGridPresentationTests` cases. The Editor
closed normally twice during validation (including after the clean preflight
for the test run); the final run request failed because Pipeline was unreachable.
Zero tests executed, so there is no UTF pass claim. The final observed scene was
clean ElevatorDemo, and no test run was active before the Editor exited.
Re-run this fixture when Unity remains open. The tests cover 0/1/24/25/26 items,
26→1 repopulation, and clearing; they narrowly expect the Editor-only prohibition
on the production deferred Destroy calls and clean up their preview objects.
The prefab audit confirmed 25 valid nested decorative cells, zero interactive
placeholders, the expected five-column sizing, and no sprite on the dimmer.

Play Mode input/navigation and end-to-end gameplay validation are deferred
under the project's normal test policy. Complete them separately when needed.
