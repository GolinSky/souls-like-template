---
title: Convert Font Assets to Static
type: plan
domains:
  - ui
  - assets
status: done
authority: advisory
updated: 2026-09-20
aliases:
  - Convert_Font_Assets_to_Static
tags:
  - work/plan
  - status/done
---

# Convert Font Assets to Static

## Plan Contract

### Goal
Convert TextMeshPro font assets ([`Inter SDF.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Fonts/Inter/Inter%20SDF.asset), [`EBGaramond SDF.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Fonts/EBGaramond/EBGaramond%20SDF.asset), and [`Cinzel[wght] SDF.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Fonts/Cinzel/Cinzel%5Bwght%5D%20SDF.asset)) from Dynamic to Static mode with pre-baked full ASCII and typography glyph sets to permanently eliminate Git dirty state and atlas churn.

### Source Research and Decisions
- `Assets/Art/Fonts/Inter/Inter SDF.asset`, `Assets/Art/Fonts/EBGaramond/EBGaramond SDF.asset`, and `Assets/Art/Fonts/Cinzel/Cinzel[wght] SDF.asset` were configured in Dynamic mode (`m_AtlasPopulationMode: 1`).
- Dynamic mode caused FreeType to modify the embedded `Texture2D` atlas (`_typelessdata`) in memory whenever UI rendered in the Editor or Play Mode, causing persistent Git churn and merge conflicts on save.
- Testing showed that full printable ASCII (32-126) plus standard typographical symbols (`—`, `–`, `…`, `“`, `”`, `‘`, `’`, `•`, `°`, `«`, `»`) total 107 characters and fit into a single 1024x1024 atlas texture per font asset.
- In Static mode, `TryAddCharacters` is disabled, and TextMeshPro never modifies atlas textures or glyph tables at runtime.

### Assumptions and Non-Goals
- Text rendering in the project is primarily in English and Latin-derived characters.
- If arbitrary user input (like custom player names) or non-Latin alphabets are introduced later, a separate dynamic fallback font asset can be configured in TMP Fallback Font Assets rather than making primary fonts dynamic.

### Success Criteria
1. All 3 font assets have `m_AtlasPopulationMode: 0` (Static).
2. All 107 standard characters are present in each font asset.
3. Atlas texture count is exactly 1 per font asset (1024x1024).
4. `AssetDatabase.SaveAssets()` produces zero modifications to the font assets in Git.

## Execution Plan

- [x] Phase 1 — Ingest standard character set (ASCII + typography, 107 characters) into each font asset.
- [x] Phase 2 — Switch `atlasPopulationMode` to `Static` on each font asset.
- [x] Phase 3 — Persist assets: mark dirty, call `AssetDatabase.SaveAssets()`, and run `AssetDatabase.ForceReserializeAssets(...)`.
- [x] Phase 4 — Validate that saving or running Editor sessions leaves font assets clean in `git status`.

## Risks and Rollback
- Risk: Missing characters if UI contains non-ASCII characters outside the 107-character set.
- Rollback: Revert the font asset changes via Git (`git restore Assets/Art/Fonts/`) or switch back to Dynamic mode if necessary.

## Validation
- Unity eval checks verified:
  - `Inter SDF.asset`: Mode = Static, 107 characters, 106 glyphs, 1 texture (1024x1024).
  - `EBGaramond SDF.asset`: Mode = Static, 107 characters, 107 glyphs, 1 texture (1024x1024).
  - `Cinzel[wght] SDF.asset`: Mode = Static, 107 characters, 107 glyphs, 1 texture (1024x1024).
- Unity Console verification confirmed 0 import or serialization errors.
- Calling `AssetDatabase.SaveAssets()` confirmed 0 new git diffs.

## Execution Handoff
- Target files:
  - [`Assets/Art/Fonts/Inter/Inter SDF.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Fonts/Inter/Inter%20SDF.asset)
  - [`Assets/Art/Fonts/EBGaramond/EBGaramond SDF.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Fonts/EBGaramond/EBGaramond%20SDF.asset)
  - [`Assets/Art/Fonts/Cinzel/Cinzel[wght] SDF.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Fonts/Cinzel/Cinzel%5Bwght%5D%20SDF.asset)
- Status: Completed and verified.
