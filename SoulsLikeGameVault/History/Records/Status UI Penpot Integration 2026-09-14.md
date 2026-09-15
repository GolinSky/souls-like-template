---
title: Status UI Penpot Integration
type: implementation-record
domains:
  - ui
status: done
authority: historical
updated: 2026-09-14
aliases: []
tags:
  - history/change
---
# Status UI Penpot Integration

## Implementation Record Contract

### Outcome

Created the Status UI scripts first, following [[Knowledge/Guides/UI/UI Code Build Guide]], then integrated the approved Penpot design into `Assets/Prefabs/Ui/Status/StatusUi.prefab`. The pause menu now has a Status entry. The feature is registered through the existing UI factory, Addressables group, asset mapping, and character lifetime scope.

### Why

Implements the user request to integrate Penpot’s Status overview into Unity while reusing existing textures. Source: page `02 Approved`, board `Status / Overview — 1920 × 1080`, board ID `07eedd92-cb2c-80b2-8008-a35eb6d78601`.

### Changed Files and Assets

- New `Assets/Scripts/Ui/Status/` view, controller, presenter/route interfaces, formatter, and typed action button.
- `CharacterFactory` and the pause navigation contracts, view, and controller register and route Status.
- New Status prefab; updated PauseNavigation prefab, Ui Addressables group, and `AssetMappingData`.
- New `Assets/Art/Textures/StatusUI/StatusPanelSoft.png` and JSON import/layout metadata. Seven existing inventory/equipment/menu textures and existing Cinzel, EB Garamond, and Inter fonts are reused.
- New `Assets/Scripts/Editor/Tests/Status/` formatter and configuration/interaction regressions.

### Decisions and Tradeoffs

The controller reads existing attributes, held currency, vitals, equipment weight/capacity, current poise, and six armament base-attack totals. Missing domain values (name, level, level cost, load class, discovery, spells/memory, defense/negation, resistances) display `—`; no fictional gameplay data was introduced. Help explains these limits. Poise is explicitly labelled current poise.

Back uses the pause route stack. Swap requests Equipment through the pause host. F uses the existing simple-view action; Help is inline. Buttons use C# events and explicit footer navigation.

The feature uses the existing canvas with a uniformly fitted 1920 × 1080 content root and separate full-screen backdrop/atmosphere. Five Cinzel headings require 40-unit bounds instead of Penpot’s 36 because the current TMP font requires 37.75 units; centers are preserved. Footer focus uses muted antique gold.

The new panel is an unedited Penpot export of shape `07eedd92-cb2c-80b2-8008-a35f9e2268ba`: 752 × 976, semantic body (96,96,560,784), 96-unit overscan, 192-pixel slice borders, opacity 0.76 baked into straight alpha. Unity Image tint remains white with alpha 1.

### Validation Evidence

- Explicit Unity recompilation completed with `failed=false`, no compiler errors.
- Official Unity CLI/Pipeline asynchronous Editor runs: `StatusUiFormatterTests` 4/4; `StatusUiConfigurationTests` 4/4; existing `SoulsLike.Editor.Tests.Configuration.UiConfigurationTests` 6/6. Total 14/14 passed, no skipped or inconclusive tests.
- Configuration checks validate required prefab references, armament/defense/resistance list sizes, typed pause entry, focus restoration, and enabled/disabled submit behavior. Formatter coverage includes large integer currency precision.
- Rendered and visually inspected 1920×1080, 1280×720, 2560×1440, 3840×2160, 1920×1200, and 2560×1080, plus selected Back. 104 visible TMP elements reported no overflow. Representative HP, equipment-load, and large-currency strings fit their fields.
- Preview outputs: `Temp/StatusUi/Previews/StatusUi-*.png`; copies retained in this task’s Codex visualization directory.
- Assets imported and saved through Unity. Final import/serialization checks reported no fresh errors. Test runner inactive; Unity ready; Play Mode stopped; the original ElevatorDemo scene remained clean with 15 roots.
- Play Mode end-to-end navigation/data behavior and a player build were not executed.

### Documentation Updated

Updated [[Knowledge/Architecture/UI/Pause Navigation]] with the Status route, DI registration, data boundaries, and prefab/layout contract.

### Follow-Up

When gameplay validation is scheduled, exercise Pause → Status → Back, Swap → Equipment, simple view, Help, and changing character/equipment data in Play Mode. Connect the explicitly unavailable fields when their domain models are implemented. No separate originating plan or issue was created; this work follows the direct user request and approved Penpot board.
