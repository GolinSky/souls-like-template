---
title: Project Organization
type: architecture
domains:
  - project-structure
  - assets
status: needs-review
authority: advisory
updated: 2026-09-07
aliases:
  - PROJECT_ORGANIZATION
tags:
  - status/needs-review
---
# Project Organization Guide - SoulsLikeTemplate

This document outlines the asset organization rules for the **SoulsLikeTemplate** Unity project. The project follows a **type-first** structure, where the root folder defines the asset type, and subfolders define the domain or category.

## Structure Overview

```text
Assets/
├── Art/                    Visual assets by kind
├── Audio/                  Ambience, music, and sound effects
├── Plugins/                Core external packages
├── Prefabs/                Models, UI, and view/service prefabs
├── Scripts/                Runtime, editor, and test C# source
├── Settings/               Input, data, build, and render settings
├── Scenes/                 Game levels
├── Sandbox/                Technical tests
├── ThirdParty/             External tools and documentation
├── AddressableAssetsData/  Addressables configuration
└── Resources/              Minimal bootstrap assets
```

The detailed folder responsibilities are listed below. This text tree replaces the previous 35-edge Mermaid graph, which was too dense to read reliably in a normal Obsidian pane.

## Root Folder Definitions

### 1. Art (`Assets/Art`)
Contains all visual assets.
- **Models/**: 3D meshes, FBX files, and their imported materials.
- **Animation/**: Animator Controllers, Animation Clips, and Avatar Masks.
- **Textures/**: Image assets and sprites.
- **Materials/**: Shared material definitions.
- **Shaders/**: Project-owned custom shaders (e.g., `GroundItemAdditive.shader`).
- **Fonts/**: Font definitions and TextMesh Pro font assets.

### 2. Audio (`Assets/Audio`)
Contains all acoustic assets.
- **AmbienceMusic/**: Ambient loops, background music, and score tracks.
- **Sfx/**: Sound effect audio clips.

### 3. Prefabs (`Assets/Prefabs`)
Contains reusable GameObject configurations.
- **Models/**: Prefabs representing physical entities (Player, Equipment, Items, Skins, Environment interactables).
- **Ui/**: Menu screens, HUD elements, and UI widgets.
- **View/**: Non-physical orchestration prefabs (Camera, Services, VContainer Scopes).

### 4. Scripts (`Assets/Scripts`)
Contains all project-owned C# source code.
- **Components/**: Logic components that drive behavior (e.g., Movement, Interaction).
- **Entities/**: Data-focused models and shared entity logic.
- **Services/**: Global systems, manager logic, and dependency injection (VContainer).
- **Ui/**: Decoupled Controller-Presenter-View UI architecture per [[Knowledge/Guides/UI/UI Code Build Guide|UI Code Build Guide]].
- **Orchestrators/**: Game state transitions, scene flow orchestration, and high-level coordinators.
- **Utilities/**: General extension methods, serialization helpers, and utility factories.
- **Editor/**: Editor-only scripts and custom inspectors (no root `Assets/Editor/`).
- **Tests/**: Automated test suites (EditMode and PlayMode; no root `Assets/Tests/`).

### 5. Settings (`Assets/Settings`)
Contains configuration and scriptable object data.
- **Data/**: Game settings and databases (e.g., ScriptableObjects inheriting from the `Data` class, like `HealthData`, `InventoryData`, and the global `AssetMappingData`).
- **Input System Actions**: The `.inputactions` and `.inputsettings` assets.
- **Player Data**: ScriptableObjects like `MovementData`.
- **RenderPipelines/**: HDRP/URP profiles, volume profiles, and quality settings.

### 6. Plugins (`Assets/Plugins`)
Reserved for major, project-wide external packages.
- **Mirror**: Networking library.
- **TextMesh Pro**: Text rendering.
- **DOTween**: Animation engine.

### 7. Sandbox (`Assets/Sandbox`)
A boundary for temporary development.
- **Scenes/**: Blocking, technical testing, and prototyping levels.
- **Prefabs/Debug/**: Debug-only objects and technical integration tests.

## Placement Rules

1. **Type-First**: Always place assets in the root folder that matches their type (e.g., a weapon model goes in `Art/Models`, not `Prefabs`).
2. **Graphics vs Art**: The folder for visual assets must always be named `Art`. Shaders belong under `Art/Shaders`.
3. **Addressables**: **Do not modify the `AddressableAssetsData` folder structure.** Assets referenced by Addressables can be moved through the Unity Editor, but the data folder itself must remain intact.
4. **Resources**: Keep `Assets/Resources` minimal. Only use it for bootstrapping assets (e.g., initial VContainer configuration).
5. **Third-Party**: External assets from the Asset Store that are not core plugins belong in `Assets/ThirdParty`.
6. **Scripts**: Maintain decoupled architectural layers (`Components`, `Entities`, `Services`, `Ui`, `Orchestrators`, `Utilities`). Tooling and tests must be contained within `Scripts/Editor` and `Scripts/Tests`.
7. **Prefabs**: Adhere to the strict 3-tier division: `Prefabs/Models/` (physical), `Prefabs/Ui/` (interface), and `Prefabs/View/` (orchestration/services).
