---
title: DefaultLocation Memory and Rendering Issues
type: issue
domains:
  - performance
  - rendering
  - scenes
status: done
authority: historical
priority: high
updated: 2026-09-14
source_commit: 3925ea83
tags:
  - work/issue
  - status/done
latest_audit_commit: aca3fd60ff59f2bb28515ec10115b4b9f2a62b8f
closed: 2026-09-14
---
# DefaultLocation Memory and Rendering Issues

## Completion — 2026-09-14

Marked completed on 2026-09-14 by explicit user instruction. Existing implementation records, measurements, and validation limitations remain historical evidence; this closure does not assert that new tests or profiles were run.

The previous problem description, proposed fixes, and unchecked scenarios below are retained for traceability, not active work. Do not reopen this issue or reimplement its proposals from an older audit or plan. See [[History/Records/Vault Simplification and Issue Closure 2026-09-14]].

**Latest audit, 2026-09-12:** [[Research/DefaultLocation Rendering and Bake Audit]] confirms incompatible occlusion bake references across the additive scene set and missing persisted lighting output despite 60 active baked-only point lights. It also verifies current instancing/static flags, classic LODGroups, and effective texture/model import settings. See the dated updates in I-04, I-05, I-06 and I-11; older undated measurements remain historical. No remediation was performed by this audit.

## Issue Contract

### Observed Behavior

The user reports out-of-memory crashes while loading the DefaultLocation level and all its scene dependencies. A saved Editor crash log independently confirms a native mesh vertex-data allocation failure after zone loading.

The original audit recorded findings, uncertainty, and proposed work before remediation. Subsequent work is recorded in [[Work/Plans/DefaultLocation Memory Optimization]] and its implementation records. The issue is now completed by user decision; the original evidence below is historical.

### Expected Behavior

DefaultLocation loads reliably within the target machine's available memory, with predictable loading peaks and graphics quality settings that provide meaningful memory scaling.

### Reproduction

- Unity 6000.3.11f1; HDRP 17.3.0; Addressables 2.9.1; Windows / Direct3D 12 / RTX 5070.
- `SceneData.asset` maps DefaultLocation to the main scene plus Zone_01 through Zone_08 and Rocks.
- The runtime loader starts all nine dependencies additively before loading the main scene. All ten remain loaded.
- The audited current Editor had those ten scenes plus `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity`, all clean. This contaminates aggregate live measurements with unrelated content.
- No new crash reproduction, Play Mode test, build, or scene loading was performed. The saved crash and current Editor sample are different observations, not a continuous capture.

### Impact and Priority

High: confirmed fatal allocation failure prevents reliable loading/authoring. Scene concurrency, large readable meshes, and restricted system commit headroom are strong investigation targets. Texture/HDRP costs are measurable, but no single setting is established as the complete root cause.

### Evidence

#### I-01 — Confirmed native VertexData allocation failure

Saved log: `%LOCALAPPDATA%/Temp/Unity/Editor/Crashes/Crash_2026-09-07_153032988/Editor.log`.

Zone_01–08 loading completes in lines 7301–7381. At line 7382, Unity reports a fatal allocation failure for **3,397,312 bytes**, memory label **VertexData**, in **Mesh/VertexData.cpp**. Subsequent fatal attempts occur in the same log; they are not separate successful reproductions.

At the first failure, allocator totals include:

| Allocator | Reported bytes |
|---|---:|
| ALLOC_GFX | 4,756,527,144 |
| ALLOC_DEFAULT | 2,508,235,588 |
| ALLOC_CACHEOBJECTS | 314,849,388 |

The failed allocation's small size is not the scene's total memory need. ALLOC_GFX is an allocator category, not a direct measurement of dedicated VRAM. The log does not identify the particular mesh responsible.

#### I-02 — Very little system commit headroom

Read-only process/system sample on 2026-09-07:

| Metric | Observed |
|---|---:|
| Physical RAM | About 31.9 GB total; 1.5 GB free |
| System commit | 60.25 / 62.96 GB, approximately 95.7% |
| Main Editor private memory | 16.04 GB |
| Main Editor working set | 7.25 GB |
| Two project AssetImportWorkers, private memory | 1.48 GB + 1.23 GB |
| NVIDIA dedicated-memory sample | 6,421 / 12,227 MiB used; 5,523 MiB free |

