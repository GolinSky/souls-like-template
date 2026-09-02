# Hitbox System Implementation Plan

## Purpose

Implement the design described in `ToDo/Hitbox System.md` for both the player Character and Enemy actors while preserving the useful parts of the current combat pipeline.

The existing implementation is a good contact layer, but it is not yet a complete hit-resolution system. Keep the current animation-timed weapon colliders and insert one shared resolver before health damage. Do not rebuild the combat stack.

The source Hitbox System note is not registered in `ai/Skill_Context_Index.md`. Treat it as advisory design input; live source and serialized assets remain authoritative.

## Existing System

### Player attack flow

```text
Character.Tick
  -> Character.StartAttack
  -> AttackComponent.ResolveAttack
  -> AnimatorComponent.PlayAttack
  -> PlayerMeleeAttackStateBehaviour
  -> PlayerMeleeCombatRelay
  -> MeleeHitboxController
  -> ApplyDamageCommand
  -> HealthComponent
  -> Character.OnDamageApplied
```

### Enemy attack flow

```text
EnemyBrain selects CharacterActionDefinition
  -> EnemyAnimationController.PlayAction
  -> EnemyActionStateBehaviour
  -> EnemyAnimationController.ReportActiveStarted
  -> MeleeHitboxController
  -> ApplyDamageCommand
  -> HealthComponent
  -> EnemyBrain.OnDamageApplied
```

### Existing code to preserve

- `Assets/Scripts/Entities/Combat/MeleeHitboxController.cs`
  - Enables and disables the trigger collider during active attack frames.
  - Rejects self and friendly contacts.
  - Deduplicates targets by entity ID.
- `Assets/Scripts/Entities/Combat/PlayerMeleeAttackStateBehaviour.cs`
  - Authors player hitbox windows using normalized animation time.
- `Assets/Scripts/Entities/Combat/PlayerMeleeCombatRelay.cs`
  - Resolves the equipped weapon, attack damage, and active weapon hitbox.
- `Assets/Scripts/Entities/Enemy/EnemyActionStateBehaviour.cs`
  - Authors enemy active, combo, and recovery windows.
- `Assets/Scripts/Entities/Enemy/EnemyAnimationController.cs`
  - Opens and closes the enemy hitbox and owns enemy action interruption.
- `Assets/Scripts/Components/Health/HealthComponent.cs`
  - Owns health, stamina, invulnerability, and death-related damage application.
- `Assets/Scripts/Entities/BaseEntity/EntityCommands/ApplyDamageCommand.cs`
  - Validates source and target and forwards health damage.

## Comparison Against the Target Design

| Area | Existing system | Required change |
|---|---|---|
| Animation windows | Player and enemy already open and close colliders from normalized animation time | Keep |
| Duplicate hits | `MeleeHitboxController` deduplicates by entity ID | Keep; one target hit per attack initially |
| Attack payload | Damage amount, hit point, hit zone, source, weapon, and action IDs | Add guard, poise, stance, impact, block/parry flags, and attack-instance ID |
| Resolution | Direct health damage | Add one shared priority resolver |
| Block/parry | Player input and animation state only | Make the state affect incoming contact |
| Hit reactions | One generic `Hit` trigger | Return direction and result type to both actor presentations |
| Backstab/riposte | Not implemented | Add one synchronized critical flow |
| Poise/stance/hyper armor | Not implemented | Add after the shared resolver is stable |

## Target Runtime Flow

```text
Animation window
  -> MeleeHitboxController
       contact + attack-instance dedupe only
  -> ResolveMeleeHitCommand
       result priority and defender state
  -> CombatDefenseComponent + HealthComponent
  -> MeleeHitResult
  -> defender presentation + attacker response
```

`MeleeHitboxController` must remain contact-focused. It must not decide block, parry, poise, stance, animation choice, or critical eligibility.

## Core Design

### New combat types

Create one top-level type per file:

- `HitDirection`
- `ImpactLevel`
- `MeleeAttackData`
- `MeleeHitRequest`
- `MeleeHitResultType`
- `MeleeHitResult`
- `CombatDefenseComponent`
- `ResolveMeleeHitCommand`
- `ParryWindowStateBehaviour`
- `CriticalAttackController`
- `CriticalDamageStateBehaviour`

### Data ownership

Do not add another ScriptableObject hierarchy.

- Player attack data is built from `WeaponDefinition + CombatProfile`.
- Enemy attack data is built from `WeaponDefinition + CharacterActionDefinition`.
- `MeleeAttackData` is the normalized runtime value passed to the hitbox.
- Preserve current weapon physical attack scaling and per-action multipliers.
- Add only the fields required by the current implementation phase.

Initial `MeleeAttackData` fields:

- Action ID
- Final health damage
- Guard damage
- Impact level
- Can be blocked
- Can be parried

