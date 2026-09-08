---
title: DefaultLocation Memory Optimization Phase 5 Rendering Path
type: implementation-record
domains:
  - performance
  - rendering
  - scenes
status: done
authority: historical
updated: 2026-09-08
aliases: []
tags:
  - history/change
---
# DefaultLocation Memory Optimization Phase 5 Rendering Path

## Implementation Record Contract

### Outcome

Completed the bounded Phase 5 rendering-path experiment. The existing SRP Batcher path remains the DefaultLocation baseline for this Editor sample. GPU Resident Drawer and a small static-batching subset did not produce a sufficient measured memory or frame-time win to justify persistent project or asset changes; the GPU Resident Drawer conclusion remains provisional until its active `Hybrid Batch Group` path is captured in a visible Player/Frame Debugger session.

### Why

Phase 5 required evidence from representative repeated objects rather than material instancing checkboxes or static flags. The experiment used the High Fidelity HDRP preset, `DefaultLocation` with `Zone_02`, and a fixed Main Camera view of the representative castle hall on the Unity Editor 6000.3.11f1 workstation.

### Changed Files and Assets

- `SoulsLikeGameVault/Work/Plans/DefaultLocation Memory Optimization.md`
- `SoulsLikeGameVault/History/Implementation Records/DefaultLocation Memory Optimization Phase 5 Rendering Path.md`

No Unity scene, prefab, material, HDRP asset, importer, or runtime source asset was persisted. `ProjectSettings/GraphicsSettings.asset` was temporarily changed to `m_BrgStripping: 2` (Keep All) for the GPU Resident Drawer compatibility check, then restored to its original `m_BrgStripping: 0` value and reserialized through Unity.

### Decisions and Tradeoffs

- Kept SRP Batcher enabled and GPU Resident Drawer disabled in the existing HDRP presets.
- Compared matched runtime samples: SRP Batcher, GPU Resident Drawer with BRG shader stripping temporarily set to Keep All, and `StaticBatchingUtility.Combine` on four runtime-selected repeated-object LOD roots.
- Did not mass-enable GPU Instancing or Batching Static. The existing audit shows those flags are already widely present, but flags alone do not prove the actual draw path and static batching can duplicate transformed geometry.
- Did not change materials, shaders, LODs, lightmaps, property blocks, or scene static flags because neither alternative won the bounded comparison.

### Validation Evidence

Matched Play-mode samples from the same camera view. Each row is one post-settle measurement snapshot; no repeated-sample median or Player build was collected:

| Path | Draw calls | Set-pass calls | Unity allocated | Unity reserved | CPU frame | GPU frame |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| SRP Batcher | 920 | 31 | 3.622 GB | 5.971 GB | 4.290 ms | 2.391 ms |
| GPU Resident Drawer | 907 | 27 | 3.630 GB | 5.971 GB | 4.652 ms | 2.303 ms |
| Static subset | 940 | 31 | 3.622 GB | 5.971 GB | 4.574 ms | 2.250 ms |

GPU Resident Drawer reduced draw calls by about 1.4% in this sample while increasing CPU time and allocated memory. The static subset increased draw calls. An Editor Scene-view Frame Debugger capture reported 198 events containing `RenderLoop.DrawSRPBatcher` and no `Hybrid Batch Group` event. Offscreen Game-view capture did not expose Frame Debugger events, so visible Player/Frame Debugger confirmation remains a follow-up validation gap and the result should not be generalized to Player-wide behavior.

The experiment generated and then removed temporary Phase 5 captures. Open scenes were clean after the experiment, Play mode was stopped, and Unity confirmed `KeepIfEntitiesGraphics` for the restored BRG stripping setting. Loading `Zone_02` emitted pre-existing negative-scale BoxCollider warnings on stair gameplay ramps; no Phase 5 serialization or import errors were introduced.

### Follow-up

Phase 6 still needs the bounded loading/concurrency experiment. If Player evidence is required for the rendering decision, repeat the comparison in a Development Player with a visible Frame Debugger/Profiler capture and record build shader-variant and buffer costs.
