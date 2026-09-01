
# Hitbox System

**Sword colliders only detect contact.**  
The final result is decided from attack data, defender state, animation windows, and hit direction.

## 1. Data Layer

### Core terms

- **Poise** controls short hit interruption.
- **Hyper armor** temporarily increases poise during specific attack frames.
- **Stance** is a separate meter. When stance reaches zero, the character enters a long vulnerable state.

### Attack data

Each attack contains:

- `AttackId`
- `HealthDamage`
- `GuardDamage` — stamina or guard damage applied on block
- `PoiseDamage` — ability to cause a short stagger
- `StanceDamage` — builds toward a large stance break
- `ImpactLevel` — `Light`, `Medium`, or `Heavy`
- `PushDistance`
- `CanBeBlocked`
- `CanBeParried`
- `CanTriggerBackstab`
- `BlockRecoil` — how strongly the attacker reacts when blocked
- `ParryStun`
- `MaxHitsPerTarget`

These values belong to the specific attack, not only to the weapon. A light attack and a charged attack from the same sword can have different damage, impact, push, block, and parry behavior.

### Hitbox registration

When an attack starts, register:

- Attacker
- Weapon collider
- Attack data
- Unique attack instance
- Already-hit targets
- Current active or inactive state

This prevents one sword swing from damaging the same target every physics frame.

### Defender data

Each character exposes:

- `Health`
- `Stamina`
- `CurrentPoise`
- `MaxPoise`
- `PoiseRecoveryDelay`
- `CurrentStance`
- `MaxStance`
- `HyperArmorBonus`
- `CanBeInterrupted`
- Hurtboxes
- Blocking state, direction, and guard angle
- Whether the character is inside an active parry window
- Whether the character can currently be backstabbed
- Whether the character is invulnerable
- Whether the character is already in a hit, stun, critical, or death state
- Directional hit reactions for front, back, left, and right impacts at light, medium, and heavy levels

### Animation window data

Attack animations define:

- Weapon hitbox active window
- Hyper armor active window
- Whether the attack can be parried

Parry animations define:

- Startup window
- Active parry window
- Recovery window

Example:

| Time | Phase |
|---|---|
| `0.00–0.25` | Startup |
| `0.25–0.40` | Parry active |
| `0.40–1.00` | Recovery |

Exact values depend on the animation.

Backstab configuration defines:

- Rear detection angle
- Maximum distance
- Maximum height difference
- Required neutral frames
- Whether a fresh light-attack press is required
- Attacker alignment position
- Victim alignment position
- Critical damage
- Damage frame inside the backstab animation

### Hit result

Every resolved interaction produces one result:

- Ignored
- Invulnerable
- Normal hit without stagger
- Stagger hit
- Stance break
- Blocked
- Guard broken
- Parried
- Backstab

---

## 2. Logic Layer

### Normal attack flow

1. Attack animation starts.
2. Create a unique attack instance.
3. Register the sword collider and attack data.
4. Animation enters active attack frames.
5. Enable the sword collider.
6. The sword touches a defender hurtbox.
7. Ignore or reject the contact when:
   - It is the attacker itself.
   - The defender was already hit by this attack instance.
   - The defender is invulnerable.
   - The defender is dead or otherwise invalid.
8. Resolve valid contact as parry, block, or normal hit. A backstab already in progress bypasses normal weapon-hit resolution.

### 1. Backstab

A normal sword hit from behind is **not automatically a backstab**. It produces a regular `HitFromBack` reaction unless a valid backstab action was started.

Backstab flow:

1. Player moves behind the enemy.
2. Player enters the enemy’s rear angle, distance, and height limits.
3. Player performs no action for at least the required number of gameplay frames.
4. Previous attack input must no longer be buffered.
5. Player makes a fresh light-attack press.
6. Rear angle, distance, height, and both character states are checked again.
7. Attacker and victim are aligned to their configured positions.
8. Normal weapon damage is disabled.
9. Synchronized backstab animations begin.
10. Critical damage is applied only at the configured animation damage frame.
11. Both characters are released after the animation; temporary protection from unrelated hits can be applied while the critical animation is active.

If any validation fails, the player performs a normal light attack.

### 2. Parry

The parry window belongs to the defender’s parry animation, not to the enemy attack animation.

A parry succeeds when:

1. Defender is inside the active parry window.
2. Enemy weapon hitbox is active and reaches the defender.
3. The attack is marked as parryable.
4. The attack comes from a valid direction, when directional restrictions apply.

Result:

