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

> Implementation note for the current SoulsLikeTemplate movement and locomotion system. This document outlines the authoritative C# runtime architecture, movement state machines, and key differences from theoretical design specifications.

## Sources of truth

- **Movement Authority**: [`Assets/Scripts/Components/Movement/MovementComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Movement/MovementComponent.cs)
- **Movement Tuning**: [`Assets/Scripts/Components/Movement/MovementData.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Movement/MovementData.cs) and [`Assets/Settings/Player/MovementData.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Player/MovementData.asset)
- **Character Aggregate Facade**: [`Assets/Scripts/Entities/Character/Character.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Character.cs)
- **Input Adapter**: [`Assets/Scripts/Entities/Character/Input/PlayerInputReader.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Input/PlayerInputReader.cs)
- **Action State Machine & Buffer**: [`Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs) and [`CharacterAction.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterAction.cs)
- **Animation Presentation Bridge**: [`Assets/Scripts/Components/Animator/AnimatorComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Animator/AnimatorComponent.cs) and [`AnimatorRootMotionRelay.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Animator/AnimatorRootMotionRelay.cs)
- **Locomotion State Definitions**: [`Assets/Scripts/Components/Movement/LocomotionState.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Movement/LocomotionState.cs)
- **Contextual Attack Follow-ups**: [`Assets/Scripts/Components/Attack/AttackComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Attack/AttackComponent.cs)

### Active Runtime Controllers
The live runtime controllers are:
- `NoWeaponAnimator.controller`
- `CharacterGreatSwordAnimator.controller`
- `CharacterGreatSwordLeftHandAnimator.controller`
- `CharacterGreatSwordDualWieldAnimator.controller`

---

## Ownership and Update Flow

```mermaid
flowchart TD
    PIR["PlayerInputReader\n(Evaluates 0.3s Sprint Hold & Actions)"] -->|CharacterInput| PC["PlayerController"]
    PC -->|Tick(CharacterInput)| C["Character (Facade)"]
    C --> CASM["CharacterActionStateMachine\n(1-Slot Buffer, 1.0s Window)"]
    C -->|Move, Jump, Roll| MC["MovementComponent\n(CharacterController, Gravity, Probing)"]
    MC -.->|MovementPresentation Snapshot| C
    C -->|SetLocomotion, SetAirborneMotion| AC["AnimatorComponent"]
    AC -->|RootMotion / MovementBlocked Tags| RMR["AnimatorRootMotionRelay"]
    RMR -->|ApplyAnimationMovement| MC
    AC -->|QueueCheck / Exit SMB DTOs| C
    C -->|State Updates| CASM
```

1. `PlayerInputReader.Read` parses raw Unity Input System presses and camera yaw into a semantic [`CharacterInput`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterInput.cs) struct.
2. `PlayerController.Tick` delivers `CharacterInput` to `Character.Tick`.
3. `CharacterActionStateMachine` holds and dispatches actions (Roll, Jump, Attack, Equipment) with a 1-slot buffer.
4. `MovementComponent` owns the `CharacterController`, horizontal and vertical velocity, gravity, ground probing, collision resolution, jump state, and roll state.
5. `MovementComponent` produces an immutable `MovementPresentation` struct snapshot each frame, which `Character` pushes to `AnimatorComponent` and `CharacterAudioComponent`.

---

## Jump State Machine

```text
Grounded
   │ jump accepted (TryStartJump)
   ▼
JumpStart
   │ vertical velocity reaches apex threshold (<= 0.35 m/s)
   ▼
Airborne
   │ walkable contact while descending
   ├──────────────► Landing (Impact < 12 m/s) ─────► Grounded
   └ hard impact ► HardLanding (Impact >= 12 m/s) ──► Grounded
```

If support is lost without a jump request (e.g. running off a ledge), the character enters `Airborne` directly after the `FallTimeout` grace window expires. A ledge fall therefore does not play the jump-start trigger.

### Jump Acceptance and Trajectory

- **Buffer Execution**: Jump is submitted as [`CharacterAction.Jump`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterAction.cs) into `CharacterActionStateMachine`.
- **Preconditions**: `TryStartJump` requires grounded status (`Model.Grounded == true`), unblocked movement (`_movementLockReasons == MovementLockReason.None`), enough stamina (`JumpStaminaCost = 10`), and completed jump cooldown (`JumpTimeout = 0.5s`).
- **Takeoff Velocity**: Physics-calculated:
  $$v_{\text{takeoff}} = \sqrt{2 \cdot \text{JumpHeight} \cdot |\text{Gravity}|} = \sqrt{2 \cdot 1.2 \cdot 15} \approx 6.0\text{ m/s}$$
- **Momentum Preservation**: Current horizontal momentum at takeoff is preserved into the air.
- **Air Control**: Directional steering in mid-air uses `Vector3.MoveTowards` scaled by `AirAcceleration * AirControl` ($8.0 \cdot 0.25 = 2.0\text{ m/s}^2$).
- **Takeoff Probe Suppression**: Ground probing is suppressed for `JumpGroundIgnoreTime = 0.12s` or while $v_y > 0$ so the capsule does not immediately re-land on the takeoff ledge.
- **Apex Detection**: Evaluated from vertical velocity reaching `JumpApexThreshold = 0.35 m/s`, transitioning `LocomotionState.JumpStart` $\rightarrow$ `LocomotionState.Airborne`.
- **Landing Evaluation**: Requires downward vertical velocity ($v_y \le 0$), minimum airborne duration (`MinimumAirborneTime = 0.08s`), and walkable ground contact (`SphereCastNonAlloc` within slope limit).
- **Landing Severity**:
  - **Normal Landing** ($|v_y| < 12.0\text{ m/s}$): Sets `LandingType.Normal`, transitions to `Landing` and immediately recovers.
  - **Hard Landing** ($|v_y| \ge 12.0\text{ m/s}$): Sets `LandingType.Hard`, transitions to `HardLanding`.

### Current Jump Tuning (`MovementData.asset`)

| Setting | Current Value | Purpose |
|---|---:|---|
| **Jump Height** | `1.2 m` | Target vertical displacement |
| **Gravity** | `-15.0 m/s²` | Downward vertical acceleration |
| **Jump Timeout** | `0.50 s` | Cooldown timer between jump starts |
| **Air Control** | `0.25` | Authority multiplier for airborne horizontal steering |
| **Air Acceleration** | `8.0 m/s²` | Horizontal acceleration rate applied to air steering |
| **Air Rotation Smooth Time** | `0.25 s` | Facing response smoothing time while airborne |
| **Jump Ground-Ignore Time** | `0.12 s` | Takeoff ground-probe suppression window |
| **Minimum Airborne Time** | `0.08 s` | Prevents same-frame takeoff/landing glitches |
| **Jump Apex Threshold** | `0.35 m/s` | Transition velocity from `JumpStart` to `Airborne` |
| **Fall Timeout** | `0.10 s` | Grace timer before airborne state on stairs/ledges |
| **Hard Landing Min Fall Speed** | `12.0 m/s` | Impact speed threshold selecting `HardLanding` |
| **Jump Stamina Cost** | `10.0 pts` | Stamina consumed on jump takeoff |

---

## Jump Animation Contract

`AnimatorComponent` receives:
- `Grounded` (bool)
- `Jump` (trigger)
- `VerticalVelocity` (float)
- `LandingType` (int: `0 = None`, `1 = Normal`, `2 = Hard`)

The live controllers use these values to drive jump takeoff, the airborne loop, falling blend trees, normal landings, and hard landing stumbles. The animation layer acts as a presentation sink; `MovementComponent` remains the sole authority for physical state, position, and velocity.

---

## Roll and Sprint Input

In [`PlayerInputReader.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Input/PlayerInputReader.cs), `Sprint` and `Roll` share the same physical button binding:
- **Press**: Starts the hold timer (`_sprintHoldTime = 0`).
- **Hold ($\ge 0.30\text{ s}$)**: Qualifies input as `SprintHeld = true`.
- **Release ($< 0.30\text{ s}$)**: Dispatches [`CharacterAction.Roll(moveInput, cameraYaw)`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterAction.cs).

