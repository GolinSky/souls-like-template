---
title: Entity Locator System
type: architecture
domains:
  - entity
  - communication
  - interaction
status: current
authority: required
updated: 2026-09-07
verified: 2026-09-07
source_commit: f7cf9aa3
context_keys:
  - entity-locator
aliases:
  - Entity Entity Locator
  - Entity Communication Rule
tags:
  - architecture/verified
  - architecture/required
---
# Entity Locator System

The Entity Locator is the required identity and discovery boundary for gameplay communication between game entities.

## Required Entity Communication Rule

> [!rule] Entity-to-Entity Communication
> Whenever gameplay code crosses an entity boundary, it **MUST** identify the other participant through `IEntityLocator` and communicate through the resolved `IEntity`.
>
> If the caller already owns a valid `IEntity` reference for itself, it may reuse that reference. Every other participant discovered by stable ID, `Collider`, `RaycastHit`, or entity type must be resolved through `IEntityLocator`.
>
> State-changing behavior must be invoked through a command registered on the target entity. Read-only access may use a registered `IEntityComponent` when no command is required.

This rule applies to combat, interaction, targeting, hazards, AI perception, and every other gameplay flow in which one entity addresses another.

## Lookup and Dispatch Contract

1. Every participating gameplay object has an `IEntity` identity and is registered with `IEntityLocator` for its active lifetime.
2. A scene object discovered through physics exposes an `IViewEntity` or `ViewEntity` in its parent hierarchy so the locator can map its collider or raycast hit to an entity ID.
3. The caller resolves the target with the appropriate locator API:
   - `TryGetEntity(Collider, out IEntity)`
   - `TryGetEntity(RaycastHit, out IEntity)`
   - `GetEntity(ulong id)`
   - `GetEntities(EntityType)`
4. The caller obtains the required capability from the resolved entity with `TryGetComponent<T>()`.
5. Mutating operations are executed by a target-owned `EntityCommand`; the source supplies request data or its own `IEntity` context.
6. Entity registration and removal follow the entity lifecycle. No second lookup registry may duplicate Entity Locator ownership.

## Examples

| Gameplay flow | Required route |
|---|---|
| Player hits enemy | Weapon contact resolves the defender through `IEntityLocator`; the defender's `ResolveMeleeHitCommand` resolves the attacker entity and applies the hit. |
| Player uses ladder | The interaction probe resolves the ladder collider through `IEntityLocator`; the ladder's `IInteractableCommand` / `LadderInteractCommand` receives the player `IEntity`. |
| Player picks up an item or uses grace/elevator | Resolve the interactable entity through the locator, then invoke its target-owned interact command. |
| AI targets or perceives another actor | Resolve or enumerate the target through `IEntityLocator`, then use capabilities registered on the returned `IEntity`. |
| Hazard affects an actor | Resolve the contacted actor through the locator, then dispatch the hazard request to the actor's registered command. |

## Boundaries and Prohibited Bypasses

Do not use any of the following to communicate across entity boundaries:

- `GetComponent*` on a foreign collider or scene object to discover gameplay identity.
- `Find*`, tags, object names, or ad-hoc global registries as an entity lookup path.
- Direct serialized references to another entity's mutable gameplay components.
- Direct mutation of a foreign entity's internal components when a target-owned command is responsible for the operation.

The Entity Locator rule does not apply to private collaboration among components already owned by the same `IEntity`, nor to immutable assets or services that are not game entities. Serialized child references may configure one entity internally, but they must not replace locator-based discovery of another entity.

## Verified Implementation References

- [`IEntityLocator`](../../Assets/Scripts/Entities/BaseEntity/IEntityLocator.cs) defines ID, type, collider, and raycast lookup.
- [`EntityLocator`](../../Assets/Scripts/Entities/BaseEntity/EntityLocator.cs) owns the runtime entity registry.
- [`MeleeHitboxController`](../../Assets/Scripts/Entities/Combat/MeleeHitboxController.cs) demonstrates player/enemy combat routing.
- [`InteractionController`](../../Assets/Scripts/Interactions/InteractionController.cs) demonstrates interactable discovery.
- [`LadderInteractCommand`](../../Assets/Scripts/Entities/BaseEntity/EntityCommands/LadderInteractCommand.cs) demonstrates target-owned ladder behavior.

The earlier [[../../Research/Interaction System Audit|Interaction System Audit]] remains evidence of the migration rationale; this note is the current required rule.
