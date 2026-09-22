---
title: Unity Test Framework Test Flow
type: guide
domains:
  - testing
  - tooling
status: current
authority: required
verified: 2026-09-11
source_commit: eb067e69
updated: 2026-09-11
context_keys:
  - unity-testing
aliases:
  - UTF Test Flow
tags:
  - agent/context
---
# Unity Test Framework Test Flow

Use the **Unity Test Framework (UTF)** to verify Unity C# behavior and regressions. Agents select a small, meaningful test group, run it through the connected Editor, and report the actual test outcome. This guide is registered as `unity-testing`; `AGENTS.md` remains the authority for scene safety and scope.

## Package and Integration

Verified on 2026-09-11 with Unity **6000.3.11f1**, UTF **1.6.0**, `com.unity.pipeline` **0.5.0-exp.1**, and official `unity` CLI **1.0.0-beta.6**.

- `Packages/manifest.json` declares `"com.unity.test-framework": "1.6.0"` directly.
- `Packages/packages-lock.json` resolves UTF 1.6.0 at depth 0. UPM reports its source as `BuiltIn`; this is expected for this Editor.
- UTF was already available through `com.unity.feature.development`. Making the same version direct makes the testing dependency explicit without upgrading it.
- Use the existing Pipeline commands: `list_tests`, `run_tests`, `test_status`, and `cancel_tests`, alongside project preflight commands `assert_test_ready` / `list_open_scenes`. No additional test server is needed.
- Future package changes go through Unity Package Manager / the official CLI. Inspect the resolved state first, preserve a working compatible version, and do not edit `Library/PackageCache`.

The package versions and command schemas above are an observed baseline. Recheck live state when they change.

## Agent Test Selection

1. Define the behavior and expected result before testing. Reuse a relevant existing fixture; add a focused regression test when changed behavior needs coverage.
2. Prefer deterministic Edit Mode tests for logic and safe Editor validation. Use NUnit `[Test]` / `[TestCase]` for synchronous cases; use UTF `[UnityTest]` only when yielding over Editor updates is necessary.
3. Inspect the fixture and setup/teardown before execution. An Edit Mode test can still load scenes, modify assets, or enter Play Mode; its label alone does not make it safe.
4. During normal agent validation, defer every Play Mode test and any test that enters Play Mode, drives gameplay, or requires unassigned scene changes. Record the coverage gap and assign a separate bounded follow-up to `unity_test_runner` when needed.
5. Select a fixture, test, assembly, or category that covers the change. Avoid unfiltered runs and third-party package suites. Leave explicit tests excluded unless the assignment specifically includes them.
6. Documentation changes and other reversible, low-impact changes do not need artificial tests. Compilation, static inspection, and tests provide different evidence; name exactly which were performed.

## Test Authoring and Assemblies

Use the project's existing test layout and assembly boundary before introducing a new test assembly. Keep NUnit/UTF test code out of production assemblies and player builds.

Current project discovery (2026-09-11):

| Test location | Discovered assembly | Edit Mode tests |
|---|---|---:|
| `Assets/Scripts/Editor/Tests` | `Assembly-CSharp-Editor` | 84 |
| `Assets/Scripts/Tests/CharacterRuntime` | `SoulsLike.Character.Runtime.Tests` | 14 |
| `Assets/Scripts/Tests/EnemyRuntime` | `SoulsLike.Enemy.Runtime.Tests` | 58 |

The two dedicated test assemblies are already Editor-only and marked with `TestAssemblies`. The Character test assembly references `SoulsLike.Character.Runtime`; Enemy tests currently use reflection against runtime code. Existing Editor-folder tests are also discovered, so UTF integration does not require a new harness or a broad assembly migration. The live list additionally contained one Addressables documentation example; exclude it from project validation unless relevant.

A safe integration check is the existing `SoulsLike.Editor.Tests.Elevator.ElevatorMotionTests` fixture: deterministic motion calculations without scene or asset setup. Its successful execution verifies the UTF/CLI path, not unrelated gameplay coverage.

