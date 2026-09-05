# Layer Service Remediation Plan

**Repository:** `GolinSky/souls-like-template`  
**Status:** Completed (Commit `9dcc0e9e`)  
**Baseline:** current `main` branch reviewed together with `LAYER_SYSTEM_AUDIT_REPORT.md`  
**Goal:** repair the broken layer configuration, make invalid configuration fail clearly, remove uncontrolled hardcoded masks, and keep feature-specific physics rules with the feature that owns them.

---

## 1. Review conclusion

The audit is correct about the immediate failures:

- `LayerData.asset` is invalid and incomplete.
- `LayerService.GetLayer()` silently converts missing/empty configuration to Unity layer `0` (`Default`).
- `GetLayer()` also accepts a multi-layer mask and silently chooses its first set bit.
- `PreviewRenderService` receives an empty culling mask and does not assign the instantiated preview object to the preview layer.
- `InteractionController` queries `Physics.AllLayers`.
- `DefaultLocationNavMeshBakeTool` uses the magic mask `55`.
- `Character.aimLayerMask` is dead serialized data.

The audit should **not** be implemented literally in one area: `LayerName` currently mixes two different concepts.

1. **A GameObject layer identity** — exactly one Unity layer, used by `GameObject.layer` and `SetLayer`.
2. **A physics/culling/navigation query mask** — one or many Unity layers, used by raycasts, overlap queries, cameras, and NavMesh baking.

Keeping both concepts under the same enum/API allows a composite mask such as `Ground` to be passed into `SetLayer`. The remediation must separate them.

---

## 2. Architecture decisions

### 2.1 Keep the existing service and VContainer registration

Do not replace VContainer registration and do not add another runtime service.

Keep:

```csharp
builder.RegisterScriptableObject<LayerData>();
builder.Register<LayerService>(Lifetime.Singleton).As<ILayerService>();
```

The registration is already correct. The problem is the data and service contract.

### 2.2 `LayerName` represents only one Unity layer

`LayerName` must contain only concepts that can be assigned to `GameObject.layer`:

```csharp
public enum LayerName
{
    Default,
    Water,
    UI,
    Player,
    Enemy,
    Walkable,
    Stairs,
    Preview,
    Interaction,
}
```

Remove `Ground` because it is a query concept, not a GameObject layer.

Remove `Terrain` unless a real Unity `Terrain` layer is intentionally added to `TagManager.asset`.

> Serialization safety: do not simply reorder/renumber the existing enum and allow Unity to reinterpret the current integer dictionary keys. Rewrite `LayerData.asset` atomically while preserving the asset GUID. Use explicit enum values during migration if needed.

### 2.3 Add `LayerMaskName` for shared composite masks

Create:

`Assets/Scripts/Services/Layer/LayerMaskName.cs`

Initial keys:

```csharp
public enum LayerMaskName
{
    PreviewCamera,
    InteractionProbe,
    NavigationBake,
}
```

Do not automatically put every feature mask here. A mask belongs in `LayerData` only when it is a project-wide shared rule.

Keep these masks with their owning feature:

- movement ground probing → `MovementData`;
- enemy line of sight → `EnemyBehaviourProfile`.

This avoids injecting `ILayerService` into every gameplay component merely to move data out of the feature that owns it.

### 2.4 Fail fast instead of returning `Default`

A missing or invalid layer configuration is an authoring error, not a recoverable runtime condition.

The service must throw a clear `InvalidOperationException` for:

- a missing `LayerName` entry;
- a single-layer entry with mask `0`;
- a single-layer entry with more than one bit set;
- a missing required `LayerMaskName` entry;
- a required shared mask with value `0`.

Do not log a warning and return `0`.

### 2.5 Separate configuration migration from gameplay tuning

First migrate the architecture while preserving current behavior where possible. Then tune questionable masks in a separate change.

This prevents grounding, perception, interaction, and NavMesh behavior from all changing in the same commit as the service refactor.

---

## 3. Target API

### 3.1 `ILayerService`

Update `Assets/Scripts/Services/Layer/ILayerService.cs`:

```csharp
public interface ILayerService
{
    LayerMask GetLayerMask(LayerName name);
    int GetLayer(LayerName name);
    LayerMask GetMask(LayerMaskName name);
    void SetLayer(GameObject gameObject, LayerName name, bool recursive = true);
}
```

