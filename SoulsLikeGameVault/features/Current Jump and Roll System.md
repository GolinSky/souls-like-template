---
tags:
  - unity
  - soulslike
  - locomotion
  - jump
  - roll
status: implemented
---

# Current Jump and Roll System

> Implementation note for the current SoulsLikeTemplate movement system. This describes the code after the jump and roll rework, and calls out important differences from the design specification.

## Sources of truth

- Design reference: `C:\Users\golin\Downloads\elden_ring_like_jump_unity_spec.md`
- Movement authority: `Assets/Scripts/Components/Movement/MovementComponent.cs`
- Movement tuning: `Assets/Scripts/Components/Movement/MovementData.cs` and `Assets/Settings/Player/MovementData.asset`
- Input and action capture: `Assets/Scripts/Entities/Character/Character.cs`
- Action buffer: `Assets/Scripts/Entities/Character/CharacterActionBuffer.cs`
- Animation bridge: `Assets/Scripts/Components/Animator/AnimatorComponent.cs`
- State definitions: `Assets/Scripts/Components/Movement/LocomotionState.cs`

The deleted `Assets/Art/Animation/CharacterAnimator.controller` is not a source of truth. The live runtime controllers are:

- `NoWeaponAnimator.controller`
- `CharacterGreatSwordAnimator.controller`
- `CharacterGreatSwordLeftHandAnimator.controller`
- `CharacterGreatSwordDualWieldAnimator.controller`

## Ownership and update flow

1. `Character.UpdateBehaviour` reads the Input System and captures actions.
2. `CharacterActionBuffer` holds the most recent action until it can execute.
3. `MovementComponent` owns the CharacterController, horizontal and vertical velocity, gravity, ground probing, collision resolution, jump state, and roll state.
4. `AnimatorComponent` receives gameplay state and presents it; animation does not decide whether the character is grounded.

This keeps one authoritative grounded and movement owner, as required by the jump specification.

## Jump state machine

```text
Grounded
   │ jump accepted
   ▼
JumpStart
   │ vertical velocity reaches the apex threshold
   ▼
Airborne
   │ walkable contact while descending
   ├──────────────► Landing ─────► Grounded
   └ hard impact ► HardLanding ──► Grounded
```

If support is lost without a jump request, the character enters `Airborne` directly. A ledge fall therefore does not play the jump-start trigger.

### Jump acceptance and trajectory

- Jump is captured through the shared action buffer, not executed directly from the input edge.
- `TryStartJump` requires a grounded character, an unblocked movement component, and a completed jump cooldown.
- Takeoff vertical velocity is physics based: `sqrt(2 * JumpHeight * abs(Gravity))`.
- Current horizontal momentum is preserved at takeoff.
- Gravity updates vertical velocity every movement tick; there is no variable-height jump, double jump, or large coyote window.
- The ground probe is ignored briefly after takeoff so the initial jump is not cancelled by the capsule still overlapping the takeoff surface.
- Apex is detected from vertical velocity, not from animation timing.
- A minimum airborne time prevents an immediate same-frame landing.
- Landing requires walkable ground contact while descending. The impact speed is measured from the lowest downward velocity reached during the flight.
- Normal landing and hard landing are separate gameplay states. Hard landing is selected when downward impact speed reaches the hard-landing threshold and temporarily blocks movement through the animation state.
- A successful landing resets vertical velocity to a small grounded value rather than leaving residual falling velocity.

### Current jump tuning

| Setting | Current value | Purpose |
| --- | ---: | --- |
| Jump height | 1.2 m | Target vertical displacement |
| Gravity | -15 m/s^2 | Downward acceleration |
| Jump timeout | 0.50 s | Minimum time between jump starts |
| Air control | 0.25 | Fraction of ground steering authority in air |
| Air acceleration | 8 m/s^2 | Rate of horizontal air steering |
| Air rotation smooth time | 0.25 s | Free-mode facing response while airborne |
| Jump ground-ignore time | 0.12 s | Takeoff ground-probe suppression |
| Minimum airborne time | 0.08 s | Prevents instant re-landing |
| Apex threshold | 0.35 m/s | Switch from `JumpStart` to `Airborne` |
| Fall timeout | 0.10 s | Support-loss grace used for stairs and ledges |
| Hard landing speed | 12 m/s downward | Selects `HardLanding` |

## Jump animation contract

