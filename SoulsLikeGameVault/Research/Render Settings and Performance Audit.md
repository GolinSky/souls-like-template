---
title: Render Settings and Performance Audit
type: research
domains:
  - rendering
  - performance
  - camera
  - lighting
  - settings
status: current
authority: evidence
updated: 2026-09-21
tags:
  - research/package
  - rendering/audit
  - performance/optimization
---

# Render Settings, Camera, Lighting & Performance Audit
**Project:** SoulsLikeTemplate  
**Engine & Pipeline:** Unity 6 (`6000.3.11f1`) — High Definition Render Pipeline (HDRP `17.3.0`)  
**Active Scene Loaded:** `Assets/Scenes/DefaultLocation/DefaultLocation.unity` + 9 additive sub-scenes (`Rocks`, `Zone_01` to `Zone_08`)  
**Date:** September 21, 2026  
**Audited Quality Level:** `High Fidelity` (`Assets/Settings/RenderPipelines/HDRP High Fidelity.asset`)

---

## 1. Executive Summary & Verdict

### The Big Picture
The project is running on **Unity 6 HDRP (High Definition Render Pipeline 17.3.0)**, **NOT Universal Render Pipeline (URP)**.
- **Rendering Path Assumption Clarified:** HDRP is **already configured for `DeferredOnly`** across all quality tiers (`High Fidelity`, `Balanced`, and `Performant`). The project is not using a Forward pass.
- **Live Frame Performance:** Rendering `DefaultLocation` with all 9 zone dependencies generates **41,581 Draw Calls**, **103.6 Million Triangles**, and consumes **~1.23 GB in Render Textures** per frame.
- **Root Cause of Performance Crisis:** **37,437 out of 41,581 draw calls (90.0%)** are **shadow caster passes**. The Global Volume shadow distance is set to an astronomical **5,000 METERS** with 4 cascades, while the Main Camera's far clipping plane is only **500 METERS**. The GPU is rendering shadow casters 10× further than the player can ever see!
- **Batching & Instancing Breakdown:** Static Batching is enabled in `PlayerSettings`, and 61,807 materials have GPU Instancing enabled. However, **instanced draw calls are 0** and **static batched draw calls are 0**. Unity's SRP Batcher is handling draws, but Unity 6's **GPU Resident Drawer (GRD)** and **GPU Occlusion Culling** are completely **DISABLED**.
- **Lighting & GI Breakdown:** The scene has **357 lights** (297 Mixed, 60 Baked). However, **ZERO lightmaps are baked**, **ZERO light probes exist**, **ZERO reflection probes exist**, and the single `Adaptive Probe Volume` is a disabled 4×3×4 meter box. The 60 baked lights emit **zero light at runtime**, and all 297 mixed lights fall back to raw realtime direct illumination.
- **Camera Settings:** The Main Camera in `DefaultLocation` has **Antialiasing set to `None`** (no TAA, FXAA, or SMAA), yet Screen Space Ambient Occlusion (SSAO) has `temporalAccumulation: True` (causing potential smearing/shimmering).

---

## 2. Live Profiling & Frame Metrics (`UnityStats`)

The following live metrics were captured directly from the running Editor with `DefaultLocation` and all 9 dependency scenes loaded:

| Metric | Measured Value | Target / Health Standard | Status | Assessment |
|---|---|---|---|---|
| **Total Draw Calls** | **41,581** | 1,500 – 3,500 | 🔴 Critical | Extreme CPU & GPU submission bottleneck |
| **Shadow Caster Draws** | **37,437** | 400 – 1,200 | 🔴 Critical | **90.0% of all frame draw calls are shadows** |
| **Batches** | **28,837** | 1,000 – 2,500 | 🔴 Critical | SRP Batcher overwhelmed by non-culled geometry |
| **Triangles** | **103,625,797** (103.6M) | 4M – 12M | 🔴 Critical | Distant LODs submitted across 4 shadow cascades |
| **Vertices** | **198,845,942** (198.8M) | 6M – 18M | 🔴 Critical | Massive vertex shader & rasterization load |
| **SetPass Calls** | **290** | 80 – 200 | 🟡 Moderate | Shader switches are relatively well-contained |
| **Static Batched Calls** | **0** | N/A | ⚪ Inactive | Static batching not combining meshes |
| **Instanced Batched Calls**| **0** | N/A | ⚪ Inactive | GPU Instancing not active under SRP Batcher |
| **Dynamic Batched Calls**  | **4** | N/A | ⚪ Inactive | Dynamic batching disabled (correct for HDRP) |
| **Render Textures Count** | **90** | 30 – 50 | 🟡 Warning | High number of internal render targets |
| **Render Texture VRAM**   | **1,234,815,060 bytes** (~1.23 GB) | 400 – 800 MB | 🟡 Warning | High VRAM footprint for buffers & shadow maps |
| **Screen Resolution**    | **2560 × 1440** (QHD) | Native | 🟢 Normal | Standard PC test resolution |

