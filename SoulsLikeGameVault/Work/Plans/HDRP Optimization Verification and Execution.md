---
title: HDRP Optimization Verification and Execution
type: plan
domains:
  - rendering
  - performance
  - lighting
  - camera
status: in-progress
authority: advisory
updated: 2026-09-21
aliases: []
tags:
  - work/plan
  - rendering/optimization
---
# HDRP Optimization Verification and Execution

## Plan Contract

### Goal

Verify the render work recorded in [[Render Settings and Frame Optimization Record]], correct settings that conflict with the revised HDRP execution plan, and retain further optimization only when a comparable Player capture demonstrates benefit without visual regressions.

### Source Research and Decisions

- User-supplied `HDRP_Optimization_Execution_Plan.md` and `HDRP_Audit_Verification.md` are proposals and evidence reviews, not project policy.
- [[Render Settings Optimization Plan]] is complete historical work. Its recorded Editor counter deltas are preserved as observations, not Player frame-time proof.
- Unity's Shadow Casters Count counts objects/occurrences, not shadow draw commands. The historical 37,437 → 13,128 figure must not be treated as a count of draw calls.
- The connected Editor is Unity 6000.3.11f1 with all ten DefaultLocation scenes loaded and clean. Current quality is High Fidelity. No comparable Development Player profile is saved.

### Assumptions and Non-Goals

- FPS target remains unspecified; report it as unknown.
- Preserve HDRP DeferredOnly, CameraService/Cinemachine ownership, scene loading, Mixed lights, and unrelated working-tree edits.
- The separate [[Lighting Bake Plan]] is draft and does not authorize its 23-scene lightmap workflow. The APV pilot in this plan is a different bounded experiment.

### Success Criteria

1. A Development Player capture at 2560×1440 on the local target records frame-time distribution and pass costs for a repeatable scene route.
2. Moving characters, doors, and zone transitions cast current shadows, with no stale cache or atlas warning.
3. GRD and GPU occlusion are retained only after eligible batching and relevant frame-time gains are measured.
4. Camera presentation and streaming are accepted with visual evidence and cost/memory measurements.
5. A bounded APV and reflection pilot produces saved data and survives the ten-scene additive load before any production expansion.

## Execution Plan

- [ ] Phase 0 — Build and profile a Development Player; record fixed route, hardware/API, quality, median/p95/p99 frame times, per-pass cost, memory and captures. Two Development/Connect Profiler Windows builds succeeded with zero errors. The instrumented build loaded additive zone content on the RTX 5070/D3D12 machine, but its hidden-window run reported zero Rendering counters and GPU time, so no valid baseline or comparison was accepted.
- [x] Phase 1a — Restore per-frame updates on 28 newly cached punctual lights that had dynamic shadow drawing disabled and no static-shadow-caster renderers. Verify four saved scenes and clean Editor state.
- [ ] Phase 1b — Pilot correct mixed shadow caching only after classifying immutable casters, checking atlas capacity and additive-scene refresh.
- [ ] Phase 2 — Inspect Shader Graph DOTS variants, Frame Debugger Hybrid Batch Group events, and GRD/GPU-occlusion A/B. Trial zero small-mesh culling threshold during isolation.
- [ ] Phase 3 — Keep TAA/dithering; remove unsubstantiated Stop NaNs pass, then verify motion/ghosting and effective tonemapping. Stop NaNs was removed from the output-camera prefab and its live inherited value was checked. A camera-mask Volume stack query resolved ACES as the effective default tonemapper with no mode override; a Neutral trial has no demonstrated need. Motion and ghosting evidence remains open.
- [ ] Phase 4 — Check texture importer eligibility, actual streaming residency, async upload stalls, and transition hitches before expanding imports or buffer size.
- [ ] Phase 5 — In the existing ten-scene Baking Set, bake a bounded APV pilot and then one local baked Reflection Probe after GI acceptance.

## Execution Evidence and Remaining Gates

- The active quality uses the High Fidelity HDRP asset at 2560×1440 with VSync 1. The ten location scenes were restored clean after the Player attempt. Editor draw counts and historical Shadow Casters Count are not Player performance evidence.
- The High Fidelity asset already selects APV. The existing ten-scene Baking Set has no baked APV cells; its scene entries report `hasProbeVolume: 0`. The placement object in `DefaultLocation` is inactive, and current environment MeshRenderers receive lightmaps rather than Light Probes. A bounded pilot needs a selected room, receiver changes, an intended sky/bake setup, and sufficient disk/memory headroom before running a bake. The separate draft lightmap workflow is excluded.
- GPU Resident Drawer, GPU occlusion, `0.5` small-mesh threshold, and the 1,024 MB streaming budget were already enabled by the earlier work. Live inventory found 26,930 MeshRenderers, 1,813 unique mesh/material pairs, no LightProbe ProxyVolume renderers and no MaterialPropertyBlocks. Shader Graph source alone does not establish DOTS incompatibility; Hybrid Batch Group and Player A/B evidence remains required. The streaming importer inventory found 64 enabled and 101 disabled castle HDRP texture metas, without residency or upload-stall measurements. Live High Fidelity async settings are 64 MB and **4 ms**, correcting the attached plan's assumption that the current slice was 2 ms.
- The normal build is `Builds/Profiling/SoulsLikeTemplate.exe`; the instrumented Player is `Builds/Profiling/HdrpBaseline.exe`. The temporary launch route used for unattended loading was removed from source after building. The hidden Player capture was discarded as unusable; its log is `Builds/Profiling/Captures/defaultlocation-player.log`.

## Risks and Rollback

The prior settings are committed in `d08d74c5`. Revert only this plan's own scene, prefab, pipeline, quality, or generated bake changes if a trial fails. Do not invoke `LocationBakeTool.BakeDefaultLocationMultiSceneLighting`; it disables APV and clears lighting. Do not overwrite unrelated working-tree edits.

## Validation

Scene and prefab mutations must be saved/imported through Unity and checked for Console errors. Player measurements require a fixed camera/route and frame-time evidence; Editor `UnityStats` alone cannot satisfy a performance gate. APV and reflection acceptance requires screenshots plus additive load/unload checks.

## Execution Handoff

Current branch: `codex/hdrp-optimization-followup`. Exact target assets include `DefaultLocation.unity`, `Zone_03.unity`, `Zone_05.unity`, `Zone_07.unity`, `CoreMainCamera.prefab`, High Fidelity HDRP settings, the existing DefaultLocation Baking Set, and selected room assets. Required vault context is `plan-workflow` and `work-routing`; no registered render-specific policy key exists.
