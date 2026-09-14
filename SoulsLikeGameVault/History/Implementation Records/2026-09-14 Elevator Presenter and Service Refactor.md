---
title: Elevator Presenter and Service Refactor
type: implementation-record
domains:
  - interaction
  - locomotion
  - camera
status: done
authority: historical
updated: 2026-09-14
tags:
  - history/change
---

# Elevator Presenter and Service Refactor

## Outcome

One `ElevatorSystem` implements `IElevatorPresenter`, owns each elevator's model, and requests camera impulses through `ICameraService`. Views retain authored references and presentation. Root and control identities register through `IEntityLocator`; rider movement uses the target-owned `PlatformRideCommand`.

## Why

The previous view held movement and unlock logic and invoked Cinemachine through serialized UnityEvents. The system also scanned scenes and discovered Unity components directly. The user requested a small refactor respecting presenter, service, and entity-command ownership.

## Changed Files and Assets

- Elevator view/system/endpoint, presenter/model, and interaction command.
- Platform ride command and its character/enemy factory registrations.
- Camera service/interface and explicit `CoreScope.elevatorViews` registration.
- `Elevator.prefab`, `CameraService.prefab`, and `ElevatorDemo.unity`, saved and verified through Unity.
- Focused Elevator tests and the existing camera-service test fake.

## Decisions and Tradeoffs

The system uses one active view-to-model dictionary. Separate per-elevator controllers and scene callbacks are unnecessary. Pressure plates, call levers, and shaft hazards retain their distinct input/damage roles. Unlock identifiers and existing rider movement implementations are preserved. Views require presenter binding before startup; disable/re-enable removes and recreates entity identities. Unregister cleanup docks the platform and stops presentation without a camera impulse.

## Validation Evidence

- Baseline: 4 of 4 Elevator Edit Mode tests passed.
- Final: 8 of 8 passed, with no failures, skipped, or inconclusive tests; duration 0.41 seconds.
- Official Unity CLI/Pipeline used explicit editor mode, the `SoulsLike.Editor.Tests.Elevator` filter, asynchronous execution, and a 120-second caller budget.
- Unity 6000.3.11f1 / UTF 1.6.0. Final runner inactive, compilation idle, Play Mode stopped, and ElevatorDemo clean. Earlier transient compilation messages remained in the Console history; no current validation errors appeared.
- Live reflection verified the new compiled fields and lifecycle methods. Saved assets verified root/control references, one scope registration, and removal of the old impulse source and UnityEvent payloads.
- Source/Markdown whitespace check passed. Independent source and asset review found no remaining material issues.

## Documentation Updated

`AGENTS.md` now prohibits project-authored gameplay/presentation UnityEvents and states presenter/service ownership. [[Architecture/Systems/Elevator System]] describes the current registration, camera, and rider routes.

## Follow-Up

Play Mode movement, jump/step-off, enemy riding, and subjective camera feel were not executed in this validation pass. The Editor tests validate logic and lifecycle callbacks without entering Play Mode.
