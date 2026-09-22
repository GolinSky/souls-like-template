---
title: GroundItem VFX Redesign Plan
type: plan
domains:
  - art
  - vfx
  - gameplay
status: draft
authority: advisory
updated: 2026-09-20
aliases:
  - grounditem-vfx-plan
tags:
  - work/plan
  - status/draft
---

# GroundItem VFX Redesign Plan

## Plan Contract

### Goal

Replace the visually inappropriate campfire-like VFX on `Assets/Prefabs/Models/Item/GroundItem.prefab` with the authentic Souls-like ground loot visual:
- A very small bright glowing point on the ground with subtle ground contact glow.
- A thin vertical beam/wisp (~0.75m) rising straight up, brightest near the ground and fading to zero at the top.
- A few tiny particles gently shimmering around the vertical beam.
- White/silver default coloration with variant materials for blue/purple (special) and gold/orange (notable).

### Source Research and Decisions

- Visual inspection of the current `GroundItem.prefab` confirmed it had a giant 1.25m floor glow, 5 thick curved ribbons spread out horizontally like a bonfire, and 74 orbiting motes copied from Site of Grace assets.
- The user requested: "small white glow → thin vertical light → few sparkles" (~0.5–1m beam, brightest near ground).
- `GroundItemAdditive.shader` requires updating so that vertex color alpha (`input.color.a`) modulates fragment alpha smoothly and no premature clipping occurs when `_Dissolve == 0`.
- `GroundItemPrefabBuilder.cs` will be updated with corrected asset paths and the new hierarchy so the prefab can be rebuilt programmatically via `Tools/SoulsLike/Build Ground Item`.
- `Sphere.prefab` variant will be resynchronized.

### Assumptions and Non-Goals

- Non-goal: Modifying the interaction mechanics, collider radius, or `GroundItem.cs` logic.
- Non-goal: Reworking `GraceView.prefab` or Grace visual effects.

### Success Criteria

- `GroundItem.prefab` displays a very small bright point on the ground, a thin vertical beam fading from ground to ~0.75m, and 8 tiny shimmering sparkles.
- Default color is luminous white/silver.
- `GroundItemVfx` collect animation continues to dissolve all active renderers and trigger the pickup burst cleanly.
- `WorldConfigurationTests` pass without regression.

## Execution Plan

- [x] Phase 1 — Update `GroundItemAdditive.shader` for proper vertex alpha fade and clean non-dissolving clipping; create white/silver and blue/purple materials.
- [x] Phase 2 — Update `GroundItemPrefabBuilder.cs` and construct the new VFX hierarchy (GroundGlow, CorePoint, CoreGlow, VerticalBeam, VerticalWisp, AmbientSparkles, PickupFlash).
- [x] Phase 3 — Persist `GroundItem.prefab` and `Sphere.prefab` through Unity's official serialization API (`ForceReserializeAssets`, `SaveAssets`).
- [x] Phase 4 — Validate via `WorldConfigurationTests`, verify dissolve animation, and capture camera view of the in-world item.

## Risks and Rollback

- Risk: Shader change affects Grace materials sharing `GroundItemAdditive.shader`.
  - Mitigation: The shader change only multiplies `input.color.a` into output alpha and prevents noisy discard when `_Dissolve <= 0.001f`. This improves Grace visuals as well, but can be verified against `GraceView.prefab`.
- Rollback: Revert `GroundItemAdditive.shader`, `GroundItem.prefab`, and `GroundItemPrefabBuilder.cs` via git.

## Validation

- UTF EditMode tests: `WorldConfigurationTests`.
- Visual camera render of `GroundItem` in `Zone_03.unity` compared against baseline.

## Execution Handoff

Files:
- `Assets/Art/Shaders/GroundItemAdditive.shader`
- `Assets/Art/Materials/GroundItems/GroundItemWhite.mat`
- `Assets/Art/Materials/GroundItems/GroundItemWhiteGlow.mat`
- `Assets/Art/Materials/GroundItems/GroundItemBlue.mat`
- `Assets/Art/Materials/GroundItems/GroundItemBlueGlow.mat`
- `Assets/Scripts/Editor/GroundItemPrefabBuilder.cs`
- `Assets/Prefabs/Models/Item/GroundItem.prefab`
- `Assets/Prefabs/Models/Item/Sphere.prefab`
