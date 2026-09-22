---
title: Render Settings Optimization Plan
type: plan
domains:
  - rendering
  - performance
  - camera
  - lighting
  - settings
status: done
authority: historical
updated: 2026-09-21
aliases:
  - render-settings-optimization
tags:
  - work/plan
  - status/done
  - rendering/optimization
---

# Render Settings Optimization Plan

## Plan Contract

### Goal
Apply the Priority Optimization Checklist derived from the Render Settings and Performance Audit to resolve the 41,581 draw call bottleneck in `DefaultLocation`, clamp the 5,000m shadow distance to 150m, enable Unity 6 GPU Resident Drawer and GPU occlusion, calibrate the Main Camera (TAA, dithering, stopNaNs), convert unbaked lights to functional Mixed lights, and activate texture streaming.

### Source Research and Decisions
- Audit findings documented in `SoulsLikeGameVault/Research/Render Settings and Performance Audit.md`.
- 90% of draw calls (37,437 of 41,581) were caused by `HDShadowSettings.maxShadowDistance = 5000m` across 4 cascades.
- 26,875 static meshes share `Shader Graphs/S_Masking` and are SRP Batcher compatible, making them ideal for Unity 6 GPU Resident Drawer (`InstancedDrawing`).
- 60 lights were marked `Baked` without any baked lightmaps in the project, producing zero light. Converting them to `Mixed` restored runtime lighting immediately.

### Assumptions and Non-Goals
- **Non-Goals:** Full offline lightmap or APV bake is deferred to a dedicated lighting bake pass; this plan focused on runtime engine settings, camera calibration, shadow budgets, batching, and streaming.
- **Assumptions:** The Main Camera's far clipping plane remains 500m; 150m shadow distance is sufficient for high-fidelity third-person gameplay.

### Success Criteria & Final Results
1. `shadowCasters` draw calls dropped from **37,437 to 13,128** (-64.9%).
2. Total frame draw calls dropped from **41,581 to 19,474** (-53.2%).
3. Total frame triangles dropped from **103.6M to 51.5M** (-50.2%).
4. Main Camera has `antialiasing = TemporalAntialiasing`, `dithering = true`, and `stopNaNs = true`.
5. Unity 6 GPU Resident Drawer is set to `InstancedDrawing` with GPU Occlusion and Small Mesh Culling enabled.
6. Texture Mipmap Streaming is enabled with a 1024MB budget and 64MB async upload buffer.

---

## Execution Plan

- [x] Phase 1 — Global Shadow Clamping & Volume Overrides:
  - Modified `Sky and Fog Global Volume` in `DefaultLocation.unity`: set `maxShadowDistance = 150m`, adjusted cascade splits to [0.067, 0.200, 0.467, 1.0].
  - Set `DirectionalLight.shadowFadeDistance = 150m`.
  - Added `Tonemapping` override (`ACES`).
- [x] Phase 2 — Punctual Light Optimization & Light Classification:
  - On the 28 shadow-casting point/spot lights in `DefaultLocation`, `Zone_03`, `Zone_05`, `Zone_07`: clamped `shadowFadeDistance` to `range * 1.2f`, set static lights to `OnEnable` (cached).
  - Switched the 60 `Baked` lights to `Mixed` so they emit light at runtime.
- [x] Phase 3 — Main Camera Calibration & Prefabrication:
  - In `DefaultLocation.unity`, configured Main Camera with TAA, dithering, and stopNaNs.
  - Created core prefab `Assets/Prefabs/View/Camera/CoreMainCamera.prefab`.
- [x] Phase 4 — Unity 6 GPU Resident Drawer & Batching:
  - In `HDRP High Fidelity.asset`, `Balanced.asset`, `Performant.asset`: enabled GRD (`InstancedDrawing`), enabled GPU occlusion culling (`enableOcclusionCullingInCameras = true`), set `smallMeshScreenPercentage = 0.5`.
  - In `ProjectSettings/GraphicsSettings.asset`: set `m_BrgStripping = Keep All`.
  - In `PlayerSettings`: disabled Standalone Static Batching.
- [x] Phase 5 — Texture Streaming & Async Upload Buffer:
  - In `QualitySettings`: enabled `streamingMipmapsActive = true`, set budget to 1024 MB, set `asyncUploadBufferSize = 64 MB`, `asyncUploadTimeSlice = 4 ms`.
- [x] Phase 6 — Verification & Profiling:
  - Captured live `UnityStats` query in running Editor.
  - Verified 0 console errors and 0 warnings.
  - Saved all assets and scenes cleanly (`AssetDatabase.SaveAssets()`, `EditorSceneManager.SaveScene()`).

---

## Execution Handoff
Completed on 2026-09-21. All 10 active scenes cleanly persisted to disk and verified.
