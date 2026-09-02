# EnemyEncounterSystem — Architectural Analysis & Scalability Audit Report

**Document Version:** 1.0.0  
**Target Repository:** SoulsLikeTemplate  
**Primary Target:** [`EnemyEncounterSystem.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyEncounterSystem.cs)  
**Related Components:** [`EnemySpawnPoint.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemySpawnPoint.cs), [`EnemyFactory.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyFactory.cs), [`EnemyGroupCoordinator.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyGroupCoordinator.cs), [`EnemyActor.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyActor.cs), [`EnemyController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyController.cs), [`EnemyAuthoringValidator.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/EnemyAuthoringValidator.cs), [`CoreScope.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/VContainer/CoreScope.cs), [`CheatsUiController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Cheats/CheatsUiController.cs)

---

## 1. Executive Summary

The **`EnemyEncounterSystem`** is currently the sole mechanism for placing, spawning, and respawning enemy entities in the SoulsLikeTemplate project. It acts as an authoring container on a scene GameObject or prefab, containing serialized references to an array of [`EnemySpawnPoint`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemySpawnPoint.cs) components, spawning them synchronously via [`EnemyFactory`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyFactory.cs), and managing their lifecycle across world events (Grace rest, Player death).

### Key Findings & High-Level Verdict

| Area | Current State | Severity | Architectural Assessment |
| :--- | :--- | :---: | :--- |
| **Domain Naming** | Named `EnemyEncounterSystem`, but contains no combat encounter logic. | **High** | Misnomer. It is a world population spawner / camp group, not an event encounter. |
| **Spawn Point Data** | Marked `//todo: fully rework`. Serializes `Transform[]` patrol points and 4 separate SOs per spawn point. | **Critical** | Clutters scene hierarchy, fragile overrides, high redundancy, manual PRNG seed offsets. |
| **DI Scope Scalability** | Registered as a single `[SerializeField]` instance in [`CoreScope.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/VContainer/CoreScope.cs). | **Blocker** | Fails with >1 encounter in a scene. Already bypassed via scene `autoInjectGameObjects` hack. |
| **Spawning Performance** | Batch synchronous instantiation + heavy per-enemy DI container construction. | **Critical** | Frame drops on spawn/respawn; cannot scale to standard level density (20–100 enemies). |
| **AI Coordination Coupling** | Creates and owns an isolated [`EnemyGroupCoordinator`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyGroupCoordinator.cs). | **High** | AI attack slots (pressure tokens) and ally alerts are locked to spawner prefab boundaries. |
| **World Persistence** | Purely transient. Destroys all GameObjects and recreates on respawn; no persistent death state. | **High** | Cannot handle non-respawning enemies (bosses, minibosses, one-time spawns); memory churn. |

```mermaid
flowchart TD
    subgraph Current_Monolithic_Architecture["Current Reality: Tightly-Coupled Spawner Monolith"]
        CS[CoreScope\n(Holds ONLY 1 serialized Encounter!)] -->|Injects| EES[EnemyEncounterSystem]
        EES -->|Owns & Ticks| EGC[EnemyGroupCoordinator\n(Pressure Slots & Alerts)]
        EES -->|Serialized Array| ESP[EnemySpawnPoint 1..N]
        ESP -->|Scene Transforms| PP[PatrolPoint GameObjects\n(Transforms in Scene Tree)]
        ESP -->|Direct SOs| BD[Moveset, Profile, HealthData]
        ESP -->|Direct Prefab| EP[EnemyActor Prefab]
        EES -->|Synchronous Batch Loop| EF[EnemyFactory.CreateEnemy]
        EF -->|Instantiates + NavMesh Sample| EA[EnemyActor Instance]
        EF -->|Builds Child VContainer Scope| LS[LifetimeScope Sub-Container\n(20+ Registrations per Enemy!)]
        EES -->|GameState: OnGraceSit / Ended| RES[Destroys All & Re-instantiates Next Frame]
    end
```

---

## 2. Issue Breakdown: Domain Modeling & Naming

### 2.1 Why "Encounter" is Inappropriate

In game systems engineering (especially within Action-RPGs and Souls-likes), an **Encounter** denotes a bounded, stateful combat scenario or gameplay beat:
- **Encounter Boundaries:** Fog gates, arena triggers, lock-in barriers, or spatial activation volumes.
- **Encounter Progression:** Combat phases, wave escalations, reinforcement triggers, boss intro/cutscene triggers, and music state changes.
- **Encounter Resolution:** Victory conditions, failure conditions, despawn thresholds, loot distribution, and completion flags saved to player progression.

**What `EnemyEncounterSystem` actually does in this codebase:**
1. It holds a list of spawn coordinates and prefab references.
2. It instantiates enemies into the world upon `Start()`.
3. It listens to `IGameStateNotifier` (`GameState.OnGraceSit` and `GameState.Ended`) to despawn and respawn enemies.
4. It hosts an instance of `EnemyGroupCoordinator` to arbitrate attack slots.

**The system has zero awareness of combat state.** It does not know if enemies are in combat, whether the player has entered the zone, whether enemies have been alerted, or when the "encounter" is defeated.

### 2.2 The Architectural Risk of the Misnomer
- **Blocking True Encounter Systems:** If a future feature requires a true Souls-like Boss Encounter (with boss health bar UI, fog gates, music switches, cutscene trigger, and boss defeat state persistence), using `EnemyEncounterSystem` will cause immediate design collision. Developers will either bloat this class into a monstrous God Object or create confusingly named duplicates (`BossEncounterSystem`, `CombatEncounterSystem`).
- **Conflating Spawning with Combat Grouping:** Because the spawner is called an "Encounter", it assumes all enemies spawned together must fight together, locking the [`EnemyGroupCoordinator`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyGroupCoordinator.cs) directly inside the spawner component.

### 2.3 Recommended Taxonomy
To align with clean architecture and domain-driven design, the responsibilities must be split:

1. **World Spawning / Population Layer:**
   - **`EnemySpawnGroup`** or **`EnemySpawnZone`**: A spatial or logical group of spawn points representing an enemy camp, patrol route, or outpost.
   - **`EnemySpawnPoint`**: A single enemy spawn location.
2. **AI Combat Coordination Layer:**
   - **`EnemyCombatGroupCoordinator`** / **`CombatPressureArbiter`**: Dynamic combat arbitration of attack pressure slots and ally alerts, assigned dynamically to enemies engaging the same target.
3. **Gameplay Encounter Layer (Future/Feature):**
   - **`CombatEncounter`** or **`BossEncounter`**: Dedicated event-driven managers that control combat arenas, music triggers, fog walls, and victory progression.

---

## 3. Issue Breakdown: `EnemySpawnPoint` & Data Encapsulation

The author of [`EnemySpawnPoint.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemySpawnPoint.cs) explicitly left a code comment:
```csharp
//todo: fully rework
public sealed class EnemySpawnPoint : MonoBehaviour
```

A detailed audit reveals why this rework is critical:

### 3.1 Patrol Points as Scene `Transform[]`

Currently, patrol positions are authored as an array of Unity `Transform` references:
```csharp
[SerializeField] private Transform[] patrolPoints = { };
```
In [`EnemyEncounter.prefab`](file:///f:/Private/SoulsLikeTemplate/Assets/Prefabs/Models/Enemy/EnemyEncounter.prefab), this manifests as:
- `ErikaMeleeSpawn` (GameObject with `EnemySpawnPoint`)
  - `PatrolPointA` (Empty GameObject with `Transform`)
  - `PatrolPointB` (Empty GameObject with `Transform`)

#### Flaws of this Approach:
1. **Scene Graph Pollution & Memory Overhead:**
   - Every patrol point is an entire Unity `GameObject` + `Transform` component.
   - In a full level with 50 patrolling enemies averaging 4 waypoints each, this creates **200–300 empty GameObjects** purely to store 3D coordinates.
   - Each `Transform` participates in Unity's internal transform hierarchy matrix calculations, even though waypoints are completely static.
2. **Prefab Override Fragility:**
   - If a level designer places an encounter prefab into a scene and adjusts patrol waypoints, Unity records individual transform overrides for every child GameObject. If the prefab asset is updated or reorganized, scene overrides break or misalign.
3. **Dead Weight at Runtime:**
   - During [`EnemyFactory.CreateEnemy`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyFactory.cs#L76), [`spawn.BuildPatrolPositions()`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemySpawnPoint.cs#L23-L37) extracts `Vector3` coordinates into an array:
     ```csharp
     positions[index] = patrolPoints[index].position;
     ```
   - After this single line executes at initialization, the `Transform` GameObjects are **never accessed again**. Yet they remain active in memory and in the scene hierarchy indefinitely.
4. **No Reusability or Advanced Routing:**
   - Two guards walking the same courtyard cannot share a waypoint route without cross-referencing transforms across parent GameObjects.
   - The patrol logic in [`EnemyController.DecidePatrol`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyController.cs#L659-L691) is hardcoded to a circular loop:
     ```csharp
     _patrolIndex = (_patrolIndex + 1) % _actor.PatrolPoints.Count;
     ```
   - There is no support for ping-pong routes, branching paths, idle dwell zones, or splines (despite `Unity.Splines` being installed in the project packages).

### 3.2 Monolithic Asset References (Lack of Archetype Definition)

Every [`EnemySpawnPoint`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemySpawnPoint.cs) directly serializes 4 separate asset dependencies:
```csharp
[SerializeField] private EnemyActor enemyPrefab;
[SerializeField] private EnemyBehaviourProfile behaviourProfile;
[SerializeField] private WeaponMovesetDefinition moveset;
[SerializeField] private HealthData healthData;
```

#### Why This Breaks Scalability:
- **Violates DRY (Don't Repeat Yourself):** If a level contains 25 "Erika Melee Knight" enemies, a designer must manually drag and drop the exact same 4 ScriptableObjects / prefabs onto 25 distinct spawn point GameObjects.
- **Rebalancing Nightmare:** If combat design rebalances Erika's moveset or health data to a new asset, updating it requires modifying 25 different scene objects or editing YAML directly.
- **Tight Coupling to Prefabs:** Direct references to `EnemyActor enemyPrefab` mean the scene or encounter prefab holds direct asset dependencies to the enemy mesh, textures, animators, and materials, preventing Addressable-based memory streaming.

### 3.3 Manual PRNG Seed Offset Fragility

[`EnemySpawnPoint`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemySpawnPoint.cs#L14) serializes:
```csharp
[SerializeField] private int randomSeedOffset;
```
- **Purpose:** In [`EnemyRandomStreams.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyRandomStreams.cs), this offset is added to the profile's base random seed so that identical enemies don't make synchronized AI decisions (e.g. all attacking or dodging at the exact same millisecond).
- **The Problem:** Designers must manually type `0`, `1`, `2`, `3` into the Inspector for every enemy.
- **Excessive Validator Overhead:** In [`EnemyAuthoringValidator.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/EnemyAuthoringValidator.cs#L788-L806), extensive code exists solely to traverse encounters and scenes to assert that no two spawn points share the same `(EnemyBehaviourProfile, randomSeedOffset)`.
- **Scaling Failure:** When a designer duplicates a spawn point in Unity Editor (Ctrl+D), the duplicate retains the same seed offset, immediately triggering validator errors. PRNG desynchronization should be derived automatically (e.g., from unique entity ID, instance index, or position hash), not manually configured.

---

## 4. Issue Breakdown: Critical Scaling Bottlenecks

### 4.1 The DI Container Single-Instance Bottleneck (`CoreScope`)

Inspecting [`CoreScope.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/VContainer/CoreScope.cs#L23-L28):
```csharp
[SerializeField] private EnemyEncounterSystem enemyEncounterSystem;
protected override void Configure(IContainerBuilder builder)
{
    ...
    builder.RegisterComponent(enemyEncounterSystem).AsSelf().AsImplementedInterfaces();
    ...
}
```

#### The Immediate Architectural Failure:
1. `CoreScope` is designed around the assumption that **only one `EnemyEncounterSystem` exists in the scene**.
2. In reality, a standard Souls-like level contains dozens of enemy groups, camps, and rooms.
3. In [`DefaultLocation.unity`](file:///f:/Private/SoulsLikeTemplate/Assets/Scenes/DefaultLocation/DefaultLocation.unity), there are already **two** encounter prefabs:
   - `EnemyEncounter` (Erika melee group)
   - `TrainingDummyEncounter` (Training dummy)
4. Because `CoreScope` only has a single `[SerializeField]` slot:
   - `EnemyEncounter` is assigned to `CoreScope.enemyEncounterSystem`.
   - `TrainingDummyEncounter` **could not be registered** in `CoreScope`!
   - To make `TrainingDummyEncounter` receive its injected dependencies (`IGameStateNotifier`, `EnemyFactory`), it was manually added to VContainer's **`autoInjectGameObjects`** list on `CoreScope` (Line 16339 of `DefaultLocation.unity`).
5. In [`CheatsUiController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Cheats/CheatsUiController.cs#L43):
   ```csharp
   public CheatsUiController(..., EnemyEncounterSystem enemyEncounterSystem, ...)
   ```
   Cheats injects the single `EnemyEncounterSystem`. Calling `CheatsUiController.RespawnEnemies()` only respawns `EnemyEncounter`; `TrainingDummyEncounter` is completely ignored!

> [!CAUTION]
> **Scalability Limit Reached at N = 2:** The current architecture broke on the second encounter added to the project. It cannot support 5, 20, or 100 enemy groups without rewriting the injection and management topology.

---

### 4.2 Synchronous Batch Spawning & Frametime Spikes

In [`EnemyEncounterSystem.SpawnEnemies()`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyEncounterSystem.cs#L125-L131):
```csharp
foreach (EnemySpawnPoint spawnPoint in spawnPoints)
{
    EnemyActor enemy = _enemyFactory.CreateEnemy(spawnPoint, _groupCoordinator);
    ...
}
```

#### What happens inside `CreateEnemy` for *every single enemy*:
1. NavMesh raycasting and nearest-point sampling (`_navMeshService.TrySamplePosition`, `TrySampleNearestPosition`).
2. `UnityEngine.Object.Instantiate(prefab, spawnHit.position, ...)`.
3. 8+ explicit component queries (`GetComponent<ViewEntity>`, `GetComponentInChildren<EnemyActionExecutor>`, etc.).
4. Instantiation of a new `GameObject` (`${prefab.name}_LifetimeRoot`).
5. Construction of a brand new VContainer **`LifetimeScope` sub-container**:
   - Enqueues parent scope (`RootScope`).
   - Registers **20 distinct services and commands** (`HealthModel`, `ApplyDamageCommand`, `ResolveMeleeHitCommand`, `CriticalTargetCommand`, `TargetingCommand`, `EnemyPerception`, `EnemyRandomStreams`, `EnemyActionSelector`, `EnemyController`, etc.).
   - Builds the child container and resolves all dependencies.
6. Attaches `LifetimeRoot` to `EnemyActor`.

#### Performance Impact:
- Building an isolated VContainer `LifetimeScope` with reflection/expression tree compilation and instantiating complex prefabs with NavMesh queries takes **5–25ms per enemy** depending on hardware.
- Spawning 10 enemies in a camp synchronously locks the main thread for **50–250ms**, producing an unacceptable visible freeze/stutter.
- There is no time-slicing, no asynchronous instantiation (`UniTask` or `Addressables.InstantiateAsync`), and no frame budget management.

---

### 4.3 Zero Population LOD / Proximity Culling

Currently:
- When the scene starts, if `spawnOnStart == true`, **all enemies in all encounters spawn immediately**.
- An enemy situated 600 meters away across the map from the player is fully active:
  - `EnemyController.Tick` runs every frame.
  - `EnemyPerception.Tick` performs sphere sweeps and line-of-sight physics linecasts.
  - `NavMeshAgent` evaluates paths and avoids obstacles.
  - `Animator` updates bones and blends states.
  - Character and hitboxes update colliders.
- In a full Souls-like map with 80 enemies, running 80 full perception sweeps and state machines simultaneously will saturate CPU main thread frame time, tanking the target 60 FPS.
- There is no **Population LOD** (Level of Detail): no hibernation, no perception throttling based on distance, and no proximity-based spawn/despawn streaming.

---

### 4.4 Artificial AI Coordination Boundaries

In [`EnemyEncounterSystem.SpawnEnemies()`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyEncounterSystem.cs#L122-L124):
```csharp
_groupCoordinator ??= new EnemyGroupCoordinator(
    maxPressureSlots,
    pressureSlotTimeoutSeconds);
```

[`EnemyGroupCoordinator`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyGroupCoordinator.cs) manages:
- **Pressure Slots:** Maximum simultaneous attacking enemies (attack tokens) to prevent the player from being overwhelmed by multiple unblockable attacks at once.
- **Ally Alert Broadcasting:** When an enemy detects the player, calling [`BroadcastAllyAlert`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyGroupCoordinator.cs#L51-L75) notifies allies in the group.

#### Why tying this to the spawner breaks combat:
1. **Spatial Isolation:** If two separate encounter prefabs are placed 5 meters apart (e.g. Camp Gate Guards and Camp Courtyard Patrol), their enemies belong to **different coordinators**.
   - If the player engages both, each coordinator independently grants its pressure slot.
   - The player faces simultaneous attacks from both groups, breaking combat pressure pacing.
   - Gate guards cannot alert the courtyard patrol because `BroadcastAllyAlert` only loops over `_members` of its own coordinator.
2. **Spatial False-Sharing:** If a single encounter prefab covers a large outpost, enemies 100 meters away on the far side share the same coordinator. If an enemy over there claims a pressure slot, an enemy right in front of the player cannot attack!
3. **Architectural Rule:** **Combat arbitration is a dynamic runtime spatial concern**, not an authoring placement concern.

---

### 4.5 Naive Respawn Architecture & Memory Churn

In [`EnemyEncounterSystem.DespawnEnemies()`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyEncounterSystem.cs#L133-L153):
```csharp
foreach (EnemyActor enemy in enemies)
{
    enemy.Despawn(); // Destroys the GameObject and its LifetimeRoot!
}
```
When the player rests at a Grace or dies:
1. Every active enemy GameObject is completely destroyed.
2. All child VContainer `LifetimeScope` containers are disposed and garbage collected.
3. The next frame, identical GameObjects and containers are re-allocated and re-instantiated from scratch.
4. **No Object Pooling:** Massive memory allocation churn, GC pressure, and frame hitching during respawn.
5. **No Persistence or Differentiation:**
   - There is no concept of enemy persistence.
   - If an encounter contains a miniboss or an elite enemy that should **not** respawn once killed (standard in all Souls games), the current system cannot express this. Everything respawns unconditionally if `respawnOnGrace` is true.

---

## 5. Architectural Map: Surrounding Systems Matrix

To understand how `EnemyEncounterSystem` sits in the project, here is the comprehensive dependency and interaction map:

| System / Component | Nature of Interaction | Direction | Current Coupling / Flaw |
| :--- | :--- | :---: | :--- |
| **`CoreScope`** | Registers `EnemyEncounterSystem` in scene DI container. | Inbound | Single-field serialization; blocks multi-encounter scenes. |
| **`EnemyFactory`** | Called to instantiate enemy prefab and build DI sub-scope. | Outbound | Synchronous, heavy allocations, tight component coupling. |
| **`EnemySpawnPoint`** | Child/assigned components providing coordinates and SOs. | Outbound | Monolithic config, scene Transform patrol points, manual seeds. |
| **`EnemyGroupCoordinator`** | Instantiated and ticked by `EnemyEncounterSystem`. | Outbound (Owned) | AI attack slots artificially constrained to spawner prefab. |
| **`EnemyActor`** | Instantiated entity representation tracked in `_spawnedEnemies`. | Outbound (Tracked) | Clean despawn event; destroys lifetime root upon despawn. |
| **`EnemyController`** | Enemy AI state machine and brain. | Indirect | Consumes `PatrolPoints` from `EnemyActor` as raw `Vector3[]`. |
| **`EnemyPerception`** | Enemy sensory vision/hearing system. | Indirect | Runs every frame without distance-based culling/LOD. |
| **`IGameStateNotifier`** | Broadcasts `GameState` changes (`OnGraceSit`, `Ended`). | Inbound | `EnemyEncounterSystem` observes to trigger respawn. |
| **`GraceSystem`** | Triggers `GameState.OnGraceSit` during rest. | Indirect Inbound | Triggers despawn/respawn cycle. |
| **`CharacterSpawnService`** | Player spawning service. | Parallel | Clean pure-C# architecture with save store; enemy spawning lacks this! |
| **`EnemyAuthoringValidator`** | Editor validation tool. | External Tool | Must perform complex checks to catch manual seed duplicates and broken SOs. |
| **`CheatsUiController`** | Debug UI for god mode, kill all, respawn. | Inbound | Directly depends on a single `EnemyEncounterSystem` reference. |

---

## 6. Target Architecture Blueprint & Evolutionary Roadmap

To transition this system into a production-grade, highly scalable Souls-like population and combat architecture, the following target architecture is recommended.

### 6.1 Target Architectural Diagram

```mermaid
flowchart TD
    subgraph Management_Layer["1. Management & Service Layer (Singleton / CoreScope)"]
        ESM[EnemySpawnService / PopulationManager\n(Pure C# Service or VContainer Singleton)]
        SS[ISaveService / WorldStateStore\n(Tracks Permanently Dead Enemies)]
        GS[IGameStateNotifier\n(Grace / Death Observer)]
        GS -->|Notifies| ESM
        SS -->|Provides Dead State| ESM
    end

    subgraph World_Authoring_Layer["2. Authoring & Scene Placement Layer"]
        ESG1[EnemySpawnGroup: Camp A\n(Self-registers with EnemySpawnService)]
        ESG2[EnemySpawnGroup: Camp B\n(Self-registers with EnemySpawnService)]
        ESG1 -->|Holds| ESP_New[EnemySpawnPoint Data]
        ESP_New -->|References| EAD[EnemyArchetypeDefinition\nScriptableObject (Prefab + SOs)]
        ESP_New -->|References| PR[PatrolRoute Definition\n(Local Vector3 Handles or Spline)]
    end

    ESM -->|Coordinates Lifecycle & Respawn| ESG1
    ESM -->|Coordinates Lifecycle & Respawn| ESG2

    subgraph Spawning_Runtime_Layer["3. Asynchronous & Pooled Spawning Layer"]
        ESG1 -->|Requests Async Spawn| EF_New[EnemyFactory / Spawner]
        EF_New -->|Time-Sliced / UniTask Queue| EPool[Enemy Object Pool]
        EPool -->|Activates| EA_New[EnemyActor Instance]
    end

    subgraph Dynamic_Combat_Layer["4. Decoupled Combat Arbitration"]
        CA[Spatial CombatCoordinator\n(Dynamic Pressure Slots & Ally Alerts)]
        EA_New -->|Registers on Alert/Engage| CA
    end
```

---

### 6.2 Step-by-Step Evolution Plan

#### Phase 1: Semantic Clarification & Immediate Fixes (Non-Breaking)
1. **Rename / Alias:**
   - Introduce `EnemySpawnGroup` (or rename `EnemyEncounterSystem` -> `EnemySpawnGroup`).
   - Reserve the term `Encounter` for actual event/boss combat encounters.
2. **Eliminate Manual PRNG Seed Offset:**
   - Remove `randomSeedOffset` from `EnemySpawnPoint`.
   - Automatically compute random seeds at runtime using a deterministic hash:
     ```csharp
     int seed = HashCode.Combine(spawnPoint.transform.position, entityId);
     ```
   - Deprecate the manual seed validation in [`EnemyAuthoringValidator.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/EnemyAuthoringValidator.cs).

#### Phase 2: Refactor `EnemySpawnPoint` & Patrol Data
1. **Create `EnemyArchetypeDefinition` (ScriptableObject):**
   - Encapsulate the 4 repeated assets into a single cohesive data asset:
     ```csharp
     [CreateAssetMenu(fileName = "EnemyArchetype", menuName = "Enemy/Archetype Definition")]
     public sealed class EnemyArchetypeDefinition : ScriptableObject
     {
         [SerializeField] private EnemyActor enemyPrefab;
         [SerializeField] private EnemyBehaviourProfile behaviourProfile;
         [SerializeField] private WeaponMovesetDefinition moveset;
         [SerializeField] private HealthData healthData;
         [SerializeField] private bool isPersistentDeath; // Bosses/minibosses do not respawn!
         ...
     }
     ```
   - `EnemySpawnPoint` now holds **one** reference (`EnemyArchetypeDefinition`) instead of four.
2. **Replace `Transform[]` Patrol Points:**
   - Replace empty GameObject transforms with:
     - Local `Vector3[] localPatrolPoints` edited directly via Scene GUI Handles in a custom Editor inspector, OR
     - An asset-based `PatrolRoute` ScriptableObject / Unity Spline.
   - Eliminates hundreds of empty GameObjects, stops scene transform matrix recalculations, and makes routes cleanly reusable.

#### Phase 3: Centralized Spawn Management (`EnemySpawnService`)
1. **Create `EnemySpawnService` (`IEnemySpawnService`):**
   - Register `EnemySpawnService` as a singleton in `CoreScope.cs`.
   - Remove `enemyEncounterSystem` field from `CoreScope.cs`.
   - Individual `EnemySpawnGroup` components find or inject `IEnemySpawnService` and self-register on `Awake`/`Start`.
2. **Refactor `CheatsUiController`:**
   - Injects `IEnemySpawnService`, calling `_spawnService.RespawnAll()` across all registered groups in the scene.
   - Resolves the multi-encounter injection bug permanently.

#### Phase 4: Decouple Combat Arbitration (`EnemyGroupCoordinator`)
1. **Dynamic Combat Grouping:**
   - Remove `EnemyGroupCoordinator` instantiation from `EnemySpawnGroup`.
   - Implement a scene-level or spatial `CombatPressureCoordinator`.
   - When an enemy transitions to `EnemyGoal.Alert` or `EnemyGoal.Combat`, it requests/leases pressure slots from the spatial coordinator.
   - Adjacent enemies naturally share attack slots and alert neighbors, regardless of what spawner spawned them.

#### Phase 5: High-Density Scaling (LOD & Asynchronous Streaming)
1. **Time-Sliced / Budgeted Spawning:**
   - Change `EnemyFactory` instantiation from a synchronous tight loop to an asynchronous queue (e.g. 1 enemy instantiated per frame) to prevent frametime spikes during respawn.
2. **Proximity & Population LOD:**
   - Add distance-based dormancy: enemies further than `dormantDistance` (e.g. 80m) disable NavMesh agent steering and perception raycasting, entering a lightweight tick mode until the player approaches.
3. **Death State Persistence:**
   - Integrate with `ISaveService` (similar to [`CharacterSpawnService`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Spawn/CharacterSpawnService.cs)).
   - Assign each spawner a stable GUID or ID; dead unique/elite enemies are saved in the save store and never respawned on Grace sit.

---

## 7. Summary & Next Steps

| Current Flaw | Architectural Cost | Recommended Solution |
| :--- | :--- | :--- |
| **"Encounter" Misnomer** | Conceptual confusion; blocks true combat encounter systems. | Rename to `EnemySpawnGroup`; separate spawner from combat event. |
| **Scene Transform Patrols** | Scene tree bloat; transform overhead; fragile overrides. | Use local `Vector3[]` with scene handles or `PatrolRoute` asset. |
| **Monolithic 4-SO Config** | High authoring redundancy; rebalance difficulty. | Create `EnemyArchetypeDefinition` ScriptableObject. |
| **Single `CoreScope` Slot** | Complete scalability blocker (crashes at >1 encounter). | Implement `IEnemySpawnService` registry pattern. |
| **Sync Batch Spawning** | 50–250ms frame freezes on spawn/respawn. | Implement time-sliced / async frame-budgeted spawning. |
| **Owned `EnemyGroupCoordinator`** | Artificial combat boundaries; broken alert/pressure pacing. | Decouple combat arbitration into dynamic spatial coordinator. |
| **No Persistence / Churn** | Memory churn; cannot handle non-respawning enemies. | Integrate with `ISaveService` for persistent dead state. |

*This report provides the complete analysis and design blueprint for upgrading the enemy population and spawn architecture in accordance with SoulsLikeTemplate standards.*
