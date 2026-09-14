---
title: Pause Navigation
type: architecture
domains:
  - ui
  - pause
  - navigation
status: current
authority: advisory
verified: 2026-09-07
source_commit: fcaf410d
context_keys:
  - pause-navigation
aliases:
  - Pause_Navigation_Route_Architecture
tags:
  - architecture/verified
---
# Pause Navigation Route Architecture

> [!success] Validation status — mostly verified
> Checked against live source on 2026-09-07 at `fcaf410d`. The route stack, injected routes, game-state gating, and current `IPauseNavigationRoute` naming resolve.

This document details the architecture, component interaction, navigation flows, and implementation rules for the **Pause Navigation System** (`Assets/Scripts/Ui/PauseNavigation/`).

---

## 1. Overview

The Pause Navigation System is the central routing hub for character management and game configuration during active gameplay. It manages modal transitions between the Pause Menu root and three primary sub-screens:
1. **Equipment** ([`IEquipmentRoute`](../../../../Assets/Scripts/Ui/Equipment/IEquipmentRoute.cs)) — Weapon, armor, and talisman loadouts.
2. **Inventory** ([`IInventoryRoute`](../../../../Assets/Scripts/Ui/Inventory/IInventoryRoute.cs)) — Item bag browsing and nested item selection for equipment slots.
3. **System** ([`ISystemRoute`](../../../../Assets/Scripts/Ui/System/ISystemRoute.cs)) — Game options, controls, and game quitting.

---

## 2. Core Components & Structure

```
Assets/Scripts/Ui/PauseNavigation/
 ├── IPauseNavigationRoute.cs            (Domain base route interface extending IUiRoute)
 ├── IPauseNavigationPresenter.cs        (Presenter contract for the root Pause UI)
 ├── IPauseMenuRouter.cs                 (Router contract for opening Pause sub-routes)
 ├── PauseNavigationUi.cs               (BaseUi view with root navigation buttons)
 └── PauseNavigationUiController.cs     (Host router controller managing state and UiRouteStack)
```

### A. Domain Route Base: `IPauseNavigationRoute`
Defined in [`Assets/Scripts/Ui/PauseNavigation/IPauseNavigationRoute.cs`](../../../../Assets/Scripts/Ui/PauseNavigation/IPauseNavigationRoute.cs):
```csharp
using System;
using SoulsLike.Ui.Navigation;

namespace SoulsLike.Ui.PauseNavigation
{
    public interface IPauseNavigationRoute : IUiRoute
    {
        event Action CloseRequested;
    }
}
```
All Pause sub-routes (`IEquipmentRoute`, `IInventoryRoute`, `ISystemRoute`) inherit from this interface, ensuring they provide a standardized `CloseRequested` event.

### B. Presenter Interface: `IPauseNavigationPresenter`
Defined in [`Assets/Scripts/Ui/PauseNavigation/IPauseNavigationPresenter.cs`](../../../../Assets/Scripts/Ui/PauseNavigation/IPauseNavigationPresenter.cs):
```csharp
namespace SoulsLike.Ui.PauseNavigation
{
    public interface IPauseNavigationPresenter
    {
        void OpenEquipment();
        void OpenInventory();
        void OpenSystem();
    }
}
```
Exposes root menu button actions to the view (`PauseNavigationUi`).

### C. Router Interface: `IPauseMenuRouter`
Defined in [`Assets/Scripts/Ui/PauseNavigation/IPauseMenuRouter.cs`](../../../../Assets/Scripts/Ui/PauseNavigation/IPauseMenuRouter.cs):
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

### D. View: `PauseNavigationUi`
Defined in [`Assets/Scripts/Ui/PauseNavigation/PauseNavigationUi.cs`](../../../../Assets/Scripts/Ui/PauseNavigation/PauseNavigationUi.cs):
- Inherits from [`BaseUi`](../../../../Assets/Scripts/Ui/Base/BaseUi.cs).
- Binds buttons (`openEquipmentButton`, `openInventoryButton`, `openSystemButton`) to presenter methods.

### E. Controller & Host Router: `PauseNavigationUiController`
Defined in [`Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs`](../../../../Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs):
- Implements `IInitializable`, `ITickable`, `IDisposable`, `IPauseNavigationPresenter`, `IPauseMenuRouter`.
- Injected dependencies:
  - `IUiService` — UI factory and view instantiation.
  - `ICoreGameOrchestrator` — Game state control (`PauseGame()`, `ResumeGame()`, `GameState`).
  - `IInputService` — Input action queries (`UiBackAction`, `Pause`, `OpenEquipmentAction`, `OpenInventoryAction`).
  - `IEquipmentRoute` — Sub-route for equipment management.
  - `IInventoryRoute` — Sub-route for inventory and item picking.
  - `ISystemRoute` — Sub-route for system settings and quit.
- Manages an internal [`UiRouteStack`](../../../../Assets/Scripts/Ui/Navigation/UiRouteStack.cs).

---

## 3. Navigation Flows & Sequence

