---
title: Character Clean Architecture Implementation
type: implementation-record
domains:
  - character
  - architecture
  - dependency-injection
status: done
authority: historical
updated: 2026-09-22
verified: 2026-09-22
source_commit: 509f9d27
tags:
  - history/change
---

# Character Clean Architecture Implementation

## Implementation Record Contract

### Outcome

Implemented the user-authorized first review pass of [[Work/Plans/Character Clean Architecture]]. VContainer composes the actor state machine/coordinator/tick and local session. Local input and camera code use session-owned contracts. Runtime actions, progression, and stamina policy compile without Unity references.

### Why

Clarify the ownership issues in [[Work/Issues/Character Aggregate Architecture and Readability]] while retaining the existing Unity components and gameplay state owners.

### Changed Files and Assets

Main source areas: `Entities/Character`, its `Runtime` and `Input` folders, `Services/PlayerSession`, `Services/VContainer/CharacterScopeInstaller.cs`, equipment/attack/health/base components, and LevelUp UI code. Ladder input and existing component fixtures received the necessary signature/vector adaptations. New scripts and metadata are imported; serialized component fields, existing GUIDs, prefabs, scenes, and controllers are unchanged.

### Decisions and Tradeoffs

- Preserve the existing VContainer factory/scope topology. Reuse actor registrations separately from local-player bindings.
- Use `ITickable -> IPostTickable -> ILateTickable` for input, actor execution, then camera updates.
- Keep CharacterActionStateMachine as the only action/buffer state owner and HealthComponent as the only mutable resource owner.
- Use the existing equipment event; remove its concrete Character dependency. Attack consumes its own resolved context.
- Keep rest as one Character operation; reject an identity-only restoration wrapper. Retain Model's public injection/setup setter with its existing fixture contract documented.
- Add short XML summaries to new and modified production classes. No networking, new recovery mechanism, or generic framework.

### Validation Evidence

Unity 6000.3.11f1 through the official CLI/Pipeline bridge:

| Check | Result |
|---|---|
| Compilation | Successful; no current compiler errors |
| CharacterActionStateMachineTests | 19 passed, 0 failed/skipped/inconclusive; 0.03s |
| LevelUpUiFormatterTests | 6 passed, 0 failed/skipped/inconclusive; 0.01s |
| Editor/scene state | Play Mode stopped; no active test; all ten loaded scenes clean |
| Console ground truth | 0 current errors; compilationFailed=false |
| Static compatibility | No serialized-field changes; no outer dependencies in runtime; no missing script metadata |
| Independent review | No remaining material findings after camera update correction and stamina policy extraction |

The initial missing UniTask import was fixed. Camera updates with zero look input while blocked/dead were restored. Existing historical console capture entries are not current compilation failures. The tests were discovered, narrowly filtered, asynchronous Edit Mode runs with a 60-second caller budget; no broad suite, build, or Play Mode run was used.

The final public actor-registration extraction was followed by another successful Unity compile. Eleven existing unused-member compiler warnings remain; their declarations were not introduced by this refactor.

### Documentation Updated

[[Knowledge/Architecture/Systems/Character System]], the selected plan, and the originating issue.

### Follow-Up

User review and separately scoped gameplay validation remain: spawning the complete local container, animation/root-motion, input/camera feel, equipment notifications, grace/death/respawn, and scripted actor binding. Compilation and pure tests do not establish those runtime outcomes. The plan and issue retain their review status; this record does not close the separate factory or lifecycle plans.
