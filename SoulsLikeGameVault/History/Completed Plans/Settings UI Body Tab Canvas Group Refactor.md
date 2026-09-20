---
title: Settings UI Body Tab Canvas Group Refactor
type: plan
domains:
  - ui
  - settings
status: done
authority: advisory
updated: 2026-09-20
aliases:
  - Settings_UI_Body_Tab_Canvas_Group_Refactor
tags:
  - work/plan
  - status/done
---

# Settings UI Body Tab Canvas Group Refactor

## Plan Contract

### Goal
Refactor the hierarchy and presentation logic of `Panel/Body` in [`SettingsUi.prefab`](file:///f:/Private/SoulsLikeTemplate/Assets/Prefabs/Ui/Settings/SettingsUi.prefab) and [`SettingsUi.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Settings/SettingsUi.cs) so each tab (Audio, Camera, Graphics, Controls) has a dedicated `CanvasGroup` container holding its related settings options. Replace individual GameObject enable/disable toggling during tab switching with container `CanvasGroup` visibility and interactivity management.

### Source Research and Decisions
- In the current implementation, all 11 settings options sit directly under `Panel/Body`, which has a `VerticalLayoutGroup`. Switching tabs iterates through all options and calls `option.SetVisible(option.Tab == activeTab);`, which invokes `gameObject.SetActive(visible)`.
- Using `CanvasGroup` on tab containers avoids UI hierarchy churn and GameObject activation/deactivation during tab switching.
- Standard project convention for CanvasGroup visibility is `CanvasGroupExt.SetActive(this CanvasGroup canvasGroup, bool active)` from `SoulsLike.Extensions`, which toggles `alpha = active ? 1 : 0`, `interactable = active`, and `blocksRaycasts = active`.
- `Panel/Body` no longer needs a `VerticalLayoutGroup` since its children will be 4 full-stretch tab containers (`AudioTab`, `CameraTab`, `GraphicsTab`, `ControlsTab`). Each tab container will hold its own `VerticalLayoutGroup` (with padding `16, 16, 14, 14`, spacing `8`, and upper-left child alignment) to lay out its specific options.
- All option GameObjects inside each tab container will remain active (`m_IsActive: 1`).
- [`UiConfigurationTests.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/Tests/Configuration/UiConfigurationTests.cs) validates UI prefab reference integrity, so the test should be updated to assert that `audioTabGroup`, `cameraTabGroup`, `graphicsTabGroup`, and `controlsTabGroup` are properly wired.

### Assumptions and Non-Goals
- Tab switching logic in [`SettingsUiController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Settings/SettingsUiController.cs) already passes `_activeTab` into `_settingsUi.Render()`. No changes are required in the controller.
- Tab buttons in the header (`audioTabButton`, `cameraTabButton`, etc.) and footer buttons (`applyButton`, `backButton`, etc.) remain unchanged.
- [`SettingsOptionUi.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Settings/SettingsOptionUi.cs) retains its `Tab` property and `SetVisible` helper for backward compatibility, but `SettingsUi.Render()` will no longer call `SetVisible`.

### Success Criteria
1. `SettingsUi.prefab` contains 4 tab containers under `Panel/Body` (`AudioTab`, `CameraTab`, `GraphicsTab`, `ControlsTab`), each with `RectTransform`, `CanvasGroup`, and `VerticalLayoutGroup`.
2. Related options are nested under their respective tab containers, and all option GameObjects are active.
3. `SettingsUi.cs` exposes serialized `CanvasGroup` references (`audioTabGroup`, `cameraTabGroup`, `graphicsTabGroup`, `controlsTabGroup`) and controls visibility via `CanvasGroupExt.SetActive`.
4. `UiConfigurationTests.UiPrefab_HasRequiredReferences` passes and verifies all tab group references.

## Execution Plan

- [x] Phase 1 — C# script updates: Add tab group serialized fields and `CanvasGroup` visibility logic to [`SettingsUi.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Settings/SettingsUi.cs); update [`UiConfigurationTests.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/Tests/Configuration/UiConfigurationTests.cs) to assert the new references; verify compilation.
- [x] Phase 2 — Prefab restructuring: Use Unity Editor APIs (`PrefabUtility.LoadPrefabContents`) to restructure `Panel/Body` in [`SettingsUi.prefab`](file:///f:/Private/SoulsLikeTemplate/Assets/Prefabs/Ui/Settings/SettingsUi.prefab), remove `VerticalLayoutGroup` from `Body`, instantiate the 4 tab containers with `CanvasGroup` and `VerticalLayoutGroup`, reparent option GameObjects, activate all option GameObjects, bind serialized fields in `SettingsUi`, and persist the asset with `PrefabUtility.SaveAsPrefabAsset` and `AssetDatabase.SaveAssets()`.
- [x] Phase 3 — Validation: Run UTF EditMode tests (`UiConfigurationTests`) via the Unity CLI bridge to verify reference wiring and layout integrity.

## Risks and Rollback
- Risk: Moving GameObjects within the prefab could break serialized references in `SettingsUi.options`.
  - Mitigation: Component references will be re-assigned explicitly and verified via `UiConfigurationTests`.
- Rollback: Revert prefab and script changes via git if needed.

## Validation
- Unity Test Framework EditMode tests: `UiConfigurationTests`
- Unity Console verification for import/serialization clean state.

## Execution Handoff
- Target files:
  - [`Assets/Scripts/Ui/Settings/SettingsUi.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Settings/SettingsUi.cs)
  - [`Assets/Prefabs/Ui/Settings/SettingsUi.prefab`](file:///f:/Private/SoulsLikeTemplate/Assets/Prefabs/Ui/Settings/SettingsUi.prefab)
  - [`Assets/Scripts/Editor/Tests/Configuration/UiConfigurationTests.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Editor/Tests/Configuration/UiConfigurationTests.cs)
- Context keys: `ui-code`
