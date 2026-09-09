---
title: Character Owned Animation Sequencing and Recovery Phases 2-4
type: implementation-record
domains: [character, animation, equipment]
status: done
authority: historical
updated: 2026-09-09
aliases: []
tags:
  - history/change
---

# Character Owned Animation Sequencing and Recovery Phases 2-4

## Implementation Record Contract

### Outcome

Completed phases 2–4 of [[../../Work/Plans/Character Owned Animation Sequencing and Recovery]]. Removed the unsupported left-hand-primary animation route, made required Animator callbacks fail visibly when uninitialized, repaired stale hit-reaction enum serialization, and added focused controller contract and callback sequence validation. Phase 4 closed without a production routing/filtering change because valid controller traces reproduced no such defect.

### Why

The plan required evidence from supported controller execution before changing callback correlation, layer filtering, or root motion. The evidence found two content/configuration defects but confirmed the existing chaining and source-layer callback contracts.

### Changed Files and Assets

- Simplified `AnimationProfile`, `AnimatorComponent`, Character loadout routing, equipment compatibility, pause equipment filtering, and inventory bootstrap code for right-hand-only primary weapons.
- Changed `AnimatorStateMachine` to deliver Enter, Progress, QueueCheck, and Exit through its required receiver directly.
- Migrated `HitFront`, `HitBack`, `HitLeft`, and `HitRight` callbacks in `CharacterGreatSwordAnimator.controller` from stale enum value 21 to `HitReaction` value 22.
- Reserialized `StraightSwordAnimationProfile.asset` to remove the obsolete missing-GUID left-hand controller field.
- Added callback contract, callback trace, receiver initialization, and left-hand compatibility Edit Mode fixtures; updated arrival/grace fixtures for the fail-fast callback contract.

### Decisions and Tradeoffs

- Preserved the synchronized two-handed source-layer route and the full-body action callback source proven by the trace.
- Preserved same-state replay order: Enter, QueueCheck, old Exit, new Enter, QueueCheck, final Exit.
- Added no playback tokens, synthetic callbacks, runtime watchdogs, correlation filters, or root-motion layer filters.

### Validation Evidence

- Unity Test Safety preflight: `TEST_READY`; `ElevatorDemo` clean.
- Focused authored callback trace: 1 passed, 0 failed.
- Broad Edit Mode Animation suite: 43 passed, 0 failed, 0 skipped.
- `CharacterActionStateMachineTests`: 14 passed, 0 failed, 0 skipped.
- Controller and profile saved and force-reserialized through Unity; no import/serialization errors.
- Independent final review reported no material findings.

### Documentation Updated

- Completed the originating plan and removed it from the executable Work Queue.

### Follow-Up

Play Mode travel/gameplay coverage remains a separate validation phase under the repository test policy. `Locked Roll State Is Cleared Before Root Motion` remains separate work.