### A. Opening Pause Menu from Gameplay
```mermaid
sequenceDiagram
    autonumber
    actor Player
    participant Input as IInputService
    participant Router as PauseNavigationUiController
    participant Orchestrator as ICoreGameOrchestrator
    participant View as PauseNavigationUi

    Player->>Input: Press CharacterActions.Pause
    Input->>Router: Tick() detects Pause pressed & GameState == Idle
    Router->>Orchestrator: PauseGame() (State -> GameState.Paused)
    Router->>View: Show()
```

### B. Nested Sub-Route Flow: Equipment to Inventory Item Picker
When selecting a weapon or armor slot in the Equipment screen:

```mermaid
sequenceDiagram
    autonumber
    actor Player
    participant EqCtrl as EquipmentUiController
    participant Router as PauseNavigationUiController
    participant Stack as UiRouteStack
    participant InvCtrl as InventoryUiController

    Player->>EqCtrl: SubmitSlot(EquipmentSlotId.RightHand1)
    EqCtrl->>Router: Fire InventoryRequested(slotId)
    Router->>Router: GetItemTypes(slotId) (resolves ItemType.Weapon)
    Router->>Stack: Open(_inventoryRoute, () => _inventoryRoute.Open(types, _equipmentRoute.SelectItem))
    Stack->>EqCtrl: Hide()
    Stack->>InvCtrl: Open(types, callback)
    InvCtrl->>InvCtrl: PopulateGrid(filtered items in _isSelectionMode)
    Player->>InvCtrl: OnItemSubmitted(selectedEntryId)
    InvCtrl->>EqCtrl: Invoke callback: SelectItem(selectedEntryId)
    EqCtrl->>EqCtrl: EquipmentComponent.Assign(slotId, entryId) & Refresh()
    InvCtrl->>Router: Fire CloseRequested
    Router->>Stack: CloseTop()
    Stack->>InvCtrl: Hide()
    Stack->>EqCtrl: Show() & FocusSlot(slotId)
```

### C. Direct Gameplay Hotkeys
Players can open Equipment or Inventory directly from gameplay without clicking through the Pause root menu:
1. `_inputService.OpenEquipmentAction.WasPressedThisFrame()` or `OpenInventoryAction` triggers in `Tick()`.
2. Controller verifies `_gameOrchestrator.CurrentGameState == GameState.Idle`.
3. Controller pauses gameplay: `_gameOrchestrator.PauseGame()`.
4. Controller opens the route directly on `UiRouteStack` (`_routeStack.Open(_equipmentRoute)`).
5. When the player backs out, `CloseTop()` pops the route and restores the root pause menu, or closing the pause menu resumes gameplay.

### D. Back & Stack Unwinding Logic
```csharp
private void HandleUiBack()
{
    if (_routeStack.HasOpenRoutes)
    {
        _routeStack.CloseTop();
        return;
    }

    _view.Hide();
    _gameOrchestrator.ResumeGame();
}
```

---

## 4. Slot-to-ItemType Mapping Rules

When opening the inventory picker for an equipment slot, `PauseNavigationUiController` applies slot filter rules:

```csharp
private static IReadOnlyCollection<ItemType> GetItemTypes(EquipmentSlotId slotId)
{
    if (slotId is >= EquipmentSlotId.RightHand1 and <= EquipmentSlotId.RightHand3)
    {
        return _rightHandItemTypes; // Weapon
    }

    if (slotId is >= EquipmentSlotId.LeftHand1 and <= EquipmentSlotId.LeftHand3)
    {
        return _leftHandItemTypes;  // Weapon, Shield
    }

    if (slotId is >= EquipmentSlotId.Arrow1 and <= EquipmentSlotId.Bolt2)
    {
        return _ammunitionItemTypes; // Ammunition
    }

    if (slotId is >= EquipmentSlotId.Head and <= EquipmentSlotId.Legs)
    {
        return _armorItemTypes;      // Armor
    }

    if (slotId is >= EquipmentSlotId.Talisman1 and <= EquipmentSlotId.Talisman4)
    {
        return _talismanItemTypes;   // Talisman
    }

    return _consumableItemTypes;     // Consumable (Quick Item slots)
}
```

---

## 5. VContainer DI Registration

Registered in [`CharacterFactory.cs`](../../../../Assets/Scripts/Entities/Character/CharacterFactory.cs):

```csharp
builder.Register<EquipmentUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
builder.Register<InventoryUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
builder.Register<PauseNavigationUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
```

---

## 6. Refactored `IPauseMenuRouter` Naming

The interface previously named `IPauseNavigationRouteNavigation` was refactored to **`IPauseMenuRouter`** to eliminate word stutter ("Navigation" repeated twice) and adhere to standard C# UI routing conventions.

### Completed Action Items
- [x] Rename interface file to [`IPauseMenuRouter.cs`](../../../../Assets/Scripts/Ui/PauseNavigation/IPauseMenuRouter.cs).
- [x] Update definition: `public interface IPauseMenuRouter { void OpenEquipment(); void OpenInventory(); void OpenSystem(); }`.
- [x] Update [`PauseNavigationUiController.cs`](../../../../Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs) implementation list.
- [x] Update DI bindings and consumers.
- [x] Tracking note: [[History/Records/Pause Navigation Naming Refactor|Pause Navigation Naming Refactor]].
