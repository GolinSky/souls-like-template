---
title: Scene Loading System
type: architecture
domains:
  - scenes
  - lifecycle
status: current
authority: advisory
updated: 2026-09-08
verification: source and isolated control-flow tests; Unity integration pending
tags:
  - architecture/system
---

# Scene Loading System

`SceneModel` holds runtime loading state (`IsLoadingScene`) alongside its source `SceneData` ScriptableObject. `SceneService.LoadScene` checks and sets that model state before its first await, rejects overlapping requests, and clears the state in `finally` when the call succeeds or throws. Services sharing the model share this guard.

The service loads Loading in Single mode, then starts all configured additive dependencies together. It starts the destination only after every dependency succeeds, activates it, and unloads Loading. Successful transitions retain the destination and all dependencies. DefaultLocation still retains its main scene, eight zones, and Rocks. Concurrent loading can reduce waiting time but may increase temporary loading peaks; final residency is unchanged and no timing/memory benefit has been measured.

Failures propagate immediately when observed, including when another dependency remains pending. By explicit user decision, there is no rollback, failure-handle cleanup, catch-and-log recovery, cancellation, or automatic retry. Already-started operations may continue and loaded scenes remain after an error. Clearing the model's call-state flag does not mean those native operations stopped. Treat a main-system failure as a defect to fix; the service provides no recovery/retry guarantee. Mandatory injected model references are used directly, without log-and-return fallbacks.

Addressables releases a successful scene-load handle when its scene unload completes; the service releases the returned unload-operation handle explicitly. Successful destination load handles are retained for scene lifetime and follow Addressables' scene-unloaded release path. `OnSceneChanged` runs after commit, so subscriber exceptions propagate without rolling back a loaded destination. `TargetScene` describes the requested destination, including after a failure; `CurrentScene` resolves the active scene. Existing progress reports cover load progress and do not guarantee activation/unload completion at 100%.

`GameOrchestrator` forwards calls. Menu/travel callers still prepare pending spawn state before requesting a load; the service gate does not protect that earlier mutation. See [[Work/Issues/Scene Transitions Allow Concurrent Load Operations]] for the remaining cross-caller ownership defect.

Source: `Assets/Scripts/Services/Scenes/SceneService.cs`, `Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs`, and `Assets/Settings/Data/SceneData.asset`.

The original sequential/rollback experiment is historical: [[History/Implementation Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]]. The user superseded it with model-owned state, concurrent dependencies, and fail-fast propagation in [[History/Implementation Records/Scene Loading Model State and Fail Fast Policy]]. This is not a memory-budget acceptance result.
