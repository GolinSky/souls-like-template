# Scene loading control-flow checks

Run from the repository root with the .NET 10 SDK:

```powershell
dotnet run --project Tests/SceneLoadingHarness/SceneLoadingHarness.csproj -p:UseSharedCompilation=false
```

This executable source-links the real SceneService, ISceneService, SceneModel,
and Model base. SceneData and Unity/Addressables operations are test doubles.
UniTask is aliased to .NET Task with a deterministic synchronization context.
No Unity Editor or external packages are required. Each asynchronous check is
limited to 1,000 scheduler ticks.

The current tests follow the user-directed concurrent/fail-fast policy:
model-owned state shared between services, concurrent dependency starts,
target waiting for every dependency, immediate failure while another dependency
is held pending, no rollback, Loading/target/start/activation/unload failures,
original callback exceptions, and no silent required-model fallback.
Successful transitions still unload Loading and release the unload operation.

These tests do not execute Unity native operations, real Addressables reference
counts, UniTask's player loop, VContainer lifetimes, gameplay, or memory allocation.
A flag reset after failure does not mean outstanding native loads have stopped.
There is deliberately no failure-recovery or retry guarantee.

The original sequential/rollback tests are preserved in Git at `a57cd2e2`.
They describe the superseded experiment, not the current expected behavior.
