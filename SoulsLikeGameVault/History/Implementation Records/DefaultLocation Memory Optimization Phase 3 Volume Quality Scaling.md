---
title: DefaultLocation Memory Optimization Phase 3 Volume Quality Scaling
type: implementation-record
domains:
  - performance
  - rendering
  - scenes
status: done
authority: historical
updated: 2026-09-08
aliases: []
tags:
  - history/change
---
# DefaultLocation Memory Optimization Phase 3 Volume Quality Scaling

## Implementation Record Contract

### Outcome

Completed Phase 3. DefaultLocation now uses project-owned normal and Editor Low Memory Volume profiles, with automatic quality-based selection that does not serialize temporary low-profile state into the scene.

### Why

The vendor profile applied a fixed 5,000-unit/four-cascade HDRP shadow setup and high-quality AO, Bloom, and volumetric fog overrides. Those settings did not follow Unity quality levels and were not safe to edit in place.

### Changed Files and Assets

- `Assets/Scripts/Rendering/QualityVolumeProfileSelector.cs`
- `Assets/Settings/RenderPipelines/DefaultLocation Volume Profile.asset`
- `Assets/Settings/RenderPipelines/DefaultLocation Volume Low Memory Profile.asset`
- `Assets/Scenes/DefaultLocation/DefaultLocation.unity` — `Sky and Fog Global Volume` now owns the selector and keeps the normal profile as serialized `sharedProfile`.

### Decisions and Tradeoffs

- The selector runs in Edit Mode and Play Mode and listens to `QualitySettings.activeQualityLevelChanged`.
- High Fidelity, Balanced, and Performant use the normal project-owned profile. Editor Low Memory receives a temporary `Volume.profile` clone, leaving serialized `sharedProfile` unchanged.
- The low profile uses 150-unit shadows, two cascades, inactive AO/Bloom, and disabled volumetric fog. The original HDR sky remains in both accepted profiles.
- A project-owned 1024-max-size HDR sky trial was rejected after the castle rendered black. The trial asset was removed; no vendor asset was modified.

### Validation Evidence

- Unity 6000.3.11f1 recompiled the selector successfully with no compile errors.
- Edit-mode quality sequence passed: High Fidelity → Editor Low Memory → High Fidelity selected normal profile → temporary low clone → normal profile, while `sharedProfile` remained `DefaultLocation Volume Profile`.
- All ten currently loaded DefaultLocation scenes remained `isDirty=false`; the Unity console had no errors after the final checks.
- Same-camera 640×360 captures were visually valid for both accepted profiles:
  - `Temp/DefaultLocation-Phase3-Final-High-Fidelity.png`
  - `Temp/DefaultLocation-Phase3-Final-Editor-Low-Memory.png`
- Final Editor stats were environment measurements with all ten scenes loaded, not a memory-reduction claim: High Fidelity reported about 2.897 GB allocated / 6.536 GB reserved; Editor Low Memory reported about 2.920 GB allocated / 6.536 GB reserved. GPU frame time was unavailable/0 ms in this Editor sample.

### Documentation Updated

- [[Work/Plans/DefaultLocation Memory Optimization]] — Phase 3 marked complete.
- Originating issue: [[Work/Issues/DefaultLocation Memory and Rendering Issues]].

### Follow-Up

Measure loaded texture residency, render-target/GPU counters, and peak commit in a clean controlled session before claiming a memory reduction. Continue with Phase 4 asset residency work if the low preset remains visually acceptable.

Link the originating research, plan, or issue. Keep this concise; Git remains the detailed change history.
