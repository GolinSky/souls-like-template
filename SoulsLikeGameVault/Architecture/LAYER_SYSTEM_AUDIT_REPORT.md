# Layer System Architecture & Audit Report

## 1. Executive Summary & Design Intent

In this repository, the layer architecture is designed around a **Semantic Key & Service Provider** pattern:
- **[`LayerName`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/LayerName.cs)** is a strongly typed C# `enum` serving as an abstract, semantic dictionary key for serialization (`SerializedDictionary<LayerName, LayerMask>`) and code access. Its integer indices are for Editor serialization stability, **not** direct Unity layer indices (0–31).
- **[`LayerData`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/Data/LayerData.cs)** is the single source of truth ScriptableObject holding the mapping from semantic [`LayerName`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/LayerName.cs) keys to actual Unity [`LayerMask`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/Data/LayerData.cs) values.
- **[`LayerService`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/LayerService.cs)** ([`ILayerService`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/ILayerService.cs)) is the VContainer-registered runtime singleton responsible for providing layer masks, single-layer bit extraction, and recursive GameObject hierarchy layer assignment.

### Current Status: Architectural Disconnect
While the architecture is well-conceived, there is an **adoption and configuration gap**:
1. **Runtime Systems Bypass `LayerService`:** Systems requiring physics queries ([`MovementComponent`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Movement/MovementComponent.cs), [`EnemyPerception`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyPerception.cs), [`InteractionController`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Interactions/InteractionController.cs)) use standalone ScriptableObject `LayerMask` fields or `Physics.AllLayers` rather than querying [`ILayerService`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/ILayerService.cs).
2. **Editor Utilities Bypass `LayerData`:** No Editor utilities use [`LayerData`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/Data/LayerData.cs). For example, [`DefaultLocationNavMeshBakeTool`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/DefaultLocationNavMeshBakeTool.cs) hardcodes an arbitrary integer bitmask (`55`).
3. **`LayerData.asset` Incomplete:** The serialized [`LayerData.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Data/LayerData.asset) contains only 3 dictionary entries (one empty, two out of enum bounds), causing the single consumer ([`PreviewRenderService`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/PreviewRender/PreviewRenderService.cs)) to fail at runtime with a warning.

```mermaid
flowchart TD
    subgraph Design_Intent["Intended Architecture"]
        LN["LayerName\n(Semantic Enum Key)"] --> LD["LayerData (ScriptableObject)\nSerializedDictionary<LayerName, LayerMask>"]
        LD --> LS["LayerService : ILayerService\n(VContainer Singleton)"]
        LS --> |GetLayerMask| MC_Intent["Movement / Grounding"]
        LS --> |GetLayerMask| EP_Intent["Enemy Perception (LOS)"]
        LS --> |GetLayerMask| IC_Intent["Interaction Probing"]
        LS --> |GetLayerMask| PR_Intent["Preview Camera Culling"]
        LD -.-> |Editor Validation / Baking| ED_Intent["Editor Tools & Validators"]
    end

    subgraph Actual_Reality["Current Codebase Reality"]
        MData["MovementData.asset\n(Direct LayerMask: 4294966783)"] --> MC_Real["MovementComponent\nPhysics.SphereCastNonAlloc"]
        EProf["EnemyBehaviour.asset\n(Direct LayerMask: 119)"] --> EP_Real["EnemyPerception\nPhysics.Linecast"]
        HardAll["Physics.AllLayers\n(Hardcoded)"] --> IC_Real["InteractionController\nPhysics.OverlapSphereNonAlloc"]
        Hard55["const int MASK = 55\n(Hardcoded)"] --> ED_Real["DefaultLocationNavMeshBakeTool\nsurface.layerMask"]
        LD_Incomplete["LayerData.asset\n(Key 0=0, Key 7=~0, Key 8=~0)"] --> LS_Real["LayerService"]
        LS_Real --> |Fails with Warning (Returns 0)| PR_Real["PreviewRenderService\n(Only Consumer)"]
    end
```

---

## 2. Unity Engine Baseline Configuration

### 2.1 Defined Layers in ProjectSettings ([`TagManager.asset`](file:///f:/Private/SoulsLikeTemplate/ProjectSettings/TagManager.asset))

| Layer Index | Layer Name | Project Asset Usage |
| :---: | :--- | :--- |
| **0** | `Default` | General geometry, props, camera rigs, scene objects (35,620 in scenes) |
| **1** | `TransparentFX` | Built-in Unity FX |
| **2** | `Ignore Raycast`| Built-in Unity ignore raycast layer |
| **3** | *(Unassigned)* | Reserved / Empty |
| **4** | `Water` | Water planes and volumes |
| **5** | `UI` | Canvas and UI elements |
| **6** | `Player` | Player root, [`CharacterController`](file:///f:/Private/SoulsLikeTemplate/Assets/Prefabs/Character/Character.prefab), `RightFistRuntime` hitbox collider |
| **7** | `Enemy` | Enemy root, [`CharacterController`](file:///f:/Private/SoulsLikeTemplate/Assets/Prefabs/Enemy/ErikaMeleeEnemy.prefab), `TargetLockNode` |
| **8** | `Walkable` | Walkable environmental terrain and surfaces (1,172 in scenes) |
| **9** | `Stairs` | Stair colliders in scenes (87 in scenes) |

### 2.2 Physics Collision Matrix ([`DynamicsManager.asset`](file:///f:/Private/SoulsLikeTemplate/ProjectSettings/DynamicsManager.asset))
Inspected and verified via `Physics.GetIgnoreLayerCollision(i, j)`:
- **Default Collision:** All layers collide with each other.
- **Ignored Layer Pair:** **`Player` (Layer 6)** ignores collision with **`Stairs` (Layer 9)**.
- **Dynamic Collision Code:** No calls to `Physics.IgnoreCollision` or `Physics.IgnoreLayerCollision` exist in the codebase. Collision filtering relies strictly on the global matrix and `LayerMask` queries.

---

## 3. Detailed Audit: `LayerService`, `LayerData`, and `LayerName`

### 3.1 `LayerName` Enum ([`LayerName.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/LayerName.cs))
```csharp
namespace SoulsLike.Services.Layer
{
    public enum LayerName
    {
        Player = 0,
        Water = 1,
        UI = 2,
        Terrain = 3,
        Ground = 4,
        Preview = 5,
        Interaction = 6,
    }
}
```
- **Design Role:** Semantic keys for dictionary indexing in Unity Inspector.
- **Missing Semantic Keys:** Real project concepts like `Enemy`, `Walkable`, and `Stairs` do not have dedicated enum entries.

