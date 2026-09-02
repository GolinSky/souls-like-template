# Enemy Combat & AI Systems

This document outlines the architecture, lifecycles, timing, and authoring contracts implemented for enemy combat and AI within the SoulsLike framework.

---

## 1. Critical Reaction Animator Routing (E-02)

### Problem & Design
Previously, enemy critical reactions were initiated via code-driven string crossfades (`CrossFadeInFixedTime`), bypassing the project's root `AnyState` trigger routing. This made C# code responsible for Animator graph navigation and broke consistency with the project's sub-state machine architecture.

### Implementation Contract
- **Trigger-Based Ingress**: Critical states are triggered semantically via Animator parameters:
  - `CriticalHitOneHand`
  - `CriticalHitOneHandDie`
  - `CriticalHitTwoHand`
  - `CriticalHitTwoHandDie`
- **AnyState Ingress Transitions**: Configured in the root Animator controller with a duration of `0.05s` (fixed duration, no exit time, `canTransitionToSelf = false`) routing into the `Combat` sub-state machine.
- **Relay Behaviours**: Attached `EnemyCriticalVictimStateBehaviour` to all critical states to notify `EnemyActionExecutor`:
  - `OnStateEnter` -> `ReportCriticalVictimEntered(isLethal)`
  - `OnStateExit` -> `ReportCriticalVictimExited(isLethal)`
- **Transient Defense Locking**: `_defense.SetCriticalState(true)` locks out new attacks/reactions while active and halts root motion.

---

## 2. Prone & Get-Up Recovery Lifecycle (E-03)

### Problem & Design
Non-lethal critical reactions had no recovery state, snapping instantly back to `Locomotion` upon animation exit. Furthermore, invulnerability flags were coupled to grace invulnerability (`_isGraceInvulnerable`), risking state leaks.

### Implementation Contract
- **Lifecycle Flow**:
  Combat -> Critical Reaction -> CriticalVictim (Prone) -> GetUp Trigger -> GetUp (Rising) -> Exit -> Locomotion
- **Execution Mode**: Extended `EnemyExecutionMode` with `GetUp`. `BlocksDecisions` evaluates to `true` during `GetUp`, ensuring AI decisions cannot execute while rising.
- **Decoupled Recovery Invulnerability**:
  - Added `SetRecoveryInvulnerable(bool)` to `IHealthComponent` and `HealthComponent`.
  - Channel separation guarantees independence:
    `IsInvulnerable = IsGraceInvulnerable || IsRecoveryInvulnerable || IsCheatInvulnerable`
- **Authored State & Egress**:
  - `GetUp` state placed in `Combat` sub-state machine using non-looping `Floor_Stand_Up_1` clip (2.0s).
  - Attached `EnemyGetUpStateBehaviour` to toggle recovery invulnerability via `ReportGetUpEntered()` and `ReportGetUpExited()`.
  - Transition from `CriticalHit` states to `GetUp` is conditioned on the `GetUp` trigger with an exit time of 0.90 to guarantee the fall finishes.
  - Egress from `GetUp` routes directly to `(Exit)` with exit time 0.95 and transition duration 0.08s, returning cleanly to the layer default `Locomotion`.
- **Transient Protection Cleanup**: `ClearTransientProtection()` reliably clears recovery invulnerability, critical flags, and triggers on interruption, death, or despawn.

---

## 3. Single-Animation Multi-Hit Attack Support (E-01)

### Problem & Design
The legacy combat model assumed `one Animator state -> one active interval -> one attack instance`. Animations featuring multiple weapon swings (e.g. `Combo3`) opened the hitbox once; the first hit added the player to `MeleeHitboxController._hitEntityIds`, preventing all subsequent swings in the same clip from dealing damage.

### Implementation Contract
- **Per-Hit Combat Data**:
  - Created `CharacterActionHitDefinition` (serializable struct) with `damageMultiplier`, `guardDamage`, `poiseDamage`, `stanceDamage`, `impactLevel`, `canBeBlocked`, and `canBeParried`.
  - Added optional `hitDefinitions` array to `CharacterActionDefinition`.
  - **Backward Compatibility**: If `hitDefinitions` is empty or null, `GetHitDefinition(int hitIndex)` automatically returns base action values.
- **Hitbox Retriggering**:
  - Added `ReportActiveStarted(CharacterActionId actionId, int hitIndex)` to `EnemyActionExecutor`.
  - Re-opening the hitbox via `MeleeHitboxController.Open()` increments `_attackInstanceId` and clears `_hitEntityIds`, allowing every swing to resolve as a fresh attack instance against previously hit targets.
- **Multi-Hit State Machine Behaviour**:
  - Created `EnemyMultiHitActionStateBehaviour` supporting an ordered array of `HitWindow` structs:
    - `hitIndex`
    - `activeStart` / `activeEnd`
    - `hasTrackingWindow` / `trackingStart` / `trackingEnd`
  - Independently drives tracking and active windows for each swing.
  - Forcibly closes hitbox, tracking, combo windows, and hyper-armor on state exit.