### Roll Execution

1. **Preconditions**: Grounded status, unblocked movement, sufficient stamina (`RollStaminaCost = 12.0`), and completed `RollCooldown = 0.20s` (or open animation cancel window).
2. **Direction Resolution**:
   - **Free-Aim Mode**: Character rotates to face `worldDirection` ($T_{\text{char}} \rightarrow \vec{D}$), and `rollDirection` is set to `Vector2.up`.
   - **Locked-On Mode**: Character faces the lock-on target. `QuantizeLockedRollDirection` clamps input to 4 cardinal bins (`Left`, `Right`, `Forward`, `Backward`).
   - **Neutral Input**: If $\|\vec{I}\| \le 0.01$, triggers **Backstep** (`rollDirection = Vector2.down`).
3. **Motion Application**:
   - Rolling animations use root motion tagged `"RootMotion"`.
   - `AnimatorRootMotionRelay` captures root delta. Planar motion is extracted (`planarDelta = Vector3(dx, 0, dz)`).
   - In Locked-On lateral rolls, `CalculateLockedRollDelta` converts linear root displacement into a circular orbit around the target:
     $$\Delta \theta = -\text{dir}_x \cdot \frac{\Delta d}{r} \cdot \frac{180}{\pi}$$
   - Vertical delta is zeroed during rolls to prevent false airborne detachment.
4. **Follow-up Attacks**:
   - In [`AttackComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Attack/AttackComponent.cs), rolling sets a 1.0s contextual attack window upon exit. Light attack within this window triggers `AttackType.RollingLightAttack` (or `AttackType.BackStepAttack` after a backstep).

---

## Action Buffering and Queue Windows

[`CharacterActionStateMachine`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs) implements a deterministic 1-slot action buffer:
- **Buffer Retention**: 1-slot with `BUFFER_DURATION_SECONDS = 1.0s`. Latest input overwrites any previously buffered action.
- **Queue Check Window**: When an animation reaches its `QueueCheck` normalized frame (via `AnimatorStateMachine` SMB), `Character` calls `TryExecuteBufferedAction(now)`.
- **Roll-to-Sprint Interrupt**: Holding sprint while rolling triggers `InterruptRollForSprint()` as soon as the `QueueCheck` window opens, breaking into a sprint without completing full recovery.
- **Chained Action Exit Suppression**: Chained attacks or rolls set `_ignoreNextActionExit = true` so the preceding animation's `Exit` signal does not prematurely pop the state machine back to `Neutral`.

---

## Current Boundaries and Non-Implemented Systems

1. **No Spatial Lower-Body Hurtbox Toggling**: The jump currently provides no lower-body ground sweep pass-through.
2. **No Weight-Tier i-Frame Scaling**: Rolls use standard root-motion clips without equipment-load i-frame branching.
3. **No Foot Placement IK**: Ground snapping is purely kinematic via `SphereCastNonAlloc` and `GroundSnapDistance = 0.35m`.
4. **Action Buffer Capacity**: Uses a 1-slot 1.0s buffer rather than a multi-command sliding frame queue.
5. **Crouch Attack Aliasing**: Crouch does not automatically alias light attacks to rolling attacks; attacks from crouch execute normal light attacks while crouched.
