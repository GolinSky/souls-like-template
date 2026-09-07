---
title: DefaultLocation Memory Optimization
type: plan
domains:
  - performance
  - rendering
  - scenes
status: draft
authority: advisory
updated: 2026-09-07
source_commit: 3925ea83
tags:
  - work/plan
  - status/draft
---
# DefaultLocation Memory Optimization

## Plan Contract

### Goal

Make DefaultLocation safe to author and load on the audited 32 GB RAM / 12 GB VRAM workstation, and establish a separate, measured Player memory budget. Introduce a custom **Editor Low Memory** quality preset, then address asset residency and loading peaks. This is a proposed plan; no settings or assets have been changed.

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

- [ ] Preserve the first-failure crash evidence and capture the exact allocation stack/context. Record background process usage and pagefile/commit limit; distinguish low physical RAM from exhausted commit and GPU allocation failure.
- [ ] Start the comparison from a clean Editor session with only the intended scenes. The current live sample also includes `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity`; exclude it from the controlled baseline without discarding unsaved work.
- [ ] Read open-scene state first. Do not close/reload unknown or dirty scenes, force GC, or take a large Memory Profiler snapshot while the machine has only a few GB of commit headroom.
- [ ] Measure empty/bootstrap, main scene only, Rocks, each zone added in order, and all ten scenes; use small telemetry first, then snapshots once safe. Avoid rerunning the known concurrent crash just to obtain a baseline.
- [ ] Run a separate Development Player capture after Editor safety is established. Record frame dimensions and camera because Editor view targets materially affect the current render-texture total.

Verify: a table identifies incremental and peak costs, with no claim that disk size equals runtime RAM.

### Phase 2 — Add Editor Low Memory

Create a new quality level `Editor Low Memory` and `Assets/Settings/RenderPipelines/HDRP Editor Low Memory.asset`, starting from Performant. Select it before loading the location in the future validation session.

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

- [ ] Use a project-owned low-quality Volume profile/selection rather than editing the vendor profile for every preset. Current scene reference: `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Scene/FC_HDRP/Sky and Fog Settings Profile.asset`.
- [ ] For the low preset, start with 100–150 units of shadow distance and two cascades; validate the castle's scale and important vistas. Current profile is 5,000 units/four cascades. Do not rely on the legacy Quality `shadowDistance: 15` value to control HDRP.
- [ ] Lower/disable the Volume's high-quality AO and Bloom; disable volumetric fog in this profile when the low HDRP asset cannot support it.
- [ ] Review the 2048 cubemap `T_HDRSKY.HDR` separately: trial a smaller project-owned/import variant, verify sky appearance and reflections.
- [ ] Keep dynamic resolution optional. Current camera and HDRP asset disable it; an asset-only toggle is insufficient. Use smaller view dimensions first and measure actual render-target allocation.
- [ ] Compare live frame settings and Volume overrides with serialized settings. Ignore obsolete serialized fields and migration remnants when deciding active behavior.

HDRP allocates resources according to supported asset features; frame settings and Volumes further determine rendering. [HDRP asset](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.3/manual/HDRP-Asset.html)

Verify: same camera screenshots and GPU/RT counters at both presets, without missing required effects in the normal preset.

### Phase 4 — Reduce mesh and texture residency

- [ ] Begin with the measured largest meshes: `SM_CastleSideBridge_02`, `SM_chandelier`, `SM_Castle_Gate_01`, `SM_Tower_Bot`, `SM_Castle_Gate_02`, and `SM_candle_holder` under `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Meshes/`.
- [ ] Audit consumers before changing Read/Write: procedural mesh access, collider cooking, negative/nonuniform transforms, navigation baking, and static batching can constrain it. Change only eligible imports in small batches; Editor inspection may retain data that a Player does not.
- [ ] Measure topology and all imported LODs, not only LOD0. Reduce genuinely excessive geometry, author more effective lower LODs and simpler collider meshes, and check whether unused imported submeshes/LODs can be excluded. Do not use mesh compression as a promise of equivalent runtime memory reduction.
- [ ] Rank repeated mesh/material pairs by instance count and vertex cost. Preserve shared mesh/material identity; check for instantiated mesh/material copies.
- [ ] Apply importer mip streaming and resolution limits to eligible world textures. The targeted audit found zero streaming-enabled textures among 145 imports and 131 readable meshes among 133 imports; these are bounded dependency/importer counts, not a complete built-content inventory. Keep UI, data textures, normal-map formats, masks, alpha clipping, cubemaps, and non-mipped assets on explicitly reviewed rules.
- [ ] Check effective Standalone overrides and imported GPU formats. JPEG/PNG/source size or Crunch/bundle compression is not the runtime GPU footprint.

Verify: per-asset before/after memory, importer values, collision/navigation correctness, LOD silhouettes and visual texture quality. Avoid a project-wide reimport that recreates the peak.

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

Status remains **draft** until reviewed and explicitly selected for execution. Start with Phases 1–2; use `unity_profiler` for baseline comparisons and one `unity_operator` writer for settings/assets. Use `csharp_worker` only after the loader change is bounded, and `unity_architect` for the later streaming design.

Required context keys: `vault-usage`, `plan-workflow`, `issue-workflow`; resolve additional domain keys only when the implementation touches their systems. Future serialized mutations must be imported, specifically reserialized if edited on disk, saved through Unity, and checked for errors. Remaining decisions are target Player hardware/budget and acceptable authoring visual reductions.
