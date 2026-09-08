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
- [x] Measured every imported LOD/submesh. Existing visual cascades already reduce triangles by roughly 50% / 25% / 12.5% / 6.25%, and every visual LOD is used, so no new visual LOD or threshold change was justified. Five project-owned readable collider meshes now use the vendor-authored convex hulls; 68 DefaultLocation colliders moved from LOD0 geometry (about 13.65M referenced triangles) to about 120K collider triangles. The five visual FBXs are non-readable; Gate 02 remains unchanged because it has no authored hull. No mesh-compression memory claim was made.
- [x] Ranked repeated mesh/material pairs by instance count and vertex cost. Tower (36 instances / 8.57M LOD0 vertex references) and Bridge (14 / 5.52M) lead the reviewed set. Shared mesh/material identity is preserved; no instantiated copies were found.
- [x] Enabled mip streaming on 64 reviewed world textures: four Batch 1 maps plus 16 opaque base maps, 24 normal maps, and 20 ORM maps. Special mask, alpha, emissive, glass, and particle maps remain intentionally excluded. Importer maximum size remains 2048 to preserve High Fidelity/Standalone quality; Editor Low Memory's global mip limit 1 is the reversible 1024-equivalent authoring limit. No format, compression, sRGB, alpha, readability, mip-generation, priority, or platform override changed.
- [x] Checked the 75 loaded texture dependencies of the reviewed meshes. They totaled about 269.0 MiB in the Editor sample, had no Standalone overrides, and imported as effective DXT1 defaults or DXT5 normals at 2048 maximum size.

Verify: Unity readback confirms all 64 reviewed textures stream and all five visual FBXs are non-readable while their derived collider assets remain readable. Exactly 68 scene colliders plus the Bridge prefab use the project-owned hulls; no affected collider still references visual LOD0. Navigation was rebaked for DefaultLocation and Zones 01/02/04/06/07/08; all seven NavMeshData assets load, six links remain, and all 7,246 loaded MeshColliders cooked without failure. Three sequential load/unload cycles with Bootstrap retained were stable but are not exact comparisons. A corrected ten-location-scene Editor Low Memory trace settled at 6.79 GiB private memory and 36.49/47.16 GiB system commit with 10.66 GiB (22.6%) headroom, passing the 20% target in that session. All 64 changed textures converged to desired/loaded mip 3. Final capture: `Temp/DefaultLocation-Phase4-Final-Editor-Low-Memory.png`. Development Player and registered gameplay traversal/path probes remain validation gaps. Avoid a project-wide reimport that recreates the peak.

> **32 GiB physical RAM concern:** The measured **58.06/66.29 GiB** is system-wide committed memory versus the Windows commit limit, backed approximately by physical RAM plus configured pagefiles (on SSD here). It is not 58.06 GiB of resident RAM or Unity-only usage, and it does not establish how much data is currently in the pagefile. At about **87.6% of the commit limit**, only **8.23 GiB** of settled commit headroom remains. This is still a concern on the 32 GiB workstation: the successful load does not demonstrate comfortable physical-memory usage or freedom from paging stalls. Record available physical RAM, process working sets, and paging activity alongside commit in the next measurement. Retain the unmet 20% commit-headroom gate; a larger pagefile alone is not an optimization result.

### Phase 5 — Select the rendering path by experiment

- [x] Preserve SRP Batcher as the baseline. All existing HDRP presets enable it; GPU Resident Drawer remains disabled after the experiment.
- [x] Compare representative repeated objects using (a) existing SRP Batcher, (b) a bounded static-batching subset, and (c) HDRP 17.3 GPU Resident Drawer with compatible materials/meshes.
- [x] Do not mass-enable GPU Instancing and Batching Static together. The audit already finds instancing enabled on all 61,814 direct scene material slots, with 61,784 also Batching Static; the 30 exceptions are fog slots. Static batching can duplicate transformed geometry, and material instancing checkboxes are not proof of instanced draws. No project-wide batching flags were changed.
- [x] GPU Resident Drawer compatibility was tested with BRG shader stripping temporarily set to Keep All. The bounded Editor sample did not show a sufficient win, so no BRG/material/LOD/lightmap changes were made and the original stripping setting was restored; this is not a Player-wide conclusion.
- [ ] Confirm the actual draw path in a visible Player/Frame Debugger capture, including Hybrid Batch Group for GPU Resident Drawer. The available Scene-view capture confirmed SRP Batcher events and no Hybrid Batch Group event, while offscreen Game-view capture did not expose Frame Debugger events.

