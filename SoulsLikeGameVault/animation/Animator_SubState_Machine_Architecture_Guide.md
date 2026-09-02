# Animator Sub-State Machine Architecture Guide

This guide establishes the project architecture, layout standards, and runtime rules for Unity Animator Controllers (such as `CharacterGreatSwordAnimator.controller` and `ErikaGreatSwordEnemy.controller`).

---

## 1. Rule: Group Connected Animations into Sub-State Machines

Do not place animation states as loose, flat lists on a controller layer's root (`Base Layer` or `OneHandedLayer`). Group all related states into dedicated, cohesive sub-state machines based on combat and locomotion roles:

- **`Locomotion`**: Free locomotion blend trees, locked locomotion, crouch-to-stand transitions, and movement idling.
- **`Attack`**: Light attacks, alternate combos, heavy attacks, charged attacks, special/skill attacks, roll attacks, and run attacks.
- **`Hits`**: Directional hit reactions (`Hit`, `HitFront`, `HitBack`, `HitLeft`, `HitRight`) and shield blocks (`Blocked`).
- **`Combat`**: Posture breaks, poise stagger, parry reactions, and critical hit/victim states (`GuardBroken`, `PoiseStaggered`, `StanceBroken`, `Parried`, `CriticalHitOneHand`, `CriticalHitTwoHand`, etc.).
- **`Death`**: Death animation states and terminal death poses (`Death`, `DeathComplete`, `DeathIdle`).
- **Additional Domain Groups** (where applicable): `Air` (jump, fall loops, landing), `Rolls` (light roll, locked roll, backstep), and `Grace` (rest start, loop, end).

### Transition Routing
- **AnyState Ingress**: Controller-level AnyState transitions reside on the root layer and point directly to the destination states inside their respective sub-state machines. Triggers (e.g. `HitFront`, `GuardBroken`) function globally across sub-state machines.
- **Exit Egress**: States that conclude and return the entity to default locomotion must connect to their sub-state machine's **`(Exit)`** node with authored `exitTime` and `duration` (e.g. `0.08s`). When a state reaches `(Exit)`, Unity automatically routes control back to the layer's default state (`Locomotion`).

---

## 2. Rule: Coordinate and Layout Standards

All animator controllers must maintain consistent spatial placement and node coordinates matching `CharacterGreatSwordAnimator.controller` to ensure immediate visual readability in the Unity Animator window.

### Root Layer Node Layout
The root layer establishes the central orchestration column:
- **`Entry` Node**: `(550, 280, 0)`
- **`Exit` Node**: `(320, 0, 0)`
- **`AnyState` Node**: `(50, 210, 0)`
- **`ParentStateMachine` Node**: `(800, 20, 0)`

#### Vertical Sub-State Machine Column (Aligned at X = 530)
Sub-state machines are stacked along the vertical axis at **X = 530**, ordered logically from movement down to combat:

| Sub-State Machine | Position `(X, Y, Z)` | Purpose |
|---|---|---|
| **`Locomotion`** | `(530, 210, 0)` | Default layer entry; continuous movement & idle |
| **`Attack`** | `(530, 130, 0)` | Offensive weapon attacks and combo chains |
| **`Air`** *(optional)* | `(530, 80, 0)` | Jumping, mid-air loop, landing states |
| **`Grace`** *(optional)* | `(530, 30, 0)` | Resting, bonfire interaction |
| **`Death`** | `(530, -20, 0)` | Fatal hit and death idle states |
| **`Hits`** (or `Rolls`) | `(530, -70, 0)` | Directional hit reactions and blocking |
| **`Combat`** | `(530, -120, 0)` | Posture breaks, parries, critical victims |

---

### Internal Sub-State Machine Layouts

#### Horizontal Layout Pattern (e.g. `Attack`)
Offensive combo sequences and weapon chains are laid out horizontally from left to right:
- **Boundary Nodes**: Entry `(770, -560, 0)`, Exit `(0, -300, 0)`, AnyState `(10, 30, 0)`, ParentSM `(-30, 100, 0)`.
- **State Row**: Positioned at **Y = -140**, spaced by `250` units along X:
  - `HeavyAttack` @ `(0, -140, 0)`
  - `LightAttack1` @ `(250, -140, 0)`
  - `LightAttack2` @ `(500, -140, 0)`
  - `Combo1` @ `(750, -140, 0)`
  - `Combo2` @ `(1000, -140, 0)`
  - `Combo3` @ `(1250, -140, 0)`

