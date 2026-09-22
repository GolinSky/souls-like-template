---
title: HDRP Shadow and Camera Verification Correction
type: implementation-record
domains:
  - rendering
  - performance
  - lighting
  - camera
status: done
authority: historical
updated: 2026-09-21
aliases: []
tags:
  - history/change
  - rendering/optimization
---
# HDRP Shadow and Camera Verification Correction

## Implementation Record Contract

### Outcome

Restored per-frame shadow updates on the 28 punctual lights changed to OnEnable caching by `d08d74c5`. Disabled the extra Stop NaNs pass in the shared camera prefab while retaining Temporal Anti-Aliasing and dithering.

### Why

All 28 cached lights had `alwaysDrawDynamicShadows=false`, and live inspection found zero renderers marked Static Shadow Caster across the ten loaded location scenes. The prior caching setup lacked the mixed static/dynamic shadow classification described by HDRP and could leave moving actors or changed geometry with stale shadows. No non-finite-pixel artifact evidence justified the Stop NaNs pass. The later audit also identified that Unity's Shadow Casters Count had been mistaken for draw commands.

### Changed Files and Assets

- `Assets/Scenes/DefaultLocation/DefaultLocation.unity`: 17 punctual lights to EveryFrame. Four linked light frustum-caster-culling values were restored to their previous values after review.
- `Assets/Scenes/DefaultLocation/Zone_03.unity`: 2 lights to EveryFrame.
- `Assets/Scenes/DefaultLocation/Zone_05.unity`: 6 lights to EveryFrame.
- `Assets/Scenes/DefaultLocation/Zone_07.unity`: 3 lights to EveryFrame.
- `Assets/Prefabs/View/Camera/CoreMainCamera.prefab`: `HDAdditionalCameraData.stopNaNs=false`.
- Three HDRP pipeline assets: retained build-computed `m_PrefilterUseLegacyLightmaps=1`, which HDRP 17.3 writes when GPU Resident Drawer is enabled to preserve its lightmap shader variant.
- `Assets/Settings/HDRPDefaultResources/HDRenderPipelineGlobalSettings.asset`: retained the GPU Resident Drawer runtime-resource registration needed by its renderer.
- [[Render Settings and Frame Optimization Record]]: appended a verification addendum without changing its historical completion status.
- [[HDRP Optimization Verification and Execution]]: active measurement and remaining-trial plan.

### Decisions and Tradeoffs

Per-frame punctual shadows can cost more than a correctly classified mixed cache. It is the safe current behavior until a small cached-light pilot shows correct dynamic casters, atlas capacity, and Player frame-time benefit. The existing 150 m directional distance, TAA, dithering, GPU Resident Drawer, GPU occlusion, streaming budget, and Mixed light modes remain unchanged pending separate evidence.

### Validation Evidence

- Unity Editor saved all four scenes and the prefab through its API. Focused scene diff contains exactly 28 update-mode values; prefab diff contains one Stop NaNs value.
- The live Main Camera inherits Stop NaNs disabled from the prefab with no override; TAA and dithering are unchanged.
- All ten loaded scenes were clean after mutation; Editor was stopped and not compiling; no new Console errors.
- Two Development/Connect Profiler Windows builds succeeded with zero errors. The instrumented build loaded additive zone content on D3D12/RTX 5070, but a hidden-window run reported zero Rendering counters and GPU time; its capture was rejected. No valid Player frame-time, gameplay-shadow, or motion-artifact comparison is available. No Play Mode tests were run.
- The benchmark build's generated Addressables `link.xml` was deleted and its tracked `addressables_content_state.bin` restored to its previous content. That temporary Player must not become a content-update baseline.

### Documentation Updated

[[Render Settings and Frame Optimization Record]] and [[HDRP Optimization Verification and Execution]].

### Follow-Up

Capture a fixed Development Player route. Pilot mixed shadow caching only on selected stationary lights after tagging immutable environment renderers Static Shadow Caster, enabling Always draw dynamic, and validating additive-scene refresh. Compare TAA motion, GRD, GPU occlusion, and streaming costs separately.
