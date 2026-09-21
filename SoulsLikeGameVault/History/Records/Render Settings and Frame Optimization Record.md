---
title: Render Settings and Frame Optimization Record
type: implementation-record
domains:
  - rendering
  - performance
  - lighting
  - camera
  - scenes
status: done
authority: historical
updated: 2026-09-21
aliases:
  - render-settings-and-frame-optimization-record
tags:
  - history/change
  - rendering/optimization
  - performance
---

# Render Settings and Frame Optimization Record

## Implementation Record Contract

### Outcome

Successfully resolved the critical frame rendering bottleneck in `DefaultLocation` and standardized HDRP project render settings:
- **Draw calls** decreased by **53.2%** (from 41,581 to 19,474, saving 22,107 calls per frame).
- **Shadow caster passes** decreased by **64.9%** (from 37,437 to 13,128, eliminating 24,309 shadow calls).
- **Triangle throughput** decreased by **50.2%** (from 103.6M to 51.5M triangles per frame).
- **Vertices throughput** decreased by **46.4%** (from 198.8M to 106.5M vertices per frame).
- **SetPass calls** dropped by **70.3%** (from 290 to 86).
- Standardized the Main Camera with Temporal Anti-Aliasing (TAA), dithering, and stopNaNs, and prefabricated `CoreMainCamera.prefab`.
- Configured Unity 6 GPU Resident Drawer (`InstancedDrawing`), GPU Occlusion Culling, and Small Mesh Culling across HDRP pipeline assets.
- Converted 60 inactive `Baked` lights without lightmaps to functional `Mixed` lights.
- Activated Texture Mipmap Streaming with a 1024 MB pool and a 64 MB asynchronous upload buffer.

### Why

An initial audit of `DefaultLocation.unity` and its 9 sub-scenes (`Rocks`, `Zone_01` through `Zone_08`) revealed severe rendering bottlenecks:
1. `HDShadowSettings.maxShadowDistance` was set to an extreme **5,000 meters**, forcing 37,437 shadow caster draw calls per frame across 4 cascades even for micro-props miles away.
2. 60 lights were marked `Baked` in a project with no baked lightmaps or APV, casting zero light and leaving dungeons pitch black.
3. 28 point and spot lights had uncached real-time shadows updating every frame with oversized fade radii.
4. The Main Camera had no anti-aliasing (`None`), dithering disabled, and NaN protection disabled, producing noticeable aliasing on high-contrast edges and volumetric fog artifacts.
5. Unity 6 GPU Resident Drawer was disabled while Static Batching was enabled, preventing modern BRG instancing of the ~26,875 modular environment meshes.
6. Texture Streaming was disabled, holding all high-resolution textures in VRAM.

### Live Rendering Table Metrics

Captured live from the connected Unity Editor 6000.3.11f1 instance via `UnityEditor.UnityStats` at the exact same camera transform overlooking the `DefaultLocation` castle environment with all 10 scenes active:

| Metric | Before Optimization | After Optimization | Absolute Change | Relative Change |
| :--- | :---: | :---: | :---: | :---: |
| **Draw Calls (Total)** | 41,581 | **19,474** | -22,107 | **-53.2%** |
| **Shadow Casters** | 37,437 | **13,128** | -24,309 | **-64.9%** |
| **Batches** | 28,837 | **16,645** | -12,192 | **-42.3%** |
| **Triangles** | 103,625,797 (103.6M) | **51,558,329 (51.5M)** | -52,067,468 | **-50.2%** |
| **Vertices** | 198,845,942 (198.8M) | **106,552,001 (106.5M)** | -92,293,941 | **-46.4%** |
| **SetPass Calls** | 290 | **86** | -204 | **-70.3%** |
| **Visible Lights** | 44 | **44** | 0 | 0.0% |
| **Shadow Distance** | 5,000 m | **150 m** | -4,850 m | **-97.0%** |
| **Shadow Update Mode** | `EveryFrame` (all) | `OnEnable` (static cached) | — | Optimized |
| **Main Camera AA** | `None` | `TemporalAntialiasing` | — | High Quality |
| **GPU Resident Drawer** | `Disabled` | `InstancedDrawing` | — | Enabled |
| **GPU Occlusion Culling** | `Disabled` | `Enabled` | — | Enabled |
| **Texture Streaming** | `Disabled` | `Enabled` (1024 MB pool) | — | Enabled |
| **Editor Console Errors**| 0 | 0 | 0 | Clean |

### Changed Files and Assets

#### Scenes and Environment Assets
- `Assets/Scenes/DefaultLocation/DefaultLocation.unity`:
  - Main Camera: `antialiasing = TemporalAntialiasing`, `dithering = true`, `stopNaNs = true`.
  - Directional Light: `shadowFadeDistance = 150m`.
  - Global Volume (`Sky and Fog Global Volume`): `HDShadowSettings.maxShadowDistance = 150m`, splits `[0.067, 0.200, 0.467, 1.0]`, added ACES `Tonemapping` override.
- `Assets/Scenes/DefaultLocation/Zone_03.unity`:
  - 8 point/spot lights clamped shadow distances and static cache modes.
- `Assets/Scenes/DefaultLocation/Zone_05.unity`:
  - 8 point/spot lights clamped shadow distances and static cache modes.
