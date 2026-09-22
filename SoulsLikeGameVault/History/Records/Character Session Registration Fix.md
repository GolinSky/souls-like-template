---
title: Character Session Registration Fix
type: implementation-record
domains:
  - character
  - dependency-injection
status: done
authority: historical
updated: 2026-09-22
verified: 2026-09-22
source_commit: 509f9d27
tags:
  - history/change
---

# Character Session Registration Fix

## Implementation Record Contract

### Outcome

Fixed the duplicate `IPlayerSession` contract introduced by [[History/Records/Character Clean Architecture Implementation]]. The actual local-player registration set now builds successfully in VContainer.

### Why

`CharacterScopeInstaller` combined `.As<IPlayerSession>()` with `.AsImplementedInterfaces()`. The installed VContainer appends implemented interfaces without deduplication, producing two `IPlayerSession` entries and a registry conflict. This interrupted `CharacterFactory.CreateCharacter` before `CoreGameOrchestrator` assigned its character; the later `Start` null reference was a downstream failure.

### Changed Files and Assets

- `Assets/Scripts/Services/VContainer/CharacterScopeInstaller.cs`: removed only the redundant `.As<IPlayerSession>()` call.
- `Assets/Scripts/Editor/Tests/VContainer/CharacterScopeInstallerTests.cs` and Unity-generated metadata: added a focused registration regression test.
- [[Knowledge/Architecture/Systems/Character System]]: documented the session registration contract.

### Decisions and Tradeoffs

Retained `.AsSelf().AsImplementedInterfaces()` so all concrete, session, observer, initialization, and disposal contracts remain exposed. No null guard or recovery path was added to hide failed character creation. Preserved the pre-existing dirty refactor changes.

The test invokes the installer's actual local-player registration method on an inactive temporary scope, builds a real container, and verifies its session registration and contracts. It does not resolve the services or load a prefab; teardown destroys the temporary scope.

### Validation Evidence

Unity 6000.3.11f1 / UTF 1.6.0 through the official CLI/Pipeline bridge, filtered to `SoulsLike.Editor.Tests.DependencyInjection.CharacterScopeInstallerTests`, asynchronous Edit Mode with a 60-second caller budget:

- Before the fix: 1 executed, 1 failed in 0.42 seconds with the exact reported duplicate `IPlayerSession` VContainer exception.
- After the fix: 1 executed, 1 passed in 0.34 seconds; 0 failed, skipped, or inconclusive.
- Independent review: no material findings.
- Compilation successful; final console ground truth had 0 errors. Historical console entries retain the earlier startup failures.
- Final Editor state: ready, Play Mode stopped, no active test, all ten loaded scenes clean.
- Focused whitespace checks passed; Unity imported the new test and generated its metadata.

### Documentation Updated

[[Knowledge/Architecture/Systems/Character System]] and this record.

### Follow-Up

Full prefab-backed character creation, service initialization, and gameplay were not exercised. This registry regression test does not establish the broader runtime outcomes listed in [[History/Records/Character Clean Architecture Implementation]].
