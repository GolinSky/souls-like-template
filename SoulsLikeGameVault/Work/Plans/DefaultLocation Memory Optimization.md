---
title: DefaultLocation Memory Optimization
type: plan
domains:
  - performance
  - rendering
  - scenes
status: in-progress
authority: advisory
updated: 2026-09-08
source_commit: 3925ea83
tags:
  - work/plan
  - status/in-progress
---
# DefaultLocation Memory Optimization

## Plan Contract

### Goal

Make DefaultLocation safe to author and load on the audited 32 GB RAM / 12 GB VRAM workstation, and establish a separate, measured Player memory budget. Introduce a custom **Editor Low Memory** quality preset, then address asset residency and loading peaks. Phases 1–3 and the first bounded Phase 4 texture batch have been executed; the remaining Phase 4 work is still measurement-gated.

### Source Research and Decisions

Evidence: [[Work/Issues/DefaultLocation Memory and Rendering Issues]]. Audit date: 2026-09-07; Unity 6000.3.11f1, HDRP 17.3.0, Addressables 2.9.1.

The crash log confirms a native `VertexData` allocation failure after the zone scenes load. The current system is near its commit limit. Existing loaded meshes total about 1.12 GiB, while textures and render textures each total about 0.4 GiB. Those object totals do not explain the entire Editor process footprint and cannot be added to OS commit as separate costs.

Prioritize a safe baseline, mesh residency, and load concurrency. Texture/HDRP reductions should provide headroom but are not a proven complete fix. Scene splitting currently changes organization, not residency: the loader requests all nine dependencies and keeps all ten location scenes loaded.

### Assumptions and Non-Goals

- “Low memory mode” means explicit Editor texture-loading settings plus a separate HDRP/Quality preset; no unverified command-line memory switch.
- Keep the existing High Fidelity, Balanced, and Performant assets available. Do not globally reduce shipping quality by changing the Standalone default merely to make authoring lighter.
- No automatic asset deletion, broad third-party reimport, project-wide static batching change, pipeline migration, or Unity upgrade in this plan's first phase.
- Do not change Obsidian configuration.
- A Player crash, specific leaking mesh, and actual static-batching duplication have not been demonstrated.
- Lower LOD bias/culling can reduce drawing without unloading the referenced high-detail meshes. Occlusion culling alone does not solve scene residency.

### Success Criteria

- A clean baseline and each experiment record loaded scenes, active quality/HDRP asset, Editor or Player mode, process private bytes, total system commit, physical RAM, VRAM, Unity native/managed counters, and top mesh/texture/render-target sizes.
- After asset/loading changes, three controlled DefaultLocation load/unload cycles complete without OOM, failed scene operations, or increasing retained objects/handles after settling.
- Proposed workstation gate: retain at least 20% system commit headroom at peak; set the final Editor/Player budgets from the isolated baseline. This is an acceptance target, not a measured current capability.
- Editor Low Memory measurably reduces peak/private memory and remains usable for authoring; preserve scene lighting fidelity in the existing normal presets.
- Frame Debugger establishes the chosen batching path. Material checkboxes and static flags alone are not evidence of batching.
- Every future asset change is imported, saved, and checked for serialization errors through Unity.

## Execution Plan

### Phase 1 — Establish a safe baseline

- [x] Preserve the first-failure crash evidence and capture the exact allocation stack/context. Record background process usage and pagefile/commit limit; distinguish low physical RAM from exhausted commit and GPU allocation failure.
- [x] Start the comparison from a clean Editor session with only the intended scenes. The current live sample also includes `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity`; exclude it from the controlled baseline without discarding unsaved work.
- [x] Read open-scene state first. Do not close/reload unknown or dirty scenes, force GC, or take a large Memory Profiler snapshot while the machine has only a few GB of commit headroom.
- [x] Measure empty/bootstrap, main scene only, Rocks, each zone added in order, and all ten scenes; use small telemetry first, then snapshots once safe. Avoid rerunning the known concurrent crash just to obtain a baseline.
- [ ] Run a separate Development Player capture after Editor safety is established. Record frame dimensions and camera because Editor view targets materially affect the current render-texture total. Deferred: current build settings contain only Bootstrap and system commit pressure makes a new build unsafe in this session.

