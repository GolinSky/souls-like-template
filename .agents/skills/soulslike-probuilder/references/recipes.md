# Geometry and project measurements

Use metres, +Y up, and +Z forward. Give every element a stable ID. A room or
corridor is a composition of bounded elements, not a separate procedural-world
system. Resolve the actual field/pivot contract from `command-contract.md`
before writing a specification.

## Measured baseline (2026-09-08)

| Source | Settings |
| --- | --- |
| `Assets/Prefabs/Models/Character/Character.prefab` | CharacterController height 1.8, radius 0.28, center (0,1,0), slope 45 degrees, step 0.30, skin 0.08 |
| `Assets/Settings/Player/MovementData.asset` | Ground radius 0.28, offset -0.2, snap 0.35, jump height 1.2; ground mask excludes Stairs |
| `Assets/Prefabs/Models/Enemy/ErikaMeleeEnemy.prefab` | Controller height 1.8, radius 0.32, step 0.4; NavMeshAgent height 1.8, radius 0.32, stopping distance 0.85 |
| `Assets/Prefabs/View/Camera/Gameplay Camera.prefab` | Camera distance 3.0, shoulder (1,0.48,0), arm -0.31, collision radius 0.15, FOV 48 degrees; obstacle mask Default and Player |
| `Assets/Settings/Data/CameraData.asset` | Lock camera distance 3.3 |
| `Assets/Settings/Data/LayerData.asset` | Navigation bake includes Default, Walkable and Stairs |

Reinspect these assets when the task depends on changed movement or camera
behavior. These values are constraints, not proof of adequate combat space.

## Composition choices

- Floors/platforms and walls: mesh dimensions carry the requested size; keep
  transform scale at one. Use Default-layer static colliders for the initial
  fixture so both camera obstruction and movement ground queries see them.
- Doorway: two solid jambs and a lintel. Keep the collision opening clear;
  never use an enclosing BoxCollider or convex hull around the complete door.
- Straight stairs: choose rise below the player's 0.30 step offset. The initial
  target is 0.25 rise. Keep visual steps and movement colliders explicit.
- Ramp: a solid wedge ascending along the documented local direction; use a
  non-convex static mesh collider. A target slope around 20–30 degrees leaves
  margin below the 45-degree controller limit, but requires later gameplay
  verification.
- Acceptance layout: arena, corridor, doorway, stair to platform, ramp return,
  and a loop to the entrance. Use a 2 m corridor and 2.4 m doorway height as
  provisional authoring values. The camera's offset/distance still needs real
  gameplay checking. Mark spawn and camera positions as placeholders.

Resolve material roles to shared real assets before mutation. Initial gray role:
`Assets/ThirdParty/Jorjouto/ACS/Sample/SampleCharacter/Materials/Mat_Gray_HDRP.mat`.
Do not create a material for each element or change global lighting settings.

## Topology editing boundary

The initial builder needs boxes, segmented doorways, stairs and ramps only.
Face extrusion, bevels, Boolean CSG and subdivision are not implied. If later
extended, inspect topology first, select faces by semantic local/world normals
where possible, and reject indexes from a stale topology revision. Instance IDs
are not persistent element identity. Whole-object material assignment is
different from per-face submesh assignment.