This proves current pressure, not the exact OS/VRAM state at the saved crash. Free dedicated VRAM in this sample does not rule out an earlier GPU allocation problem. Process private bytes, Unity allocated/reserved counters, object sizes, and GPU accounting have different scopes and must not be summed.

#### I-03 — All location dependencies load concurrently and stay resident

**Latest user-directed revision (2026-09-08):** concurrent dependencies are restored; only the main scene waits for all dependencies to succeed. The model owns transition state, and failures propagate without rollback. The sequential source experiment in the following update is superseded. Final residency remains unchanged, while temporary peak memory remains unmeasured. See [[History/Records/Scene Loading Model State and Fail Fast Policy]].

**2026-09-08 update:** The original concurrent loader below is historical evidence. [[History/Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]] changes dependency loads to sequential execution, adds reverse failure cleanup and failed-handle release, and rejects overlapping service transitions. Two executable baseline reproductions and ten candidate checks validate isolated control flow. Real memory, Addressables handle recovery, and travel remain unverified because preflight commit headroom was only 1.23 GiB (1.9%). All ten destination scenes still remain resident after success; this issue is not resolved.

Sources: `Assets/Settings/Data/SceneData.asset:28`; `Assets/Scripts/Services/Scenes/SceneService.cs:61`; `Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs:30`.

The loader opens Loading in Single mode, starts nine additive dependency loads concurrently, awaits them, loads DefaultLocation additively, activates it, and unloads only Loading. Zone partitioning currently provides no spatial residency reduction.

Static scene inspection identifies about **35,935 GameObjects, 26,909 MeshRenderers, and 7,246 MeshColliders** across the location scene files. These are serialized counts, not visible draws or a byte estimate. The stored spatial split report is historical evidence and cannot replace current scene inspection.

There is no in-flight level-load guard in this path. Overlapping requests are a possibility, not an observed event. A failed dependency can throw before cleanup of already successful scene handles. Intentional residency on a successful transition is not, by itself, an Addressables leak.

#### I-04 — Large readable environment meshes dominate the sampled asset objects