Later fields:

- Poise damage
- Stance damage
- Hyper-armor interaction
- Push distance
- Block recoil
- Parry stun
- Maximum hits per target

Use `MaxHitsPerTarget = 1` as the initial invariant. Keep the existing target-ID `HashSet` until authored multi-hit attacks are required.

### Defender state

Add one actor-lifetime `CombatDefenseComponent` to both Character and Enemy. It owns only combat-defense state:

- Blocking state and guard angle
- Active parry window
- Current poise and maximum poise
- Current stance and maximum stance
- Hyper-armor bonus
- Whether the actor can currently be interrupted
- Current critical opportunity
- Whether the actor is already in hit, stun, critical, or death state
- An `OnHitResolved` event for presentation

Keep health, stamina, invulnerability, and death application in `HealthComponent`.

### Hitbox changes

Change `MeleeHitboxController.Open` to accept `MeleeAttackData`.

For each open:

1. Increment an internal attack-instance ID.
2. Store the normalized attack data.
3. Clear the processed target-ID set.
4. Enable the trigger collider.

For contact:

1. Resolve the target entity.
2. Reject self, friendly, invalid, dead, or already processed targets.
3. Build `MeleeHitRequest` with attacker position, contact point, IDs, attack instance, and attack data.
4. Invoke `ResolveMeleeHitCommand` on the target.
5. Mark a valid contacted entity as processed even when the result is invulnerable, blocked, or parried.
6. Publish the returned result to the attacker-side relay/controller.

Rename `OnHitConfirmed` to a typed `OnHitResolved` event.

## Result Priority

Resolve exactly one result in this order:

1. Invalid, self, friendly, dead, or repeated contact -> `Ignored`.
2. An established critical flow has normal hitboxes disabled and does not enter normal resolution.
3. Invulnerable -> `Invulnerable`.
4. Active valid parry -> `Parried`.
5. Valid guard -> `Blocked` or `GuardBroken`.
6. Normal health hit.
7. If the defender survives: stance break takes priority over poise stagger, which takes priority over no stagger.
8. Death suppresses all non-death reactions.

A rear normal contact is `HitFromBack`; it never automatically becomes a backstab.

## Hit Direction

Calculate the attacker position in defender-local space and select the dominant axis:

| Local source direction | Result |
|---|---|
| Dominant positive Z | Front |
| Dominant negative Z | Back |
| Dominant positive X | Right |
| Dominant negative X | Left |

Reaction names describe where the attack came from.

## Delivery Phases

### Phase 1: Shared resolution and directional reactions

Code:

1. Add the initial hit contracts, `CombatDefenseComponent`, and `ResolveMeleeHitCommand`.
2. Change `MeleeHitboxController` to pass normalized attack data instead of direct raw health damage.
3. Update `PlayerMeleeCombatRelay` and `EnemyAnimationController` to construct the same runtime attack data.
4. Register the defense component and resolver command in `CharacterFactory` and `EnemyFactory`.
5. Move hit presentation from generic positive-health callbacks to `MeleeHitResult`.
6. Keep existing health events for HUD, death, audio, and persistence.
7. Add explicit player attack cancellation that closes the active weapon hitbox; enemy cancellation continues through `EnemyAnimationController.Interrupt`.

Assets:

- Add/configure `CombatDefenseComponent` on:
  - `Assets/Prefabs/Character/Character.prefab`
  - `Assets/Prefabs/Enemy/ErikaMeleeEnemy.prefab`
- Retain existing hitbox ownership in:
  - `Assets/Prefabs/Swords/LongSword.prefab`
  - `Assets/Prefabs/Enemy/ErikaMeleeEnemy.prefab`
- Migrate attack values in:
  - `Assets/Settings/Items/StraightSwordCombatProfile.asset`
  - `Assets/Settings/Enemy/Actions/*.asset`
- Modify only the active project controllers:
  - `Assets/Art/Animation/CharacterGreatSwordAnimator.controller`
  - `Assets/Art/Animation/Enemy/ErikaLongSwordEnemy.controller`

Replace the generic full-body hit route with four directional states. Do not change the existing player action-layer weight rules.

### Phase 2: Block and parry

1. Write the Character's existing shield/weapon guard state into `CombatDefenseComponent`.
2. Resolve block only when the attack is blockable and inside the guard angle.
3. Consume `GuardDamage` from stamina.
4. Return `GuardBroken` when stamina reaches zero; otherwise return `Blocked`.
5. Let rear and out-of-angle attacks bypass guard.
6. Attach `ParryWindowStateBehaviour` to the real Parry animation state.
7. Only the authored active normalized-time range counts as parry-active.
8. On successful parry:
   - Prevent defender health damage.
   - Close the attacker hitbox immediately.
   - Interrupt the attacker action.
   - Put the attacker into parry stun.
   - Expose a riposte opportunity.