`AnimatorComponent` receives:

- `Grounded` bool
- `Jump` trigger
- `VerticalVelocity` float
- `LandingType` int (`Normal` or `Hard`)

The live controllers use those values to select jump start, the airborne loop, ledge fall, normal landing, and hard landing. The animation is a pose and timing layer; the movement component remains responsible for the physical arc and grounded state.

Roll and backstep root motion is applied as planar motion. Its animation Y delta is ignored so a roll cannot lift the CharacterController and leave the character falsely airborne. Collision flags are still resolved by `MovementComponent`.

## Roll and sprint input

`Sprint` and `Roll` share the same physical Space binding in `ProjectInputActions.inputactions`, so the action is disambiguated by hold duration:

- Press starts the sprint-hold timer.
- Holding for at least `0.30 s` qualifies the input as sprint.
- Releasing before the threshold captures a roll.
- Pressing Space while an existing roll is active sets `_rollPressedDuringRoll`; releasing it can then capture the next roll even though the previous animation is still finishing.

This release-based mapping is why a roll is not emitted on every key-down event. It is also the reason the active-roll press latch is needed when the player spams the shared key near the end of a roll.

### Roll execution

- A buffered roll stores the movement input and camera yaw from capture time.
- In free mode, a directional roll faces the resolved travel direction and the animation drives the displacement.
- With lock-on enabled, the character faces the target. Forward/back rolls move along the target radial direction, while left/right rolls orbit around the target.
- A neutral roll input becomes a backstep.
- `TryStartRoll` rejects blocked, ungrounded, or cooldown-locked starts. If the character is descending but already has valid walkable contact, it completes the landing first and then starts the roll. This closes the falling-after-roll edge case.
- The roll cooldown is currently `0.20 s`.

## Action buffering and animation interruption

`CharacterActionBuffer` is a one-slot buffer with a nominal `1.0 s` lifetime. A new action replaces the previous buffered action. While another action is active, the pending action is retained instead of expiring; it is consumed when the corresponding transition window becomes executable.

Roll execution can interrupt an active animation only when the animation transition window is open. A roll input captured during the current roll therefore waits for that window and starts the next roll instead of being silently discarded. Repeated inputs are not a multi-entry queue: the latest captured action wins.

The design specification describes a shorter 15–30 frame (approximately 250–500 ms) sliding buffer. The current implementation intentionally uses one retained slot and a 1 second timer, which makes spam input more forgiving but does not preserve a sequence of multiple actions.

## Free movement and lock-on behavior

- Free mode rotates toward travel direction. Airborne rotation follows horizontal momentum with a slower smoothing time, so the character does not snap instantly when the stick changes direction.
- Lock-on mode keeps combat-facing orientation toward the target while movement and rolls use target-relative directions.
- Air control uses `MoveTowards` with the air-control multiplier, so reversing direction is gradual rather than an instantaneous velocity replacement.

## Alignment and current boundaries

Implemented from the specification:

- One authoritative movement, gravity, collision, and grounded owner.
- Explicit `Grounded`, `JumpStart`, `Airborne`, `Landing`, and `HardLanding` states.
- Momentum-preserving takeoff and limited air steering.
- Physics-driven apex and landing severity.
- Ledge falls entering the fall loop without a jump-start animation.
- Target-facing lock-on behavior and travel-facing free movement.
- Buffered roll/jump actions and a roll-end interruption window.
- Planar roll root motion with controller collision resolution.

Current boundaries:

- No jump attack, fall-damage, or lower-body hurtbox system is implemented in the movement code.
- No variable-height jump, double jump, or explicit coyote-time mechanic is implemented.
- The current sprint/roll threshold is `0.30 s`; the specification describes approximately `0.25 s` at 60 FPS.
- The current action buffer is one slot with a 1 second retention policy, rather than a multi-command 15–30 frame queue.

## Useful verification points

- Inspect `CurrentLocomotionState`, `CurrentLandingType`, `VerticalVelocity`, and `Model.Grounded` in `MovementComponent` when diagnosing jump or landing behavior.
- Verify the active Animator controller is one of the four live controllers listed above.
- If a roll causes upward drift or `grounded` to become false, check that the animation movement path is applying planar delta only while the roll is active.
- If a spammed roll is missed, check the shared Space press/release sequence and `_rollPressedDuringRoll` before changing animation transitions.