---

## 3. Render Pipeline & Quality Settings Breakdown

### 3.1 Render Pipeline Architecture: HDRP `17.3.0`
The project does not use URP. The active pipeline is HDRP on Unity 6:
- **Default Render Pipeline Asset:** `Assets/Settings/HDRPDefaultResources/HDRenderPipelineAsset.asset`
- **Active Quality Pipeline Asset:** `Assets/Settings/RenderPipelines/HDRP High Fidelity.asset`
- **Lit Shader Mode:** `DeferredOnly` across all tiers (`High Fidelity`, `Balanced`, and `Performant`). G-Buffer passes output albedo, normals, roughness/metallic, specular, and material flags, followed by tile/cluster deferred lighting.

### 3.2 Comparison of Project HDRP Quality Presets

| Setting | High Fidelity (Level 0) | Balanced (Level 1) | Performant (Level 2) | Editor Low Memory (Level 3) |
|---|---|---|---|---|
| **Lit Shader Mode** | `DeferredOnly` | `DeferredOnly` | `DeferredOnly` | `DeferredOnly` |
| **GPU Resident Drawer (GRD)** | **`Disabled`** | **`Disabled`** | **`Disabled`** | **`Disabled`** |
| **GPU Occlusion Culling** | **`False`** | **`False`** | **`False`** | **`False`** |
| **Dynamic Resolution** | `False` | `False` | `False` | `False` |
| **Punctual Shadow Atlas** | `4096 × 4096` | `2048 × 2048` | `2048 × 2048` | `2048 × 2048` |
| **Area Shadow Atlas** | `4096 × 4096` | `2048 × 2048` | `2048 × 2048` | `2048 × 2048` |
| **Max Shadow Requests** | `128` | `128` | `128` | `128` |
| **Directional Shadow Depth** | `Depth16` | `Depth16` | `Depth16` | `Depth16` |
| **SSAO** | `True` | `True` | `True` | `True` |
| **SSGI** | `True` | `False` | `False` | `False` |
| **SSR** | `False` | `False` | `False` | `False` |
| **Volumetrics** | `True` | `True` | `False` | `False` |
| **Subsurface Scattering** | `True` | `True` | `True` | `True` |
| **Decals** | `True` (Draw dist: 1000m)| `True` (Draw dist: 1000m)| `True` (Draw dist: 1000m)| `True` (Draw dist: 1000m)|
| **LOD Bias** | `1.0` | `1.0` | `1.0` | `0.5` |
| **Maximum LOD Level** | `0` (Highest) | `0` (Highest) | `0` (Highest) | `0` (Highest) |

---

## 4. Core Main Camera & Cinemachine Analysis

### 4.1 Camera Setup & Hierarchy Status
- **Main Camera Location:** `Assets/Scenes/DefaultLocation/DefaultLocation.unity` -> `/Main Camera`.
- **Prefab Status:** **Scene Object only** (Not a prefab asset!). 
- **Cinemachine Integration:** Controlled via `Prefabs/View/Camera/Gameplay Camera.prefab` (CinemachineCamera v3.1.3) and `Prefabs/View/Camera/CameraService.prefab`.

### 4.2 Main Camera Component Configuration