Verify (Phase 5): Unity 6000.3.11f1, High Fidelity HDRP, `DefaultLocation` plus `Zone_02`, Main Camera fixed to the representative hall view, 640x360 capture path, and runtime-only experiments. The table records one post-settle measurement snapshot per condition, not a repeated-sample median or Player build. SRP baseline: 920 draw calls, 31 set-pass calls, 3.622 GB Unity allocated, 5.971 GB reserved, 4.290 ms CPU, 2.391 ms GPU; GPU Resident Drawer: 907 draw calls, 27 set-pass calls, 3.630 GB allocated, 5.971 GB reserved, 4.652 ms CPU, 2.303 ms GPU; bounded static subset (`StaticBatchingUtility.Combine` on four repeated-object LOD roots): 940 draw calls, 31 set-pass calls, 3.622 GB allocated, 5.971 GB reserved, 4.574 ms CPU, 2.250 ms GPU. The GPU Resident Drawer draw-call reduction was small and did not offset the higher CPU/allocated-memory sample; the static subset increased draw calls. Scene-view Frame Debugger reported 198 events containing `RenderLoop.DrawSRPBatcher` and no `Hybrid Batch Group`. Temporary runtime/static changes were discarded on exiting Play mode; `ProjectSettings/GraphicsSettings.asset` was restored to `m_BrgStripping: 0` and no HDRP asset, scene, prefab, or material was persisted.

Unity's current HDRP guidance favors SRP Batcher/GPU Resident Drawer and warns that static batching conflicts with BRG; material instancing is not the default recommendation for HDRP. [Optimization methods](https://docs.unity3d.com/6000.3/Documentation/Manual/optimizing-draw-calls-choose-method.html), [HDRP GPU Resident Drawer](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.3/manual/gpu-resident-drawer.html)

### Phase 6 — Bound loading and then consider spatial streaming

**Latest user decision (2026-09-08):** supersedes the sequential/rollback experiment below. Runtime loading state belongs in `SceneModel`; dependencies load concurrently and the main scene starts only after all succeed. Failure cleanup is explicitly removed: throw on the first observed failure and fix the defect, with no recovery guarantee for outstanding native operations. This choice preserves final residency but does not bound temporary dependency-loading peaks. Historical comparison/rollback checklist results below describe `a57cd2e2`; they are not current behavior. See [[History/Implementation Records/Scene Loading Model State and Fail Fast Policy]]. Live memory/build-layout validation remains outstanding.

Execution started on 2026-09-08. **Measurement gate blocked:** the read-only preflight found 64.59/65.81 GiB system commit, only 1.23 GiB (1.9%) headroom, with a clean ElevatorDemo scene open. No live loading comparison, test run, Play Mode session, or build was started. The bounded source experiment is tracked in [[History/Implementation Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]]; it is not an accepted memory optimization until the controlled comparison passes.

