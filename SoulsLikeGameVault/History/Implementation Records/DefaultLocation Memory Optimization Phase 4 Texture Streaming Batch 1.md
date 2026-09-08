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

Completed the first bounded Phase 4 texture-import batch and a controlled sequential DefaultLocation capture under Editor Low Memory. Four reviewed opaque 2K world textures now opt into mip streaming. Phase 4 remains in progress because the 20% commit-headroom target was not met, Player evidence is absent, and mesh readability/topology work is not complete.

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
- Replaced the ten clean loaded DefaultLocation scenes with clean `Bootstrap.unity` before importing because commit headroom fell to 0.63 GiB. A later controlled capture selected Editor Low Memory before loading the location sequentially and returned to Bootstrap/High Fidelity afterward.
- Accepted Unity 6000 TextureImporter serialization migration fields because live readback confirmed no additional effective importer change.

### Validation Evidence

- Unity dry-runs and sequential imports completed with at least 3.75 GiB pre-write commit headroom; final headroom was about 5.13 GiB.
- Unity readback confirmed all four textures have mip streaming enabled, mipmaps enabled, readability disabled, 2048 maximum size, compressed default import, and disabled Standalone override.
- `Bootstrap.unity` is the sole open scene and is clean; Unity is ready, not compiling, and not playing.
- Independent review found no material correctness or scope findings. No new import or serialization errors were observed.
- A fresh one-scene-per-command Editor Low Memory trace loaded DefaultLocation, Rocks, and Zone_01–Zone_08 without OOM or failed scene operations. Unity private memory rose from 12.65 GiB at DefaultLocation-only to 13.83 GiB with all ten scenes; system commit rose from 56.33 to 58.06 GiB; Unity allocated memory rose from 2.20 to 2.97 GiB.
- The lowest commit headroom was 8.20 GiB at Zone_06. This is 12.4% of the 66.29 GiB commit limit and fails the plan's 20% target of about 13.26 GiB.
- Streaming observations at all-ten settled: `T_CandleHolder_BC` loaded mip 3 at 49,730 bytes; `T_Column1_BC` loaded mip 1 at 726,774 bytes; `T_Trim1_BC` and `T_Trim2_BC` loaded mip 1 at 721,474 bytes each.
- Valid 640×360 camera capture: `Temp/DefaultLocation-Phase4-Editor-Low-Memory.png`. The frame is non-black and uses the same castle-hall camera composition as the accepted Phase 3 capture.
- The Editor returned to sole clean `Bootstrap.unity` and High Fidelity with no project-settings diff. These are Editor measurements, not a Development Player or GPU-performance result.

### Documentation Updated

- [[Work/Plans/DefaultLocation Memory Optimization]] — Phase 4 audit findings and Batch 1 progress recorded; Phase 4 remains incomplete.
- [[Work/Work Queue]] — corrected to list the in-progress plan.
- Originating issue: [[Work/Issues/DefaultLocation Memory and Rendering Issues]].

### Follow-Up

Run the equivalent Development Player capture and three controlled load/unload cycles before claiming the final memory budget. Expand texture batches only when the Editor/Player comparison and visual evidence support them. Treat Tower and Bridge collider/LOD simplification as the next mesh investigations; do not disable Read/Write until bounded collision, navigation, and Player validation passes.
