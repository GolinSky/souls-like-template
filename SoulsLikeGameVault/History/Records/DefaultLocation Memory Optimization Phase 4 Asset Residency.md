---
title: DefaultLocation Memory Optimization Phase 4 Asset Residency
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
# DefaultLocation Memory Optimization Phase 4 Asset Residency

## Implementation Record Contract

### Outcome

Completed the implementation portion of Phase 4. Five visual FBXs now release CPU-readable mesh copies after their DefaultLocation colliders were decoupled into project-owned readable hull meshes. Sixty additional reviewed world textures now opt into mip streaming, bringing the Phase 4 total to 64.

### Why

The six audited FBXs total about 411.85 MiB of imported mesh resources. Their visual LOD cascades are already effective and fully referenced, but 68 MeshColliders used expensive LOD0 geometry. The reviewed material graph also contained 60 additional opaque base, normal, and ORM textures with mipmaps but streaming disabled.

### Changed Files and Assets

- Created five readable meshes under `Assets/Art/DefaultLocation/Colliders/`.
- Reassigned 68 MeshColliders across `Assets/Scenes/DefaultLocation/Zone_01.unity` through `Zone_08.unity` and the Bridge vendor prefab.
- Reimported `SM_CastleSideBridge_02`, `SM_Tower_Bot`, `SM_chandelier`, `SM_Castle_Gate_01`, and `SM_candle_holder` with Read/Write disabled.
- Rebuilt navigation data under `Assets/Scenes/DefaultLocation/Navigation/` and updated the main scene links.
- Enabled mip streaming on 16 opaque base, 24 normal, and 20 ORM texture importers under the FantasyCastle texture directory.

### Decisions and Tradeoffs

- Preserved every visual LOD, transition threshold, submesh, material slot, FBX GUID, and visual mesh fileID. Existing cascades already reduce topology effectively, and every visual LOD is used.
- Reduced referenced collider topology from about 13.65M triangles to about 120K across the 68 changed instances by cloning vendor-authored hulls without recalculation or further simplification.
- Left Gate 02 readable and on its current LOD0 collider because it has no authored collision hull.
- Kept derived collider meshes readable so collider cooking remains valid independently of the visual importers.
- Kept importer maximum size at 2048. A permanent 1024 cap would reduce High Fidelity and Standalone quality; Editor Low Memory already applies a reversible global mip limit of 1.
- Excluded mask, alpha, emissive, glass, particle, cubemap, UI, and other special textures.

### Validation Evidence

- Independent review found no material reference, serialization, or scope errors.
- Exactly 68 scene MeshColliders and the Bridge prefab reference the five derived assets; zero affected colliders reference the non-readable visual FBXs, and no MeshFilter uses a collider asset.
- All 7,246 loaded MeshColliders cooked successfully. Seven rebuilt NavMeshData assets load, six inter-zone links remain, and zone triangulations are non-empty; the main asset is intentionally link-only/empty.
- Three sequential 11-scene cycles (Bootstrap plus the ten location scenes) completed stably. A corrected ten-location-scene trace settled at 6.79 GiB Unity private memory, 36.49/47.16 GiB system commit, 10.66 GiB headroom (22.6%), and 1.95/2.39 GiB Unity allocated/reserved.
- All 64 changed textures reported streaming enabled with desired and loaded mip 3 in the corrected capture.
- Accepted 640×360 camera capture: `Temp/DefaultLocation-Phase4-Final-Editor-Low-Memory.png`.
- Unity returned to sole clean Bootstrap and High Fidelity with no ProjectSettings diff or final console error.

### Documentation Updated

- [[Work/Plans/DefaultLocation Memory Optimization]] — remaining Phase 4 implementation items marked complete.
- Originating issue: [[History/Closed Issues/DefaultLocation Memory and Rendering Issues]].

### Follow-Up

Run a Development Player capture and explicit character/NavMeshAgent traversal across bridge, tower, doorway, and inter-zone links before declaring runtime gameplay equivalence. The repository currently has no registered deterministic path probes, and normal validation intentionally skipped Play Mode coverage.
