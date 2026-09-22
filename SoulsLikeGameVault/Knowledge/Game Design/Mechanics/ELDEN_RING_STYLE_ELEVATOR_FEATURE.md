# Elden Ring–Style Elevator Feature

## Overview

An elevator is a **moving platform connecting two vertically separated areas**. It usually serves as a shortcut, progression gate, or environmental hazard.

## Player Interaction

- Step on the platform’s **pressure plate** to start movement.
- The elevator travels between fixed stops, usually **bottom ↔ top**.
- When the elevator is on another floor, use a nearby **call lever** to summon it.
- The player can activate the plate and quickly step off, sending the elevator away.
- The open shaft remains dangerous: the player can fall into it and take damage or die.
- Movement, combat, healing, and other actions remain available while riding.

## Elevator Logic

Basic states:

```text
At Bottom
Moving Up
At Top
Moving Down
```

Rules:

1. A pressure plate only starts the elevator while it is stationary.
2. During movement, additional activation is ignored.
3. The platform accelerates, moves at a stable speed, and slows near the destination.
4. Characters standing on the platform move together with it.
5. At the destination, the pressure plate resets and can be activated again.
6. A call lever moves the elevator toward the floor where the lever was used.

## Shortcut Behavior

Many elevators are initially accessible from only one side.

Example:

```text
Player explores the long route
→ reaches the upper elevator entrance
→ activates the elevator
→ elevator now connects the upper area with an earlier checkpoint
→ shortcut is permanently available
```

Call levers can remain disabled until the elevator has been discovered or activated for the first time.

## Enemies

- Enemies standing on the platform can be carried by it.
- Enemies normally do not intentionally use call levers.
- AI should not require elevators to continue chasing the player.
- NavMesh links between floors should be disabled while the elevator is absent or moving.
- An enemy that walks onto an active pressure plate may trigger it, depending on the desired design.

## Presentation

The elevator should include:

- Pressure-plate depression animation.
- Lever interaction animation.
- Heavy mechanical or stone movement audio.
- Start and stop impact sounds.
- Small camera shake or controller vibration.
- Dust, sparks, chains, gears, or magical effects depending on the elevator type.
- Clear visual indication when a lever cannot currently be used.

## Special Elevator Types

### Standard Lift

A reusable two-floor shortcut.

### Progression Lift

Requires a key item, medallion, lever, or story condition.

### One-Way Unlock Lift

Cannot be called until activated from the far side.

### Large Cinematic Lift

Uses a longer interaction, character animation, camera sequence, and loading or area-transition logic.

## Core Design Principle

The elevator is not only transportation—it creates shortcuts, controls progression, and leaves a dangerous open shaft that the player can manipulate.
