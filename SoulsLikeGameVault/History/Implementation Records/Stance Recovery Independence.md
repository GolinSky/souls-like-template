---
title: Stance Recovery Independence
type: implementation-record
domains:
  - combat
status: done
authority: historical
updated: 2026-09-09
aliases: []
tags:
  - history/change
---

# Stance Recovery Independence

## Implementation Record Contract

### Outcome

Stance recovery now runs independently while the poise recovery delay is active. An active critical opportunity remains the sole stance-recovery suppressor.

### Why

`CombatDefenseComponent.TickRecovery` returned from the shared recovery method while the poise delay was positive, unintentionally skipping stance recovery and contradicting the combat architecture.

### Changed Files and Assets

- `Assets/Scripts/Entities/Combat/CombatDefenseComponent.cs` — moved stance recovery before the poise-delay return.
- `Assets/Scripts/Tests/EnemyRuntime/CombatDefenseRecoveryTests.cs` — added focused recovery regression coverage.
- No serialized assets or tuning values changed.

### Decisions and Tradeoffs

- Applied the rule to every `CombatDefenseComponent`, including player and enemy users.
- Preserved the existing frame-level poise delay timing.
- Added no stance delay and no additional recovery gates.

### Validation Evidence

Unity Edit Mode validation passed 3/3 focused tests after a clean-scene preflight:

- stance recovers while poise delay remains active;
- critical opportunity suppresses stance recovery;
- poise recovery resumes on the tick after its delay expires.

Independent review found no material issue. Play Mode and broader suites were intentionally outside the approved scope.

### Documentation Updated

- [[Architecture/Systems/Hitbox System]]
- [[Work/Issues/Stance Recovery Is Gated By Poise Delay]]
- [[Work/Work Queue]]

### Follow-Up

None required for the approved scope.

Originating issue: [[Work/Issues/Stance Recovery Is Gated By Poise Delay]].
