---
title: DefaultLocation Memory Optimization Phase 4 Texture Streaming Batch 1
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
# DefaultLocation Memory Optimization Phase 4 Texture Streaming Batch 1

## Implementation Record Contract

### Outcome

Completed the first bounded Phase 4 texture-import batch. Four reviewed opaque 2K world textures now opt into mip streaming. Phase 4 remains in progress because mesh readability/topology work and controlled memory/visual measurement are not complete.

### Why

The six largest reviewed mesh assets accounted for about 411.8 MiB of already-resident Editor mesh resources, but every LOD0 is also used by MeshColliders and several consumers have transform conditions that can require readable meshes. With system commit initially below 1 GiB, the safe evidence-backed change was a small texture batch after unloading the clean location scenes.

### Changed Files and Assets

- `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Textures/T_CandleHolder_BC.TGA.meta`
- `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Textures/T_Column1_BC.TGA.meta`
- `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Textures/T_Trim1_BC.TGA.meta`
- `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Textures/T_Trim2_BC.TGA.meta`

### Decisions and Tradeoffs

- Enabled only `streamingMipmaps`; kept mip generation, 2048 maximum size, readability, compression, and disabled Standalone overrides unchanged.
- Did not change mesh Read/Write, compression, topology, LODs, colliders, navigation, formats, normal/mask/alpha textures, scenes, or project settings.
- Replaced the ten clean loaded DefaultLocation scenes with clean `Bootstrap.unity` before importing because commit headroom fell to 0.63 GiB. The crash-prone location set was not reopened.
- Accepted Unity 6000 TextureImporter serialization migration fields because live readback confirmed no additional effective importer change.

### Validation Evidence

- Unity dry-runs and sequential imports completed with at least 3.75 GiB pre-write commit headroom; final headroom was about 5.13 GiB.
- Unity readback confirmed all four textures have mip streaming enabled, mipmaps enabled, readability disabled, 2048 maximum size, compressed default import, and disabled Standalone override.
- `Bootstrap.unity` is the sole open scene and is clean; Unity is ready, not compiling, and not playing.
- Independent review found no material correctness or scope findings. No new import or serialization errors were observed.
- Current High Fidelity quality has mip streaming disabled, so this record does not claim a measured residency reduction.

### Documentation Updated

- [[Work/Plans/DefaultLocation Memory Optimization]] — Phase 4 audit findings and Batch 1 progress recorded; Phase 4 remains incomplete.
- [[Work/Work Queue]] — corrected to list the in-progress plan.
- Originating issue: [[Work/Issues/DefaultLocation Memory and Rendering Issues]].

### Follow-Up

Run a controlled DefaultLocation comparison under Editor Low Memory, then expand texture batches only when visual and residency evidence supports them. Treat Tower and Bridge collider/LOD simplification as the next mesh investigations; do not disable Read/Write until bounded collision, navigation, and Player validation passes.