- **Vertical Slice Configuration (`Combo3`)**:
  - Configured `Combo3.asset` with 3 scaled hits:
    - Hit 0: 0.6x dmg, 15 guard, 15 poise, 10 stance, Light impact
    - Hit 1: 0.7x dmg, 18 guard, 18 poise, 12 stance, Light impact
    - Hit 2: 1.2x dmg, 35 guard, 40 poise, 25 stance, Medium impact
  - Migrated `Combo3` state in `ErikaGreatSwordEnemy.controller` to `EnemyMultiHitActionStateBehaviour` with 3 non-overlapping windows matching `1Hand_Up_Skill_4` swings:
    - Window 0: active [0.15, 0.28], tracking [0.00, 0.14]
    - Window 1: active [0.32, 0.44], tracking [0.28, 0.31]
    - Window 2: active [0.48, 0.60], tracking [0.44, 0.47]
    - Recovery start: 0.65

---

## 4. Timing Architecture & Decision Pacing (E-08)

### Problem & Design
`EnemyController._waitUntil` was an overloaded timer used simultaneously for patrol dwell, circling commitment, post-action recovery, and defensive stun locks. It was being pushed forward every frame while decisions were blocked, causing post-animation hesitation to scale with animation length.

### Implementation Contract
- **Elimination of `_waitUntil`**: Removed entirely from `EnemyController.cs`.
- **Dedicated Timers**:
  - `_patrolWaitUntil`: Exclusively tracks dwell waiting at patrol waypoints, configured via `PatrolWaitSeconds`.
  - `_postActionDecisionUntil`: Exclusively gates combat decisions and attack initiation following action completion or interruption, configured via `PostActionDecisionDelaySeconds`.
  - `_nextDecisionTime`: Continues to govern continuous decision tick evaluation (`DecisionIntervalSeconds`).
- **Elimination of Frame-by-Frame Timer Inflation**:
  - Removed `_waitUntil` assignment from defensive stun checks (`IsInCriticalState`, `IsParryStunned`, `IsGuardBroken`) and `_executor.BlocksDecisions`.
  - Blocked branches now stop the motor, zero locomotion, and return without modifying future decision delays.
- **Single-Shot Post-Action Pacing**:
  - `OnActionCompleted` sets `_postActionDecisionUntil = Time.time + PostActionDecisionDelaySeconds` **only** when `_executor.Mode == EnemyExecutionMode.Locomotion`. Intermediate combo moves (where `Mode == Action`) do not incur recovery delays.
  - `OnActionInterrupted` sets `_postActionDecisionUntil` once upon interruption.
- **Combat Decision Gates**:
  - `DecideCombat` and `CanStartAttack` evaluate `now < _postActionDecisionUntil`.
  - Retreat and `EnterGoal(Combat)` reset `_postActionDecisionUntil = 0f;`.
  - Circling (`CircleCombatTarget`) no longer modifies decision delays; strafing is evaluated naturally at decision ticks.
- **Profile Migration**:
  - Renamed `waitSeconds` to `postActionDecisionDelaySeconds` in `EnemyBehaviourProfile.cs` using `[FormerlySerializedAs("waitSeconds")]`.
  - Maintained backward-compatible `WaitSeconds` property.
  - Reserialized all behaviour profile assets (`ErikaMeleeBehaviour.asset`, `BackstabDummyBehaviour.asset`, `RiposteDummyBehaviour.asset`) with zero data loss.

---

## 5. Authoring Validation & Regression Coverage

### Validation Rules (`EnemyAuthoringValidator.cs`)
- **Critical & Recovery**:
  - Verifies presence of all 5 critical trigger parameters (`CriticalHitOneHand`, `CriticalHitOneHandDie`, `CriticalHitTwoHand`, `CriticalHitTwoHandDie`, `GetUp`).
  - Verifies all 4 critical states have `EnemyCriticalVictimStateBehaviour`.
  - Verifies `GetUp` state exists, has `EnemyGetUpStateBehaviour`, and has an outgoing transition.
- **Action State Behaviours**:
  - Accepts either `EnemyActionStateBehaviour` or `EnemyMultiHitActionStateBehaviour`.
  - Rejects states attaching both single-hit and multi-hit behaviours simultaneously.
  - Enforces at least 2 hit windows for multi-hit states, strictly non-overlapping active intervals, valid [0, 1] timing bounds, and valid `hitIndex` referencing.

### Test Suite (`Assets/Scripts/Tests/EnemyRuntime/`)
- `EnemyExecutionModeTests.cs`: Verifies `BlocksDecisions` is true for `GetUp`.
- `EnemyCriticalLifecycleTests.cs`: Verifies independent recovery invulnerability channel and get-up state transitions.
- `EnemyMultiHitActionTests.cs`: Verifies fallback/indexed hit definitions, indexed executor methods, and multi-hit timing serialization.
- `EnemyTimingTests.cs`: Verifies complete absence of `_waitUntil`, presence of dedicated timers, and combo-aware post-action delay gating.
