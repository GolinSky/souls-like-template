# Ladders

Ladders are traversal entities with two interactable endpoints. They use the same
`IInteractable` discovery path as ground items and also register a `ViewEntity` and
`LadderView` with the entity system. Collider lookup resolves the ladder entity;
`IEntity.TryGetComponent<LadderView>` exposes its traversal data.

## Controls

The feature uses the existing character input bindings:

| Action | Keyboard/mouse |
| --- | --- |
| Enter at either end / deploy a locked shortcut from above | E |
| Climb up / down | W / S |
| Hold position | Release movement |
| Climb faster / slide down | Hold Space while moving up / down |
| Drop | Tap and release Space |
| Punch above | Left mouse button |
| Kick below | Shift + left mouse button |
| Use the selected healing flask | R |

Normal movement, weapon attacks, guarding, and equipment switching are suspended
while attached. Reaching an end starts the exit animation automatically.

## Authoring

The reusable asset is `Assets/Prefabs/Models/Ladder/Ladder.prefab`. Its initial
height is six metres. The ladder faces local +Z; the character climbs on its
local -Z side.

- `bottomMount` and `topMount` define the climbing line for the character's feet.
- `bottomExit` and `topExit` define safe standing positions on the two landings.
- Each `LadderEndpoint` has its own interaction anchor and trigger collider.
  Keep both endpoints beneath the ladder's root `ViewEntity`.
- `minOccupantSpacing` prevents climbers passing through one another.
- `deployRoot` contains the visible rails and rungs. Mount and exit markers stay
  outside it so deploying the mesh does not move the traversal path.

Position the exit markers on clear, walkable landings and keep the climbing line
clear of walls. Eligible enemies need reachable NavMesh at both approaches.
The prefab is supplied without a placement in the main level.

`LadderSystem` registers ladders already in loaded scenes and handles subsequent
scene loads. Code that instantiates a ladder during play must call
`LadderSystem.Register(ladderView)`; destruction unregisters it and releases its
occupants.

For a shortcut, enable `startsLocked`, assign a unique, nonempty `saveIdentifier`
to the scene instance, and configure the locked and unlocked local poses of
`deployRoot`. The default visual poses retract the ladder upward by 4.5 metres.
Unlocking from the top persists in the `UnlockedLadders` storage record. Keep the
identifier stable across sessions; duplicated shortcut instances need distinct
identifiers.

## Characters and animation

The player always supports ladder traversal. Enemy archetypes opt in through
`EnemyBehaviourProfile.CanUseLadders`; the Erika melee profile is enabled, while
the training dummy profiles retain the disabled default.

Both character prefabs carry `LadderClimber`. Their Animator Controllers have a
last, full-body override layer named `Ladder`, weighted only during traversal.
The layer groups the ladder states in a sub-state machine with an inert `Empty`
default. Runtime animation calls use short state names.

Project-owned clips are in `Assets/Art/Animation/Ladder/`. Climb, entry, exit,
idle, slide, drink, and shortcut deployment reuse available humanoid animation.
The punch and downward kick are provisional muscle animations derived from the
ladder hold pose. Preserve the state names when replacing them. Action timings
are matched to the supplied clips in `LadderClimber`; update those timings if
replacement clips have different durations or impact frames.