For a new Edit Mode test assembly, Unity's **Assets > Create > Testing > Test Assembly Folder** creates an assembly definition with the testing references. Restrict it to the Editor platform and reference only the production assemblies it exercises. UTF's generated configuration references `nunit.framework.dll`, `UnityEngine.TestRunner`, and `UnityEditor.TestRunner`. See [Unity's test assembly instructions](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/workflow-create-test-assembly.html).

An assembly defined by an asmdef cannot directly reference predefined `Assembly-CSharp` code. Confirm the current production boundary before adding references; do not migrate unrelated runtime code merely to introduce tests.

Write observable assertions, including relevant failure cases, rather than tests that mirror the implementation or always pass. Destroy temporary objects and release subscriptions/resources in teardown. Tests must not save over project assets or leave scenes dirty. Follow the normal single-writer C# and Unity asset workflows when authoring tests; the validation agent executes them without editing expectations.

## Safe Execution Flow

Run from this repository root. If multiple Editors are available, select this project's exact path with the CLI's `--project-path` option. Use the official `unity` executable.

The flow is **discover → inspect Editor and scenes → select tests → run asynchronously → inspect results → confirm cleanup**. These are separate decision points; do not blindly paste the entire sequence into an unattended script.

### 1. Discover and preflight

```powershell
unity status --json
unity command --json
unity command editor_status --json
unity command list_open_scenes --json
```

Wait until compilation and domain reload are finished and Play Mode is stopped. Inspect **every** scene; all must have `isDirty=false`. Stop on a dirty scene. Report a dirty Untitled scene as `BLOCKED_DIRTY_UNTITLED_SCENE` under the project safety policy. Save a named scene only when the assignment explicitly permits saving it, then inspect again. Never use `open_scene`, `save_all`, a dialog, or scene reload to bypass this condition.

After the clean-scene preflight:

```powershell
unity command test_status --json
unity command list_tests --mode editor --json
```

Confirm no previous run remains active. Discover the selected fixture or assembly and inspect its source for safety. A missing fixture is a discovery/configuration problem, not a pass.

### 2. Start only the selected tests

Immediately before execution, repeat the scene gate:

```powershell
unity command assert_test_ready --json
```

Only after it succeeds, run the selected group. Replace the filter with a name observed in discovery:

```powershell
unity command run_tests --mode editor --filter "SoulsLike.Editor.Tests.Elevator.ElevatorMotionTests" --filter_type testName --async_tests true --timeout 120 --json
```

The current bridge uses `editor` for Edit Mode. Its unsafe-for-normal-validation defaults are `mode=all` and synchronous execution; always supply the explicit parameters above. `filter_type` also supports `assembly` and `category`. The schema describes the name filter as a case-insensitive partial match, so confirm that the reported executed tests match the intended scope.

Set the expected budget for the selected group; **120 seconds is an example budget**, not permission to broaden the run. **The installed Pipeline 0.5.0-exp.1 async runner does not enforce `--timeout`.** Its `PipelineTestRunner.ExecuteAsyncMode` receives `timeoutSeconds` but does not use it. The caller must track wall-clock time and request cancellation at the deadline; the argument alone will not stop this run.

### 3. Poll and inspect

```powershell
unity command test_status --json
```

Poll at short bounded intervals until completion. The CLI may return `data.result` as a JSON-encoded string; parse it before evaluating test state. A top-level `success=true` means that the request succeeded, not that the tests passed.

Require an actual completed outcome, a nonzero executed selection, no failed tests, and no cancellation/error. Inspect skipped/inconclusive counts and failed test names/messages. A zero-test, blocked, timed-out, cancelled, or unavailable run must be reported as incomplete validation.

Finish by checking:

```powershell
unity command editor_status --json
unity command list_open_scenes --json
unity command console --level error --tail 30 --json
```

Confirm the runner is inactive and the Editor has stopped compiling, reloading, and playing. Report any changed scene state or relevant console error.

## Timeout and Recovery

Enforce the assigned wall-clock budget while polling. For the current async bridge, `--timeout` is not enforced; caller-side monitoring and `cancel_tests` are the timeout control.

If the budget expires, inspect `test_status` and `editor_status`, request `cancel_tests` for the still-active run, and continue observing until no run remains active. Inspect Console and `Editor.log`, including evidence of a modal dialog. Do not open scenes, dismiss save dialogs automatically, launch another Editor against the open project, or retry blindly.

Cancellation is a request; verify that execution actually stopped. If the Editor is unresponsive or cancellation cannot be confirmed, report the active/unresolved state and required action. Never claim successful validation or start another test run while the previous run may still be active.

If the CLI/Editor is unavailable, continue any safe assigned static checks and record that UTF execution was unavailable. Do not install another MCP server as a fallback.

## Evidence and Handoff

Return a compact record containing:

- Unity/UTF versions and scene/compilation preflight outcome.
- Exact mode, filter, filter type, timeout, and discovered selection.
- Final test status; executed, passed, failed, skipped/inconclusive counts and duration.
- Failed test names and relevant messages/stack traces; whether evidence shows a pre-existing failure.
- Confirmation of inactive tests, stopped Play Mode, and final scene state.
- Coverage not executed and the reason, including all deferred Play Mode coverage.

For a test failure, return evidence to the owning writer. The `unity_test_runner` must not repair source, assets, settings, or expected results. The writer can fix the assigned defect and request a targeted rerun when the new change justifies it.

## Verified Integration Run

The [[History/Records/Unity Test Framework Integration|2026-09-11 integration record]] captures the package change and live check: both elevator motion tests passed in 0.42 seconds, with no failures, skipped tests, or inconclusive results. The runner finished inactive, the Editor remained outside Play Mode, and the original scene stayed clean. This establishes that the UTF/CLI flow works; it does not claim full project coverage.

## Sources

- [UTF 1.6 package documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/index.html).
- [Unity 6.3: Create a test assembly](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/workflow-create-test-assembly.html).
- [Unity 6.3: Edit Mode and Play Mode tests](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/edit-mode-vs-play-mode-tests.html).
- [Unity 6.3: Run tests](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/running-tests.html).
- Live project evidence: `Packages/manifest.json`, `Packages/packages-lock.json`, and the connected Editor's `unity command --json` schemas.