- Defender receives no normal hit.
- Enemy attack is cancelled.
- Sword hitbox is disabled.
- Enemy enters a strong parry stun.
- Enemy becomes available for a riposte.

Before or after the active parry window, the defender receives the normally resolved hit.

### 3. Block

Block succeeds when:

- Defender is holding block.
- Attack is blockable.
- Attack comes from inside the defender’s guard angle.

Result:

- Reduce or remove health damage.
- Apply stamina or guard damage.
- Apply `BlockRecoil` to the attacker when configured.
- Apply the block reaction to the defender.

If the defender has enough stamina, the defender remains guarding. If stamina reaches zero, guard breaks, the defender enters a long guard-break stun, and becomes open for a critical attack.

### 4. Normal hit

When the attack is not parried, blocked, ignored, or part of an established backstab:

1. Apply health damage.
2. Apply poise damage.
3. Apply stance damage.
4. Evaluate stance break, poise, hyper armor, and whether the defender can currently be interrupted.
5. Select the reaction using `ImpactLevel` and hit direction.
6. Register the defender as already hit by this attack instance.

#### Hit without stagger

When the defender still has enough effective poise, or is temporarily uninterruptible:

- Health and stance damage still apply.
- The current attack continues.
- Blood, sound, and hit effects still play.
- No forced movement or attack cancellation occurs.

This is why some enemies appear to ignore a hit even though damage was applied.

#### Short poise stagger

When poise reaches zero and no higher-priority stance break occurs:

- Cancel the defender’s current attack.
- Play a directional short stagger animation at the attack’s impact level.
- Move the defender slightly opposite the attack source using `PushDistance`.
- Apply a short input and action lock.
- Recover poise after the stagger or configured recovery delay.

#### Stance break

When stance reaches zero:

- Cancel the defender’s current action.
- Enter a long stance-break animation.
- Become vulnerable to a critical attack.
- Suppress the shorter poise-stagger result.
- Recover stance after the vulnerable state.

#### Hyper armor and interruption

Attacking first does not guarantee interruption.

- During ordinary startup frames, the enemy may have normal poise and can be staggered.
- During configured hyper-armor frames, `HyperArmorBonus` raises effective poise, so the same player attack may not interrupt.
- Special attacks can set `CanBeInterrupted` to false for explicitly uninterruptible frames.

### Hit direction

Calculate hit direction relative to the defender. Reaction names describe where the attack came from, not where the victim moves.

| Attack source | Reaction | Victim movement |
|---|---|---|
| Front | `HitFromFront` | Backward |
| Back | `HitFromBack` | Forward |
| Right | `HitFromRight` | Left |
| Left | `HitFromLeft` | Right |

For example, a hit from the enemy’s right side plays `HitFromRight` and moves the enemy left. Heavy impacts move farther; light impacts produce only a small movement.

### Attack end

1. Animation leaves active attack frames.
2. Disable the sword collider.
3. Unregister the active hitbox.
4. Keep the attack instance only as long as necessary.
5. Clear it before the next attack.

---

## 3. Presentation Layer

Presentation reacts to the resolved result. It does not decide gameplay.

### Hit without stagger

- Blood or impact effect
- Hit sound
- Optional small body twitch
- Optional small hit-stop and camera shake
- No attack cancellation
- No forced movement

### Stagger hit

- Current attack animation stops
- Directional stagger animation plays at the resolved impact level
- Small movement opposite the attack source
- Short input and action lock
- Stronger hit-stop or camera shake when appropriate

### Stance break

- Strong collapse or vulnerable animation
- Longer stun
- Critical attack opportunity feedback

### Block

- Shield spark
- Metallic impact sound
- Defender block animation
- Attacker recoil animation
- Stronger effect when guard is broken

### Parry

- Distinct parry spark and sound
- Strong hit-stop
- Attacker animation stops immediately
- Attacker parry-stun animation
- Defender recovery animation
- Riposte opportunity feedback

### Backstab

- Characters snap into aligned positions
- Synchronized attacker and victim animations play
- Normal weapon hitbox is disabled
- Critical sound and effects
- Damage occurs only at the critical impact frame

## Result priority

1. Reject self-contact, invalid or dead targets, invulnerability, and repeated contact from the same attack instance.
2. An established backstab state uses the synchronized critical flow instead of normal weapon-hit resolution.
3. Resolve parry.
4. Resolve block, including guard break.
5. Resolve a normal hit. Within that hit, stance break takes priority over short poise stagger, which takes priority over a hit without stagger.
