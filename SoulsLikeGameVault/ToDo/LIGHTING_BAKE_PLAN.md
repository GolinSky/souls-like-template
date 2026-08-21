# Bake DefaultLocation Lighting Across 23 Scenes

## Summary

Update the existing location bake workflow, then run and monitor a complete per-scene bake across all 23 scenes under `Assets/Scenes/DefaultLocation`.

Each scene will bake with the 27 `PointLights` and 33 `SpotLights` from the actual source scene `DefaultLocaiton.unity`. LOD lightmap density will halve at each level, using Unity's per-renderer `Scale in Lightmap` mechanism documented for LODGroups in the [Unity LOD Group manual](https://docs.unity3d.com/Manual/class-LODGroup.html).

## Implementation Changes

- Surgically update `Assets/Editor/LocationBakeTool.cs`:
  - Copy only the complete `PointLights` and `SpotLights` hierarchies, preserving world transforms and component properties.
  - Fail immediately if either required source root is missing.
  - Remove every existing `_BakeCopiedLightsContainer`, including inactive duplicates; this fixes the current duplicates in Blueprints, CandleHolder, and Candles.
  - Create exactly one 60-light copied container in each of the 22 non-source scenes.
  - Enable every copied GameObject and Light component and set each Light to `Baked` during its scene's bake.
  - Disable the copied hierarchy and Light components after baking so additive loading does not multiply runtime lights.
- Bake `DefaultLocaiton.unity` with its original `PointLights` and `SpotLights` rather than duplicating them:
  - Temporarily enable them and set them to `Baked`.
  - Restore every original active/enabled state and bake type afterward.
  - Remove its obsolete copied-light container.
- Before each scene bake, assign:
  - LOD0: `1.0` -> 10 texels/unit
  - LOD1: `0.5` -> 5 texels/unit
  - LOD2: `0.25` -> 2.5 texels/unit
  - LOD3: `0.125` -> 1.25 texels/unit
  - LOD4: `0.0625` -> 0.625 texels/unit
  - Leave non-LOD renderers unchanged.
- Preserve the current Progressive GPU settings: 10 texels/unit, 1024 maximum lightmap size, directional maps, AO, 32 direct samples, 128 indirect/environment samples, and two bounces.
- Keep the existing editor menu path, but queue the synchronous workflow through `EditorApplication.delayCall` so the official Unity CLI request returns before the long bake blocks the Editor.

## Execution and Failure Handling

- Recompile through the official `unity` CLI and confirm there are no compilation errors.
- Invoke `Tools/Bake/Bake Subscenes With Copied Baked Lights` through `unity command menu --path`.
- Monitor `bake_progress.txt` until all 23 scenes report successful completion.
- If a bake fails, stop at that scene, disable its copied lights, save its state, restore source-light states, and log the exact failure. Keep already completed scenes intact.
- Save each scene and generated lighting assets through Unity, then reopen all 23 scenes additively with `DefaultLocaiton.unity` active.

## Validation

- Confirm all 23 scenes completed and have persisted lighting data/lightmaps where geometry requires them.
- Audit the current 26,464 LOD renderer assignments:
  - 7,247 at `1.0`
  - 7,247 at `0.5`
  - 7,247 at `0.25`
  - 4,178 at `0.125`
  - 545 at `0.0625`
- Confirm the 22 non-source scenes each contain exactly one disabled copied-light container with 60 lights; the source scene contains none.
- Confirm original source-light states were restored and `DirectionalLight` was never copied or modified.
- Check the Unity console for compilation, import, serialization, lightmapping, and GPU-lightmapper errors.
- Do not run Unity test suites, per project instructions; verification is the completed bake, asset persistence checks, hierarchy audit, and console inspection.

## Assumptions

- Continue from the workspace's current intentionally cleared lighting state; do not restore the 2,344 deleted previous bake artifacts.
- Preserve all unrelated working-tree changes.
- Cross-scene geometry will not contribute shadows or indirect bounce to another scene because the selected workflow bakes each scene independently.
