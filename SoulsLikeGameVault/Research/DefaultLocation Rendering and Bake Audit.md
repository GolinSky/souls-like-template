---
title: DefaultLocation Rendering and Bake Audit
type: research
domains:
  - rendering
  - performance
  - scenes
  - assets
status: draft
authority: evidence
updated: 2026-09-12
source_commit: aca3fd60ff59f2bb28515ec10115b4b9f2a62b8f
tags:
  - research/package
  - rendering/audit
---
# DefaultLocation Rendering and Bake Audit

## Required Package

### Question and Desired Decision

Check GPU-instancing material flags versus static batching, the static-batching/LOD0 concern, baked occlusion and lighting, and texture/mesh compression. Decide which confirmed configuration defects to repair and which performance candidates need measurement.

**Result:** the additive scene set has incompatible occlusion bake references and no persisted lighting bake despite 60 active baked-only point lights. Classic LODGroups are populated; the newer Mesh LOD/static-batching restriction is not exercised. Texture GPU compression is already enabled. Mesh CPU readability and streaming policy remain optimization candidates.

### Scope and Non-Goals

Inspected on **2026-09-12**, source commit **aca3fd60ff59f2bb28515ec10115b4b9f2a62b8f**, with existing unrelated working-tree edits present. Unity **6000.3.11f1**, StandaloneWindows64, Direct3D12, **High Fidelity**, `Assets/Settings/RenderPipelines/HDRP High Fidelity.asset`.

Primary scope is `Assets/Scenes/DefaultLocation/DefaultLocation.unity`, `Zone_01.unity` through `Zone_08.unity`, and `Rocks.unity`. WorkShop was checked separately because it is currently the configured default.

Evidence is a read-only, imported Editor inventory with resolved prefab instances, plus serialized bake references and live source navigation. Each scene was inspected in an isolated preview scene and closed. These counts include inactive objects and alternative LOD renderers, and are **not draw-call counts or simultaneous visible geometry**. No Player profiling, gameplay validation, bake, asset change, or optimization was performed.

### Current System Map

`ProjectSettings/EditorBuildSettings.asset:7` enables only Bootstrap. Bootstrap opens MainMenu. Resume uses saved spawn/grace state when available; otherwise `SceneData.defaultScene: 3` selects WorkShop.

`Assets/Settings/Data/SceneData.asset` maps DefaultLocation to nine dependencies. `SceneService.LoadSceneWithDependencies` loads those dependencies additively, then loads and activates DefaultLocation. They remain resident together; these zones are not distance-streamed.

### Entry Points, Dependencies, and Consumers

- `Assets/Scripts/Services/Scenes/SceneService.cs:76`: dependency loading and target activation.
- `Assets/Scripts/Services/Spawn/CharacterSpawnService.cs:40`: default versus saved resume.
- `Assets/Settings/Data/SceneData.asset:15`: configured default; dependency list later in the same asset.
- `Assets/Scripts/Editor/LocationBakeTool.cs`: existing multi-scene lighting/occlusion bake tooling.
- Material and model assets primarily under `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/`.
- Current issue owner: [[Work/Issues/DefaultLocation Memory and Rendering Issues]]. Earlier comparison: [[Research/Texture Audio and Mesh RAM Audit 2026-09-09]].

### Evidence and Findings

#### Mesh renderers, material instancing, and static batching

| Scene | MeshRenderers | Batching Static | Renderers using an instancing-enabled material | Classic LODGroups |
|---|---:|---:|---:|---:|
| DefaultLocation | 15 | 0 | 0 | 0 |
| Zone_01 | 3,558 | 3,551 | 3,558 | 1,111 |
| Zone_02 | 3,401 | 3,401 | 3,401 | 884 |
| Zone_03 | 2,312 | 2,298 | 2,310 | 593 |
| Zone_04 | 3,346 | 3,342 | 3,346 | 821 |
| Zone_05 | 2,073 | 2,073 | 2,073 | 604 |
| Zone_06 | 3,312 | 3,306 | 3,312 | 788 |
| Zone_07 | 3,085 | 3,084 | 3,085 | 825 |
| Zone_08 | 3,528 | 3,528 | 3,528 | 1,047 |
| Rocks | 2,296 | 2,296 | 2,296 | 574 |
| **DefaultLocation set** | **26,926** | **26,879** | **26,909** | **7,247** |
| WorkShop, separate control | 670 | 0 | 661 | 200 |

