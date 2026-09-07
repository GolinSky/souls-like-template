---
title: Addressable Asset Loads Have No Release Owner
type: issue
domains:
  - assets
  - lifecycle
status: open
authority: evidence
priority: medium
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
verification: resource lifetime defect
aliases: []
tags:
  - work/issue
  - status/open
  - audit/architecture
---
# Addressable Asset Loads Have No Release Owner

## Issue Contract

### Observed Behavior

Every asset-service load creates an Addressables load reference and discards its operation handle. IAssetService exposes no release/lifetime contract, and authored code has no matching asset-release path. Scene-scoped factories repeatedly load UI prefabs and mapping data, so these references survive the scene that consumed them.

Evidence classification: **resource lifetime defect**. Verified by static inspection on 2026-09-07 at source commit `3925ea83e7cd80b111331a6105835c3e84eeb2b6`; runtime symptoms are not claimed as reproduced.

### Expected Behavior

Every load has a matching owner that releases it after all instantiated consumers are finished, or a bounded cache deliberately retains one load reference for its lifetime.

### Reproduction

Trace repeated MainMenu → gameplay → MainMenu creation: UiFactory loads prefab assets on each scope lifetime. In a follow-up profiler run, compare Addressables reference counts before and after returning to the same state.

### Impact and Priority

**MEDIUM** — resource lifetime defect. Retained references are confirmed by source. Memory size, crash contribution, and performance impact were not measured; use a separate unity_profiler assignment before optimization decisions.

### Evidence

- Assets/Scripts/Services/AssetService/AddressableAssetService.cs:6-29 — all three APIs LoadAssetAsync/WaitForCompletion without release ownership.
- Assets/Scripts/Services/Ui/UiFactory.cs:29-44,48-70 — mapping and prefab loads followed by Object.Instantiate.
- Assets/Scripts/Utilities/Extensions/VContainerExt.cs:10-67 — additional static asset-service consumer.
- Repository-wide authored C# search for Addressables.Release/ReleaseInstance found only SceneService.cs:116, which releases a scene unload operation, not these asset loads.
- Library/PackageCache/com.unity.addressables@8460f1c9c927/Documentation~/memory-assets.md:7-14 — installed Addressables 2.9.1 documents balancing every load with a release.

### Hypotheses

The causal path above is source-backed. Any visual, timing, memory, or failure-injection outcome still requires the validation below; no broader failure mode is assumed.

### Open Questions

Retained references are confirmed by source. Memory size, crash contribution, and performance impact were not measured; use a separate unity_profiler assignment before optimization decisions.

## Resolution Handoff

### Approved Fix Scope

The current request authorizes audit and issue documentation only. Proposed remediation scope: Define load ownership or a bounded cache and implement balanced release at the matching lifetime boundary. Do not release a prefab while manually instantiated consumers still need its dependencies.

### Acceptance Criteria

Repeated equivalent scene cycles do not accumulate asset references; scope disposal releases assets when unused; live UI, characters, and shared assets remain valid.

### Validation

Static call-path and source inspection completed. No implementation, test run, save fault injection, Play Mode session, or performance measurement was performed. Use focused Edit Mode/unit fixtures where practical. Any required gameplay reproduction belongs in a separate `unity_test_runner` follow-up under Unity Test Safety; memory measurements belong to `unity_profiler`. Do not treat this note as a passing runtime test.

When resolved, set `status: done`, link the implementation record, and update affected architecture notes.

Audit: [[Research/Architecture and Systems Audit 2026-09-07]].

