# Graph Report - Assets/Scripts  (2026-08-07)

## Corpus Check
- Corpus is ~17,084 words - fits in a single context window. You may not need a graph.

## Summary
- 715 nodes · 815 edges · 64 communities (28 shown, 36 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Camera Service|Camera Service]]
- [[_COMMUNITY_Character Input Bindings|Character Input Bindings]]
- [[_COMMUNITY_Animator Component|Animator Component]]
- [[_COMMUNITY_Game Orchestrator Contracts|Game Orchestrator Contracts]]
- [[_COMMUNITY_Character Entity|Character Entity]]
- [[_COMMUNITY_Custom Button Component|Custom Button Component]]
- [[_COMMUNITY_UI Asset Mappings|UI Asset Mappings]]
- [[_COMMUNITY_VContainer Core Scope|VContainer Core Scope]]
- [[_COMMUNITY_Camera and Layers|Camera and Layers]]
- [[_COMMUNITY_Character Base Components|Character Base Components]]
- [[_COMMUNITY_Scene Reference Data|Scene Reference Data]]
- [[_COMMUNITY_Core Game Orchestrator|Core Game Orchestrator]]
- [[_COMMUNITY_Component Mediator Events|Component Mediator Events]]
- [[_COMMUNITY_File Save Service|File Save Service]]
- [[_COMMUNITY_Main Menu UI|Main Menu UI]]
- [[_COMMUNITY_Health Component|Health Component]]
- [[_COMMUNITY_Custom Button Editors|Custom Button Editors]]
- [[_COMMUNITY_UI Factory Services|UI Factory Services]]
- [[_COMMUNITY_Base UI Components|Base UI Components]]
- [[_COMMUNITY_Health Component Contract|Health Component Contract]]
- [[_COMMUNITY_UI Input Bindings|UI Input Bindings]]
- [[_COMMUNITY_Button Hierarchy Tools|Button Hierarchy Tools]]
- [[_COMMUNITY_Pause Menu Controller|Pause Menu Controller]]
- [[_COMMUNITY_Player Controller|Player Controller]]
- [[_COMMUNITY_Addressable Asset Service|Addressable Asset Service]]
- [[_COMMUNITY_Main Menu Controller|Main Menu Controller]]
- [[_COMMUNITY_Custom Button Contract|Custom Button Contract]]
- [[_COMMUNITY_Input Service Lifecycle|Input Service Lifecycle]]
- [[_COMMUNITY_Scene Model|Scene Model]]
- [[_COMMUNITY_Storage Registry|Storage Registry]]
- [[_COMMUNITY_Game Orchestrator|Game Orchestrator]]
- [[_COMMUNITY_Main Menu Orchestration|Main Menu Orchestration]]
- [[_COMMUNITY_Serialized Key Values|Serialized Key Values]]
- [[_COMMUNITY_Timer Contract|Timer Contract]]
- [[_COMMUNITY_Movement Component Contract|Movement Component Contract]]
- [[_COMMUNITY_Game State Notifier|Game State Notifier]]
- [[_COMMUNITY_Main Menu Contract|Main Menu Contract]]
- [[_COMMUNITY_Layer Service Contract|Layer Service Contract]]
- [[_COMMUNITY_Main Menu Presenter|Main Menu Presenter]]
- [[_COMMUNITY_Pause Menu Presenter|Pause Menu Presenter]]
- [[_COMMUNITY_Health Model|Health Model]]
- [[_COMMUNITY_Movement Model|Movement Model]]
- [[_COMMUNITY_UI Controller Factory|UI Controller Factory]]
- [[_COMMUNITY_Occlusion Editor Tool|Occlusion Editor Tool]]
- [[_COMMUNITY_Game State Observer|Game State Observer]]
- [[_COMMUNITY_Preview Render Contract|Preview Render Contract]]
- [[_COMMUNITY_Unity Dictionary Factory|Unity Dictionary Factory]]
- [[_COMMUNITY_Canvas Group Extensions|Canvas Group Extensions]]
- [[_COMMUNITY_Timer Factory|Timer Factory]]
- [[_COMMUNITY_Animator Model|Animator Model]]
- [[_COMMUNITY_Equipment Model|Equipment Model]]
- [[_COMMUNITY_Base Model|Base Model]]
- [[_COMMUNITY_Base Factory|Base Factory]]
- [[_COMMUNITY_Key Value Contract|Key Value Contract]]
- [[_COMMUNITY_Damage Request|Damage Request]]
- [[_COMMUNITY_Damage Result|Damage Result]]
- [[_COMMUNITY_Health Stats|Health Stats]]
- [[_COMMUNITY_Health Stat Update|Health Stat Update]]
- [[_COMMUNITY_Movement State|Movement State]]
- [[_COMMUNITY_Speed Multiplier Key|Speed Multiplier Key]]
- [[_COMMUNITY_Game State|Game State]]
- [[_COMMUNITY_Layer Names|Layer Names]]
- [[_COMMUNITY_Scene Types|Scene Types]]
- [[_COMMUNITY_UI Input Types|UI Input Types]]

