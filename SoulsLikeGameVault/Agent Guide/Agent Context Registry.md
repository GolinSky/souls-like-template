---
title: Agent Context Registry
type: registry
domains:
  - documentation
status: current
authority: required
verified: 2026-09-07
source_commit: fcaf410d
tags:
  - agent/context
---

# Agent Context Registry

This is the allow-list for task-specific vault context. Agents may discover an entry by exact key or by a concrete match in **Task signals**, but must read only the registered headings. A tag or property never makes an unregistered note policy.

| Context key | Exact vault-relative note path | Task signals | Registered headings | Authority | Verification | Disk fallback path |
|---|---|---|---|---|---|---|
| `vault-usage` | `Agent Guide/Vault Guide.md` | vault, documentation, naming, properties, tags, plan storage, issue storage, research package, implementation record | `Agent Retrieval Workflow`, `Note Lifecycle`, `Naming Rules`, `Diagram Rules` | required | current; 2026-09-07 | `SoulsLikeGameVault/Agent Guide/Vault Guide.md` |
| `ui-code` | `Guides/UI/UI Code Build Guide.md` | UI controller, presenter, view, UI prefab, Addressables, AssetMappingData | `1. C# Script Architecture`, `2. Prefab UI Asset Creation & Organization`, `3. Addressables & AssetMappingData Setup` | required | current; 2026-09-07 path migration only | `SoulsLikeGameVault/Guides/UI/UI Code Build Guide.md` |
| `inventory-ui` | `Guides/UI/Inventory UI and UX Guide.md` | inventory UI, inventory layout, item grid, inventory focus, inventory input | `2. Spatial Layout & Screen Breakdown`, `4. Cell UI Architecture (Item Grid Slots)`, `6. UI/UX View State Machine`, `7. Navigation / Focus Management / Input Mapping` | advisory | needs-review | `SoulsLikeGameVault/Guides/UI/Inventory UI and UX Guide.md` |
| `equipment-ui` | `Guides/UI/Equipment UI and UX Guide.md` | equipment UI, comparison UI, equipment navigation, equipment state | `2. Spatial UI Breakdown (What is Located Where)`, `3. Interactive UX States & Navigation Flow`, `4. Visual UI Layout Hierarchy` | advisory | needs-review | `SoulsLikeGameVault/Guides/UI/Equipment UI and UX Guide.md` |
| `ui-navigation` | `Architecture/UI/UI Route Navigation.md` | route stack, UI navigation, back input, UI route, pause route, grace route | `2. Core Abstractions`, `3. The Two Route Navigation Hubs`, `4. Architecture Rules & Guidelines` | advisory | partial; stale `UiController` path | `SoulsLikeGameVault/Architecture/UI/UI Route Navigation.md` |
| `pause-navigation` | `Architecture/UI/Pause Navigation.md` | pause menu, pause navigation, pause route, direct gameplay hotkey | `2. Core Components & Structure`, `3. Navigation Flows & Sequence`, `4. Slot-to-ItemType Mapping Rules`, `5. VContainer DI Registration` | advisory | mostly verified; 2026-09-07 | `SoulsLikeGameVault/Architecture/UI/Pause Navigation.md` |
| `grace-navigation` | `Architecture/UI/Grace Navigation.md` | grace UI, grace route, fast travel, fade sequence | `2. Core Components & Structure`, `3. Grace Rest & Fading Sequence`, `4. Sub-Route Navigation: Fast Travel Flow`, `5. Exit Grace Navigation Flow` | advisory | needs-review; stale fade path and ViewEntity claim | `SoulsLikeGameVault/Architecture/UI/Grace Navigation.md` |
| `animation-code` | `Guides/Animation/Animator Sub-State Machine Guide.md` | Animator Controller, animation state, transition, sub-state machine, ActionExecutor, CrossFade | `1. Rule: Group Connected Animations into Sub-State Machines`, `2. Rule: Coordinate and Layout Standards`, `3. Rule: Inert Empty Default State Inside Action Sub-State Machines`, `4. Rule: Runtime CrossFade Resolution Compatibility`, `5. Rule: Unity Asset Persistence` | required | workflow verified; controller layouts require asset inspection | `SoulsLikeGameVault/Guides/Animation/Animator Sub-State Machine Guide.md` |
| `entity-locator` | `Architecture/Systems/Entity Locator System.md` | Entity Locator, IEntityLocator, entity communication, cross-entity communication, player hits enemy, player uses ladder, interaction target, hazard target, AI targeting | `Required Entity Communication Rule`, `Lookup and Dispatch Contract`, `Examples`, `Boundaries and Prohibited Bypasses` | required | current; 2026-09-07 | `SoulsLikeGameVault/Architecture/Systems/Entity Locator System.md` |
| `character-architecture` | `Architecture/Systems/Character System.md` | Character, character aggregate, action state machine, movement or combat gating | `1. Overview & Core Architectural Philosophy`, `3. Input Pipeline & Semantic Control Translation`, `4. Action State Machine & Action Lifecycle`, `6. Component Responsibilities & Boundaries`, `7. Frame Execution & Update Order`, `10. Rules of the Character System (Durable Invariants)` | advisory | partial; 2026-09-07 | `SoulsLikeGameVault/Architecture/Systems/Character System.md` |
| `hitbox-architecture` | `Architecture/Systems/Hitbox System.md` | melee hitbox, hit resolution, defense, critical hit, combat relay | `1. Overview & Core Philosophy`, `3. System Architecture & Component Taxonomy`, `5. Authoritative Hit Resolution Pipeline`, `7. Combat Defense Meters & Recovery Rules`, `8. Synchronized Critical System (Riposte & Backstab)`, `11. Architectural Invariants & Hard Rules` | advisory | mostly verified; 2026-09-07 | `SoulsLikeGameVault/Architecture/Systems/Hitbox System.md` |
| `layer-architecture` | `Architecture/Systems/Layer Service.md` | layer service, LayerData, LayerName, LayerMaskName, physics query mask, NavMesh layer | `1. Overview & Architectural Philosophy`, `3. Layer Registry & Mask Specifications`, `5. Core Contracts & API`, `7. Authoring Guidelines & Rules of Thumb` | advisory | mostly verified; asset values still require Unity inspection | `SoulsLikeGameVault/Architecture/Systems/Layer Service.md` |
| `locomotion-current` | `Architecture/Systems/Jump and Roll System.md` | jump, roll, sprint, movement component, locomotion implementation, action buffer | `Sources of truth`, `Ownership and Update Flow`, `Jump State Machine`, `Jump Animation Contract`, `Roll and Sprint Input`, `Action Buffering and Queue Windows`, `Current Boundaries and Non-Implemented Systems` | advisory | current candidate; 2026-09-07 | `SoulsLikeGameVault/Architecture/Systems/Jump and Roll System.md` |
| `interaction-audit` | `Research/Interaction System Audit.md` | interaction, IEntityLocator, IInteractable, IInteractableCommand, interactable migration | `1. Executive Summary & Design Intent`, `2. Deep Dive: Current Architecture Audit`, `3. Comparison Matrix: Current vs. Required Standard`, `4. Architectural Target Design` | evidence | stale/partial; 2026-09-07 | `SoulsLikeGameVault/Research/Interaction System Audit.md` |
| `settings-review` | `Architecture/Systems/Settings System.md` | settings service, settings UI, graphics settings, settings persistence | `1. Overview & Architectural Philosophy`, `2. Reference Archetype: Audio Settings System`, `3. Domain Segregation Blueprint for Other Subsystems`, `4. Future Settings Persistence & UI Coordination`, `5. Summary of Architecture Rules & Best Practices` | evidence | needs-review; legacy future-state sections remain | `SoulsLikeGameVault/Architecture/Systems/Settings System.md` |
| `game-design` | `Game Design/Game Design Document.md` | game design, player behavior, mechanic intent, feature rules, GDD | `Design Pillars`, `Feature Map`, `Decision Boundary` | advisory | draft; 2026-09-07 | `SoulsLikeGameVault/Game Design/Game Design Document.md` |
| `research-workflow` | `Templates/Research Package Template.md` | investigate system, research package, dependency survey, ChatGPT Pro review handoff | `Required Package`, `Evidence Rules` | required | current; 2026-09-07 | `SoulsLikeGameVault/Templates/Research Package Template.md` |
| `plan-workflow` | `Templates/Plan Template.md` | implementation plan, remediation plan, Plan mode, execution checklist | `Plan Contract`, `Execution Handoff` | required | current; 2026-09-07 | `SoulsLikeGameVault/Templates/Plan Template.md` |
| `issue-workflow` | `Templates/Issue Template.md` | bug, defect, issue, regression, blocked work | `Issue Contract`, `Resolution Handoff` | required | current; 2026-09-07 | `SoulsLikeGameVault/Templates/Issue Template.md` |
| `history-workflow` | `Templates/Implementation Record Template.md` | work completed, implementation artifact, change history, bug fix record | `Implementation Record Contract` | required | current; 2026-09-07 | `SoulsLikeGameVault/Templates/Implementation Record Template.md` |
| `work-routing` | `Work/Work Queue.md` | named plan, current plan, execute plan, named issue, active issue, work queue, work status | `Draft Plans`, `Executable Plans`, `Issues`, `Lifecycle` | required | current; 2026-09-07 | `SoulsLikeGameVault/Work/Work Queue.md` |

## Registry Rules

- Direct task-signal matches are deterministic routing, not free-form semantic search.
- Load all matching required entries first. Load advisory or evidence entries only when their domain is in scope.
- `required` constrains work. `advisory` explains current or intended behavior. `evidence` supports investigation. `historical` records the past.
- Entries marked `needs-review` or `stale` cannot establish current architecture or project policy.
- Missing files, missing headings, ambiguous matches, or conflicts with live evidence must be reported.
- Obsidian MCP is the preferred reader. The exact checked-in Markdown path is the offline fallback.
