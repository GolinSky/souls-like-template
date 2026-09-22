---
title: Standard Elevator Implementation
type: implementation-record
domains:
  - interaction
  - locomotion
status: done
authority: historical
updated: 2026-09-07
tags:
  - history/change
---

# Standard Elevator Implementation

## Implementation Record Contract

### Outcome

Implemented the standard two-floor lift from [[Knowledge/Game Design/Mechanics/ELDEN_RING_STYLE_ELEVATOR_FEATURE]] as a reusable prefab and separate playable demo. Pressure plates activate automatically; call levers use the existing interaction controller. Both resolve registered entities through `IEntityLocator` and the existing command contracts.

### Why

The user requested execution of the feature note, explicitly required the existing interaction system and entity locator, and selected a reusable prefab with a separate demo scene.

### Changed Files and Assets

- `Assets/Scripts/Entities/Elevator/`: motion, view, endpoint, world-owned system, rider interface, and shaft hazard.
- `ElevatorInteractCommand`, `EntityType`, and `CoreScope`: existing interaction and DI integration.
- `MovementComponent`, `Character`, and `EnemyNavigationMotor`: support detection and platform displacement without actor parenting or action locking.
- `GameOrchestrator`, `CoreGameOrchestrator`, and `CharacterSpawnService`: direct gameplay startup and unregistered-scene respawn through the scene's existing spawn provider.
- `Assets/Prefabs/Models/Elevator/Elevator.prefab`, its materials/audio, and `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity` with three saved scoped NavMesh assets.
- `Assets/Scripts/Editor/Tests/Elevator/`: motion and interaction/lifetime tests.

### Decisions and Tradeoffs

The world owns an accepted journey, so leaving the plate or cancelling the caller does not cancel travel. `ILateTickable` samples currently supported riders after their normal movement. Upward displacement moves riders before the deck; downward displacement moves the deck first. Enemy arrival synchronizes only the current supported sample.

Locking is optional and persists through the storage registry. The supplied standard prefab starts unlocked; enable `startsLocked` and assign a unique stable identifier for a shortcut. Enemies can ride; endpoint enemy activation is separately configurable.

Kinematic sensor interpolation is disabled because interpolation caused the plate to drift beneath its moving parent. The upper lintel leaves three metres of headroom. Navigation is baked on fixed dock islands with gated links, avoiding moving NavMesh data. Mechanical audio, dust, plate/lever animation, unavailable indicators, and a local Cinemachine impulse provide feedback.

### Validation Evidence

- Runtime and Editor C# builds: zero errors; runtime build repeated after the respawn fix.
- Unity EditMode elevator tests: 4/4 passed. CharacterRuntime: 14/14 passed.
- EnemyRuntime: 45/55 passed. Eight failures reference `CharacterActionId` in `Assembly-CSharp` although the unchanged enum belongs to `SoulsLike.Character.Runtime`; two additional failures are null references in unchanged critical-lifecycle/timing code or fixtures. No baseline run was made, so these are not claimed as proven pre-existing failures.
- Actual PlayMode pressure-plate entry carried the player from approximately y=-0.020 to y=7.974 and back. The plate retained local y=0.040. Staying on the plate at arrival did not restart movement; exiting and re-entering did.
- The live `InteractionController` selected `TopLever`, displayed `Operate elevator`, and executed `ElevatorInteractCommand` through its existing interaction path; the empty lift arrived at the top landing.
- An in-motion jump reported `moving=True`, `supported=False`, positive vertical velocity, and `inputBlocked=False`.
- Disabling the lift removed its endpoint from the locator; re-enabling registered a fresh identity.
- Actual shaft falling reduced health to zero. After the respawn fix, the player returned to the demo spawn at (0,-0.02,-5), health 300, input available, with no new errors.
- Persisted navigation was verified after closing/reopening the demo: 48 vertices and valid bottom, top, and walkway samples.
- A temporary locator-registered enemy entity using the real `EnemyNavigationMotor`, CharacterController, and NavMeshAgent was supported at the bottom, carried to (1.400,8.009,0), and remained on the upper NavMesh. This validates the motor integration, not a full combat/AI pursuit scenario.
- Final PlayMode run had no new console errors. Prefab, scene, and navigation assets were saved through Unity. The original ten clean DefaultLocation scenes were restored, with DefaultLocation active.

### Documentation Updated

[[Knowledge/Architecture/Systems/Elevator System]] documents ownership, authoring, sensor physics, navigation rebaking, and demo startup.

### Follow-Up

The broader enemy test failures require a separate investigation. Cinematic lifts, key-item gates, and loading-transition variants remain outside this standard-lift scope.