Contract:

- `GetLayerMask(LayerName)` always returns exactly one bit.
- `GetLayer(LayerName)` returns its Unity layer index.
- `GetMask(LayerMaskName)` may return one or several bits.
- `SetLayer(...)` accepts only `LayerName`, so a composite mask cannot be assigned accidentally.

### 3.2 `LayerData`

Update `Assets/Scripts/Services/Layer/Data/LayerData.cs`:

```csharp
[SerializeField]
private SerializedDictionary<LayerName, LayerMask> singleLayers;

[SerializeField]
private SerializedDictionary<LayerMaskName, LayerMask> sharedMasks;
```

Do not expose the mutable dictionaries publicly.

Provide narrow lookup methods:

```csharp
public bool TryGetLayerMask(LayerName name, out LayerMask mask);
public bool TryGetMask(LayerMaskName name, out LayerMask mask);
```

Optional read-only access may be exposed for editor validation, but callers must not be able to mutate the dictionaries at runtime.

### 3.3 `LayerService`

Refactor `Assets/Scripts/Services/Layer/LayerService.cs`:

1. `GetLayerMask` loads from `singleLayers` and validates that the mask is one-hot.
2. `GetLayer` extracts the bit index only after one-hot validation.
3. `GetMask` loads from `sharedMasks` and validates that the required mask is nonzero.
4. `SetLayer` resolves the integer layer once, then applies that integer recursively.
5. A private recursive helper performs assignment without repeating dictionary lookup for every child.
6. Reject a null root GameObject with `ArgumentNullException`.

One-hot validation should use unsigned bits so layer 31 is handled safely:

```csharp
uint bits = unchecked((uint)mask.value);
bool isSingleBit = bits != 0 && (bits & (bits - 1)) == 0;
```

Do not retain the current “first matching bit wins” behavior.

---

## 4. Implementation phases

## Phase 1 — Add validation before changing consumers

### Files

- `Assets/Scripts/Services/Layer/LayerName.cs`
- `Assets/Scripts/Services/Layer/LayerMaskName.cs` — new
- `Assets/Scripts/Services/Layer/Data/LayerData.cs`
- `Assets/Scripts/Services/Layer/ILayerService.cs`
- `Assets/Scripts/Services/Layer/LayerService.cs`

### Tasks

1. Split single layers from shared masks.
2. Encapsulate dictionary access.
3. Add strict one-hot validation for single layers.
4. Add missing/nonzero validation for shared masks.
5. Replace warning-plus-zero fallbacks with clear exceptions.
6. Optimize recursive assignment so the service resolves the layer once.
7. Leave `ProjectScope` registration unchanged.

### Acceptance criteria

- A missing key cannot silently become `Default`.
- A zero single-layer mask fails clearly.
- A multi-layer single-layer entry fails clearly.
- A valid shared composite mask is returned unchanged.
- `SetLayer` cannot accept `LayerMaskName`.

---

## Phase 2 — Repair Unity layers and `LayerData.asset`

### Files

- `ProjectSettings/TagManager.asset`
- `ProjectSettings/DynamicsManager.asset`
- `Assets/Settings/Data/LayerData.asset`

### Tasks

1. Keep existing Unity layers:
   - `Default` = 0
   - `Water` = 4
   - `UI` = 5
   - `Player` = 6
   - `Enemy` = 7
   - `Walkable` = 8
   - `Stairs` = 9
2. Add dedicated layers in currently unused slots:
   - `Preview` = 10
   - `Interaction` = 11
3. Review the collision matrix:
   - preview objects must not participate in gameplay collisions;
   - interaction detection colliders should be query-oriented and must not create unwanted physical contacts.
4. Preserve the current `LayerData.asset` GUID.
5. Remove all malformed serialized entries, including old keys `7` and `8` containing `Everything`.
6. Populate every `LayerName` entry using the Inspector layer selections, not hand-authored magic integers.
7. Populate the shared masks:
   - `PreviewCamera` → `Preview` only;
   - `InteractionProbe` → `Default | Interaction` during migration;
   - `NavigationBake` → start with `Default | Walkable | Stairs`, then verify against the authored location scenes before finalizing.

