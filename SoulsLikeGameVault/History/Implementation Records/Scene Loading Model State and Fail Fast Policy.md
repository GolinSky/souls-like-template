---
title: Scene Loading Model State and Fail Fast Policy
type: implementation-record
domains:
  - scenes
  - lifecycle
status: done
authority: historical
updated: 2026-09-08
tags:
  - history/change
---

# Scene Loading Model State and Fail Fast Policy

## Implementation Record Contract

### Outcome

User-directed revision of the Phase 6 source experiment: model-owned runtime state, concurrent dependencies with the destination last, and fail-fast errors without rollback.

### Why

The user defines SceneModel as a runtime model that stores mutable state alongside the original ScriptableObject data. The user prioritizes concurrent dependency loading and wants main-system failures to propagate for diagnosis rather than being handled through cleanup/recovery.

### Changed Files and Assets

- `Assets/Scripts/Services/Scenes/Data/SceneModel.cs`: runtime `IsLoadingScene` state.
- `Assets/Scripts/Services/Scenes/SceneService.cs`: model-backed guard, concurrent dependency starts, target-last ordering, immediate observed failure propagation, removed failure cleanup and required-model log-and-return fallbacks.
- `Tests/SceneLoadingHarness/`: links the real SceneModel/base and tests the revised policy with fake SceneData and native operations.
- Current scene architecture, Phase 6 plan/issues, and supersession notice on the previous implementation record.

No Unity assets or Obsidian configuration are changed.

### Decisions and Tradeoffs

- Keep normal success-path Loading-scene unload and unload-handle release.
- Reset the model flag in `finally` when the call ends. After failure, pending native loads can continue and loaded scenes/handles remain; there is no rollback, cancellation, automatic retry, or safe-retry guarantee.
- Throw on a failed dependency while other dependencies may still be pending. Never start the main destination after a dependency failure.
- Concurrent loading has the same final scene residency as sequential loading. It can increase temporary peak memory; no measured timing or memory gain is claimed.
- Existing caller-side pending-spawn ownership remains outside this change.

### Validation Evidence

All ten revised source-linked checks passed, independently confirmed with `dotnet run --project Tests/SceneLoadingHarness/SceneLoadingHarness.csproj -p:UseSharedCompilation=false`. The tests compile the real SceneService, SceneModel, and Model base with fake SceneData/Unity operations and a Task alias for UniTask. They verify shared model state, concurrent dependencies with target-last ordering, immediate failure while another dependency remains pending, deliberate absence of rollback, Loading/target/start/activation/unload failures, callback exception preservation, and required-model failure without a silent fallback.

Live Serena C# diagnostics were empty for both production files; targeted `git diff --check` passed. Independent source review found no material defects within the requested semantics. No Unity scene loads, Play Mode, tests, or build were performed; native runtime and memory acceptance remain unverified.

### Documentation Updated

- [[Architecture/Systems/Scene Loading System]]
- [[Work/Plans/DefaultLocation Memory Optimization]]
- [[Work/Issues/DefaultLocation Memory and Rendering Issues]]
- [[Work/Issues/Scene Transitions Allow Concurrent Load Operations]]
- [[History/Implementation Records/DefaultLocation Memory Optimization Phase 6 Bounded Loading]] (historical supersession notice)

### Follow-Up

Run live loading/memory validation when the workstation permits it. Fix a main-system failure before attempting further transitions; this service does not provide recovery. Address pending-spawn ownership separately.