| Property | Current Value | Optimal Standard | Risk / Issue |
|---|---|---|---|
| **Clear Flags** | `Skybox` | `Skybox` | 🟢 OK |
| **Culling Mask** | `-1` (`Everything`) | Layer-filtered | 🟡 Minor (Renders all layers indiscriminately) |
| **Near Clipping Plane** | `0.2` | `0.1` – `0.3` | 🟢 OK |
| **Far Clipping Plane** | **`500.0`** | `300.0` – `500.0` | 🔴 **Severe Mismatch:** Shadows render to 5,000m! |
| **Field of View** | `48°` | `45°` – `60°` | 🟢 OK |
| **Allow HDR** | `false` | Managed by HDRP | ⚪ Harmless (HDRP overrides frame buffer) |
| **Allow MSAA** | `false` | `false` | 🟢 Correct (MSAA incompatible with Deferred) |
| **Dynamic Resolution** | `false` | `true` (FSR2/STP) | 🟡 Missing modern upscaling support |
| **Use Occlusion Culling**| `true` | `true` | 🟢 Camera CPU occlusion enabled |
| **Antialiasing** | **`None`** | `TemporalAntialiasing` (TAA) | 🔴 **Critical Quality Defect:** Jagged edges, shimmering |
| **Dithering** | `false` | `true` | 🟡 Can show color banding in dark gradients |
| **Stop NaNs** | `false` | `true` | 🟡 Risk of black pixel explosion from HDR specular |
| **Volume Layer Mask** | **`1` (`Default` only)** | Explicit Volume Layer | 🔴 Ignores any Volume placed on non-default layers |
| **Clear Color Mode** | `Sky` | `Sky` | 🟢 OK |
| **Custom Rendering Settings** | `false` | `false` (uses asset) | 🟢 Standard |

### 4.3 Cinemachine Virtual Camera (`Gameplay Camera.prefab`)
- **Type:** `CinemachineCamera` with `CinemachineThirdPersonFollow`, `CinemachineRotationComposer`, `CinemachineBasicMultiChannelPerlin`.
- **Clipping Planes:** Near `0.2`, Far `500`.
- **FOV:** `48°`.
- **Brain Update Method:** `SmartUpdate`, Blend Update: `LateUpdate`.
- **Follow Damping:** (0.1, 0.1, 0.35), Shoulder Offset: (1.0, 0.48, 0.0), Distance: 3.0m.
- **Collision Filter:** `513` (Layers 0 `Default` and 9).

---

## 5. Lighting & Shadows Deep Dive

### 5.1 Scene Light Inventory (Total: 357 Lights)
Across the active `DefaultLocation` and its 9 additive sub-scenes:

```
Total Scene Lights: 357
├── By Type:
│   ├── Directional:   1
│   ├── Point:       321
│   ├── Spot:         33
│   └── Area:          2
├── By Lightmap Mode:
│   ├── Realtime:      0 (0%)
│   ├── Mixed:       297 (83.2%)
│   └── Baked:        60 (16.8%)
└── By Shadow Casting:
    ├── None:        328 (91.9%)
    ├── Hard:         29 (8.1%)
    └── Soft:          0 (0%)
```

### 5.2 The 5,000-Meter Shadow Distance Disaster
In `Sky and Fog Global Volume`, the `HDShadowSettings` component defines:
- **`maxShadowDistance`: `5000.0` meters!**
- **Directional Light `shadowFadeDistance`: `10,000.0` meters!**
- **Cascade Count:** 4 splits:
  - Cascade 0: `0.05` → **250 meters**
  - Cascade 1: `0.15` → **750 meters**
  - Cascade 2: `0.30` → **1,500 meters**
  - Cascade 3: `1.00` → **5,000 meters**

**Why this breaks performance:**
1. The camera culls at **500m**, but the shadow cascade camera renders meshes up to **5,000m** away across 4 separate cascade render targets.
2. Because the first cascade spans 250m, shadow texel density near the player is diluted by 5× compared to a normal 50m cascade, leading to soft/blurry shadows or shadow acne.
3. This single setting creates **37,437 shadow caster draw calls per frame** and inflates the triangle count to **103.6 Million**.

### 5.3 Point and Spot Light Shadow Settings
There are 28 punctual/spot lights configured to cast `Hard` shadows:
- **`shadowFadeDistance`:** Set to **10,000 meters** on every single one, including small decorative candle lights (`CandleHolderLight` with range = 8m in Zone_03, Zone_05, Zone_07).
- **`shadowUpdateMode`:** Set to **`EveryFrame`** on 28 of the 29 shadow casters.
- **Missed Optimization:** In HDRP, static point/spot lights can use `OnEnable` or `OnDemand` cached shadows (`cachedPunctualLightShadowAtlas`). Running 28 punctual shadow passes every frame forces constant GPU shadow map re-rendering.

