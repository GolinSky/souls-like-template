---
name: soulslike-unity-assets
description: Perform one bounded SoulsLike Unity Editor or serialized-asset mutation. Use for assigned scene, prefab, asset, component, import, or serialization work that must be persisted through Unity.
---

# SoulsLike Unity Assets

1. Confirm the exact target and inspect current Editor, compilation, scene, prefab, or asset state.
2. Inspect unfamiliar Unity command schemas before invoking them.
3. Perform only the assigned mutation through official Unity tooling.
4. Save inside the same Unity operation where possible; otherwise refresh and reserialize each directly edited asset.
5. Verify import, persistence to disk, and relevant console errors.
6. Use `$soulslike-context` only for an applicable registered key; add `$soulslike-ui-workflow` for UI assets.

Report every modified scene, prefab, object, component, property, and asset. Do not redesign C# architecture.