### 3.2 `LayerData` ScriptableObject ([`LayerData.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/Data/LayerData.cs))
```csharp
[CreateAssetMenu(fileName = "LayerData", menuName = "Data/LayerData")]
public class LayerData : Model.Data
{
    [SerializeField] private SerializedDictionary<LayerName, LayerMask> layers;
    public Dictionary<LayerName, LayerMask> Layers => layers.Dictionary;
}
```
Only one asset instance exists in the project: [`Assets/Settings/Data/LayerData.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Data/LayerData.asset).

#### Current Serialized Content in `LayerData.asset`:
```yaml
layers:
  keyValue:
  - key: 0         # LayerName.Player
    value:
      serializedVersion: 2
      m_Bits: 0    # Mask is 0 (Nothing)!
  - key: 7         # Out of enum bounds (LayerName only has 0..6)
    value:
      serializedVersion: 2
      m_Bits: 4294967295 # ~0 (Everything)
  - key: 8         # Out of enum bounds
    value:
      serializedVersion: 2
      m_Bits: 4294967295 # ~0 (Everything)
```
- Keys `1` (`Water`), `2` (`UI`), `3` (`Terrain`), `4` (`Ground`), `5` (`Preview`), and `6` (`Interaction`) are **absent**.
- Key `0` (`Player`) has bitmask `0`, meaning no layers are selected.
- Keys `7` and `8` have full bitmasks (`~0`), but do not map to valid `LayerName` enum members.

### 3.3 `LayerService` Implementation ([`LayerService.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/LayerService.cs))
- **`GetLayerMask(LayerName name)`**: Returns `_layerData.Layers[name]`. If missing, logs `Debug.LogWarning($"[LayerService] LayerMask for {name} not found in LayerData.")` and returns `0`.
- **`GetLayer(LayerName name)`**: Computes mask and extracts the first set bit index (`1 << i`). If mask is `0`, returns `0` (`Default`).
- **`SetLayer(GameObject go, LayerName name, bool recursive)`**: Applies the layer returned by `GetLayer` to the GameObject hierarchy.