Current bit values, only as a verification aid:

| Semantic layer | Unity index | One-hot mask |
|---|---:|---:|
| Default | 0 | 1 |
| Water | 4 | 16 |
| UI | 5 | 32 |
| Player | 6 | 64 |
| Enemy | 7 | 128 |
| Walkable | 8 | 256 |
| Stairs | 9 | 512 |
| Preview | 10 | 1024 |
| Interaction | 11 | 2048 |

Do not make gameplay code depend on these integer constants.

### Acceptance criteria

- No out-of-enum dictionary keys remain.
- Every `LayerName` entry is present and one-hot.
- Every required shared mask is present and nonzero.
- Opening the project produces no LayerService configuration warnings/errors.

---

## Phase 3 — Fix preview rendering first

### File

- `Assets/Scripts/Services/PreviewRender/PreviewRenderService.cs`

### Tasks

1. Replace:

```csharp
previewCamera.cullingMask = _layerService.GetLayerMask(LayerName.Preview);
```

with:

```csharp
previewCamera.cullingMask = _layerService.GetMask(LayerMaskName.PreviewCamera);
```

2. Restore recursive layer assignment after instantiating the preview object:

```csharp
_layerService.SetLayer(
    animatorComponentInstance.gameObject,
    LayerName.Preview,
    recursive: true);
```

3. Apply the layer before the preview camera performs its first render.
4. Verify all instantiated child renderers are on `Preview`.
5. Verify the gameplay camera does not accidentally render the preview object. If gameplay cameras currently use `Everything`, explicitly exclude `Preview` from their culling masks.
6. Verify cleanup still destroys the generated preview instance and render texture resources correctly.

### Acceptance criteria

- Preview camera culling mask is nonzero.
- The item/character preview appears.
- Preview hierarchy is recursively assigned to layer 10.
- No preview object appears in the gameplay world camera.

---

## Phase 4 — Replace uncontrolled runtime query masks

## 4.1 Interaction probing

### File

- `Assets/Scripts/Interactions/InteractionController.cs`

### Tasks

1. Inject `ILayerService` into `InteractionController`.
2. Resolve and cache `LayerMaskName.InteractionProbe` in the constructor.
3. Replace `Physics.AllLayers` in `OverlapSphereNonAlloc` with the cached mask.
4. Do not query the service every frame/probe.
5. Migrate dedicated interaction detection colliders to the `Interaction` layer.
6. Do not recursively move an entire visual prefab to `Interaction` unless that is intentional. Prefer a dedicated collider child when the visual hierarchy should stay on `Default`.
7. Keep `Default` in `InteractionProbe` while existing interactables are being migrated.
8. After a project scan confirms all interactables use the dedicated layer, narrow the mask to `Interaction` only.

### Acceptance criteria

- `InteractionController` contains no `Physics.AllLayers`.
- Existing grace points, doors, pickups, and other interactables remain discoverable.
- Unrelated world/player/enemy colliders are no longer scanned after final migration.

## 4.2 Movement grounding

### Files

- `Assets/Scripts/Components/Movement/MovementData.cs`
- `Assets/Scripts/Components/Movement/MovementComponent.cs`
- corresponding movement data asset

### Decision

Keep this mask in `MovementData`. It is movement behavior, not a general layer identity, and `MovementComponent` already consumes it through its model. Do not inject `ILayerService` into `MovementComponent` only to centralize the field.

### Tasks

1. Rename `GroundLayers` to `GroundProbeMask` for clarity.
2. Use `[FormerlySerializedAs("GroundLayers")]` to preserve the asset value.
3. Add validation that the mask is not zero.
4. Preserve the current mask during the architecture migration.
5. In a separate gameplay-tuning commit, test changing the current “everything except Stairs” mask to the intended minimum, likely `Default | Walkable`.
6. Test stairs independently before excluding or including `Stairs`; do not infer the correct value only from the layer name.

### Acceptance criteria

- No movement behavior changes in the core LayerService refactor.
- Ground probing works on current terrain, ordinary `Default` geometry, slopes, edges, and stairs.
- The final mask does not include UI, Player, Enemy, or unrelated trigger-only layers without an explicit reason.

## 4.3 Enemy line of sight

### Files