9. Keep elemental guard reduction and enemy-authored guarding/parrying out of this phase.

### Phase 3: Poise, stance, and hyper armor

1. Add poise and stance values to `CombatDefenseComponent`.
2. Apply health, poise, and stance damage from the same resolved hit.
3. Add optional hyper-armor windows to the existing player and enemy attack state behaviours.
4. During hyper armor, add the configured bonus to effective poise.
5. Resolve surviving normal hits as:
   - Stance break
   - Short poise stagger
   - Hit without stagger
6. Reset/recover poise after the configured delay.
7. Reset stance after the vulnerable state ends.
8. Do not add procedural push while using root-motion reaction clips; otherwise movement will be applied twice.

### Phase 4: One critical system for riposte and backstab

Use one player-owned `CriticalAttackController`. Do not create separate synchronization systems.

Before an ordinary player light attack:

1. Reject buffered attacks for critical initiation.
2. Check an existing riposte opportunity from parry, guard break, or stance break.
3. Otherwise check rear angle, distance, height, required neutral time, and both actor states for backstab.
4. Require a fresh light-attack press.
5. If validation fails, continue through the existing normal light-attack path.

On success:

1. Lock both actors in critical state.
2. Close normal weapon hitboxes.
3. Keep the victim in place and align the player to a serialized victim-relative offset.
4. Rotate both actors for the authored animation pair.
5. Calculate and cache the critical result before playback so the correct victim animation is known.
6. Play the synchronized attacker and victim clips.
7. Apply the cached damage exactly once from `CriticalDamageStateBehaviour` at the authored impact progress.
8. Select the `_Die` victim clip only when the cached result is lethal.
9. Release both actors on animation exit.

`CharacterActionStateMachine` must distinguish direct input from buffered execution. A buffered light attack may execute normally but cannot initiate riposte or backstab.

Enemy-initiated critical attacks are outside the initial scope. Enemies still attack, defend, receive directional hits, and act as critical victims through the shared system.

## Animation Plan

### Hard exclusion rule

Never reference an animation whose asset path or clip name contains `inPlace`, case-insensitively. This includes names containing `~inPlace`, `InPlace`, or equivalent capitalization.

Do not extend the current `EnemyAiBootstrap` in-place setup for this work.

### Directional hit MVP

Use one clip per direction first:

| Direction | Clip |
|---|---|
| Front | `Assets/ThirdParty/DoubleL/FBX_Animations/Hit/Hit/Front/Hit_F_1.fbx` |
| Back | `Assets/ThirdParty/DoubleL/FBX_Animations/Hit/Hit/Back/Hit_B_1.fbx` |
| Left | `Assets/ThirdParty/DoubleL/FBX_Animations/Hit/Hit/Left/Hit_L_1.fbx` |
| Right | `Assets/ThirdParty/DoubleL/FBX_Animations/Hit/Hit/Right/Hit_R_1.fbx` |

Available non-`inPlace` variants:

- Front: `Hit_F_1` through `Hit_F_5`, plus `Hit_F_Up` and `Hit_F_Down`.
- Back: `Hit_B_1` through `Hit_B_7`.
- Left: `Hit_L_1`, `Hit_L_2`.
- Right: `Hit_R_1`, `Hit_R_2`.

Add impact-level mappings only after the four-direction MVP is correct. Left and right have only two variants; do not invent a third heavy clip. Reuse the stronger existing variant when necessary.

### Shield block reaction

Use the requested fallback:

`Assets/ThirdParty/DoubleL/FBX_Animations/One Hand Base/Shield/Hit/1Hand_Base_Shield_Block_Hit_4.fbx`

Use it as the one-shot defender reaction, then return to the existing shield guard pose.

### Riposte and backstab pairs

One-handed:

- Attacker: `Assets/ThirdParty/DoubleL/FBX_Animations/One Hand Up/Fatal/Attack/1Hand_Up_Fatal_Attack_1.fbx`
- Victim: `Assets/ThirdParty/DoubleL/FBX_Animations/One Hand Up/Fatal/Hit/1Hand_Up_Fatal_Hit_1.fbx`
- Lethal victim: matching `1Hand_Up_Fatal_Hit_1_Die.fbx`

Two-handed:

- Attacker: `Assets/ThirdParty/DoubleL/FBX_Animations/Two Hand Up/Fatal/Attack/2Hand_Up_Fatal_Attack_1.fbx`
- Victim: `Assets/ThirdParty/DoubleL/FBX_Animations/Two Hand Up/Fatal/Hit/2Hand_Up_Fatal_Hit_1.fbx`
- Lethal victim: matching `2Hand_Up_Fatal_Hit_1_Die.fbx`

