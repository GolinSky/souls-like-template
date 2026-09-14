---
title: Equipment UI Integration
type: implementation-record
domains:
  - ui
  - equipment
  - inventory
status: done
authority: historical
updated: 2026-09-14
aliases: []
tags:
  - history/change
---

# Equipment UI Integration

## Outcome

Integrated the approved Equipment design into the existing `EquipmentUi.prefab`, retaining its GUID and Addressables mapping. The left panel contains the project's 28 equipment slots without category row labels. The center uses the linked inventory `LoreCard.prefab`; the right uses the linked `CharacterStats.prefab`. Both shared source assets remain unchanged.

Origin: the user's Penpot design and Unity integration request; [design package and Penpot link](../../../Design/EquipmentUI/README.md).

## Why

Equipment needed the approved visual layout and recognizable empty categories while sharing the inventory lore and character-stat presentation paths.

## Changed Files and Assets

- `Assets/Prefabs/Ui/Equipment/EquipmentUi.prefab`: layout, linked shared panels, editable TMP text, background reuse, independent slot fill/border/glow/badge/quantity layers, and serialized bindings.
- `Assets/Art/Textures/EquipmentUI/EmptySlots/`: ten imported 256px transparent sprites and Unity metadata.
- `EquipmentUi.cs`: selected slot/name and equipped count, shared lore display including empty slots, focus restoration when returning from inventory, and current action prompts.
- `EquipmentSlotUI.cs`: category ghosts, independent equipped badges, quantity/lock handling, and focus retention during rebinding.
- `LoreCardUi.cs`: empty-equipment presentation alongside the unchanged item-description/lore path.
- `EquipmentUiPresentationTests.cs`: five focused EditMode regressions.
- `Design/EquipmentUI/`: accepted generation sources, 256/512 exports, Penpot exports and ledger, metadata, Unity captures, and reproducible Unity eval helpers.

## Decisions and Tradeoffs

- Kept the existing shared-inventory selection route. The legacy picker/inspector remains under its overlay with its required hierarchy names and bindings intact.
- Equipment-specific layout overrides disable inherited layout components that otherwise overwrite the narrower panel bounds. Source prefabs are not unpacked or edited.
- Runtime lore and stats use existing data fields. Illustrative Penpot-only level, skill and additional lore headings were not turned into static runtime data.
- Preview captures use real ItemDatabase and CharacterData values with a sample three-item loadout. The temporary HDRP camera disables exposure and postprocessing to match overlay UI colors.
- Unrelated concurrent guard-removal changes in `EquipmentUi.cs` were left in the working tree and excluded from this commit. Other unrelated source, font-cache, settings and documentation changes were also excluded.

## Validation Evidence

- Unity 6000.3.11f1, UTF 1.6.0, official CLI 1.0.0-beta.6.
- Final `EquipmentUiPresentationTests`: **5/5 passed**, 0.37 seconds. Covered occupied/empty transition, locked visuals, focus retention during binding, focus restoration on return, and empty lore replacement.
- `InventoryRuntimeBindingTests`: **2/2 passed**, 0.30 seconds.
- Workspace `EquipmentUiConfigurationTests`: **19/19 passed**, 0.47 seconds; required references and slot topology. This separately authored fixture is not included in this commit.
- All runs used explicit EditMode, bounded fixture filters, asynchronous execution and a 120-second caller budget. The initial focus-test fixture needed explicit EditMode EventSystem lifecycle registration; after source refresh, the corrected live assembly passed.
- Reviewed populated captures at 1920×1080 and 1280×720, plus shield, flask and empty-chest states. No visible text clipping; 28 slots and linked LoreCard/CharacterStats sources verified. Current sample data reports no TMP text overflow.
- Assets were imported and saved through Unity. Final scene: only `ElevatorDemo`, clean. Editor ready, Play Mode stopped, test runner inactive, error console empty.
- Focused source/document whitespace checks passed. Unity's own serialized empty YAML values retain its normal trailing spaces.

## Documentation Updated

- [[Guides/UI/Equipment UI and UX Guide]]: current layout, shared lore, active inventory route, slot layers, palette and typography.
- [Design README](../../../Design/EquipmentUI/README.md): integration details, capture links, source provenance and reproduction helpers.

## Follow-Up

Play Mode navigation and gameplay equip/unequip were intentionally not run under the project's normal validation policy. The guide retains its advisory `needs-review` status for older sections outside this integration.
