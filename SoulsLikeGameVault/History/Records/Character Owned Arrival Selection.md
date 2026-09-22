---
title: Character Owned Arrival Selection
type: implementation-record
domains: [character, animation, save, lifecycle]
status: done
authority: historical
updated: 2026-09-09
aliases: []
tags:
  - history/change
---

# Character Owned Arrival Selection

## Implementation Record Contract

### Outcome

Character now chooses exactly one arrival presentation. World-position resumes play Spawn and unlock on Spawn Exit. Saved-grace and travel arrivals enter GraceRestIdle directly without issuing or resetting Spawn.

### Why

`Character.Initialize` previously triggered Spawn unconditionally while Core independently forced GraceRestIdle for grace arrivals. The earlier fix reset the pending Spawn trigger after creating that conflict. Save data also did not persist whether menu resume should return to grace rest or a normal world position.

### Changed Files and Assets

- Added `CharacterArrival` and `Character.BeginArrival`.
- Updated Character initialization, Core startup/quit, and the GraceRestIdle Animator adapter.
- Added backward-compatible `CharacterSpawnData.ResumesAtGrace` handling in `CharacterSpawnService`.
- Added focused Character arrival, Animator contract, Grace animation, and spawn persistence tests.
- No prefab, Animator Controller, scene, or other serialized runtime asset was modified.

### Decisions and Tradeoffs

- Preserved the feature rules: world position uses Spawn; grace uses direct GraceRestIdle.
- Reused existing input/movement protection and Animator callbacks.
- Added no async arrival workflow, timeout recovery, playback generations, new lock registry, same-state replay delay, or root-motion/layer policy.
- Missing callbacks remain authoring defects caught by focused controller validation; runtime code does not manufacture success.
- Old saves omit `ResumesAtGrace` and therefore retain their previous saved-position behavior.

### Validation Evidence

- Runtime and Editor C# assemblies: 0 build errors.
- Unity preflight: `TEST_READY`; `ElevatorDemo` open and clean.
- Focused asynchronous Edit Mode validation: 8 passed, 0 failed, 0 skipped.
- Independent review: no material runtime findings.
- Play Mode scene travel was not run under normal validation policy.

### Documentation Updated

- [[History/Completed Plans/Character Owned Animation Sequencing and Recovery]]
- [[Work/Work Queue]]

### Follow-Up

Action exit correlation and inactive-layer root-motion filtering remain deferred until a concrete gameplay failure is reproduced. They are not part of this arrival fix.