- `Assets/Scripts/Entities/Enemy/EnemyBehaviourProfile.cs`
- `Assets/Scripts/Entities/Enemy/EnemyPerception.cs`
- enemy behavior profile assets

### Decision

Keep `LineOfSightMask` in `EnemyBehaviourProfile` because LOS filtering may legitimately vary per enemy archetype/profile.

### Tasks

1. Preserve the field and current behavior during the core migration.
2. Add profile validation that the mask is nonzero.
3. In a separate behavior-tuning commit, review the current mask `119`.
4. Confirm which layers should:
   - block vision;
   - represent the target player;
   - be ignored, such as UI and `Ignore Raycast` unless deliberately required.
5. Test walls, doors, water, stairs, the target player, and another enemy between observer and target.
6. Keep the existing entity-ID verification after the linecast.

### Acceptance criteria

- LOS remains profile-owned.
- Player is detected when unobstructed.
- Real world blockers obstruct vision.
- UI and unrelated colliders do not affect LOS unless explicitly intended.
- Another enemy blocks or does not block vision according to an explicit design decision.

## 4.4 Melee hitboxes

No LayerService migration is required for `MeleeHitboxController` in this task.

Its trigger + `IEntityLocator` filtering is a separate collision-routing design and should remain unchanged unless combat collision rules are reviewed independently.

---

## Phase 5 — Fix editor access and NavMesh baking

### Files

- `Assets/Scripts/Editor/DefaultLocationNavMeshBakeTool.cs`
- `Assets/Scripts/Editor/LayerDataEditorProvider.cs` — new

### Tasks

1. Add one editor-only loader for the canonical asset:

```text
Assets/Settings/Data/LayerData.asset
```

2. Use `AssetDatabase.LoadAssetAtPath<LayerData>()` only in the editor assembly/folder.
3. Fail with a clear editor dialog/exception when the asset is missing or invalid.
4. Replace:

```csharp
private const int NAVIGATION_LAYER_MASK = 55;
```

with the configured `LayerMaskName.NavigationBake` value.
5. Do not add `AssetDatabase`, `Resources.Load`, or a static editor path lookup to runtime `LayerService`.
6. Verify the bake source mask includes intended walkable geometry and excludes UI, water, FX, player, enemies, preview objects, and interaction-only probe colliders unless intentionally needed.

### Acceptance criteria

- The integer mask `55` is removed.
- The tool refuses to bake with invalid layer configuration.
- `Default`, `Walkable`, and intended stair geometry contribute to the NavMesh.
- UI/preview/interaction detection objects do not contribute.

---

## Phase 6 — Add authoring validation

### New files

Suggested locations:

- `Assets/Scripts/Editor/LayerConfigurationValidator.cs`
- `Assets/Tests/EditMode/Services/Layer/LayerServiceTests.cs`
- `Assets/Tests/EditMode/Services/Layer/LayerConfigurationTests.cs`

Use the repository's actual test folder/assembly conventions if they differ.

### `LayerData` validation rules

Validate in `OnValidate`, a custom inspector, or a shared validator called by both:

1. Every `LayerName` key exists exactly once.
2. Every single-layer mask is one-hot.
3. The selected bit maps to an assigned Unity layer name.
4. Required `LayerMaskName` keys exist.
5. Required shared masks are nonzero.
6. No serialized key exists outside its enum.
7. `LayerName.Player` resolves to Unity layer `Player`, and likewise for other same-named concepts.
8. No obsolete `Ground`/`Terrain` single-layer entries remain.

### Project authoring scan

Add `Tools/SoulsLike/Validate Layer Configuration` and report:

- player prefab root not on `Player`;
- enemy prefab root not on `Enemy`;
- interactable detection collider outside `InteractionProbe`;
- scene objects expected to be walkable but outside the intended ground/navigation masks;
- missing Preview/Interaction Unity layer names;
- stale malformed `LayerData.asset` entries.

Do not silently mutate prefabs/scenes from the validator. Report exact asset paths and GameObject hierarchy paths. A separate explicit “Fix Selected” command may be added later.

---

## Phase 7 — Remove dead data and document ownership

### File

- `Assets/Scripts/Entities/Character/Character.cs`

### Tasks