Across the ten location scenes, **70 of 74 distinct assigned material assets** enable GPU instancing. There are **61,858 material slots**: 61,841 use enabled materials; 61,811 also belong to Batching Static renderers. Counts supersede the older direct-YAML sample only for this resolved-instance scope.

The 70 enabled materials comprise 69 using `Shader Graphs/S_Masking` and one using `HDRP/Lit`. None of the 74 materials reports `DisableBatching=True`. The four disabled materials are `TemplateIronMaterial.mat`, `TemplateWoodenMaterial.mat`, `Grace/GraceGroundGlow.mat`, and `GroundItems/GroundItemGroundGlow.mat`; these are not evidence of a missing global instancing switch.

Examples of substantial repetition, keyed by mesh GUID **and local file ID**, not merely FBX path:

- Rocks has 285 occurrences of `SM_GroupedRock_4_LOD0` with the same three rock materials, and 285 occurrences at each of its other three classic LOD levels.
- Zone_06 has 274 occurrences of `SM_Exterior_Floor_1_LOD1` with `M_Tiles_3`, with separate equivalent repetition at lower levels.

Standalone static batching is enabled and dynamic batching disabled (`ProjectSettings/ProjectSettings.asset:490`). The active SRP Batcher is enabled. All four quality HDRP assets have GPU Resident Drawer mode 0 and GPU occlusion disabled. The material checkbox does not establish actual hardware-instanced drawing: Unity chooses paths according to shader/renderer compatibility. Static batching and conventional instancing do not provide two simultaneous geometry savings for the same draw. A Frame Debugger capture is needed before changing strategy. [Unity 6.3 draw-call methods](https://docs.unity3d.com/6000.3/Documentation/Manual/optimizing-draw-calls-choose-method.html)

This is a **strategy/measurement gap**, not a confirmed batching defect. Repeated geometry makes static-batch memory versus instancing worth comparing. GPU Resident Drawer would be a separate reviewed experiment; it is not currently enabled.

#### Static batching and the LOD0 warning

Unity has two different LOD mechanisms:

| Mechanism | Current content | Meaning of the LOD0 warning |
|---|---|---|
| Classic `LODGroup`, separate renderers/meshes | 7,247 groups in the location set | No evidence here of static batching forcing every group to LOD0 |
| New Mesh LOD, generated inside a mesh on import | All 160 model importers have `generateMeshLods=false`; all 632 imported Mesh subassets have `lodCount=1`; all 26,926 inspected renderer meshes also report 1 | Unity explicitly selects only Mesh LOD0 under static batching and conventional GPU instancing |

The warning is **real for the newer Mesh LOD feature**, but that feature is not used by this audited geometry. Unity also advises against combining Mesh LOD with LODGroup. [Unity 6.3 Mesh LOD limitations](https://docs.unity3d.com/6000.3/Documentation/Manual/lod/mesh-lod-introduction.html)

All inspected classic groups have multiple levels, no empty level, no renderer shared across different levels, and no disabled renderer references. All 7,247 use fade mode None. No project runtime calls were found to `ForceLOD`, `StaticBatchingUtility.Combine`, or assignments to `QualitySettings.lodBias`/`maximumLODLevel`. Live quality values are lodBias 1 and maximumLODLevel 0; the latter allows the highest detail and does not pin every object to it.

A separate caveat applies to LOD fading and batching eligibility; this content uses no LOD fading. [Unity batching requirements](https://docs.unity3d.com/6000.3/Documentation/Manual/DrawCallBatching-SetUp.html)

Another possible source of the rumor is occlusion baking: Unity uses an occluder's **LOD0 silhouette** for occlusion data. That does not mean LOD0 is always submitted for rendering. [Unity occluder setup](https://docs.unity3d.com/6000.3/Documentation/Manual/occlusion-culling-getting-started.html)

Actual near/far Player draw submission remains unverified.

#### Confirmed issue: occlusion bakes are incompatible with additive loading

**Priority: high for rendering validation.** Every zone and Rocks has a real, populated occlusion asset, with nonempty `m_PVSData`, a static-renderer table, and exactly **one scene GUID**. They are nine separate bakes. DefaultLocation has `m_OcclusionCullingData: {fileID: 0}` and an all-zero occlusion scene GUID (`DefaultLocation.unity:11–12`).

| Scene | Referenced occlusion asset GUID |
|---|---|
| DefaultLocation | None |
| Zone_01 | 17eb0aaa8a331e246995abf370c9de32 |
| Zone_02 | d7aebe2ef46c4404d9cf7cd1418c8973 |
| Zone_03 | e3ae01cf58976784386e0ceb4efc441c |
| Zone_04 | d5486b93b1ad1b645b229d50b88769a4 |
| Zone_05 | 3c6e002429bb6ba4db27d59cf797ccbb |
| Zone_06 | 45f24ee2d6fdacc4ab876a102eec6ef1 |
| Zone_07 | 8e6b5102358440240ae4087bb070ae2f |
| Zone_08 | f2e510c3d7420084fa1a157bddaf9c42 |
| Rocks | c907ab349f38b8d4c8193f7809a82e8f |

The main camera enables occlusion culling, but that checkbox does not repair the data relationship. Unity uses one occlusion asset at runtime and requires scenes used together to be baked together with a shared reference. This configuration therefore cannot establish the intended combined cross-zone occlusion. Which subset, if any, gets useful culling with this loader order requires runtime inspection. [Unity 6.3 multiple-scene occlusion](https://docs.unity3d.com/6000.3/Documentation/Manual/occlusion-culling-scene-loading.html)

`Assets/Scenes/DefaultLocation/occlusion_report.txt` is dated August 12 and describes the obsolete 23-scene layout. It is historical evidence, not current validation. Reuse the existing location bake workflow for remediation and validate the actual dependency-first runtime load order afterward.

#### Confirmed issue: lighting is configured for baking, but bake output is absent

**Priority: medium, visual correctness.** All ten location scenes have `m_LightingDataAsset: {fileID: 0}` near line 96. No inspected MeshRenderer has a usable lightmap index. Baked GI is enabled in the serialized scene settings.

DefaultLocation has **60 enabled, active-in-hierarchy Baked point lights**, plus one Mixed directional light. Zone_03 has two active Mixed lights, Zone_05 six, and Zone_07 two. Other dependency scenes have no active lights; the 60 Rocks lights are disabled and must not be counted as a realtime lighting workload.

The main scene contains ProbeVolume/ProbeVolumePerSceneData, but `DefaultLocation.unity:11176` has a null `serializedBakingSet`. `Rocks/Rocks Baking Set.asset` has empty scene/cell/scenario data and empty streamable asset references. No ReflectionProbe components were found in these scenes.

There is no persisted bake providing the authored baked-light contribution. Baked-only lights rely on precomputed output; Mixed lights still have a realtime component, so this does not imply a completely black scene. Visual severity needs a Player/view comparison. [Unity baked-light behavior](https://docs.unity3d.com/es/2021.1/Manual/LightMode-Baked.html)

#### Texture compression: already configured; streaming remains partial

The ten-scene AssetDatabase dependency closure has 639 paths and 164 TextureImporters. **156 are project assets**; eight package/editor dependencies are kept outside the gameplay-texture conclusion.

All 156 project textures have Read/Write off, mipmaps on, and effective GPU compression for the current Windows target:

- 79 DXT1.
- 76 DXT5.
- One BC6H HDR cubemap, `T_HDRSKY.HDR`.

Of these, **143 textures are assigned through the shader-declared texture properties of the 74 actual MeshRenderer materials**: 70 DXT1 and 73 DXT5. This narrower count excludes sky/other dependency references. No uncompressed mesh-material texture was found.

Default texture settings request normal compression, quality 50, with Crunch off and no enabled platform overrides. 142 project textures import at 2048 × 2048. Mobile/WebGL defaults were inventoried, but those targets were not built or validated.

64 of the 143 mesh-material textures enable mip streaming and 79 do not. High Fidelity, Balanced, and Performant disable streaming globally; the Editor Low Memory preset enables it. The live High Fidelity 512 MiB streaming budget is inactive. Thus per-texture flags alone do not supply a shipping streaming policy. This is an existing residency candidate, not a compression failure.

#### Mesh compression: Off is not automatically a defect

The dependency closure contains **160 ModelImporters / 632 Mesh subassets**, plus five separate collider Mesh assets.

- All 160 model importers use Mesh Compression Off.
- 126 retain Read/Write; 34 disable it.
- Polygon optimization is on for all 160; vertex optimization is off for 131 and on for 29.
- All five derived collider meshes are readable and uncompressed; that is a deliberate consumer-specific category, not a blanket removal candidate.
- `VertexChannelCompressionMask: 4054` enables Normal, Tangent, TexCoord0, and TexCoord2–7. Position, Color, and TexCoord1 are excluded. Mask interpretation was checked against the live VertexAttribute enum; effective Player buffers were not measured.

Importer Mesh Compression primarily reduces serialized size and adds decompression tradeoffs. Vertex Compression reduces runtime channel precision, and requires eligible non-readable, non-skinned meshes; enabling importer compression prevents vertex compression for that mesh. **Do not enable Mesh Compression everywhere as a RAM fix.** [Unity compression types](https://docs.unity3d.com/6000.3/Documentation/Manual/types-of-mesh-data-compression.html), [vertex compression requirements](https://docs.unity3d.com/6000.3/Documentation/Manual/configure-vertex-compression.html)

The 126 readable FantasyCastle model dependencies are candidates for consumer review. Project gameplay code has no identified vertex-buffer CPU consumer or runtime mesh recombination, and navigation uses baked data; nevertheless collider cooking, transforms and third-party tooling can require readability. Examples: `SM_CastleSideBridge_01.fbx.meta:152` and `SM_Castle_Gate_02.fbx.meta:134`. Their imported meshes total 432,042 and 410,702 vertices respectively across subassets; these are geometry counts, not measured memory or guaranteed savings. Earlier derived-collider work must be preserved.

#### WorkShop control

WorkShop has no declared scene dependencies, 670 MeshRenderers, zero Batching Static renderers, 661 using instancing-enabled materials, and 200 valid classic LODGroups. Its camera enables occlusion, but its scene has no occlusion data. It has one active Mixed light. Its lighting-data reference is the **built-in default resource**, not a project bake asset; there are no usable MeshRenderer lightmap indices.

Its nine project texture dependencies use DXT1/DXT5, mipmaps, and Read/Write off. Its three model dependencies use Mesh Compression Off and no generated Mesh LOD; the two FantasyCastle models remain readable and the StarterAssets stairs model does not. These prototype settings are recorded separately and are not automatically treated as defects in the DefaultLocation level.

### Options and Tradeoffs

1. Repair the combined occlusion bake and references, then validate using the actual additive load sequence.
2. Complete the intended lighting/APV bake or explicitly revise the light-mode design. Verify output and visual contribution rather than accepting enabled bake settings as completion.
3. Capture representative near/far Player frames to compare actual static batching, SRP batching and instancing, including memory. Preserve classic LODGroups while measuring.
4. Continue bounded readable-mesh/collider reviews and define texture-streaming policy. Compression changes should follow a measured disk, RAM or GPU budget.

### Risks, Unknowns, and Open Questions

No new FPS, GPU-time, draw-call saving, combined-mesh memory or visible occlusion measurement is claimed. Editor imports and isolated previews cannot substitute for built Addressables/Player behavior. A timed-out aggregate evaluation was replaced by successful bounded per-scene captures; all previews were subsequently confirmed closed and ElevatorDemo remained the sole clean live scene.

No production assets, scripts, import settings or Obsidian configuration were changed. Existing unrelated working-tree changes were preserved.

### Recommended Review Questions

- Does the remediation target shared occlusion plus lighting first?
- Which shipping quality/hardware combination should define the rendering and texture budget?
- Which readable meshes can safely lose CPU copies after collider-consumer review?

### Handoff

Confirmed bake issues and updated optimization evidence are recorded in [[Work/Issues/DefaultLocation Memory and Rendering Issues]]. This audit does not authorize or execute the existing draft bake plan.

## Evidence Rules

Raw local evidence, relative to the repository:

- `output/default-scene-audit-2026-09-12/summary.json`.
- `output/default-scene-audit-2026-09-12/render/scene-rendering-evidence-<scene>.json` — authoritative bounded scene captures.
- `output/default-scene-audit-2026-09-12/import-inventory.json`.
- `output/default-scene-audit-2026-09-12/materials-and-settings.json`.
- `output/default-scene-audit-2026-09-12/workshop-import-inventory.json`.
- Read-only inspection sources are retained beside these JSON files for reproduction.

The older aggregate scene JSON is an initial capture; use the per-scene files and summary for mesh IDs, Mesh LOD counts and active-light state. Project source and imported/serialized assets take precedence over earlier notes and Graphify.
