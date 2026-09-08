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

`SceneService.LoadScene` owns one asynchronous scene transition at a time. It rejects a second call before starting another Loading scene and holds ownership until success or failure cleanup finishes. There is no cancellation or automatic retry; the caller observes the result and can explicitly retry afterward.

The service loads Loading in Single mode, then loads each configured dependency additively in order, waiting for completion before starting the next. It loads the destination last, activates it, and unloads Loading. Successful transitions retain the destination and all dependencies. DefaultLocation still retains its main scene, eight zones, and Rocks; this is bounded loading, not spatial streaming.

On failure, the service waits for its outstanding operation, unloads successful destination scenes in reverse order, and releases valid failed load handles. A successful Loading scene remains as the recovery owner. Cleanup errors are logged individually while the original failure propagates. Failed cleanup can retain scene resources; real retry/unload behavior must be verified through Unity. A later Single-mode Loading operation replaces the previous scene set.

Addressables releases a successful scene-load handle when its scene unload completes; the service releases the returned unload-operation handle explicitly. Successful destination load handles are retained for scene lifetime and follow Addressables' scene-unloaded release path. `OnSceneChanged` runs after commit, so subscriber exceptions propagate without rolling back a loaded destination. `TargetScene` describes the requested destination, including after a failure; `CurrentScene` resolves the active scene. Existing progress reports cover load progress and do not guarantee activation/unload completion at 100%.

`GameOrchestrator` forwards calls. Menu/travel callers still prepare pending spawn state before requesting a load; the service gate does not protect that earlier mutation. See [[Work/Issues/Scene Transitions Allow Concurrent Load Operations]] for the remaining cross-caller ownership defect.

Source: `Assets/Scripts/Services/Scenes/SceneService.cs`, `Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs`, and `Assets/Settings/Data/SceneData.asset`.

Evidence and limitations: [[History/Implementation Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]]. This is a source-verified description of an unmeasured loading experiment, not a memory-budget acceptance result.