- [ ] First compare the existing nine-way concurrent dependency load with sequential loading or a small concurrency limit. This targets the peak; all ten scenes will still be resident at completion.
- [x] Implemented a sequential dependency-loading **source experiment**, preserving all-ten-scene residency and dependency-plus-target progress. The controlled memory comparison above is still required before accepting it as a memory optimization.
- [x] Added explicit reverse-order failure cleanup and a single in-flight service transition after executable baseline reproduction confirmed overlapping Loading requests and unreleased partial loads. The service owns completion/cleanup; overlapping requests fault; there is no cancellation or automatic retry. A successful Loading scene remains for recovery. Ten isolated source-linked control-flow checks pass; actual Unity handle/recovery behavior is still unverified. The broader pending-spawn mutation before service admission remains open in [[Work/Issues/Scene Transitions Allow Concurrent Load Operations]].
- [ ] Inspect a current Addressables build layout for shared material/mesh/texture duplication. The scene group uses Pack Separately; that alone does not prove duplication. Existing August build reports predate the current zoning and are not validation.

  Freshness inspection completed: all available local build products/layouts are from August 19 and contain none of the current location scene group/scenes. The current ten-scene group/schema dates from August 31. Building a current layout is deferred until memory headroom permits; legacy duplicate records cannot answer this item.
- [ ] If steady-state residency still exceeds budget, design real zone streaming with bounds, neighbor prefetch, unload hysteresis, and a global scene for shared services. Include Rocks in the residency decision.
- [ ] Before streaming, inspect cross-scene references, entity/service lifetime, enemies, save state, physics and NavMesh links, lighting, and occlusion data. Do not replace the loader with distance-based unloading without that architecture work.

Exact entry points: `Assets/Scripts/Services/Scenes/SceneService.cs`, `Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs`, `Assets/Settings/Data/SceneData.asset`, `Assets/AddressableAssetsData/AssetGroups/Scenes - DefaultLocation.asset`.

Verify: peak and settled memory, transition completion, released handles, failure recovery, and repeated travel. Spatial streaming requires a separate reviewed design.

Spatial streaming remains conditional and unimplemented. The current safety sample does not measure DefaultLocation steady-state residency. Recover headroom, perform a matched bounded comparison and three travel cycles, and establish the Player budget before deciding whether a streaming design is required. That design must cover Rocks and every lifetime/reference concern listed above.

## Risks and Rollback

Keep each experiment in a separate small change. Preserve original asset GUIDs/references and record importer overrides. Revert only the experiment, reimport/save through Unity, and remeasure the baseline. Lower texture detail, shadow coverage, AO/fog, collider geometry, and LODs can affect appearance or gameplay. Profile changes can affect other scenes sharing the vendor profile. Static batching can improve CPU time while worsening memory. A larger pagefile is not evidence that content has been optimized.

## Validation

No automated Play Mode tests or builds were run. Phase 6 passed two isolated executable baseline reproductions and ten candidate control-flow checks using the real SceneService source with .NET Task and fake Unity/Addressables operations; these do not validate native handle lifetime, runtime travel, or memory improvement. Phase 5 did use a bounded, runtime-only Play-mode rendering experiment; it left no scene changes. Future Unity tests require `unity command list_open_scenes --json` or `assert_test_ready`, every scene clean, asynchronous execution, a fixed time budget, and final confirmation that no test remains active. A dirty Untitled scene blocks testing.

Assign gameplay/Play Mode coverage to a separate `unity_test_runner` phase under project policy. Run targeted load/handle Edit Mode coverage only where it can safely model the behavior. Do not take large snapshots until adequate memory headroom exists.

## Execution Handoff

Status is **in-progress** after explicit execution of Phases 1–3, Phase 4 texture Batch 1, Phase 5 rendering-path selection, and the Phase 6 bounded source experiment. Phase 6 memory comparison, native handle/failure recovery, repeated travel, and fresh build-layout validation remain blocked by system memory pressure. Use `unity_profiler` for controlled comparisons; use `csharp_worker` only after the loader change is bounded, and `unity_architect` for any later streaming design.

Required context keys: `vault-usage`, `plan-workflow`, `issue-workflow`; resolve additional domain keys only when the implementation touches their systems. Future serialized mutations must be imported, specifically reserialized if edited on disk, saved through Unity, and checked for errors. Remaining decisions are target Player hardware/budget and acceptable authoring visual reductions.