## God Nodes (most connected - your core abstractions)
1. `Character` - 36 edges
2. `MovementComponent` - 27 edges
3. `AnimatorComponent` - 23 edges
4. `ICharacterActions` - 20 edges
5. `CoreGameOrchestrator` - 19 edges
6. `CameraService` - 18 edges
7. `HealthComponent` - 16 edges
8. `PlayerController` - 16 edges
9. `CustomButton` - 16 edges
10. `IComponentMediator` - 15 edges

## Surprising Connections (you probably didn't know these)
- `MovementComponent` --references--> `Dictionary`  [EXTRACTED]
  Components/Movement/MovementComponent.cs → Utilities/EditorSerialization/Dictionary/SerializedDictionary.cs
- `AnimatorComponent` --references--> `float`  [EXTRACTED]
  Components/Animator/AnimatorComponent.cs → Utilities/Timer/Timer.cs
- `AnimatorComponent` --references--> `Vector3`  [EXTRACTED]
  Components/Animator/AnimatorComponent.cs → Services/PreviewRender/PreviewRenderService.cs
- `AnimatorComponent` --references--> `bool`  [EXTRACTED]
  Components/Animator/AnimatorComponent.cs → Utilities/Timer/Timer.cs
- `Character` --references--> `Transform`  [EXTRACTED]
  Entities/Character/Character.cs → Services/Ui/UiService.cs

## Communities (64 total, 36 thin omitted)

### Community 0 - "Camera Service"
Cohesion: 0.05
Nodes (17): bool, CameraService, ICameraService, SoulsLike.Services.CameraService, CharacterController, CinemachineCamera, CinemachineThirdPersonFollow, Ease (+9 more)

### Community 1 - "Character Input Bindings"
Cohesion: 0.06
Nodes (12): IInputActionCollection2, AddCallbacks(), Disable(), Enable(), Get(), ICharacterActions, @ProjectInputActions, RemoveCallbacks() (+4 more)

### Community 2 - "Animator Component"
Cohesion: 0.06
Nodes (14): Animator, AnimatorComponent, SoulsLike.Entities.Character.Components, BaseComponent, Canvas, EquipmentComponent, SoulsLike.Entities.Character.Components.Equipment, IComponentMediator (+6 more)

### Community 3 - "Game Orchestrator Contracts"
Cohesion: 0.06
Nodes (19): Data, SoulsLike.Services, IGameOrchestrator, SoulsLike.Services, HealthData, IHealthData, SoulsLike.Entities.Character.Components.Health, InventoryData (+11 more)

### Community 4 - "Character Entity"
Cohesion: 0.07
Nodes (10): AnimatorComponent, Character, Character, CharacterScope, SoulsLike.Entities.Character, EquipmentComponent, HealthComponent, InventoryComponent (+2 more)

### Community 5 - "Custom Button Component"
Cohesion: 0.08
Nodes (12): CustomButton, System.Ui.Base, CustomButtonToggle, OnValidate(), UI.Base, Button, ColorBlock, GameObject (+4 more)

### Community 6 - "UI Asset Mappings"
Cohesion: 0.07
Nodes (17): CustomButtonMapping, UI.Base, CustomButtonToggle, AssetMappingData, SoulsLike, LayerData, SoulsLike.Services.Layer.Data, SceneData (+9 more)

### Community 7 - "VContainer Core Scope"
Cohesion: 0.09
Nodes (14): CameraService, SoulsLike.Entities.Character, SoulsLike.Entities.Character, LifetimeScope, PreviewRenderService, UiService, CoreScope, SoulsLike (+6 more)

### Community 8 - "Camera and Layers"
Cohesion: 0.1
Nodes (11): Camera, ILayerService, IPreviewRenderService, LayerService, SoulsLike.Services.Layer, LayerData, Light, PreviewRenderService (+3 more)

### Community 9 - "Character Base Components"
Cohesion: 0.11
Nodes (9): BaseComponent, IComponent, SoulsLike.Entities.Character.Components, CoroutineService, ICoroutineService, SoulsLike.Services.Coroutines, InventoryComponent, SoulsLike.Entities.Character.Components.Inventory (+1 more)

### Community 10 - "Scene Reference Data"
Cohesion: 0.11
Nodes (11): AutoUpdateReference(), OnAfterDeserializeHandler(), SceneReference, SceneReferencePropertyDrawer, SoulsLike.Services.Scenes.Data, GUIContent, GUIStyle, IComparable (+3 more)

