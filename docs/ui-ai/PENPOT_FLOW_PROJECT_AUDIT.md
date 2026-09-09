# Penpot Flow Project Audit

## Scope

This audit records the live project state before the Penpot proof of concept. It does not propose a new UI architecture and does not modify production UI.

## Current Runtime and Tooling

| Area | Detected state |
|---|---|
| Unity Editor | 6000.3.11f1 |
| Render pipeline | HDRP 17.3.0 |
| Unity CLI | 1.0.0-beta.6 |
| Unity Pipeline | `com.unity.pipeline` 0.5.0-exp.1 |
| Unity MCP | Existing official `unity mcp` connection is enabled and responsive |
| UI runtime | uGUI 2.0.0; no production UI Toolkit screen flow found |
| Text | TextMeshProUGUI/TMP assets |
| Dependency injection | VContainer 1.19.0 |
| Async | UniTask |
| Asset loading | Addressables 2.9.1 |

The live Editor is connected to this project, is not compiling, and reports no console errors. The active `ElevatorDemo` scene was clean during discovery.

## Existing UI Architecture

The production UI follows the registered Controller-Presenter-View workflow:

```text
VContainer scope
  -> UI controller Initialize()
  -> UiController.CreateUi<T>()
  -> UiService.CreateUi<T>()
  -> UiFactory.CreateUi<T>()
  -> AssetMappingData lookup by UI class name
  -> Addressable prefab load and instantiation
  -> child VContainer scope registers the UI component
  -> controller assigns presenter and controls visibility
```

Primary implementation points:

- `Assets/Scripts/Ui/Base/BaseUi.cs` owns `CanvasGroup` visibility and active state.
- `Assets/Scripts/Ui/Base/UiController.cs` delegates creation to `IUiService`.
- `Assets/Scripts/Services/Ui/UiService.cs` owns the normal UI parent and overlay canvas.
- `Assets/Scripts/Services/Ui/UiFactory.cs` resolves, loads, instantiates, validates, and scopes UI prefabs.
- `Assets/Scripts/Services/AssetService/Data/AssetMappingData.cs` maps UI class names to Addressable prefab references.
- `Assets/Scripts/Services/VContainer/SharedSceneScope.cs` registers `UiFactory` and `UiService`.
- `Assets/Scripts/Services/VContainer/CoreScope.cs`, `CharacterFactory.cs`, and `MainMenuScope.cs` register feature controllers according to lifetime/ownership.

`InventoryUi` and `EquipmentUi` are the closest medium-complexity references. They already demonstrate TMP, item grids, icons, selected state, controller navigation, details panels, and presenter/controller separation.

## Canvas and Layout Conventions

`Assets/Prefabs/View/Services/UiService.prefab` is the shared production Canvas root. It uses:

- Screen Space - Overlay
- `CanvasScaler` with Scale With Screen Size
- 1920 x 1080 reference resolution
- `GraphicRaycaster`
- `UiService`

Production prefabs use anchors plus uGUI layout components including `VerticalLayoutGroup`, `HorizontalLayoutGroup`, `GridLayoutGroup`, `LayoutElement`, `ContentSizeFitter`, `ScrollRect`, `Image`, and project-specific controls. Existing Inventory layout combinations must be inspected before copying to avoid recursive layout dependencies.

## Prefabs, Addressables, and Mapping

The required feature pattern is:

```text
Assets/Scripts/Ui/<FeatureName>/
Assets/Prefabs/Ui/<FeatureName>/<FeatureName>Ui.prefab
```

Every factory-created UI prefab must:

1. Derive from `BaseUi` and have its required root `CanvasGroup` reference.
2. Be Addressable in `Assets/AddressableAssetsData/AssetGroups/Ui.asset`.
3. Use the UI class name as its address.
4. Be registered in `Assets/Settings/Data/AssetMappingData.asset`.
5. Be created and controlled through the existing VContainer/UI service path.

## Reusable Project Assets

- TMP/Cinzel assets: `Assets/Art/Fonts/Cinzel/`
- Item icons: `Assets/Art/Textures/ItemIcons/`
- Menu icons: `Assets/Art/Textures/MenuIcons/`
- Sword icons: `Assets/Art/Textures/SwordIcons/`
- Existing button prefabs: `Assets/Prefabs/Ui/UiElements/`
- Reference screens: `Assets/Prefabs/Ui/Inventory/InventoryUi.prefab` and `Assets/Prefabs/Ui/Equipment/EquipmentUi.prefab`

Existing UI may use project/third-party wrappers such as `CustomButton` and `MPUIKit.MPImage`. The proof of concept must reuse compatible wrappers instead of assuming every visual or control is a raw uGUI component.

## Validation Facilities

- The official Unity MCP can query status, scenes, hierarchy, console, assets, and prefabs.
- It exposes Game/Scene screenshot capture.
- `Assets/Editor/Automation/AgentTestSafetyCommands.cs` provides the required test readiness check.
- No dedicated UI screenshot comparison harness or UI test scene was found.

Validation screenshots should be written under `Temp/UiValidation/`, not `Assets/`.

## Proposed Proof-of-Concept Location

```text
Assets/Scripts/Ui/PenpotFlow/
Assets/Prefabs/Ui/PenpotFlow/
Assets/Scenes/PenpotFlow/PenpotUiFlowTest.unity   # only if a scene is necessary
Temp/UiValidation/
```

The first test should be an isolated equipment/inventory-style subsection. Production Equipment and Inventory prefabs must remain unchanged.

## Dependencies That Must Not Be Duplicated

- Unity CLI or Unity MCP server
- VContainer scopes and registration patterns
- `UiService`, `UiFactory`, `UiController`, or `BaseUi`
- Addressables group/mapping infrastructure
- TMP font assets
- Existing button/control wrappers
- Production Canvas and input/event infrastructure

## Risks

- A standalone scene must reproduce the required `UiService` references without changing boot flow or global Canvas settings.
- `UiFactory` depends on exact class-name/address/mapping conventions.
- Controller navigation is partly explicit and cannot be validated from visual layout alone.
- Existing synchronous Addressables loading is authoritative for the experiment and is not to be refactored here.
- No current Penpot MCP server or Penpot AI Kit installation is present.
- The worktree contains unrelated user changes which are explicitly excluded from this branch's experiment changes.

## Decision

The safest first implementation target is a new `PenpotFlow` feature using existing assets and architecture. No production UI or shared Canvas modifications are required for the proof of concept.