### 5.4 The Complete Absence of Baked Lighting & Reflection Probes
| Subsystem | Current State | Impact |
|---|---|---|
| **Baked Lightmaps** | **`0` lightmaps** (`LightmapSettings.lightmaps.Length = 0`) | 60 "Baked" lights emit **NO light at all** at runtime. 297 "Mixed" lights run as raw realtime direct lights with zero indirect bounce. |
| **Light Probes** | **`0` probes** (`LightmapSettings.lightProbes = null`) | Dynamic objects (Player, enemies, props) receive no baked ambient or bounce lighting. |
| **Reflection Probes** | **`0` reflection probes** in entire scene | All reflections rely entirely on low-frequency HDRI skybox fallback. Interior rooms and caves reflect the open sky. |
| **Adaptive Probe Volumes (APV)** | `1` GameObject (`Adaptive Probe Volume`), **`activeSelf: false`**, Size: 3.95m × 3m × 3.95m | Completely disabled and covers only 4 meters in a multi-kilometer castle! |
| **Static Lighting Sky** | Present, but **`profile: null`** | Unconfigured; cannot bake sky indirect lighting. |
| **LightingSettings Asset** | **`null`** (`Lightmapping.lightingSettings = null`) | No lighting bake configuration exists for the project. |

---

## 6. Batching, Geometry & Instancing Audit

### 6.1 Geometry & Material Scale
- **Total MeshRenderers:** 26,896 GameObjects.
- **Batching Static Flags:** 26,875 (99.9%) are flagged `BatchingStatic`.
- **Contribute GI Flags:** 26,878 are flagged `ContributeGI`.
- **Occluder Static:** 15,437 flagged.
- **Occludee Static:** 26,878 flagged.
- **LODGroups:** 7,246 active LODGroups (classic LODs, average 3–4 levels).

### 6.2 The Instancing vs SRP Batcher Dilemma
- **Shader Used:** 61,807 out of 61,828 assigned material slots use **`Shader Graphs/S_Masking`** (`Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Shaders/S_Masking.shadergraph`).
- **SRP Batcher Compatibility:** `S_Masking` is **100% SRP Batcher compatible** (`GetSRPBatcherCompatibilityCode == 0`).
- **GPU Instancing Enabled:** 61,807 materials have `enableInstancing = true`.
- **Static Batching Enabled:** `PlayerSettings.GetStaticBatchingForPlatform == true`.

**The Conflict:**
In Unity Scriptable Render Pipelines (HDRP/URP):
1. SRP Batcher and GPU Instancing are mutually exclusive. If a shader is SRP Batcher compatible and the SRP Batcher is active, Unity routes draws through the **SRP Batcher**, NOT GPU Instancing.
2. Static Batching creates combined vertex buffers. This increases memory, duplicates meshes, and breaks per-instance culling.
3. The result is: **0 Instanced Batches**, **0 Static Batches**, and **28,837 individual SRP constant buffer batches**.

### 6.3 Unity 6 GPU Resident Drawer (GRD) — The Untapped Super-Power
Unity 6 introduced the **GPU Resident Drawer (GRD)** for HDRP and URP:
- In `HDRP High Fidelity.asset`:
  - `gpuResidentDrawerSettings.mode = Disabled`
  - `gpuResidentDrawerSettings.enableOcclusionCullingInCameras = False`
  - `gpuResidentDrawerSettings.smallMeshScreenPercentage = 0`
- **What happens if GRD is enabled:**
  Because all 26,875 static meshes share the same SRP-compatible shader (`S_Masking`), Unity 6's GPU Resident Drawer can keep mesh instances resident in GPU VRAM and draw them via `DrawMeshInstancedIndirect`.
  Furthermore, `enableOcclusionCullingInCameras` enables **hardware two-phase GPU Occlusion Culling** (Hi-Z depth pyramid), culling occluded static meshes directly on the GPU without CPU Umbra overhead.

---