#### Vertical Column Layout Pattern (e.g. `Hits` & `Combat`)
Reactions and status afflictions are stacked vertically in a clean column:
- **Boundary Nodes**: Entry `(50, 120, 0)`, Exit `(800, 120, 0)`, AnyState `(50, 20, 0)`, ParentSM `(800, 20, 0)`.
- **State Column**: Positioned at **X = 440**, spaced by `70` to `80` units along Y:
  - **`Hits`**:
    - `Hit` @ `(440, -340, 0)`
    - `HitFront` @ `(440, -260, 0)`
    - `HitBack` @ `(440, -180, 0)`
    - `HitLeft` @ `(440, -100, 0)`
    - `HitRight` @ `(440, -20, 0)`
    - `Blocked` @ `(440, 60, 0)`
  - **`Combat`**:
    - `GuardBroken` @ `(440, -140, 0)`
    - `PoiseStaggered` @ `(440, -70, 0)`
    - `StanceBroken` @ `(440, 0, 0)`
    - `Parried` @ `(440, 70, 0)`
    - Critical Hit states: `(440, 140, 0)`, `(440, 210, 0)`, `(440, 280, 0)`, `(440, 350, 0)`

#### Locomotion Sub-State Machine Layout
- **Boundary Nodes**: Entry `(360, -230, 0)`, Exit `(610, 60, 0)`, AnyState `(-160, -10, 0)`, ParentSM `(600, -40, 0)`.
- **States**: Blend tree states @ `(340, 40, 0)` and `(340, -40, 0)`.

#### Death Sub-State Machine Layout
- **Boundary Nodes**: Entry `(50, 120, 0)`, Exit `(800, 120, 0)`, AnyState `(470, -50, 0)`, ParentSM `(800, 20, 0)`.
- **States**: `Death` @ `(450, 10, 0)`, `DeathComplete` / `DeathIdle` @ `(450, 90, 0)`.

---

## 3. Rule: Inert `Empty` Default State Inside Action Sub-State Machines

In every action or reaction sub-state machine (`Attack`, `Hits`, `Combat`, `Death`, etc.), **never assign an active animation state (such as `LightAttack1` or `Hit`) as the sub-state machine's default state**.

### Rationale
- In Unity Mecanim, a sub-state machine's `defaultState` is connected to the internal **`Entry`** node with an orange transition arrow.
- If an active animation state is the default state, any traversal through `Entry` will involuntarily begin playing that animation.
- Action sub-state machines should only play an animation when explicitly driven by an AnyState trigger or code-directed CrossFade.

### Implementation Standard
1. Inside each action sub-state machine, create a dedicated state named **`Empty`** with **`motion = null`**.
2. Position `Empty` adjacent to the internal Entry node:
   - In `Attack`: `(740, -490, 0)` (next to Entry at `(770, -560, 0)`)
   - In `Hits`, `Combat`, `Death`: `(30, 180, 0)` (adjacent to Entry at `(50, 120, 0)`)
3. Set `subStateMachine.defaultState = emptyState`.
4. *Exception*: **`Locomotion`** is the active default state machine of the entire layer; its default state must remain the locomotion blend tree (`Locomotion` / `FreeLocomotion`), never an empty state, so characters immediately idle and walk upon spawning.

---

## 4. Rule: Runtime CrossFade Resolution Compatibility

When triggering states inside sub-state machines from C# via `Animator.CrossFadeInFixedTime` or `Animator.Play`:

### Hierarchy Path Syntax Warning
In Unity, passing a path with a dot (`.`) causes Unity to interpret it as a hierarchy path:
- `animator.CrossFadeInFixedTime("Base Layer.LightAttack1", ...)` will **fail with a warning** if `LightAttack1` is inside the `Attack` sub-state machine, because the state is no longer in the root layer.
- Unity expects either the full path (`"Base Layer.Attack.LightAttack1"`) or the **short state name** (`"LightAttack1"`).

### Standard
Always trigger states by their **short name** or **short name hash**:
```csharp
// Correct: works regardless of whether state is on root or inside any sub-state machine
animator.CrossFadeInFixedTime(action.ActionId.ToString(), transitionSeconds);
// Or by hash:
animator.CrossFadeInFixedTime(Animator.StringToHash(stateName), transitionSeconds);
```
Never concatenate layer prefixes (`"Base Layer." + stateName`) when targeting states inside sub-state machines.

---

## 5. Rule: Unity Asset Persistence

Whenever an Animator Controller (`.controller`) is modified programmatically in Editor scripts:
1. Call `EditorUtility.SetDirty(controller)`.
2. Call `AssetDatabase.SaveAssets()`.
3. Force reserialize the asset:
   ```csharp
   AssetDatabase.ForceReserializeAssets(new[] { controllerAssetPath });
   AssetDatabase.SaveAssets();
   ```
4. Verify the Unity console contains 0 import/serialization errors.