**2026-09-12 imported-dependency update:** the ten-scene AssetDatabase closure contains 160 ModelImporters (632 Mesh subassets): 126 Read/Write enabled and 34 disabled, all Mesh Compression Off. Five separate derived collider meshes remain readable. Vertex compression mask 4054 is enabled for Normal, Tangent, UV0 and UV2–7; effective Player buffers are unmeasured. There is no identified project runtime CPU vertex-buffer consumer, but collider cooking/transform and third-party consumers still require per-model review. This is a narrower dependency scope than the September 9 whole-project model count. Do not mass-enable Mesh Compression: it targets disk size, can add loading cost, and prevents vertex compression on the same mesh. Current evidence and references: [[Research/DefaultLocation Rendering and Bake Audit#Mesh compression: Off is not automatically a defect]].

Bounded live census: `Resources.FindObjectsOfTypeAll` and `Profiler.GetRuntimeMemorySizeLong` on already loaded objects; no new assets loaded.

| Object category | Count | Reported runtime-object bytes |
|---|---:|---:|
| Mesh | 544 | 1,199,557,940 (1.117 GiB) |
| Textures excluding RenderTexture | 1,036 | 434,171,957 (414 MiB) |
| RenderTexture | 48 | 422,538,136 (403 MiB) |

The largest meshes are readable. Sample contributors:

| Mesh/subasset | Vertices | Approximate reported MB | Scene usage |
|---|---:|---:|---|
| SM_CastleSideBridge_02_LOD0 | 394,291 | 51.57 | Zone_02 |
| SM_chandelier_LOD0 | 333,601 | 43.63 | Zone_03, Zone_05 |
| SM_Castle_Gate_01_LOD0 | 242,890 | 31.75 | Zone_02 |
| SM_CastleSideBridge_02_LOD1 | 248,831 | 31.58 | Zone_02 |
| SM_Tower_Bot_LOD0 | 238,068 | 30.62 | Zone_01, Zone_02 |
| SM_Castle_Gate_02_LOD0 | 212,200 | 27.16 | Zone_04 |
| SM_chandelier_LOD1 | 203,719 | 25.95 | Zone_03, Zone_05 |
| SM_candle_holder_LOD0 | 172,876 | 22.65 | Zone_03, Zone_05 |

Sources are FBX assets under `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Meshes/`. These are shared Mesh object measurements, not per-instance costs multiplied by scene occurrence. High-detail lower LODs also contribute. Readability is confirmed; whether each consumer permits removing CPU access is unknown.

Repeated serialized MeshFilter references include `SM_Exterior_Floor_1` 1,370 times in Zone_06 and 1,310 in Zone_04; Rocks references `SM_GroupedRock_4` 1,425 times and `SM_Grouped_Rock_3` 1,205 times. Repetition indicates an important rendering/batching workload but does not prove duplicated shared-mesh allocations.

No loaded Mesh name contained “combined.” That does not conclusively exclude every batching allocation, but this census provides no affirmative evidence that static-combined meshes caused the crash.

#### I-05 — No existing quality preset reduces texture/LOD residency settings

**2026-09-12 correction to the historical title/table below:** an Editor Low Memory preset now exists and enables streaming, while High Fidelity, Balanced and Performant still disable it. The live preset is High Fidelity. All 156 project texture dependencies inspected are already GPU-compressed, mipmapped and non-readable. The 143 textures assigned through actual mesh-material shader properties comprise 70 DXT1 and 73 DXT5; 64 enable per-texture streaming and 79 do not. This is a residency/quality-policy gap, not an uncompressed-world-texture defect. See [[Research/DefaultLocation Rendering and Bake Audit#Texture compression: already configured; streaming remains partial]].

Source: `ProjectSettings/QualitySettings.asset`.

| Setting | High Fidelity | Balanced | Performant |
|---|---:|---:|---:|
| Global texture mipmap limit | 0 | 0 | 0 |
| Mipmap streaming active | 0 | 0 | 0 |
| Streaming budget, currently inactive | 512 | 512 | 512 |
| LOD bias | 1 | 1 | 1 |
| Maximum LOD level | 0 | 0 | 0 |
| Anisotropic texture setting | 2 | 2 | 2 |

The current quality index is 0, High Fidelity, confirmed by the live Editor. Standalone's configured default is also 0. No custom Low Memory quality level exists.

`ProjectSettings/EditorSettings.asset:22` enables streaming in Edit and Play modes, but the Quality streaming switch is off. `m_EnableEditorAsyncCPUTextureLoading: 0` at line 24 means CPU texture loading on demand is disabled. Both Editor and Quality switches are relevant to streaming. [Unity Editor settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-EditorManager.html)

#### I-06 — Material/static flags do not establish the active batching path

**2026-09-12 resolved-instance update:** DefaultLocation plus nine dependencies contain 26,926 MeshRenderers, of which 26,879 are Batching Static. Of 74 distinct actually assigned material assets, 70 enable GPU instancing. 61,811 of 61,858 material slots have both flags. The current SRP Batcher is on and GPU Resident Drawer is off. These imported-instance counts differ in scope from the earlier direct-YAML table below and still do not prove an actual GPU-instanced or static-batched draw.

**LOD0 concern resolved for the current configuration:** 7,247 classic LODGroups are populated with multiple levels, no empty levels/shared cross-level renderer references, and fade mode None. All inspected renderer meshes report Mesh LOD count 1; all 160 model importers disable generated Mesh LOD. Unity's documented static-batching/conventional-instancing restriction to LOD0 applies to the newer **Mesh LOD feature**, which this content does not use. It does not establish that these classic LODGroups are locked to LOD0. Actual near/far Player draw submission remains unverified. [Unity Mesh LOD limitations](https://docs.unity3d.com/6000.3/Documentation/Manual/lod/mesh-lod-introduction.html). Full distinction and scene table: [[Research/DefaultLocation Rendering and Bake Audit]].

`ProjectSettings/ProjectSettings.asset:490`: Standalone static batching is enabled; dynamic batching is disabled.

All three HDRP quality assets have `enableSRPBatcher: 1` and `gpuResidentDrawerSettings.mode: 0`. Live SRP Batcher is enabled. Thus GPU Resident Drawer is not configured as the current path.

A material's GPU-instancing flag and a GameObject's Batching Static flag are independent. Compatibility, material/shader path, renderer conditions, and the actual frame determine batching. Static batching can store combined geometry; material instancing flags are not evidence of reduced residency. [Unity draw-call optimization methods](https://docs.unity3d.com/6000.3/Documentation/Manual/optimizing-draw-calls-choose-method.html)

The targeted serialized audit resolved **81 material assets**: **72 have GPU instancing enabled; nine do not**. Scope: ten scene YAML files and their targeted material/mesh/texture/prefab references, not an imported AssetDatabase or built-bundle dependency manifest.

For directly serialized scene MeshRenderer material slots:

| Scene | Material slots | Instancing enabled | Also Batching Static |
|---|---:|---:|---:|
| Zone_01 | 5,528 | 5,528 | 5,521 |
| Zone_02 | 8,736 | 8,736 | 8,736 |
| Zone_03 | 5,638 | 5,638 | 5,626 |
| Zone_04 | 9,332 | 9,332 | 9,328 |
| Zone_05 | 2,759 | 2,759 | 2,759 |
| Zone_06 | 11,924 | 11,924 | 11,918 |
| Zone_07 | 7,622 | 7,622 | 7,621 |
| Zone_08 | 4,495 | 4,495 | 4,495 |
| Rocks | 5,780 | 5,780 | 5,780 |
| **Total** | **61,814** | **61,814** | **61,784** |

A material slot is not a unique renderer, GameObject, draw call, or runtime instance. The main scene has no direct MeshRenderer slots in this count. The 30 non-Batching-Static slots all use `M_Fog.mat`: Zone_03 12, Zone_01 7, Zone_06 6, Zone_04 4, Zone_07 1. Their static flags value 19 lacks BatchingStatic. That difference is recorded, not asserted to be a defect in a fog renderer.

The audit also finds 17 slots in four prefab **source** assets, separate from the direct scene totals. These are defaults, not resolved live prefab-instance overrides. Nine non-instancing materials belong to Grace/GroundItem and TemplateIron/Wood references; none occurs in the direct-scene slot table.

Most frequent direct material slots: `M_Bricks` 5,907; `M_Ornamental_2` 4,653; `M_ExBrickWall` 4,489; `M_Grass` 4,086; `M_Column` 4,062; `M_Tiles_3` 3,988; `M_Trim_1` 3,541; `M_Trim_2_A` 3,441. These environment materials are under the FantasyCastle HDRP art material hierarchy.

The requested pairing is therefore overwhelmingly already enabled: **99.95% of direct slots have both flags**. That is configuration evidence, not proof of simultaneous GPU instancing and static batching, or of a batching-caused OOM.

#### I-07 — HDRP quality tiers retain significant buffer/atlas capacity

Sources: `Assets/Settings/RenderPipelines/HDRP High Fidelity.asset`, `HDRP Balanced.asset`, `HDRP Performant.asset`.

| Active configuration field | High Fidelity | Balanced | Performant |
|---|---|---|---|
| Color buffer | R16G16B16A16 (48) | R11G11B10 (74) | R11G11B10 (74) |
| SSGI support | On | Off | Off |
| Volumetrics support | On | On | Off |
| SSAO / subsurface scattering | On / On | On / On | On / On |
| Ray tracing / SSR support | Off / Off | Off / Off | Off / Off |
| Unified reflection atlas | 4096² | 4096² | 4096² |
| Sky reflection resolution | 1024 | 512 | 256 |
| Punctual shadow atlas | 4096² | 4096² | 4096² |
| Area shadow atlas | 4096² | 2048² | 2048² |
| Cached punctual/area atlas | 4096² / 4096² | 2048² / 2048² | 2048² / 2048² |
| Transparent decal atlas | 2048² | 2048² | 2048² |
| Lit shader mode | Both | Both | Both |
| Dynamic resolution | Off | Off | Off |

High Fidelity selects APV (`lightProbeSystem: 1`), with serialized pool budget 1024, SH band setting 1, and GPU/disk streaming disabled. Balanced and Performant select the other light-probe system; their retained APV budget fields are not proof of an APV allocation.

Live RT examples include a 4096² reflection-probe cache at about 89.48 MB and shadow resources. Configured maximum atlas settings do not guarantee that every such atlas is currently instantiated at that size.

Ray tracing, SSR, volumetric clouds, and water are disabled in the active asset. Those disabled support fields are not established causes of this crash.

#### I-08 — Scene Volume overrides retain expensive authoring settings

`Assets/Scenes/DefaultLocation/DefaultLocation.unity:12172` references the vendor `Sky and Fog Settings Profile.asset` under `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Scene/FC_HDRP/`.

The profile has:
- Active HDShadowSettings: **5,000-unit shadow distance and four cascades** (around lines 487–500).
- Active AmbientOcclusion with quality 2 and full-resolution serialized settings.
- Active Fog with volumetric fog enabled, depth extent 512, and fog budget 0.5.
- Active Bloom with quality 2 and high-quality filtering/prefiltering.
- DepthOfField inactive.

These are serialized overrides, not proof that every effect is rendered by every camera. The scene's HD camera uses default rendering settings and disables dynamic resolution; its far clip is 500. Legacy Quality shadow-distance values do not describe this HDRP Volume override.

#### I-09 — HDR sky and Editor targets are material measured costs

Largest sampled imported texture: `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Textures/T_HDRSKY.HDR`, imported as a 2048² cubemap, about **67.18 MB** reported runtime size.

Global mipmap limits do not cover cubemaps, non-mipmapped textures, or render targets. [Unity mipmap-limit scope](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/QualitySettings-globalTextureMipmapLimit.html)

The live RT census contains about **97.26 MB GUIViewHDRRT** and **44.24 MB GameView RT**. These are Editor view costs, not transferable directly to a Player budget. Several loaded UI textures are 11–13 MB each; their scope includes unrelated currently loaded content.

Targeted asset-reference/importer inspection found **145 textures**: all serialize maximum texture size 2048, Read/Write off, mip streaming off, texture compression 1, and Crunch off. Source-header inspection found 132 with a dimension at least 2048; `Tiles1_N`, `Fountain_N`, `Tiles1_BC`, and `Tiles1_ORM` include 4096² source images capped by the importer. The cap is not proof of actual resident dimensions or effective per-platform overrides. Read/Write textures are not established as an issue in this scope.

The same targeted closure contains **133 mesh assets; 131 serialize Read/Write enabled and all 133 serialize meshCompression 0**. This corroborates the large readable meshes in I-04. Compression flags alone do not quantify savings, and CPU access may be required by consumers. Other imported subassets, package/built-in resources, and runtime-created objects are outside this disk closure.

#### I-10 — Graphics fallback differs sharply from the selected quality asset

`ProjectSettings/GraphicsSettings.asset` references `Assets/Settings/HDRPDefaultResources/HDRenderPipelineAsset.asset` as the default fallback, while the quality override currently selects High Fidelity.

The fallback's `reflectionProbeTexCacheSize: 1073750016` decodes to **16384 × 8192**, not a raw byte count or invalid dimension. It also configures a 4096² decal atlas and 32-bit shadow atlases. Enum interpretation is verified against HDRP 17.3's `Runtime/Lighting/LightLoop/GlobalLightLoopSettings.cs:69`.

This is a latent configuration discrepancy; the live Editor is using High Fidelity, so the fallback is not attributed as its current crash cause.

#### I-11 — Existing bake/build evidence does not validate the current configuration

**2026-09-12 confirmed occlusion configuration defect — high priority:** the nine dependency scenes each reference a different, nonempty occlusion asset, each containing only its own scene GUID. DefaultLocation has no occlusion asset or valid occlusion scene GUID. SceneService loads all dependencies additively and then activates DefaultLocation. Unity supports one runtime occlusion asset and requires scenes used together to share a joint bake. The current data therefore cannot establish correct combined cross-zone occlusion, even though the main camera enables occlusion culling. Exact references and current bake evidence are in [[Research/DefaultLocation Rendering and Bake Audit#Confirmed issue: occlusion bakes are incompatible with additive loading]]. [Unity multiple-scene occlusion](https://docs.unity3d.com/6000.3/Documentation/Manual/occlusion-culling-scene-loading.html)

**2026-09-12 confirmed lighting-output gap — medium priority:** all ten scenes have null LightingDataAsset and no inspected MeshRenderer has a usable lightmap index. DefaultLocation has 60 enabled, active Baked point lights and one Mixed directional light; zones have ten additional active Mixed lights. The main APV scene component has a null baking-set reference, and Rocks Baking Set has empty baked data. No ReflectionProbe components were found. The authored baked contribution has no persisted output; this is not a claim that the whole scene renders black. See [[Research/DefaultLocation Rendering and Bake Audit#Confirmed issue: lighting is configured for baking, but bake output is absent]].

**Resolution handoff for these bake findings:** review/reuse the existing location bake tooling; persist a shared occlusion bake across the intended scene set and validate the actual dependency-first load sequence. Generate and verify the intended lighting/APV output, or explicitly revise the lighting design. Acceptance requires linked nonempty data, correct occlusion across zone boundaries and visible baked-light contribution in the Player. This investigation changed no scene, importer or bake data; existing draft bake plans were not executed.

Historical observations:

- All ten location scenes serialize a null `m_LightingDataAsset`. The main scene's ProbeVolumePerSceneData has a null `serializedBakingSet`; the location Baking Set has empty shared-data GUIDs. This does not establish a large baked-lighting payload as the cause.
- Nine scenes have occlusion-data references; the main scene has none. Presence alone does not establish correct cross-zone culling. The stored `occlusion_report.txt` dates to August 12 and names the old 23-scene layout, before the zone split.
- The Addressables scene group schema uses `m_BundleMode: 1`, verified as **Pack Separately** in installed Addressables source. Shared-asset duplication has not been measured. Available August build reports precede the current zone layout.
- Serialized light-component counts include many disabled components; a raw count is not an active-light budget. For example, Rocks' 60 inline Light components are all disabled.

#### I-12 — Current counters are not a gameplay performance baseline

Read-only Editor stats report about 54.10M triangles, 108.19M vertices, 18,533 draw calls and 170 SetPass calls. Idle CPU/GPU values of 0.507 ms / 0 ms are not useful Player timings.

Unity counters at that sample were approximately 2.920 GB allocated / 7.312 GB reserved, with Mono 1.191 GB used / 1.447 GB heap. They do not reconcile all OS process memory and cannot isolate the allocation that failed in the saved crash.

### Hypotheses

- Nine-way concurrent dependency loading increases temporary allocations enough to exhaust remaining commit.
- Large readable meshes and imported LOD data materially increase native residency.
- Static batching could increase memory in a built/runtime configuration, but current evidence does not demonstrate those combined allocations.
- Shared dependencies may be duplicated in built scene bundles; no current build layout proves it.
- Partial failed loads or overlapping transitions may retain/multiply allocations; no reproduction has isolated this path.
- Editor-managed/importer/driver allocations account for part of the gap between sampled object sizes and process private memory.

### Open Questions

- Was the reported crash triggered by runtime travel, scene opening, Play Mode transition, importing, or another Editor action? The saved log proves the allocation failure after zone loads, not the exact user action.
- What are isolated peak and settled footprints for each zone and for a Development Player?
- Which mesh consumers require CPU readability, and how much data can actually be removed from Player and Editor independently?
- Which repeated mesh/material pairs use instancing, SRP batching, static batching, or individual draws in a captured frame?
- Is Addressables running against built bundles or the asset database in the failing reproduction?
- What target Player hardware and visual-quality budget should define acceptance?

## Resolution Handoff

This remains an **open issue**. [[Work/Plans/DefaultLocation Memory Optimization]] is in progress after explicit execution requests. Phase 6 adds a bounded loader source experiment with isolated control-flow validation; it does not establish a measured memory improvement or native handle recovery. Current Addressables layouts are obsolete and a new build is deferred under memory pressure. See [[History/Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]] for completed source work, exact safety evidence, and remaining acceptance gates. The original audit evidence below its dated updates remains historical, not a description of every current setting.