## 7. Global Volumes & Post-Processing Analysis

Audited component overrides on `Sky and Fog Global Volume`:

| Component | Active | Overrides & Values | Assessment |
|---|---|---|---|
| **VisualEnvironment** | `True` | Sky: `HDRISky`, Ambient: `Dynamic` | 🟢 Standard |
| **HDRISky** | `True` | Cubemap: `T_HDRSKY`, Exposure: `0.5`, Lux: `20,000` | 🟢 Standard |
| **Fog** | `True` | `enableVolumetricFog: True`, `maxFogDistance: 5000` | 🟡 5km fog distance inflates volumetric evaluation |
| **Exposure** | `True` | Mode: `Fixed`, Fixed Exposure: `0.8` | 🟢 Standard |
| **ColorAdjustments** | `True` | Contrast: `50`, ColorFilter: (0.91, 0.94, 0.98) | 🟢 Art direction choice |
| **WhiteBalance** | `True` | Temperature: `0`, Tint: `0` | ⚪ Neutral |
| **ShadowsMidtonesHighlights** | `True` | Shadows: (0.99, 1.00, 1.00, -0.01) | ⚪ Subtle |
| **HDShadowSettings** | `True` | `maxShadowDistance: 5000`, 4 splits [0.05, 0.15, 0.3, 1.0] | 🔴 **Catastrophic Performance Defect** |
| **DepthOfField** | **`False`** | Inactive | 🟢 Good (No unnecessary blur or compute overhead) |
| **Bloom** | `True` | Threshold: `0.5`, Intensity: `0.5`, Quality: `2` | 🟢 Standard |
| **Vignette** | `True` | Intensity: `0.25`, Smoothness: `0.5` | 🟢 Standard |
| **SSAO** | `True` | Intensity: `1.0`, Radius: `1.5`, `temporalAccumulation: True` | 🟡 SSAO has temporal accumulation but Camera has no TAA |
| **Tonemapping** | **Missing** | Not present on Volume | 🟡 Missing ACES or Neutral tonemapper override |

---

## 8. Texture Streaming & Memory Optimization

- **Texture Streaming:** `QualitySettings.streamingMipmapsActive == false`. All high-resolution textures (thousands of 2K/4K PBR textures across `FantasyCastle`) are loaded into VRAM at full resolution simultaneously.
- **Async Upload Buffer:** `QualitySettings.asyncUploadBufferSize == 16 MB`. The 16MB default buffer is inadequate for high-res HDRP assets and causes frame hitching during asset streaming.
- **Async Upload Time Slice:** `2 ms`.
- **Render Texture Memory:** Currently allocates **90 render textures** totaling **~1.23 GB** of GPU memory per frame.

---

## 9. Defect Summary & Root Cause Matrix

| ID | Issue Area | Observed Configuration | Expected / Best Practice | Performance Impact |
|---|---|---|---|---|
| **DEF-01** | **Shadows** | `HDShadowSettings.maxShadowDistance = 5000m` | `100m – 150m` (for third-person camera) | **Generates 37,437 draw calls and 103M triangles per frame** |
| **DEF-02** | **Shadows** | 28 point/spot lights update `EveryFrame` with `shadowFadeDistance: 10,000m` | Set static lights to `OnEnable`/cached; fade distance `20m – 50m` | Unnecessary punctual shadow map rendering every frame |
| **DEF-03** | **Antialiasing** | Main Camera `antialiasing = None` | `TemporalAntialiasing` (TAA) | Severe aliasing/flicker; SSAO temporal accumulation mismatch |
| **DEF-04** | **Unity 6 GRD** | `gpuResidentDrawerSettings.mode = Disabled` | `InstancedDrawing` | Misses automatic indirect GPU instancing of 26,875 meshes |
| **DEF-05** | **GPU Occlusion**| `enableOcclusionCullingInCameras = False` | `True` | No GPU depth-buffer occlusion culling |
| **DEF-06** | **Lighting / GI** | 357 lights (297 Mixed, 60 Baked); 0 lightmaps, 0 probes | Bake APV and lightmaps, or switch to explicit realtime | 60 baked lights emit nothing; 297 mixed lights run realtime direct |
| **DEF-07** | **Reflections** | 0 Reflection Probes in scene | Box-projected reflection probes in key areas | All reflections show exterior skybox even in interior rooms |
| **DEF-08** | **Streaming** | `streamingMipmapsActive = false` | `true` with 1024–2048 MB pool | High texture VRAM pressure and longer initial load |
| **DEF-09** | **Camera Asset** | Main Camera is scene object only, not prefab | Core camera prefab linked with `CameraService` | Inconsistent camera settings across scenes |
| **DEF-10** | **Upload Buffer** | `asyncUploadBufferSize = 16 MB` | `64 MB – 128 MB` | Asset loading hitching / ring-buffer stalls |