### 3.4 Dependency Injection Registration ([`ProjectScope.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/VContainer/ProjectScope.cs#L43-L44))
```csharp
builder.RegisterScriptableObject<LayerData>();
builder.Register<LayerService>(Lifetime.Singleton).As<ILayerService>();
```
The service is correctly registered as a singleton across the project lifetime.

### 3.5 The Sole Consumer: `PreviewRenderService` ([`PreviewRenderService.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/PreviewRender/PreviewRenderService.cs))
- **Line 45:** `previewCamera.cullingMask = _layerService.GetLayerMask(LayerName.Preview);`
  - **Result:** Look up fails because `LayerName.Preview` is missing from `LayerData.asset`. It logs a warning and returns `0`, effectively setting the preview camera culling mask to `Nothing`.
- **Line 110:** `// _layerService.SetLayer(animatorComponentInstance.gameObject, LayerName.Preview);`
  - **Result:** The recursive layer assignment call was commented out.

---

## 4. Audit: Runtime Systems (Physics Raycasting & Colliders)

| Subsystem | File / Component | Direct Layer Usage | Uses `LayerService`? | Details & Architectural Assessment |
| :--- | :--- | :--- | :---: | :--- |
| **Locomotion / Grounding** | [`MovementComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Movement/MovementComponent.cs#L442) | [`MovementData.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Player/MovementData.asset) `GroundLayers` (`4294966783`) | **NO** | Uses `Physics.SphereCastNonAlloc` with `Model.GroundLayers`. The mask `4294966783` equals `~512` (all layers except Layer 9 `Stairs`). Bypasses `LayerService.GetLayerMask(LayerName.Ground)`. |
| **Enemy Perception (Line of Sight)** | [`EnemyPerception.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyPerception.cs#L133) | [`EnemyBehaviourProfile.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Enemy/ErikaMeleeBehaviour.asset) `lineOfSightMask` (`119`) | **NO** | Uses `Physics.Linecast` with `profile.LineOfSightMask`. The mask `119` (`0b01110111`) hits Default, TransparentFX, Ignore Raycast, Water, UI, and Player, but deliberately excludes Enemy (bit 7) and Stairs (bit 9). Bypasses `LayerService`. |
| **Interaction Probing** | [`InteractionController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Interactions/InteractionController.cs#L110) | `Physics.AllLayers` | **NO** | Uses `Physics.OverlapSphereNonAlloc` with `Physics.AllLayers`. Filters candidates via `collider.GetComponentInParent<IInteractable>()`. Completely ignores `LayerName.Interaction` and `LayerService`. |
| **Character Aiming** | [`Character.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Character.cs#L47) | Serialized Field `aimLayerMask` | **NO** | `[SerializeField] private LayerMask aimLayerMask;` is declared on `Character`, but never referenced in any method or calculation. Dead code. |
| **Melee Combat Hitboxes** | [`MeleeHitboxController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Combat/MeleeHitboxController.cs#L70) | Unity Matrix & `IEntityLocator` | **NO** | Hitbox collider is on Layer 6 (`Player`). Detection uses `OnTriggerEnter` and queries `IEntityLocator.TryGetEntity`. Filtering relies on the Physics matrix and entity lookup rather than dynamic layer masks. |

---

## 5. Audit: Editor Utilities

| Tool Name | Script Path | Layer References | Uses `LayerData`? | Assessment |
| :--- | :--- | :--- | :---: | :--- |
| **NavMesh Bake Tool** | [`DefaultLocationNavMeshBakeTool.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/DefaultLocationNavMeshBakeTool.cs#L30) | `const int NAVIGATION_LAYER_MASK = 55;`<br>`surface.layerMask = NAVIGATION_LAYER_MASK;` | **NO** | Hardcodes bitmask `55` (`0b00110111`, layers 0, 1, 2, 4, 5). Does not read `LayerData.asset` or `LayerMask.GetMask`. |
| **Enemy Authoring Validator** | [`EnemyAuthoringValidator.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/EnemyAuthoringValidator.cs) | Checks Animator `"Base Layer"` and triggers. | **NO** | Does not validate whether enemy prefabs or root GameObjects are assigned to Layer 7 (`Enemy`). |
| **Ground Item Prefab Builder** | [`GroundItemPrefabBuilder.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/GroundItemPrefabBuilder.cs) | Disables UI `raycastTarget`; adds `SphereCollider`. | **NO** | Leaves ground item prefabs on Layer 0 (`Default`). |
| **Player Animator Tool** | [`PlayerAnimatorTool.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/PlayerAnimatorTool.cs) | `animator.GetLayerName(layerIndex)` | **NO** | Operates strictly on Mecanim Animator controller layers, not Unity GameObject layers. |
| **Other Utilities** | `OcclusionOptimizer.cs`, `ToolbarSceneTools.cs`, `InventoryEquipmentBootstrap.cs` | None | **NO** | No layer interaction. |

---

## 6. Root Causes of the Architectural Disconnect

1. **Self-Contained Data Assets vs. Centralized Service:**
   Data-driven gameplay systems ([`MovementData`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Movement/MovementData.cs), [`EnemyBehaviourProfile`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Enemy/EnemyBehaviourProfile.cs)) were implemented as standalone ScriptableObjects. In Unity, ScriptableObjects cannot easily resolve runtime DI services like `ILayerService` at edit time, so developers serialized raw `LayerMask` properties directly on each asset.
2. **Editor Context Availability:**
   Editor tools (such as [`DefaultLocationNavMeshBakeTool`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/DefaultLocationNavMeshBakeTool.cs)) run outside of VContainer DI containers. Because `LayerService` was packaged strictly as an injectable runtime service, Editor tools bypassed it rather than loading `LayerData.asset` via `AssetDatabase`.
3. **Incomplete Asset Migration:**
   [`LayerData.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Data/LayerData.asset) was created during early project scaffolding (commits `ee08da48`, `d56336ea`, `68356b85`) with placeholder values (key 0, 7, 8) and was never fully populated with the production project layers.

---

## 7. Recommendations & Actionable Remediation Roadmap

For any engineer or AI agent task assigned to unify the layer architecture:

### Step 1: Align `LayerName` Enum
Expand [`LayerName.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Layer/LayerName.cs) to cover all semantic concepts required across the project:
```csharp
namespace SoulsLike.Services.Layer
{
    public enum LayerName
    {
        Default = 0,
        Player = 1,
        Enemy = 2,
        Ground = 3,
        Walkable = 4,
        Stairs = 5,
        Water = 6,
        UI = 7,
        Preview = 8,
        Interaction = 9,
    }
}
```

### Step 2: Populate `LayerData.asset`
Configure [`LayerData.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Data/LayerData.asset) with the actual Unity `LayerMask` values:
- `Default` $\rightarrow$ Layer 0 (`Default`)
- `Player` $\rightarrow$ Layer 6 (`Player`)
- `Enemy` $\rightarrow$ Layer 7 (`Enemy`)
- `Ground` $\rightarrow$ Layers 0 + 8 (`Default` + `Walkable`, mask `1 + 256 = 257`)
- `Walkable` $\rightarrow$ Layer 8 (`Walkable`, mask `256`)
- `Stairs` $\rightarrow$ Layer 9 (`Stairs`, mask `512`)
- `Water` $\rightarrow$ Layer 4 (`Water`, mask `16`)
- `UI` $\rightarrow$ Layer 5 (`UI`, mask `32`)
- `Preview` $\rightarrow$ Dedicated preview layer or designated culling layer

### Step 3: Enable Static/Editor Access on `LayerData`
Add a static editor lookup helper (e.g., `LayerData.LoadDefault()` or `LayerData.GetDefaultMask(LayerName)`) so Editor tools like [`DefaultLocationNavMeshBakeTool.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/DefaultLocationNavMeshBakeTool.cs) can replace magic numbers (like `55`) with `layerData.Layers[LayerName.Ground]`.

### Step 4: Fix `PreviewRenderService`
Once `LayerName.Preview` is populated in `LayerData.asset`, verify that `_layerService.GetLayerMask(LayerName.Preview)` resolves to a valid culling mask, and restore/test the object layer assignment in `SetupPreview`.

### Step 5: Clean Up Dead Code
Remove `[SerializeField] private LayerMask aimLayerMask;` from [`Character.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Character.cs#L47) if aiming does not use physics raycasting.
