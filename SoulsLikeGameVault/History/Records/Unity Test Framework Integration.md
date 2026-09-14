---
title: Unity Test Framework Integration
type: implementation-record
domains:
  - testing
  - tooling
status: done
authority: historical
verified: 2026-09-11
source_commit: eb067e69
updated: 2026-09-11
tags:
  - history/change
---
# Unity Test Framework Integration

## Implementation Record Contract

### Outcome

Unity Test Framework (UTF) 1.6.0 is an explicit project dependency. Agents now route Unity test work through the registered [[Knowledge/Guides/Testing/Unity Test Framework Test Flow|Unity Test Framework Test Flow]] using the existing official Unity CLI/Pipeline bridge.

### Why

The requested integration makes UTF use discoverable and repeatable for AI agents and records a safe test flow in Obsidian. UTF already existed indirectly through the development feature; retaining its version avoids an unnecessary upgrade.

### Changed Files and Assets

- `Packages/manifest.json`: direct `com.unity.test-framework` dependency at 1.6.0, applied with asynchronous `package_add`.
- `Packages/packages-lock.json`: UTF depth changed from 1 to 0; its version and other packages are unchanged.
- `AGENTS.md`, `.agents/skills/soulslike-validation/SKILL.md`, and `.codex/agents/unity-test-runner.toml`: UTF preference, registered context lookup, explicit filtered asynchronous Edit Mode execution, result interpretation, and validation gaps.
- Vault guide, context registry, Home, and Implementation History links.

No C# or serialized Unity assets were changed.

### Decisions and Tradeoffs

The existing test assemblies and Pipeline commands already support UTF. No additional harness, sample tests, package server, or runtime assembly migration was needed. The existing elevator motion fixture provides a deterministic integration check; it is not evidence of full gameplay coverage.

Independent review confirmed that the installed Pipeline async runner accepts but does not enforce `--timeout`. The guide, repository rules, and validation skill therefore require caller-side wall-clock monitoring, cancellation, and confirmation that the runner stopped. The package bridge itself was not modified.

### Validation Evidence

Verified 2026-09-11 on Unity 6000.3.11f1, UTF 1.6.0, Pipeline 0.5.0-exp.1, and official Unity CLI 1.0.0-beta.6.

UPM completed successfully and reported UTF as installed, direct, and BuiltIn. Manifest/lockfile changes were limited to the explicit dependency and its depth. Editor compilation and domain reload were idle; Console error output was empty.

Test preflight found the loaded `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity` scene clean and no active test run. Discovery found the intended fixture in `Assembly-CSharp-Editor`.

```powershell
unity command run_tests --mode editor --filter "SoulsLike.Editor.Tests.Elevator.ElevatorMotionTests" --filter_type testName --async_tests true --timeout 120 --json
```

The asynchronous run completed in **0.42 seconds**: **2 total, 2 passed, 0 failed, 0 skipped, 0 inconclusive**.

- `TrapezoidalProfile_UsesCruiseAndReachesExactEndpoint`
- `TriangularProfile_ReachesExactEndpoint`

Final checks confirmed the test runner inactive, Editor ready, Play Mode stopped, the original scene clean, and no Console errors. Skill validation and agent TOML parsing passed; the task diff passed whitespace checks.

### Documentation Updated

- [[Knowledge/Guides/Testing/Unity Test Framework Test Flow]]
- [[Meta/Agent Context Registry]]: required `unity-testing` context.
- [[Home]] and [[History/Implementation History]]: navigation links.

### Follow-Up

Play Mode and the broader test suite were not executed. Feature-specific changes should select their own relevant tests. Any required gameplay validation belongs in a separately scoped follow-up under the project test safety rules.