### Community 11 - "Core Game Orchestrator"
Cohesion: 0.15
Nodes (3): CoreGameOrchestrator, ICoreGameOrchestrator, SoulsLike.Services

### Community 13 - "File Save Service"
Cohesion: 0.18
Nodes (6): IStorageRegistry, FileService, SoulsLike.Services.Save, SoulsLike.Services.Storage, StorageRegistry, string

### Community 14 - "Main Menu UI"
Cohesion: 0.12
Nodes (10): ButtonTypeMap, BaseUi, CustomButton, InputTypes, IPauseMenuPresenter, IStartable, MainMenuUi, SoulsLike.Ui.MainMenu (+2 more)

### Community 15 - "Health Component"
Cohesion: 0.2
Nodes (4): HealthComponent, SoulsLike.Entities.Character.Components.Health, HealthStats, IHealthComponent

### Community 16 - "Custom Button Editors"
Cohesion: 0.13
Nodes (7): ButtonEditor, CustomButtonEditor, UI.Base.Editor, CustomButtonToggleEditor, UI.Base.Editor, SerializedProperty, ToggleEditor

### Community 17 - "UI Factory Services"
Cohesion: 0.15
Nodes (7): AddressableAssetService, AssetMappingData, BaseFactory, SoulsLike.Extensions, VContainerExt, SoulsLike.Services, UiFactory

### Community 18 - "Base UI Components"
Cohesion: 0.24
Nodes (4): BaseUi, IBaseUi, SoulsLike.Ui.Base, CanvasGroup

### Community 22 - "Pause Menu Controller"
Cohesion: 0.22
Nodes (4): ICoreGameOrchestrator, PauseMenuUiController, SoulsLike, PauseMenuUi

### Community 23 - "Player Controller"
Cohesion: 0.18
Nodes (6): PlayerController, GameState, ICameraService, IGameStateNotifier, IGameStateObserver, ILateTickable

### Community 24 - "Addressable Asset Service"
Cohesion: 0.22
Nodes (3): AddressableAssetService, IAssetService, SoulsLike.Services.Repository

### Community 25 - "Main Menu Controller"
Cohesion: 0.2
Nodes (5): IMainMenuPresenter, MainMenuUiController, SoulsLike.Ui.MainMenu, MainMenuUi, UiController

### Community 27 - "Input Service Lifecycle"
Cohesion: 0.25
Nodes (6): IDisposable, IInitializable, IInputService, InputService, SoulsLike.Services, ProjectInputActions

### Community 30 - "Game Orchestrator"
Cohesion: 0.29
Nodes (3): GameOrchestrator, IInputService, ITickable

### Community 31 - "Main Menu Orchestration"
Cohesion: 0.29
Nodes (3): IGameOrchestrator, IMainMenuOrchestrator, MainMenuOrchestrator

### Community 32 - "Serialized Key Values"
Cohesion: 0.29
Nodes (5): KeyValue, SoulsLike, IKeyValue, TKey, TValue

### Community 40 - "Health Model"
Cohesion: 0.5
Nodes (3): HealthModel, SoulsLike.Entities.Character.Components.Health, IHealthData

### Community 41 - "Movement Model"
Cohesion: 0.5
Nodes (3): IMovementData, MovementModel, SoulsLike.Entities.Character.Components.Movement

## Knowledge Gaps
- **124 isolated node(s):** `SoulsLike.Entities.Character.Components`, `SoulsLike.Entities.Character.Components`, `Animator`, `SoulsLike.Entities.Character.Components`, `AnimatorModel` (+119 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **36 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character Entity` to `Camera Service`, `Animator Component`, `VContainer Core Scope`, `Character Base Components`, `Player Controller`, `Input Service Lifecycle`?**
  _High betweenness centrality (0.111) - this node is a cross-community bridge._
- **Why does `CoreGameOrchestrator` connect `Core Game Orchestrator` to `UI Asset Mappings`, `Main Menu UI`, `Player Controller`, `Input Service Lifecycle`, `Main Menu Orchestration`?**
  _High betweenness centrality (0.090) - this node is a cross-community bridge._
- **Why does `@ProjectInputActions` connect `Character Input Bindings` to `Animator Component`, `Input Service Lifecycle`, `UI Asset Mappings`?**
  _High betweenness centrality (0.083) - this node is a cross-community bridge._
- **What connects `SoulsLike.Entities.Character.Components`, `SoulsLike.Entities.Character.Components`, `Animator` to the rest of the system?**
  _124 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Camera Service` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._
- **Should `Character Input Bindings` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._
- **Should `Animator Component` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._