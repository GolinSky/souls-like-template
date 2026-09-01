# UI Code Build & Architecture Guide

This document outlines the step-by-step process for creating and wiring new UI features within the project. It covers C# code architecture (MVP / Controller pattern with VContainer), Prefab creation according to project organization guidelines, and Addressables configuration with `AssetMappingData`.

---

## 1. C# Script Architecture (`Assets/Scripts/Ui/<FeatureName>/`)

UI features follow a decoupled **Controller-Presenter-View** pattern.
All scripts for a feature reside in:
`Assets/Scripts/Ui/<FeatureName>/` (e.g. [`Assets/Scripts/Ui/MainMenu`](../../Assets/Scripts/Ui/MainMenu))

### A. Create Presenter Interface (`I<FeatureName>Presenter.cs`)
Defines the user actions and callbacks that the UI view can invoke.

```csharp
namespace SoulsLike.Ui.MainMenu
{
    public interface IMainMenuPresenter
    {
        void PlayGame();
        void OpenOptions();
        void ExitGame();
    }
}
```

### B. Create UI View Script (`<FeatureName>Ui.cs`)
Inherits from `BaseUi` (from `SoulsLike.Ui.Base`). Implements view lifecycle (e.g., `IStartable` from `VContainer.Unity` or standard Unity methods) to subscribe/unsubscribe button clicks.

```csharp
using SoulsLike.Ui.Base;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.MainMenu
{
    public class MainMenuUi : BaseUi, IStartable
    {
        [SerializeField] private CustomButton playButton;
        [SerializeField] private CustomButton optionsButton;
        [SerializeField] private CustomButton exitButton;

        private IMainMenuPresenter Presenter { get; set; }

        public void AssignPresenter(IMainMenuPresenter presenter)
        {
            Presenter = presenter;
        }

        void IStartable.Start()
        {
            playButton.onClick.AddListener(Presenter.PlayGame);
            optionsButton.onClick.AddListener(Presenter.OpenOptions);
            exitButton.onClick.AddListener(Presenter.ExitGame);
        }

        public void OnDestroy()
        {
            playButton.onClick.RemoveListener(Presenter.PlayGame);
            optionsButton.onClick.RemoveListener(Presenter.OpenOptions);
            exitButton.onClick.RemoveListener(Presenter.ExitGame);
        }
    }
}
```

### C. Create UI Controller Script (`<FeatureName>UiController.cs`)
Inherits from `UiController` (from `SoulsLike`) and implements `IInitializable` (from `VContainer.Unity`) as well as the Presenter interface (`I<FeatureName>Presenter`).

```csharp
using SoulsLike.Orchestrators.MainMenu;
using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike.Ui.MainMenu
{
    public class MainMenuUiController : UiController, IInitializable, IMainMenuPresenter
    {
        private readonly IMainMenuOrchestrator _mainMenuOrchestrator;
        private MainMenuUi _mainMenuUi;

        public MainMenuUiController(IUiService uiService, IMainMenuOrchestrator mainMenuOrchestrator)
            : base(uiService)
        {
            _mainMenuOrchestrator = mainMenuOrchestrator;
        }

        public void Initialize()
        {
            _mainMenuUi = CreateUi<MainMenuUi>();
            _mainMenuUi.AssignPresenter(this);
            _mainMenuUi.Show();
        }

        public void PlayGame() => _mainMenuOrchestrator.PlayGame();
        public void OpenOptions() => _mainMenuOrchestrator.OpenOptions();
        public void ExitGame() => _mainMenuOrchestrator.ExitGame();
    }
}
```

### D. Register Controller in VContainer Scope
In the corresponding `LifetimeScope` (e.g. [`MainMenuScope.cs`](../../Assets/Scripts/Services/VContainer/MainMenuScope.cs)), register the UI Controller:

```csharp
builder.Register<MainMenuUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
```

---

## 2. Prefab UI Asset Creation & Organization

### Save Folder Pattern
According to the project organization rules defined in [`PROJECT_ORGANIZATION.md`](../Arhitecture/PROJECT_ORGANIZATION.md):
- The project follows a **type-first** structure: root folder = asset type (`Prefabs/`), subfolder = domain (`Ui/`).
- Save location: `Assets/Prefabs/Ui/<FeatureName>/<FeatureName>Ui.prefab`
- Example: [`Assets/Prefabs/Ui/MainMenu/MainMenuUi.prefab`](../../Assets/Prefabs/Ui/MainMenu/MainMenuUi.prefab)

### Hierarchy Setup in Unity
1. Create a Canvas / Root UI GameObject with the `<FeatureName>Ui` component attached.
2. Attach `CanvasGroup` to the root (required by `BaseUi`).
3. Connect UI components (e.g. `CustomButton`, text labels) to serializable fields in the `<FeatureName>Ui` script inspector.
4. Save the UI as a prefab in `Assets/Prefabs/Ui/<FeatureName>/`.

> [!NOTE]
> Unity MCP tools can be used to generate, modify, and manage UI prefabs directly inside Unity.

---

## 3. Addressables & AssetMappingData Setup

After creating the UI prefab asset:

1. **Mark as Addressable**:
   - In Unity Editor, select the prefab.
   - Check the **Addressable** box in the Inspector.
   - Assign the asset to the **`Ui`** Addressable Group (defined in [`Assets/AddressableAssetsData/AssetGroups/Ui.asset`](../../Assets/AddressableAssetsData/AssetGroups/Ui.asset)).
   - Set the Addressable Address to the UI class name (e.g. `MainMenuUi`). Existing entries in the `Ui` group include `SystemUi`, `EquipmentUi`, `MainMenuUi`, `LockOnUi`, `PlayerHudUi`.

2. **Register in `AssetMappingData`**:
   - Navigate to [`Assets/Settings/Data/AssetMappingData.asset`](../../Assets/Settings/Data/AssetMappingData.asset).
   - In the `uiMappings` dictionary, add a key-value entry:
     - **Key**: UI C# Class Name (e.g. `MainMenuUi`).
     - **Value**: Reference to the Addressable UI Prefab GameObject asset.