Verify: a table identifies incremental and peak costs, with no claim that disk size equals runtime RAM.

### Phase 2 — Add Editor Low Memory

Create a new quality level `Editor Low Memory` and `Assets/Settings/RenderPipelines/HDRP Editor Low Memory.asset`, starting from Performant. Select it before loading the location in the future validation session.

- [x] Added the quality level and copied the Performant HDRP asset.
- [x] Applied the initial mip, streaming, LOD, editor CPU-texture-loading, atlas, shadow, decal, and SSAO settings listed below.

Proposed starting settings, subject to visual and memory measurement:

| Area | Proposed setting | Purpose / qualification |
|---|---|---|
| Quality textures | Global mipmap limit 1; test 2 only if needed | Half/quarter width and height for eligible mipmapped textures |
| Mipmap streaming | Enabled; initial 512 MB budget; max reduction 3–4 | Start from measured demand; the budget is not a cap on total Editor memory |
| Editor texture loading | Keep Edit/Play mip streaming enabled; enable **Load texture data on demand** | Reduce retained CPU-side texture data |
| LOD | Bias 0.5; retain maximum LOD level 0 initially | Earlier LOD switching without silently deleting needed detail |
| HDRP color | R11G11B10 | Smaller color buffers; confirm no alpha-output requirement |
| HDRP features | SSGI, SSR, ray tracing, volumetrics/clouds off; SSAO off initially | Remove unnecessary authoring buffers and work |
| Other HDRP support | Review SSS, distortion, transparent pre/postpasses, custom passes, decals; disable only verified unused features | Avoid removing required rendering such as project custom effects |
| Lighting system | Use Performant's non-APV baseline; if APV is required, test a smaller supported pool and streaming separately | Do not equate the serialized APV budget with actual allocated bytes |
| Reflection cache | Start with 1024² unified reflection atlas; sky reflection 128–256 | Tune from measured probe use; lower sizes can evict probes |
| Shadows | Directional cap 1024; punctual atlas 2048; area/cached atlases 1024 where used; 16-bit precision | Bound atlas allocations; validate overflow and shadow loss |
| Decals | Start with 1024² transparent-decal atlas | Measure; retain required decals |
| Editor view | One modest-size shaded view; fixed Game view e.g. 1280×720 | Reduce Editor render targets; not a content-residency fix |

Unity documents that the global mip limit excludes cubemaps, non-mipmapped textures, and render targets, and changing it reuploads affected loaded textures. Set the preset before large loads; handle the HDR sky separately. [Global mipmap limit](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/QualitySettings-globalTextureMipmapLimit.html)

Editor streaming requires the Quality streaming switch as well as Editor switches. CPU texture loading on demand trades some processing/temporary low-resolution display for memory. [Editor settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-EditorManager.html)

