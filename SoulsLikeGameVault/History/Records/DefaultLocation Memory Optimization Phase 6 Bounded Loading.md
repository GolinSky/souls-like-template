---
title: DefaultLocation Memory Optimization Phase 6 Bounded Loading
type: implementation-record
domains:
  - performance
  - scenes
status: done
authority: historical
updated: 2026-09-08
tags:
  - history/change
---

# DefaultLocation Memory Optimization Phase 6 Bounded Loading

> Superseded by explicit user direction on 2026-09-08: [[History/Records/Scene Loading Model State and Fail Fast Policy]]. The sequential-loading and rollback behavior below describes commit `a57cd2e2`, not the current loader.

## Implementation Record Contract

### Outcome

Implemented the Phase 6 bounded loader experiment and passed isolated control-flow validation. Dependencies load one at a time; failed transitions roll back partial destination loads; overlapping service calls are rejected until loading/cleanup completes. Memory acceptance, current bundle duplication analysis, and runtime travel remain pending. Phase 6 remains incomplete; this record does not establish a measured memory improvement.

### Why

The original loader starts nine dependency scene operations before waiting for any completion. It retains successful partial loads on failure and accepts overlapping transitions. Bounding operations targets temporary loading peaks; all ten DefaultLocation scenes, including Rocks, remain resident on success.

### Changed Files and Assets

- `Assets/Scripts/Services/Scenes/SceneService.cs`: sequential dependency starts, reverse failure cleanup, failed-handle release, and one in-flight owner.
- `Tests/SceneLoadingHarness/`: source-linked .NET executable with deterministic fake scene/Addressables operations and a Task alias for UniTask.
- Plan, issue, architecture, and implementation-history notes listed below.

No scene, prefab, importer, Addressables group, or quality asset was changed. Obsidian configuration was not edited by this task.

### Decisions and Tradeoffs

- Keep spatial streaming outside this implementation. The prior Phase 4 sample passed the proposed commit gate, while the current sample measures an unrelated Editor scene and system-wide pressure. Neither establishes that the current location's settled residency requires spatial streaming.
- The plan's comparison-first gate cannot run safely in this session. Treat source changes as an unmeasured experiment pending a controlled before/after comparison.
- Do not infer current bundle duplication from Pack Separately or from obsolete August reports. Do not rebuild Addressables under the current memory pressure.
- The service owns the entire transition through cleanup; no cancellation API, background abandonment, or automatic retry was introduced. Callers receive faults and may explicitly retry after completion. Overlapping requests fault immediately rather than queueing or coalescing.
- A successful Loading scene remains as the recovery owner after failure. Successfully loaded destination scenes unload in reverse order; valid failed load handles are released. Each cleanup error is logged while the original transition exception is preserved. Cleanup failure does not prove zero retained scenes: a later Single-mode Loading request replaces remaining scenes, and actual engine handle recovery still needs validation.
- Success retains all destination scenes. Addressables owns their scene-unloaded release path. The returned unload-operation handle is explicitly released; the original successful scene-load handle is released by Addressables' unload completion. `OnSceneChanged` runs after commit, so a subscriber exception does not roll back the destination.
- The broader pending-spawn issue remains open: `PrepareResume`/`PrepareGraceSpawn` still run before service admission and can change spawn intent even when a second scene transition is rejected. No orchestrator or spawn API was changed in this bounded experiment.

### Validation Evidence

Read-only preflight: Unity 6000.3.11f1 is stopped, ready, and not compiling/reloading. Only `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity` is open, active, and clean; it was left untouched. The current StandaloneWindows64 build configuration is non-development, has no profiler, and includes only Bootstrap.

| Current safety sample | Value |
|---|---:|
| System commit / limit | 64.59 / 65.81 GiB |
| Commit headroom | 1.23 GiB / 1.9% |
| Available physical RAM | 5.92 GiB |
| Main Unity private bytes | 11.57 GiB |
| Main Unity working set | 1.73 GiB |
| Pages / second | 192 |
| Page reads / second | 15 |

This is a momentary safety sample, not a loading profile. It fails the plan's proposed 20% headroom target. No scene loading comparison, Unity test run, Play Mode session, build, or large memory snapshot was started.

The current DefaultLocation Addressables group contains main, Rocks, and eight zones with IncludeInBuild enabled and Pack Separately. Its group/schema date is August 31. Available local build products are from August 19 and contain only Data, Services, Settings, UI, and Default Local groups. None contain the current location group or its scenes. Legacy duplicate records are not evidence about current DefaultLocation duplication.

Executable baseline at `30e5f953`: **2 reproductions passed** against the original service source. Two simultaneous calls started two Loading operations before either completed. A dependency failure left a successful partial scene loaded with no handle releases; three dependencies were pending together in the bounded three-dependency fixture.

Candidate: **10 checks passed** with `dotnet run --project Tests/SceneLoadingHarness/SceneLoadingHarness.csproj -p:UseSharedCompilation=false`. Cases cover overlap, dependency failure/retry, three successful transitions, target failure with reverse rollback, Loading failure/retry, synchronous start failure, activation failure, progress callback failure during an outstanding load, multiple cleanup failures preserving the primary fault, and post-commit subscriber failure. The source-linked fixture validates a maximum of one pending additive load and the total dependency-plus-target progress denominator. The candidate overlap assertion failed before the gate was added, then passed afterward.

These tests alias UniTask to .NET Task and use fake native operations. They establish control-flow behavior, not actual Unity/Addressables reference counts, player-loop behavior, travel, or memory reduction. Live Serena diagnostics reported no C# diagnostics; targeted whitespace checks passed. Unity compilation/build and integration tests remain unverified under the memory blocker.

### Documentation Updated

- [[Work/Plans/DefaultLocation Memory Optimization]]
- [[History/Closed Issues/DefaultLocation Memory and Rendering Issues]]
- [[Work/Issues/Rejected Scene Transition Can Overwrite Pending Spawn Intent]]
- [[Knowledge/Architecture/Systems/Scene Loading System]]
- [[History/Implementation History]]

### Follow-Up

1. Recover sufficient system commit headroom and start a clean controlled session. A settled 20% margin alone does not guarantee a safe loading peak; monitor and stop before exhausting memory. Do not blindly rerun the known crash workload.
2. Preflight clean scenes; lock quality, camera, resolution, Addressables play mode/build content, and initial scene set. Compare the original loader and bounded loader under matched conditions, using smaller controlled subsets first where needed.
3. Record peak/settled private bytes, working set, system commit/physical RAM, paging activity, VRAM, Unity counters, load-to-activation duration, loaded scenes, new Console errors, and handle ownership over three load/unload cycles. Do not count isolated fake-backend tests as real Addressables travel evidence.
4. Run runtime failure/retry and handle-release checks in a separate `unity_test_runner` validation phase, following Unity Test Safety and the project's normal Play Mode exclusion. Build fresh Addressables content/layout only when memory and build configuration permit; inspect duplicated mesh/material/texture GUIDs in that layout.
5. Define a Player hardware/budget target and capture a Development Player separately. If measured settled residency still exceeds that budget, create a separately reviewed streaming design covering Rocks, bounds, neighbors, hysteresis, shared services, cross-scene references, entity/enemy lifetime, saves, physics/NavMesh links, lighting, and occlusion.

Origin: [[Work/Plans/DefaultLocation Memory Optimization#Phase 6 — Bound loading and then consider spatial streaming]].