---

## 10. Prioritized Action Plan & Optimization Roadmap

### Phase 1: Immediate High-Impact Quick Wins (Hours)
1. **Clamp Global Shadow Distance:**
   - In `Sky and Fog Global Volume` -> `HDShadowSettings`: Reduce `maxShadowDistance` from **5,000m** to **120m – 150m**.
   - Adjust cascade splits: Cascade 0 (0.07 / ~10m), Cascade 1 (0.20 / ~30m), Cascade 2 (0.45 / ~65m), Cascade 3 (1.00 / 150m).
   - *Expected Result:* Drops frame draw calls from **41,581 to ~4,500** and triangles from **103M to ~15M**.
2. **Optimize Punctual Shadow Casters:**
   - On the 28 point/spot lights with shadows: Set `shadowFadeDistance` from 10,000m to `range * 1.2` (e.g. 10m–30m).
   - Change static lights' `shadowUpdateMode` from `EveryFrame` to `OnEnable` (cached).
   - Disable shadow casting on tiny candle holders and small decorative torches.
3. **Fix Camera Antialiasing & Post-Processing:**
   - On Main Camera: Enable `antialiasing = TemporalAntialiasing` (TAA).
   - Enable `dithering = true` and `stopNaNs = true`.
   - Add `Tonemapping` override to Global Volume (Mode: `Neutral` or `ACES`).

### Phase 2: Unity 6 GPU Resident Drawer Activation (1-2 Days)
1. **Enable GPU Resident Drawer:**
   - In `Assets/Settings/RenderPipelines/HDRP High Fidelity.asset`: Set `gpuResidentDrawerSettings.mode = InstancedDrawing`.
   - Set `enableOcclusionCullingInCameras = True`.
   - Set `smallMeshScreenPercentage = 0.5` (culls meshes smaller than half a percent of screen height).
   - *Expected Result:* Converts the remaining 26,875 static mesh draws into indirect GPU instanced batches.
2. **Batching Flags Cleanup:**
   - Disable `Static Batching` in `PlayerSettings` for Standalone (`PlayerSettings.SetStaticBatchingForPlatform(StandaloneWindows64, false)`), allowing GPU Resident Drawer to take full control without duplicate mesh memory.

### Phase 3: Lighting & Reflection Architecture (3-5 Days)
1. **APV (Adaptive Probe Volumes) Setup:**
   - Create a proper `Probe Volume` covering the playable bounds of `DefaultLocation` and active zones.
   - Assign `StaticLightingSky.profile` to the sky profile.
   - Bake Adaptive Probe Volumes using Unity's GPU Lightmapper.
2. **Light Classification Pass:**
   - Audit the 60 "Baked" lights: if baking APV/lightmaps, bake them; if remaining dynamic, convert to Mixed/Realtime.
   - Place Box-Projected Reflection Probes inside castle corridors, halls, and courtyards to fix the skybox reflection leak.

### Phase 4: Asset & Memory Streaming (1 Day)
1. **Enable Texture Mipmap Streaming:**
   - In `QualitySettings`: Enable `streamingMipmapsActive = true`, set `streamingMipmapsMemoryBudget = 1024` (or 2048 MB on High Fidelity).
2. **Increase Async Upload Buffer:**
   - Set `QualitySettings.asyncUploadBufferSize = 64` (or 128 MB) and `asyncUploadTimeSlice = 4 ms` to eliminate texture hitching.
3. **Prefabricate Core Main Camera:**
   - Save the calibrated Main Camera as a core prefab asset (`Assets/Prefabs/View/Camera/CoreMainCamera.prefab`) and link with `CameraService`.