Non-streaming texture memory and minimum retained mips can prevent meeting the configured streaming budget. [Streaming budget](https://docs.unity3d.com/6000.3/Documentation/Manual/TextureStreaming-configure.html)

### Phase 3 — Make the location's Volume settings scale with quality

- [x] Use a project-owned low-quality Volume profile/selection rather than editing the vendor profile for every preset. `DefaultLocation.unity` now keeps `Assets/Settings/RenderPipelines/DefaultLocation Volume Profile.asset` as its serialized normal source, with `Assets/Settings/RenderPipelines/DefaultLocation Volume Low Memory Profile.asset` selected through a scene-local quality selector.
- [x] For the low preset, use 150 units of shadow distance and two cascades; the castle's scale and the main-vista camera were checked at 640×360. The normal profile retains the vendor values in the project-owned copy. The legacy Quality `shadowDistance: 15` value is not used to control HDRP.
- [x] Disable the Volume's high-quality AO and Bloom and disable volumetric fog in the low profile because the low HDRP asset does not support volumetrics.
- [x] Review the 2048 cubemap `T_HDRSKY.HDR` separately: a project-owned 1024 import trial rendered the castle black and was rejected; the accepted low profile keeps the known-good original HDR sky.
- [x] Keep dynamic resolution optional. The current camera and both HDRP assets remain disabled; 640×360 view dimensions were used for measurement and screenshots.
- [x] Compare live frame settings and Volume overrides with serialized settings. The quality switch was tested in both directions; the normal profile remained active for High Fidelity and the low clone for Editor Low Memory, without dirtying the loaded scenes.

HDRP allocates resources according to supported asset features; frame settings and Volumes further determine rendering. [HDRP asset](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.3/manual/HDRP-Asset.html)

Verify: same camera screenshots and GPU/RT counters at both presets, without missing required effects in the normal preset. Final captures: `Temp/DefaultLocation-Phase3-Final-High-Fidelity.png` and `Temp/DefaultLocation-Phase3-Final-Editor-Low-Memory.png`.

### Phase 4 — Reduce mesh and texture residency

- [x] Audited the measured largest meshes: `SM_CastleSideBridge_02`, `SM_chandelier`, `SM_Castle_Gate_01`, `SM_Tower_Bot`, `SM_Castle_Gate_02`, and `SM_candle_holder` under `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Meshes/`. The six already-resident imported mesh resources totaled about 411.8 MiB in the Editor sample.
- [x] Audited consumers before changing Read/Write. Every target's LOD0 is used by a MeshCollider; Bridge and Tower also have negative or nonuniformly scaled consumers, and all targets participate in static/navigation authoring. No Read/Write change was made without collider, navigation, and Player validation.
- [ ] Measure topology and all imported LODs, not only LOD0. Reduce genuinely excessive geometry, author more effective lower LODs and simpler collider meshes, and check whether unused imported submeshes/LODs can be excluded. Do not use mesh compression as a promise of equivalent runtime memory reduction.
- [x] Ranked repeated mesh/material pairs by instance count and vertex cost. Tower (36 instances / 8.57M LOD0 vertex references) and Bridge (14 / 5.52M) lead the reviewed set. Shared mesh/material identity is preserved; no instantiated copies were found.
- [ ] Apply importer mip streaming and resolution limits to eligible world textures. Batch 1 enabled mip streaming for `T_CandleHolder_BC`, `T_Column1_BC`, `T_Trim1_BC`, and `T_Trim2_BC`; no resolution, normal-map, mask, alpha, cubemap, or format changes were made. The broader eligible set and visual/memory measurement remain pending.
- [x] Checked the 75 loaded texture dependencies of the reviewed meshes. They totaled about 269.0 MiB in the Editor sample, had no Standalone overrides, and imported as effective DXT1 defaults or DXT5 normals at 2048 maximum size.

Verify: Unity readback and persisted metadata confirm only mip streaming changed for Batch 1, with no new import/serialization errors. Per-asset residency savings, collision/navigation correctness, LOD silhouettes, and visual texture quality remain pending under Editor Low Memory or a Development Player. Avoid a project-wide reimport that recreates the peak.

### Phase 5 — Select the rendering path by experiment

- [ ] Preserve SRP Batcher as the baseline. All existing HDRP presets enable it; GPU Resident Drawer is disabled.
- [ ] Compare representative repeated objects using (a) existing SRP Batcher, (b) a bounded static-batching subset, and (c) HDRP 17.3 GPU Resident Drawer with compatible materials/meshes.
- [ ] Do not mass-enable GPU Instancing and Batching Static together. The audit already finds instancing enabled on all 61,814 direct scene material slots, with 61,784 also Batching Static; the 30 exceptions are fog slots. Static batching can duplicate transformed geometry, and material instancing checkboxes are not proof of instanced draws.
- [ ] If GPU Resident Drawer wins, satisfy its BRG shader-variant requirements and remove conflicting batching only in the reviewed scope; check shader/material compatibility, alpha-clipped vegetation, property blocks, LODs, and lightmaps. Record additional buffers/build cost.
- [ ] Confirm the actual draw path in Frame Debugger, including Hybrid Batch Group for GPU Resident Drawer, and compare peak memory as well as frame time.

Unity's current HDRP guidance favors SRP Batcher/GPU Resident Drawer and warns that static batching conflicts with BRG; material instancing is not the default recommendation for HDRP. [Optimization methods](https://docs.unity3d.com/6000.3/Documentation/Manual/optimizing-draw-calls-choose-method.html), [HDRP GPU Resident Drawer](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.3/manual/gpu-resident-drawer.html)

### Phase 6 — Bound loading and then consider spatial streaming

- [ ] First compare the existing nine-way concurrent dependency load with sequential loading or a small concurrency limit. This targets the peak; all ten scenes will still be resident at completion.
- [ ] Add explicit failure cleanup for successfully loaded dependencies and a single in-flight level transition if reproduction confirms the need. Define cancellation/retry ownership rather than hiding failed operations.
- [ ] Inspect a current Addressables build layout for shared material/mesh/texture duplication. The scene group uses Pack Separately; that alone does not prove duplication. Existing August build reports predate the current zoning and are not validation.
- [ ] If steady-state residency still exceeds budget, design real zone streaming with bounds, neighbor prefetch, unload hysteresis, and a global scene for shared services. Include Rocks in the residency decision.
- [ ] Before streaming, inspect cross-scene references, entity/service lifetime, enemies, save state, physics and NavMesh links, lighting, and occlusion data. Do not replace the loader with distance-based unloading without that architecture work.

Exact entry points: `Assets/Scripts/Services/Scenes/SceneService.cs`, `Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs`, `Assets/Settings/Data/SceneData.asset`, `Assets/AddressableAssetsData/AssetGroups/Scenes - DefaultLocation.asset`.

Verify: peak and settled memory, transition completion, released handles, failure recovery, and repeated travel. Spatial streaming requires a separate reviewed design.

## Risks and Rollback

Keep each experiment in a separate small change. Preserve original asset GUIDs/references and record importer overrides. Revert only the experiment, reimport/save through Unity, and remeasure the baseline. Lower texture detail, shadow coverage, AO/fog, collider geometry, and LODs can affect appearance or gameplay. Profile changes can affect other scenes sharing the vendor profile. Static batching can improve CPU time while worsening memory. A larger pagefile is not evidence that content has been optimized.

## Validation

No Play Mode tests, builds, scene changes, or crash reproduction were run for this documentation audit. Future tests require `unity command list_open_scenes --json` or `assert_test_ready`, every scene clean, asynchronous execution, a fixed time budget, and final confirmation that no test remains active. A dirty Untitled scene blocks testing.

Assign gameplay/Play Mode coverage to a separate `unity_test_runner` phase under project policy. Run targeted load/handle Edit Mode coverage only where it can safely model the behavior. Do not take large snapshots until adequate memory headroom exists.

## Execution Handoff

Status is **in-progress** after explicit execution of Phases 1–3 and Phase 4 texture Batch 1. Use `unity_profiler` for the controlled Editor Low Memory comparison; use `csharp_worker` only after the loader change is bounded, and `unity_architect` for the later streaming design.

Required context keys: `vault-usage`, `plan-workflow`, `issue-workflow`; resolve additional domain keys only when the implementation touches their systems. Future serialized mutations must be imported, specifically reserialized if edited on disk, saved through Unity, and checked for errors. Remaining decisions are target Player hardware/budget and acceptable authoring visual reductions.
