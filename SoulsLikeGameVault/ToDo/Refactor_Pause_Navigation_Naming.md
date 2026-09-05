# Refactor `IPauseNavigationRouteNavigation` Naming

**Status**: Completed  
**Domain**: UI / Navigation Architecture  
**Priority**: Low (Clean Code / Naming Consistency)  

---

## 1. Problem Statement

The interface [`IPauseNavigationRouteNavigation`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/PauseNavigation/IPauseNavigationRouteNavigation.cs) contains redundant word repetition:
- Namespace: `SoulsLike.Ui.PauseNavigation`
- Interface Name: `IPauseNavigationRouteNavigation` ("Navigation" appears twice)

Furthermore, the suffix `RouteNavigation` is awkward compared to established C# UI architecture patterns.

---

## 2. Current Implementation

File: [`Assets/Scripts/Ui/PauseNavigation/IPauseNavigationRouteNavigation.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/PauseNavigation/IPauseNavigationRouteNavigation.cs)

```csharp
namespace SoulsLike.Ui.PauseNavigation
{
    public interface IPauseNavigationRouteNavigation
    {
        void OpenEquipment();
        void OpenInventory();
        void OpenSystem();
    }
}
```

Implemented by [`PauseNavigationUiController`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs):
```csharp
public sealed class PauseNavigationUiController : UiController,
    IInitializable,
    ITickable,
    IDisposable,
    IPauseNavigationPresenter,
    IPauseNavigationRouteNavigation
{
    // ...
}
```

---

## 3. Recommended Renaming Candidates

| Candidate Name | Pros | Cons | Recommendation |
|---|---|---|---|
| **`IPauseMenuRouter`** | Clean, concise, uses industry standard "Router" terminology, eliminates duplicate "Navigation". | Slight deviation from `PauseNavigation` namespace root. | **Preferred (Cleanest)** |
| **`IPauseNavigationRouter`** | Preserves the `PauseNavigation` domain prefix while fixing the `RouteNavigation` stutter. | Retains the slightly verbose `PauseNavigation` prefix. | **Alternative (Minimal Change)** |
| **`IPauseRouteHost`** | Accurately reflects its role as the host managing `UiRouteStack` for Pause routes. | Less conventional than "Router". | Alternative |

---

## 4. Migration Plan

1. **Rename Interface File**:
   - Rename `Assets/Scripts/Ui/PauseNavigation/IPauseNavigationRouteNavigation.cs` to `IPauseMenuRouter.cs` (or `IPauseNavigationRouter.cs`).
   - Rename the interface symbol:
     ```csharp
     namespace SoulsLike.Ui.PauseNavigation
     {
         public interface IPauseMenuRouter
         {
             void OpenEquipment();
             void OpenInventory();
             void OpenSystem();
         }
     }
     ```
2. **Update Implementing Controller**:
   - In [`PauseNavigationUiController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs), replace `IPauseNavigationRouteNavigation` with `IPauseMenuRouter`.
3. **Verify Registrations**:
   - `PauseNavigationUiController` is registered via `AsImplementedInterfaces()` in [`CharacterFactory.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/CharacterFactory.cs), which will automatically resolve the renamed interface.
4. **Update Documentation**:
   - Update references in [`SoulsLikeGameVault/ui/Pause_Navigation_Route_Architecture.md`](../ui/Pause_Navigation_Route_Architecture.md) and [`SoulsLikeGameVault/ui/UI_Route_Navigation_Architecture.md`](../ui/UI_Route_Navigation_Architecture.md).
5. **(Optional) Evaluate `IGraceRouteNavigation` Consistency**:
   - Consider similarly renaming [`IGraceRouteNavigation.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/Grace/IGraceRouteNavigation.cs) to `IGraceMenuRouter.cs` or `IGraceNavigationRouter.cs` for project-wide consistency.

---

## 5. Acceptance Criteria

- [x] File and interface renamed following the 1-type-per-file rule (`IPauseMenuRouter.cs` / `IPauseMenuRouter`).
- [x] No compilation or DI binding errors.
- [x] Unity compiles and tests (when explicitly requested) pass.