- `Assets/Scenes/DefaultLocation/Zone_07.unity`:
  - 12 point/spot lights clamped shadow distances and static cache modes.
- `Assets/Settings/RenderPipelines/DefaultLocation Volume Profile.asset`:
  - Added ACES tonemapping and clamped shadow settings.

#### Prefabs
- `Assets/Prefabs/View/Camera/CoreMainCamera.prefab` (+ `.meta`):
  - Created standardized core camera prefab containing `Camera`, `HDAdditionalCameraData`, `AudioListener`, and standard layer culling masks.

#### Pipeline and Project Settings
- `Assets/Settings/RenderPipelines/HDRP High Fidelity.asset`:
  - `gpuResidentDrawerSettings.mode = InstancedDrawing`, `enableOcclusionCullingInCameras = true`, `smallMeshScreenPercentage = 0.5`.
- `Assets/Settings/RenderPipelines/HDRP Balanced.asset`:
  - Synchronized GPU Resident Drawer, Occlusion Culling, and Small Mesh Culling.
- `Assets/Settings/RenderPipelines/HDRP Performant.asset`:
  - Synchronized GPU Resident Drawer, Occlusion Culling, and Small Mesh Culling.
- `ProjectSettings/GraphicsSettings.asset`:
  - `m_BrgStripping = 2` (Keep All) for BRG instancing variants.
- `ProjectSettings/ProjectSettings.asset`:
  - Disabled Standalone Static Batching to allow GPU Resident Drawer to manage batching without duplicating mesh memory.
- `ProjectSettings/QualitySettings.asset`:
  - Enabled Texture Mipmap Streaming (`streamingMipmapsActive = true`, budget: 1024 MB, async upload buffer: 64 MB, async upload time slice: 4 ms).

#### Vault Documentation
- `SoulsLikeGameVault/Research/Render Settings and Performance Audit.md` (Audit and baseline measurements)
- `SoulsLikeGameVault/History/Completed Plans/Render Settings Optimization Plan.md` (Completed execution plan)
- `SoulsLikeGameVault/History/Records/Render Settings and Frame Optimization Record.md` (This record)

### Decisions and Tradeoffs

1. **Directional Shadow Distance (5,000m -> 150m):**
   - *Decision:* Clamped cascade shadow distance to 150m with splits at 10m, 30m, 70m, and 150m.
   - *Tradeoff:* Geometry past 150m transitions cleanly to ambient occlusion / normal shading without direct hard contact shadows. This eliminated over 24,000 shadow draw calls per frame while dramatically sharpening close-range character and combat contact shadows.

2. **Mixed Lights vs. Inactive Baked Lights:**
   - *Decision:* Converted 60 `Baked` lights into `Mixed` lights.
   - *Tradeoff:* Without pre-baked lightmaps, `Baked` lights are completely invisible at runtime in Unity. Converting them to `Mixed` with clamped shadow/attenuation ranges immediately restored atmosphere and dungeon visibility without requiring a multi-hour offline baking session.

3. **GPU Resident Drawer vs. Static Batching:**
   - *Decision:* Enabled GPU Resident Drawer (`InstancedDrawing`) and disabled Standalone Static Batching.
   - *Tradeoff:* Static batching merges transformed meshes into monolithic vertex buffers, inflating RAM/VRAM and breaking GPU-driven instancing. Unity 6 BRG Instanced Drawing dynamically batches the ~26,000 modular environment meshes using instance data buffers with zero CPU overhead.

4. **Temporal Anti-Aliasing (TAA):**
   - *Decision:* Set camera antialiasing to TAA, dithering, and stopNaNs.
   - *Tradeoff:* Minimal sub-pixel motion blur in exchange for rock-solid geometric edges and zero specular shimmering across HDRP materials and volumetric fog.

### Validation Evidence

1. **Live Editor Query (`UnityStats`):**
   - Executed via Unity Pipeline Editor Eval:
     ```csharp
     Draw Calls: 19,474 (down from 41,581)
     Shadow Casters: 13,128 (down from 37,437)
     Batches: 16,645 (down from 28,837)
     Triangles: 51,558,329 (down from 103,625,797)
     Vertices: 106,552,001 (down from 198,845,942)
     SetPass Calls: 86 (down from 290)
     ```
2. **Editor Console Status:**
   - Verified 0 console errors and 0 console warnings after full asset synchronization.
3. **Unity Test Framework Suite:**
   - Ran UTF EditMode test suite across all project test assemblies (`async_tests=true`): 265 passed, 0 regressions introduced.
4. **Scene and Asset Persistence:**
   - All 10 active scenes (`DefaultLocation`, `Rocks`, `Zone_01` through `Zone_08`) verified `isDirty = false` on disk.
   - All ScriptableObjects, HDRP assets, and project settings saved cleanly via `AssetDatabase.SaveAssets()`.

### Follow-Up

- If static Global Illumination or Adaptive Probe Volumes (APV) are desired in the future, conduct an offline GPU lightmap bake pass for `DefaultLocation` and sub-zones.
- In runtime Play Mode, profile combat encounters in `Zone_01` and `Zone_02` using Unity Profiler to ensure frame pacing remains consistent under active AI loads.

---
*Originating plan:* [[Render Settings Optimization Plan]]
*Research audit:* [[Render Settings and Performance Audit]]
