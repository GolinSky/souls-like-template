# Scene loading control-flow checks

Run from the repository root with the .NET 10 SDK:

```powershell
dotnet run --project Tests/SceneLoadingHarness/SceneLoadingHarness.csproj -p:UseSharedCompilation=false
```

This executable compiles the real `SceneService.cs` and `ISceneService.cs` through
source links. It aliases `UniTask` to .NET `Task` and supplies a deterministic
synchronization context, scene model, and fake Unity/Addressables operations.
No Unity Editor, external packages, scene, asset, or build is needed.
Each asynchronous check is limited to 1,000 scheduler ticks.

Checks cover overlapping requests, sequential starts, dependency/target/loading
failure, synchronous start failure, reverse rollback, failed-handle release,
activation failure, progress-callback failure while a load is pending, cleanup
error reporting, completion subscriber failure, retries, and three successful
transitions. The fake backend detects invalid-handle reads and double releases.

## Baseline reproduction

To exercise the pre-change service, extract the source from commit `30e5f953`
into `Temp/Phase6-SceneService-Baseline.cs`, then run:

```powershell
dotnet run --project Tests/SceneLoadingHarness/SceneLoadingHarness.csproj -p:SceneServiceSource=F:/Private/SoulsLikeTemplate/Temp/Phase6-SceneService-Baseline.cs -p:UseSharedCompilation=false -- --baseline
```

Adjust the absolute source override for another checkout. The baseline checks
prove two Loading requests start before either completes, and a failed dependency
leaves successful partial loads without releasing their handles. They are
expected observations of the original defect, not assertions of correct behavior.

## Boundary

This validates production **control flow against a fake backend**. It does not
execute UniTask's Unity player loop, Unity native scene operations, Addressables
reference counting, VContainer lifetimes, gameplay, or real memory allocation.
The fake unload behavior models a successful unload releasing its load handle;
the returned unload-operation handle remains caller-owned. Real unload failures
and native reference counts require separate Unity validation.

The Phase 6 preflight found only 1.9% system commit headroom. Unity tests, loads,
Play Mode, builds, and memory comparisons were therefore deferred. This harness
does not replace the plan's controlled three-cycle travel/memory acceptance gate.
