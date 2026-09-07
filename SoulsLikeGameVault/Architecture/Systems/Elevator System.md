---
title: Elevator System
type: architecture
domains:
  - interaction
  - locomotion
status: current
tags:
  - architecture/elevator
---

# Elevator System

The standard elevator connects two fixed landings. Its pressure plate sends it to the other landing; a call lever summons it to the lever's landing. Requests during travel are ignored. The reusable asset is `Assets/Prefabs/Models/Elevator/Elevator.prefab`; the separate demonstration is `Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity`.

## Interaction and Ownership

Elevator endpoints use the existing `ViewEntity`, `Entity`, and `IInteractableCommand` contracts. `ElevatorSystem` registers them with the shared `IEntityLocator` and owns their scene lifetime. Call levers are discovered by the existing `InteractionController`. The pressure plate resolves the entering actor and its own command through the locator and invokes that command automatically.

The lift owns travel after accepting a request. Walking away from a lever or stepping off the plate does not cancel the journey. An occupied plate must be exited before it can initiate another journey; remaining on the plate at arrival must not send the elevator back automatically.

## Rider Movement

The elevator system updates in `ILateTickable`, after the player and enemy controllers. It samples support at the platform's old position, resolves actors through the locator, and applies the platform's displacement through `IPlatformRiderMotor`. Actors remain unparented and retain their normal action state. A jump or step off during the actor's update therefore removes support before the platform moves.

The character facade delegates displacement to `MovementComponent`; enemy displacement also synchronizes the navigation agent. Each landing needs compatible baked navigation for enemy arrival. Navigation links are available only at the landing where the platform is stationary.

## Shortcut and Shaft

Optional first-use locking disables call levers until the lift is first activated from its accessible side. Unlocks use the existing storage registry and a stable, unique save identifier per placed lift.

The platform leaves a real opening when absent. The shaft hazard resolves falling actors through `IEntityLocator` and uses the existing damage command. It does not introduce a general fall-damage system.

## Authoring and Validation

Place the prefab with its root stationary. `platform` is the moving child; `bottomDock` and `topDock` specify its world positions at the stops. The supplied demo travels from zero to eight metres. `maxSpeed` and `acceleration` produce a triangular profile for short journeys and an acceleration/cruise/deceleration profile for longer ones.

Keep the complete rider volume clear along the shaft, including above the upper stop. The supplied overhead lintel has three metres of clearance above the upper deck; it must not become a fixed floor covering the shaft.

`platformSupportCollider` is the solid deck. `riderDetectionVolume` is a separate trigger immediately above it. Keep both attached to the moving platform. The plate trigger uses the Walkable layer; the lever triggers use the Interaction layer so the existing interaction probe finds them. Each endpoint has its own `ViewEntity` and is listed in `endpoints`. The plate and shaft hazard have kinematic Rigidbodies with interpolation disabled. Interpolation must remain disabled for the transform-driven plate: physics interpolation otherwise overwrites its position relative to the moving parent. The plate sensor extends above the deck to overlap the character capsule reliably despite its skin width.

Set `startsLocked` and a unique `saveIdentifier` to enable a first-use shortcut. The pressure plate unlocks the call levers on accepted activation. Levers that are locked, busy, or already at their floor reject requests. Enemies may activate a plate when its `allowEnemyActivation` option is enabled.

The demo has separate navigation data for the landing geometry and each dock island, under its own `NavMeshData` folder. Access to the dock islands is controlled by the corresponding `bottomNavMeshLinks` and `topNavMeshLinks` arrays. A level using the prefab must provide compatible landing navigation and position those link endpoints for its geometry.

The demo's dedicated bake proxies are triggers during gameplay to avoid creating invisible solid floors across the shaft. To rebuild these physics-collider surfaces, temporarily make the proxies solid, build and save each scoped NavMesh, then restore the proxies to triggers and save the scene. Trigger-only sources produce empty navigation data. Validate the saved result after reopening the scene; the supplied bake has 48 triangulation vertices and valid samples at both dock islands and the lower walkway.

The prefab includes spatial mechanical audio, plate/lever movement, dust, and a small local Cinemachine impulse at travel start and stop. The demo gameplay camera includes the corresponding channel-1 impulse listener; other scenes need a listener to receive that feedback. Presentation events allow scene-specific feedback. The shaft hazard occupies the bottom of the pit and retries damage while an actor remains inside, so a brief invulnerability window does not leave a surviving actor permanently trapped there.

The source specification is [[Game Design/Mechanics/ELDEN_RING_STYLE_ELEVATOR_FEATURE]]. Cinematic lifts, key-item gates, and loading transitions are separate variants outside this standard-lift implementation. Validation evidence is recorded in [[History/Implementation Records/2026-09-07 Standard Elevator Implementation]].

The demo uses the existing gameplay scopes, character factory, input, camera, and interaction UI. `GameOrchestrator` preserves an already-loaded gameplay scene when a `CoreScope` is present. Startup without a gameplay scope retains the menu-loading behavior. When a directly opened gameplay scene is not registered in `SceneType`, respawn uses its `PlayerSpawnPositionProvider` without replacing the saved last grace. Registered scenes retain their existing grace-based respawn behavior.
