# Graph Report - SoulsLikeTemplate  (2026-08-20)

## Corpus Check
- 234 files · ~41,815 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2036 nodes · 3567 edges · 232 communities (99 shown, 133 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 292 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f07b7153`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- ItemDefinition
- Entity
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneType
- PlayerHudUiController
- EquipmentSlotId
- AnimatorComponent
- Character
- IGameStateNotifier
- bool
- EquipmentUiController
- EquipmentUi
- AudioService
- EquipmentComponent
- CharacterRuntime.cs
- InventorySlotUI
- .StartSwap
- InventoryEntry
- UiService
- CameraService
- PauseNavigationUiController
- SoulsLike
- OnGuiFpsCounter
- CharacterActions
- PauseMenuUiController
- AmbienceService
- Animator
- AttackComponent
- InventoryData.cs
- CustomButtonToggle
- EquipmentSlotChange
- SpeedMultiplierKey
- InputService
- SoulsLike.Entities.Character.Components.Health
- MainMenuOrchestrator
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- IHealthComponent
- InventoryItemViewData
- AudioClip
- LockOnUiController
- PlayerController
- System.Ui.Base
- .CreateButton
- SoulsLike.Entities.Character.Components.Movement
- InventoryUi
- ColorConverter
- SceneService
- ICameraService
- EquipmentSlotUI
- InventoryUiController
- Vector2
- PauseNavigationUi
- MovementComponent
- IContainerBuilder
- CustomButtonMapping
- TargetLockNode
- float
- CoroutineService
- IUniqueIdGenerator
- Quaternion
- MainMenuUiController
- SaveService
- ICustomButton
- InventoryItemSO
- .PopulateGrid
- .Tick
- InventoryViewStateController
- ItemId
- IInitializable
- GameOrchestrator
- CustomButtonEditor
- WeaponDefinition
- CharacterRuntime
- BasePopup
- SoulsLike.Entities.Character.Runtime
- AnimatorRootMotionRelay
- BaseUi.cs
- .Read
- StateMachineName
- SoulsLike.Services.Scenes.Data
- SoulsLike.Entities.Character.Components
- GameObject
- Tween
- ShieldDatabase
- List
- List
- CoreGameOrchestrator
- BaseUi
- ILayerService
- Component
- EquipmentLoadout
- EquipmentPresentation
- ICharacterActionExecutor
- ConsumableDatabase
- CharacterActionStateId
- CoreScope.cs
- IComponentMediator
- DamageRequest
- IComponentMediator
- IComponentMediator
- IComponentMediator
- LayerName
- IComponentMediator
- IComponentMediator
- Dictionary
- IComponentMediator
- bool
- CharacterActions
- LayerMask
- bool
- float
- Vector2
- .OnItemFocused
- Quaternion
- Vector2
- Vector3
- BufferedCharacterAction
- EquipmentSwapPhase
- string
- ICameraService
- IInputService
- float
- DamageRequest
- DamageResult
- SoulsLike.Services.Layer
- bool
- InventoryComponent
- ItemType
- CharacterActionStateId
- CharacterInputBatch
- LayerData
- MovementModel
- Camera
- SoulsLike.Services
- AttackComponent.cs
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- SceneReferencePropertyDrawer
- float
- SharedSceneScope
- AnimatorControllerParameterType
- ScriptableObject
- CharacterAttributeStats.cs
- Transform
- AnimatorComponent
- Tween
- EquipmentComponent
- int
- CharacterActionStateMachine
- AnimatorComponent
- int
- Inject
- Transform
- SoulsLike.Ui.PlayerHud
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- IContainerBuilder
- MonoBehaviour
- EquipmentSlotGroup
- IAnimationStateSink
- Inject
- IRootMotionSink
- IPauseNavigationRouteNavigation
- LandingType
- Transform
- Vector2
- Vector3
- ITimer
- StateMachineName
- Vector2
- EquipmentPresentation
- AnimatorStateMachineDto
- AttackType
- bool
- float
- HandMode
- RuntimeAnimatorController
- InteractionController
- bool
- HealthStatUpdate
- AttackType
- EquipmentLoadout
- InventoryComponent
- EquipmentLoadout
- EquippedItemContext
- CharacterCommandExecutionStatus
- EquipmentComponent
- EquipmentLoadout
- LandingType
- Vector2
- Action
- IReadOnlyCollection
- RectTransform
- AttackComponent
- AttackRequest
- CharacterAttributeStats
- CombatProfile
- EquipmentSlotId
- EquippedItemContext
- HealthStats
- InventoryChange
- InventoryComponent
- InventoryEntry
- InventoryEntryId
- IReadOnlyList
- ItemDatabase
- WeaponDefinition
- WeaponRuntime
- .ApplyDamage
- ICameraService
- GameObject
- string
- float
- Inject
- ITimer
- LandingType
- Transform
- Vector2
- Vector3
- MovementModel
- Character

## God Nodes (most connected - your core abstractions)
1. `AnimatorComponent` - 59 edges
2. `Character` - 58 edges
3. `MovementComponent` - 56 edges
4. `InventoryUiController` - 37 edges
5. `EquipmentUi` - 36 edges
6. `AmbienceService` - 36 edges
7. `EquipmentUiController` - 33 edges
8. `EquipmentSlotUI` - 33 edges
9. `EquipmentComponent` - 32 edges
10. `SoulsLike.Items` - 32 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `CoreGameOrchestrator` --references--> `CharacterFactory`  [EXTRACTED]
  Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs → Assets/Scripts/Entities/Character/CharacterFactory.cs
- `PlayerController` --references--> `InteractionController`  [EXTRACTED]
  Assets/Scripts/Entities/Character/PlayerController.cs → Assets/Scripts/Interactions/InteractionController.cs
- `InteractionCandidate` --references--> `IInteractable`  [EXTRACTED]
  Assets/Scripts/Interactions/InteractionController.cs → Assets/Scripts/Interactions/IInteractable.cs
- `ItemCatalog` --references--> `ConsumableDatabase`  [EXTRACTED]
  Assets/Scripts/Items/ItemCatalog.cs → Assets/Scripts/Items/ConsumableDatabase.cs

## Import Cycles
- None detected.

## Communities (232 total, 133 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.16
Nodes (9): SpeedMultiplierKey, SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character, SoulsLike.Ui.Inventory (+1 more)

### Community 1 - "SceneReference"
Cohesion: 0.16
Nodes (7): bool, string, SceneReference, IComparable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.22
Nodes (5): DamageRequest, HealthStats, HealthStatUpdate, HealthComponent, HealthModel

### Community 3 - "ItemDefinition"
Cohesion: 0.14
Nodes (12): Dictionary, IReadOnlyList, List, ItemDatabase, float, int, IReadOnlyList, List (+4 more)

### Community 4 - "Entity"
Cohesion: 0.07
Nodes (19): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+11 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.14
Nodes (8): IPreviewRenderService, Camera, Transform, Vector3, PreviewRenderService, Bounds, Light, RenderTexture

### Community 7 - "SceneType"
Cohesion: 0.21
Nodes (6): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType

### Community 8 - "PlayerHudUiController"
Cohesion: 0.07
Nodes (24): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud (+16 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.16
Nodes (8): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, EquipmentSlotId

### Community 10 - "AnimatorComponent"
Cohesion: 0.15
Nodes (10): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, HandMode, IAnimationStateSink (+2 more)

### Community 11 - "Character"
Cohesion: 0.07
Nodes (19): AnimatorComponent, CharacterAttributeStats, float, HealthStats, Inject, Quaternion, Vector3, Character (+11 more)

### Community 14 - "EquipmentUiController"
Cohesion: 0.11
Nodes (11): HandMode, EquipmentLoadout, CharacterAttributeStats, EquipmentSlotId, IInputService, InventoryChange, InventoryEntry, InventoryEntryId (+3 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.11
Nodes (9): Dictionary, GameObject, IEquipmentPresenter, Image, int, IReadOnlyList, TMP_Text, Transform (+1 more)

### Community 16 - "AudioService"
Cohesion: 0.06
Nodes (17): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+9 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.10
Nodes (15): EquipmentSlotGroup, EquipmentSlotId, HandMode, IEquipmentLoadoutSink, Inject, InventoryChange, InventoryEntry, InventoryEntryId (+7 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.18
Nodes (13): float, Vector2, AttackIntent, AttackRequest, CharacterCommand, CharacterCommandBuffer, CharacterCommandKind, CharacterControlFrame (+5 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.12
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - ".StartSwap"
Cohesion: 0.18
Nodes (11): EquipmentSlotGroup, AnimatorStateMachineDto, CharacterCommandExecutionStatus, EquipmentComponent, EquipmentLoadout, EquipmentSlotGroup, EquippedItemContext, EquipmentSwapCoordinator (+3 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.18
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "UiService"
Cohesion: 0.18
Nodes (8): IUiService, UiService, BaseUi, Canvas, Inject, List, Transform, UiFactory

### Community 23 - "CameraService"
Cohesion: 0.13
Nodes (7): CameraService, bool, Camera, CinemachineCamera, CinemachineThirdPersonFollow, Ease, Tween

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.16
Nodes (4): IEquipmentRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.13
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "PauseMenuUiController"
Cohesion: 0.14
Nodes (3): IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

### Community 29 - "AmbienceService"
Cohesion: 0.07
Nodes (24): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+16 more)

### Community 31 - "AttackComponent"
Cohesion: 0.07
Nodes (19): AnimatorStateMachineDto, AttackRequest, bool, CombatProfile, float, HandMode, Inject, AttackComponent (+11 more)

### Community 32 - "InventoryData.cs"
Cohesion: 0.21
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "EquipmentSlotChange"
Cohesion: 0.50
Nodes (3): EquipmentSlotId, InventoryEntryId, EquipmentSlotChange

### Community 36 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 37 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.16
Nodes (8): bool, float, DamageResult, HealthModel, bool, float, HealthStats, SoulsLike.Entities.Character.Components.Health

### Community 38 - "MainMenuOrchestrator"
Cohesion: 0.18
Nodes (4): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator

### Community 39 - "StorageRegistry"
Cohesion: 0.20
Nodes (6): Enum, IStorageRegistry, Enum, string, StorageRegistry, SoulsLike.Services.Storage

### Community 40 - "IMovementComponent"
Cohesion: 0.16
Nodes (6): Quaternion, SpeedMultiplierKey, Transform, Vector2, Vector3, IMovementComponent

### Community 41 - "CustomButton"
Cohesion: 0.13
Nodes (8): bool, ColorBlock, Image, SelectionState, Sprite, TMP_Text, CustomButton, Button

### Community 42 - "InventoryEntryId"
Cohesion: 0.16
Nodes (4): string, InventoryEntryId, IEquipmentPresenter, IEquatable

### Community 43 - "IHealthComponent"
Cohesion: 0.18
Nodes (5): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, IHealthComponent

### Community 44 - "InventoryItemViewData"
Cohesion: 0.20
Nodes (8): EquipmentSlotId, EquipmentSlotId, List, InventoryEntryId, InventoryPrimaryCategory, Sprite, InventoryItemViewData, IReadOnlyDictionary

### Community 46 - "LockOnUiController"
Cohesion: 0.15
Nodes (10): Camera, RectTransform, Transform, Vector3, LockOnUi, bool, Camera, LockOnUiController (+2 more)

### Community 47 - "PlayerController"
Cohesion: 0.18
Nodes (9): Character, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable, ITargetingService (+1 more)

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "SoulsLike.Entities.Character.Components.Movement"
Cohesion: 0.25
Nodes (4): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Components.Movement

### Community 51 - "InventoryUi"
Cohesion: 0.13
Nodes (12): CharacterAttributeStats, Color, IInventoryPresenter, Image, int, List, ScalingGrade, TMP_Text (+4 more)

### Community 52 - "ColorConverter"
Cohesion: 0.31
Nodes (6): ColorConverter, JsonReader, JsonSerializer, JsonWriter, Type, Color

### Community 53 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 54 - "ICameraService"
Cohesion: 0.14
Nodes (3): Transform, ICameraService, Ray

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.14
Nodes (10): AxisEventData, BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text (+2 more)

### Community 56 - "InventoryUiController"
Cohesion: 0.16
Nodes (10): Action, bool, IInputService, InventoryChange, InventoryPrimaryCategory, InventoryUiController, IInventoryPresenter, IInventoryRoute (+2 more)

### Community 59 - "MovementComponent"
Cohesion: 0.06
Nodes (22): MovementComponent, LandingType, Vector2, IMovementPresentationSink, JsonReader, JsonSerializer, JsonWriter, Type (+14 more)

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

### Community 64 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 65 - "IUniqueIdGenerator"
Cohesion: 0.33
Nodes (3): IUniqueIdGenerator, UniqueIdGenerator, SoulsLike.Services.IdGeneration

### Community 67 - "MainMenuUiController"
Cohesion: 0.15
Nodes (5): IMainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController, IStartable

### Community 68 - "SaveService"
Cohesion: 0.09
Nodes (10): string, FileService, SaveKeys, string, ISaveService, SaveService, string, SaveStore (+2 more)

### Community 70 - "InventoryItemSO"
Cohesion: 0.12
Nodes (10): InventoryPrimaryCategory, InventorySubCategory, bool, float, int, Sprite, string, InventoryItemSO (+2 more)

### Community 73 - "InventoryViewStateController"
Cohesion: 0.20
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 74 - "ItemId"
Cohesion: 0.23
Nodes (6): AttackRequest, Sprite, ItemCatalog, ItemId, CharacterAttributeStats, InventoryEntry

### Community 75 - "IInitializable"
Cohesion: 0.15
Nodes (12): Inject, InventoryEntry, InventoryEntryId, IReadOnlyList, InventoryComponent, Collider, int, GroundItem (+4 more)

### Community 76 - "GameOrchestrator"
Cohesion: 0.20
Nodes (6): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.10
Nodes (20): AnimationProfile, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot, ScalingGrade (+12 more)

### Community 79 - "CharacterRuntime"
Cohesion: 0.21
Nodes (6): bool, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy, CharacterActionStateMachine

### Community 80 - "BasePopup"
Cohesion: 0.10
Nodes (13): Action, GenericPopupService, IGenericPopupService, AcceptPopup, Button, AlertPopup, Action, Button (+5 more)

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.33
Nodes (4): UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime

### Community 82 - "AnimatorRootMotionRelay"
Cohesion: 0.09
Nodes (13): AnimatorStateMachineDto, Animator, bool, string, AnimatorRootMotionRelay, AnimatorStateMachineDto, IAnimationStateSink, EquipmentLoadout (+5 more)

### Community 83 - "BaseUi.cs"
Cohesion: 0.22
Nodes (4): IBaseUi, CanvasGroup, CanvasGroupExt, SoulsLike.Extensions

### Community 84 - ".Read"
Cohesion: 0.11
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 87 - "SoulsLike.Entities.Character.Components"
Cohesion: 0.33
Nodes (3): AnimatorModel, SoulsLike.Entities.Character.Components, Model

### Community 90 - "ShieldDatabase"
Cohesion: 0.21
Nodes (7): Dictionary, IReadOnlyList, List, ShieldDatabase, float, GameObject, ShieldDefinition

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.27
Nodes (4): List, CoreGameOrchestrator, ICoreGameOrchestrator, GameState

### Community 94 - "BaseUi"
Cohesion: 0.27
Nodes (4): bool, CanvasGroup, Transform, BaseUi

### Community 95 - "ILayerService"
Cohesion: 0.22
Nodes (4): GameObject, LayerMask, ILayerService, Inject

### Community 99 - "ICharacterActionExecutor"
Cohesion: 0.47
Nodes (3): CharacterCommandExecutionResult, CharacterCommandExecutionStatus, ICharacterActionExecutor

### Community 100 - "ConsumableDatabase"
Cohesion: 0.18
Nodes (7): Dictionary, IReadOnlyList, List, ConsumableDatabase, float, ConsumableDefinition, ItemUseType

### Community 101 - "CharacterActionStateId"
Cohesion: 0.32
Nodes (5): AnimatorStateMachineDto, CharacterAnimationAdapter, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind

### Community 102 - "CoreScope.cs"
Cohesion: 0.29
Nodes (3): SoulsLike.Services.Targeting, SoulsLike.Services.CameraService, SoulsLike.Ui.LockOn

### Community 104 - "DamageRequest"
Cohesion: 0.40
Nodes (4): float, int, Vector3, DamageRequest

### Community 108 - "LayerName"
Cohesion: 0.46
Nodes (4): LayerName, GameObject, LayerMask, LayerService

### Community 134 - "ItemType"
Cohesion: 0.22
Nodes (6): IReadOnlyList, ItemType, Action, IReadOnlyCollection, IInventoryRoute, IReadOnlyCollection

### Community 137 - "LayerData"
Cohesion: 0.33
Nodes (6): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, Data

### Community 138 - "MovementModel"
Cohesion: 0.31
Nodes (7): AnimationCurve, LayerMask, IMovementData, MovementData, AnimationCurve, LayerMask, MovementModel

### Community 140 - "SoulsLike.Services"
Cohesion: 0.13
Nodes (7): UiController, IContainerBuilder, MainMenuScope, SoulsLike.Services, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu, SoulsLike.Ui.Base

### Community 144 - "AddressableAssetService"
Cohesion: 0.09
Nodes (15): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+7 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 147 - "SharedSceneScope"
Cohesion: 0.17
Nodes (8): IContainerBuilder, CoreScope, ProjectScope, SharedSceneScope, IContainerBuilder, LifetimeScope, OnGuiFpsCounter, PreviewRenderService

### Community 149 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

### Community 157 - "CharacterActionStateMachine"
Cohesion: 0.17
Nodes (11): CharacterActionStateMachine, CharacterCommandDisposition, CharacterActionStateId, CharacterAnimationSignal, CharacterCommand, CharacterCommandBuffer, CharacterCommandDisposition, CharacterCommandExecutionResult (+3 more)

### Community 167 - "MonoBehaviour"
Cohesion: 0.83
Nodes (3): BaseComponent, IComponent, MonoBehaviour

### Community 180 - "EquipmentPresentation"
Cohesion: 0.16
Nodes (10): bool, GameObject, Inject, Quaternion, Transform, Vector3, EquipmentPresentation, float (+2 more)

### Community 188 - "InteractionController"
Cohesion: 0.06
Nodes (29): CancellationToken, UniTask, InteractionCommand, Character, CharacterFactory, CancellationToken, UniTask, IInteractable (+21 more)

### Community 190 - "HealthStatUpdate"
Cohesion: 0.50
Nodes (3): bool, float, HealthStatUpdate

### Community 219 - ".ApplyDamage"
Cohesion: 0.40
Nodes (3): DamageResult, DamageRequest, DamageResult

## Knowledge Gaps
- **13 isolated node(s):** `AttackType`, `CharacterAttributeStats`, `IPlayerHudPresenter`, `SpeedMultiplierKey`, `SwapPhase` (+8 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **133 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `AmbienceData` (2× useful, score=1.917978476)
- `CharacterCommandFactory` (2× useful, score=1.90595769)
- `ICharacterCommand` (2× useful, score=1.905302458)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `AnimatorComponent`, `EquipmentUiController`, `EquipmentUi`, `EquipmentComponent`, `.SetMovementBlocked`, `CharacterActionStateMachine`, `AttackComponent`, `MonoBehaviour`, `EquipmentPresentation`, `ICameraService`, `.BeginRootMotionAction`, `InventoryUiController`, `MovementComponent`, `.Tick`, `ItemId`, `IInitializable`, `.ApplyDamage`?**
  _High betweenness centrality (0.135) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `MovementModel`, `IInitializable`, `Character`, `SoulsLike.Entities.Character.Components.Movement`, `.Read`, `ICameraService`, `CameraService`, `UiService`, `.SetMovementBlocked`, `InteractionController`, `AttackComponent`?**
  _High betweenness centrality (0.097) - this node is a cross-community bridge._
- **Why does `SoulsLike.Services` connect `SoulsLike.Services` to `SoulsLike.Items`, `SoulsLike.Services.Layer`, `InputService`, `MainMenuOrchestrator`, `PreviewRenderService`, `CoreScope.cs`, `IGameStateNotifier`, `AudioService`, `AddressableAssetService`, `SharedSceneScope`, `SoulsLike.Services.Scenes.Data`, `UiService`, `CoreGameOrchestrator`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **What connects `AttackType`, `CharacterAttributeStats`, `IPlayerHudPresenter` to the rest of the system?**
  _13 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ItemDefinition` be split into smaller, more focused modules?**
  _Cohesion score 0.1368421052631579 - nodes in this community are weakly interconnected._
- **Should `Entity` be split into smaller, more focused modules?**
  _Cohesion score 0.07171717171717172 - nodes in this community are weakly interconnected._
- **Should `AnimatorStateMachineReceiver` be split into smaller, more focused modules?**
  _Cohesion score 0.0782312925170068 - nodes in this community are weakly interconnected._