The package does not contain clips explicitly named `Riposte` or `Backstab`. Treat the matching Fatal Attack/Fatal Hit clips as synchronized critical pairs. Do not guess the damage frame; preview the pair and author it from the actual contact moment.

Relevant reference scenes are under `Assets/ThirdParty/DoubleL/Demo Scenes/`, including:

- `Demo Enemy Attack & Hit & Magic.unity`
- `Demo One Hand Base.unity`
- `Demo One Hand Up.unity`
- `Demo Two Hand Base.unity`
- `Demo Two Hand Up.unity`

## Presentation Ownership

### Defender side

- `Character` subscribes to `CombatDefenseComponent.OnHitResolved`.
- `AnimatorComponent` receives direction/result-specific triggers.
- `EnemyBrain` or `EnemyAnimationController` consumes the same result.
- Hit without stagger does not cancel the current action.
- Poise stagger, stance break, guard break, parry stun, critical, and death explicitly cancel or replace the current action.

### Attacker side

- `MeleeHitboxController.OnHitResolved` returns the outcome to the active owner.
- `PlayerMeleeCombatRelay` handles hit/block/parry audio and attacker recoil.
- `EnemyAnimationController` handles enemy recoil/parry stun and closes its hitbox.
- A parried attacker receives a riposte opportunity on its own defense component.

## Invariants

- Weapon colliders detect contact; they do not decide gameplay outcomes.
- Player and Enemy use the same hit resolver.
- One attack resolves each entity once even if it has multiple hurtbox colliders.
- A normal rear hit is never promoted to backstab.
- Parry is valid only during its authored active window.
- Critical damage never comes from the normal weapon trigger.
- Critical damage is applied once at the authored impact frame.
- Death overrides hit, block, parry stun, stance break, and non-death critical reactions.
- Non-`inPlace` root-motion reactions must not receive a second procedural push.
- Required dependencies fail fast; do not silently skip combat behavior.

## Risks and Rollback Points

### Risks

- Player runtime controller swaps mean directional and critical states must be present in the equipped weapon controller, not only a no-weapon override.
- Existing player action-layer weights are sensitive; do not change their mutual-exclusion logic while adding hit states.
- Fatal clips require measured alignment and impact progress; names alone do not prove synchronization.
- Multiple hurtbox colliders must continue to deduplicate by entity ID.
- Root-motion reactions can double-move the victim if procedural push is also applied.

### Rollback points

1. New resolver/components are inert until the hitbox call site switches to them.
2. `MeleeHitboxController` can temporarily return to `ApplyDamageCommand` without removing Animator work.
3. Directional controller states can temporarily return to the generic `Hit` state.
4. The critical controller is self-contained and can be removed without changing normal contact resolution.

## Implementation Ownership

Use non-overlapping scopes:

1. Core combat writer:
   - New hit contracts
   - `CombatDefenseComponent`
   - `ResolveMeleeHitCommand`
   - `MeleeHitboxController`
2. Player writer:
   - `CombatProfile`
   - `PlayerMeleeCombatRelay`
   - `Character`
   - `AnimatorComponent`
   - `CharacterFactory`
   - `CriticalAttackController`
3. Enemy writer:
   - `CharacterActionDefinition`
   - `EnemyBrain`
   - `EnemyAnimationController`
   - `EnemyFactory`
4. Unity asset writer:
   - Character and Enemy prefabs
   - Player and Enemy AnimatorControllers
   - Combat profile and enemy action assets
   - StateMachineBehaviour configuration
   - Animation clip assignments

Use exactly one C# writer for overlapping production files. Perform Unity asset work only after code compiles.

## Validation and Acceptance Criteria

- Player sword damages Enemy and Enemy sword damages Character through the same resolver.
- One swing resolves an entity once despite multiple hurtbox colliders.
- Front, back, left, and right contacts select the correct non-`inPlace` clips on both actors.
- Front guard consumes stamina and prevents health damage.
- Rear and out-of-angle attacks bypass guard.
- Guard reaching zero produces guard break.
- Parry succeeds only during authored active frames; early and late contacts resolve normally.
- Parry closes the attacker hitbox, prevents defender damage, and exposes riposte.
- A rear ordinary swing produces `HitFromBack`, not backstab.
- Valid fresh riposte/backstab starts the paired fatal animations.
- Invalid critical validation continues into the normal light attack.
- Buffered light attacks cannot initiate criticals.
- Critical damage occurs exactly once at the authored frame.
- The `_Die` victim animation is used only for a lethal cached result.
- Death suppresses every ordinary reaction.
- No project AnimatorController references a path or clip containing `inPlace`.
- Every modified Unity asset is imported, saved, and reserialized through Unity.
- Unity reports no import or serialization errors.
- No manual Editor save or user interaction is required.

Tests were not run while producing this design. Focused Play Mode scenarios should validate the cases above; automated resolver tests should only be executed when test execution is explicitly requested.