1. Perform one final repository search for `aimLayerMask`.
2. Remove the unused serialized field.
3. Remove or adjust the `Aim Settings` header only if no other fields remain under it.
4. Do not add a replacement mask until aiming actually performs a physics query.

### Documentation

Add a short comment or architecture note:

- `LayerName` = assignable single Unity layer;
- `LayerMaskName` = shared project-wide composite mask;
- feature-owned query masks remain in their feature/profile data;
- no raw integer masks in code;
- no silent fallback to `Default`.

---

## 5. Tests

## 5.1 EditMode unit tests for `LayerService`

Cover:

1. valid one-bit mask returns the correct Unity index;
2. layer 31 one-bit mask is accepted;
3. missing `LayerName` throws;
4. zero single-layer mask throws;
5. multi-bit single-layer mask throws;
6. missing shared mask throws;
7. valid multi-bit shared mask is returned unchanged;
8. nonrecursive `SetLayer` changes only the root;
9. recursive `SetLayer` changes root and all descendants;
10. null root throws.

## 5.2 Asset/configuration tests

Cover:

1. the canonical `LayerData.asset` exists;
2. all enum entries are configured;
3. Preview and Interaction Unity layers exist;
4. single-layer mappings are one-hot and map to expected Unity layer names;
5. required shared masks are nonzero;
6. no obsolete/out-of-range serialized keys remain.

## 5.3 PlayMode/manual smoke tests

### Preview

- open inventory/equipment preview;
- verify object is visible in preview;
- verify it is absent from world camera;
- repeatedly open/close preview and check cleanup.

### Interaction

- approach and interact with each current interactable category;
- verify nearest/priority selection remains correct;
- verify unrelated colliders do not enter candidate processing.

### Movement

- stand/move on Default geometry;
- move across Walkable terrain;
- climb and descend stairs;
- test edges, slopes, jumps, landing, and ground snap.

### Enemy LOS

- clear line to player;
- solid wall between enemy and player;
- another enemy in the line;
- doors, stairs, and water where relevant.

### NavMesh

- bake the default location;
- inspect source inclusion;
- verify expected connected surfaces and stairs;
- verify no UI/preview/interaction-only geometry contributes.

---

## 6. Recommended implementation order / pull requests

### PR 1 — Core contract and valid data

- split `LayerName` and `LayerMaskName`;
- refactor `LayerData`, `ILayerService`, and `LayerService`;
- repair TagManager and `LayerData.asset`;
- add LayerService unit/configuration tests;
- keep `ProjectScope` unchanged.

### PR 2 — Preview and interaction consumers

- fix `PreviewRenderService`;
- replace `Physics.AllLayers` in `InteractionController`;
- add Preview/Interaction authoring migration and smoke tests.

### PR 3 — Editor/NavMesh and validation

- add editor-only LayerData provider;
- remove mask `55`;
- add project validator and NavMesh verification.

### PR 4 — Feature-mask cleanup

- rename/validate movement ground mask;
- tune ground mask only after locomotion regression tests;
- review/tune enemy LOS masks;
- remove `Character.aimLayerMask`.

Keeping mask tuning in PR 4 makes regressions easier to identify and revert.

---

## 7. Definition of done

The layer audit is resolved when all of the following are true:

- `LayerData.asset` has no invalid, missing, zero, or accidental `Everything` entries.
- `LayerService` never silently falls back to `Default`.
- Assignable layers and composite masks use different key types.
- `SetLayer` can only consume a one-hot `LayerName`.
- Preview rendering works and uses a dedicated Preview layer.
- `InteractionController` no longer uses `Physics.AllLayers`.
- `DefaultLocationNavMeshBakeTool` no longer contains mask `55`.
- Movement and enemy LOS mask ownership is explicit and validated.
- The dead `aimLayerMask` field is removed.
- Layer configuration tests and the editor validator pass.
- Manual preview, interaction, movement, LOS, and NavMesh smoke tests pass.

---

## 8. Explicit non-goals

Do not include these unrelated changes in the layer remediation:

- changing `IEntityLocator` or entity command architecture;
- rewriting melee trigger detection;
- broad collision-matrix redesign beyond Preview/Interaction requirements;
- replacing VContainer registration;
- adding a second layer service;
- moving every feature-specific mask into one global asset merely for centralization.
