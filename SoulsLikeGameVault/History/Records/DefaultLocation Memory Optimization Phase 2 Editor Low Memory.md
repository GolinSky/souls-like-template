---
title: "DefaultLocation Memory Optimization Phase 2 Editor Low Memory"
type: implementation-record
domains:
  - performance
  - rendering
  - scenes
status: done
authority: historical
updated: 2026-09-07
aliases: []
tags:
  - history/change
---

# DefaultLocation Memory Optimization Phase 2 Editor Low Memory

## Implementation Record Contract

### Outcome

Created the `Editor Low Memory` quality level and its HDRP asset from the existing Performant preset. The current Editor remained on High Fidelity while the ten clean DefaultLocation scenes stayed loaded.

### Why

Phase 2 establishes a bounded authoring preset for the next controlled memory comparison without changing the existing shipping quality presets or the currently loaded location.

### Changed Files and Assets

- `ProjectSettings/QualitySettings.asset` — added `Editor Low Memory` at index 3 and assigned the new HDRP asset.
- `ProjectSettings/EditorSettings.asset` — enabled editor async CPU texture loading on demand.
- `Assets/Settings/RenderPipelines/HDRP Editor Low Memory.asset` — copied from Performant and tuned for the initial low-memory experiment.
- `Assets/Settings/RenderPipelines/HDRP Editor Low Memory.asset.meta` — created by Unity with a new GUID.

### Decisions and Tradeoffs

- Quality settings: global mip limit 1 (half resolution), streaming enabled with a 512 MB budget and max reduction 3, LOD bias 0.5, maximum LOD level 0.
- HDRP settings: retained Performant's R11G11B10 color buffer and disabled SSGI, SSR, volumetrics/clouds, and ray tracing; disabled SSAO; reduced reflection cache to 1024², sky reflection to 256, punctual shadow atlas to 2048, area/cached shadow atlases to 1024, and decal atlas to 1024².
- The current quality level was intentionally not switched because the location was already loaded; the preset must be selected before the next controlled load session.

### Validation Evidence

- Unity 6000.3.11f1 Editor status: ready, not compiling, play mode stopped.
- All ten DefaultLocation scenes remained loaded and `isDirty=false`.
- Unity reports four quality levels: High Fidelity, Balanced, Performant, Editor Low Memory.
- `QualitySettings.GetRenderPipelineAssetAt(3)` resolves to `HDRP Editor Low Memory`.
- Serialized verification passed for mip limit, streaming, budget, max reduction, LOD bias, editor CPU texture loading, SSAO, reflection/sky/shadow/decal atlas values.
- Unity console was clear after validation; no import or serialization errors remained.

### Documentation Updated

- [[Work/Plans/DefaultLocation Memory Optimization]] — Phase 2 marked complete; plan remains in progress for later phases.
- [[History/Closed Issues/DefaultLocation Memory and Rendering Issues]] remains evidence-only and open.

### Follow-Up

Use the new preset before loading DefaultLocation in the next controlled session and measure peak/private memory, commit headroom, Unity counters, render targets, and visual fidelity. No memory reduction claim is made until that comparison is captured.

Originating plan: [[Work/Plans/DefaultLocation Memory Optimization]].
