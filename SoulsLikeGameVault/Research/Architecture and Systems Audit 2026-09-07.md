---
title: Architecture and Systems Audit 2026-09-07
type: research
domains:
  - architecture
  - combat
  - ui
  - persistence
  - lifecycle
status: draft
authority: evidence
updated: 2026-09-07
source_commit: 3925ea83e7cd80b111331a6105835c3e84eeb2b6
aliases: []
tags:
  - research/package
  - audit/architecture
---
# Architecture and Systems Audit 2026-09-07

## Required Package

### Question and Desired Decision

Read the project's Obsidian architecture, compare it with the current implementation, and record actionable issues. Decide which confirmed defects to fix first and which conditional gaps need content or runtime validation.

Review date: **2026-09-07**. Source commit: `3925ea83e7cd80b111331a6105835c3e84eeb2b6`. Result: **11 new open issue notes** — eight code/resource/error-handling findings (including a persistence resilience gap), two conditional integration gaps, and one documentation reconciliation issue. Two findings have high priority; nine have medium priority. This is a broad static audit, not proof that every defect in the repository has been found.

### Scope and Non-Goals

Inspected character input/action/movement flow, melee defense and recovery, enemy-related ownership, inventory/equipment routes and current content, settings/save paths, scene/spawn orchestration, asset load ownership, entity lookup, ground items, ladders, elevators, and relevant layer contracts.

Source and serialized prefab/configuration reads were used where relevant. No C# or Unity assets were changed. No tests, Play Mode runs, profiling, builds, save corruption experiments, or Unity scene operations were performed. Existing working-tree and Obsidian configuration changes belong to their existing owners and were not edited by this audit.

### Current System Map

| Boundary | Current implementation and ownership |
|---|---|
| Composition and scene lifetime | ProjectScope registers shared services; CoreScope registers gameplay systems and the core orchestrator. SceneService coordinates Loading, dependencies, and the destination scene. |
| Character behavior | Character coordinates semantic actions, movement locks, equipment, and animation presentation; MovementComponent owns motion and roll metadata. |
| Combat | CombatDefenseComponent owns poise/stance and recovery; entity commands mediate authoritative hit processing. |
| Entity identity and interactions | EntityLocator resolves physics-discovered identities; the current interaction probe dispatches through target-owned IInteractableCommand. The old migration audit does not describe the complete current state. |
| UI | Views call presenter interfaces; controllers drive routes and domain state. Pause equipment selection currently reuses InventoryUiController. |
| Persistence | SaveService/SaveStore handles spawn/settings; StorageRegistry remains an active JSON path for world state. Both directly overwrite live files. |
| Assets | AddressableAssetService synchronously returns loaded assets; ownership of the corresponding load references is absent. |

Key source entry points: `Assets/Scripts/Services/VContainer/ProjectScope.cs`, `Assets/Scripts/Services/VContainer/CoreScope.cs`, `Assets/Scripts/Entities/Character/Character.cs`, `Assets/Scripts/Entities/Combat/CombatDefenseComponent.cs`, `Assets/Scripts/Interactions/InteractionController.cs`, `Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs`, `Assets/Scripts/Services/Scenes/SceneService.cs`.

### Entry Points, Dependencies, and Consumers

- Player input → Character.Tick → action execution → MovementComponent → Animator root-motion callback.
- Melee damage → defense meters → CombatDefenseComponent.TickRecovery, used by player/enemy flows.
- Equipment slot submission → pause router → inventory picker → assignment; candidate focus currently loses the selected-slot context.
- Main menu Play → GameOrchestrator → SceneService; the shared spawn service holds pending destination data.
- Settings Apply → SettingsService.Commit → SaveService; the void persistence contract cannot convey a write failure.
- UI creation → UiFactory → AddressableAssetService.Load → Object.Instantiate, without a balanced asset-release lifetime.

### Evidence and Findings

Each issue contains the observed source behavior, expected behavior, a reproduction procedure, evidence, unresolved questions, proposed bounded fix scope, and acceptance criteria.

| # | Issue | Priority | Evidence classification |
|---|---|---|---|
| 1 | [[Work/Issues/Locked Roll State Is Cleared Before Root Motion|Locked Roll State Is Cleared Before Root Motion]] | high | code defect |
| 2 | [[Work/Issues/Stance Recovery Is Gated By Poise Delay|Stance Recovery Is Gated By Poise Delay]] | medium | code and architecture mismatch |
| 3 | [[Work/Issues/Inventory Category Controls Are Not Connected|Inventory Category Controls Are Not Connected]] | high | code and prefab defect |
| 4 | [[Work/Issues/Equipment Picker Compares Against The Wrong Slot|Equipment Picker Compares Against The Wrong Slot]] | medium | code defect |
| 5 | [[Work/Issues/Scene Transitions Allow Concurrent Load Operations|Scene Transitions Allow Concurrent Load Operations]] | medium | code defect |
| 6 | [[Work/Issues/Addressable Asset Loads Have No Release Owner|Addressable Asset Loads Have No Release Owner]] | medium | resource lifetime defect |
| 7 | [[Work/Issues/Settings Apply Hides Persistence Failures|Settings Apply Hides Persistence Failures]] | medium | error propagation defect |
| 8 | [[Work/Issues/Save Writes Can Replace The Last Valid File With Partial Data|Save Writes Can Replace The Last Valid File With Partial Data]] | medium | resilience gap |
| 9 | [[Work/Issues/Respawn Assumes The Last Grace Is In The Current Scene|Respawn Assumes The Last Grace Is In The Current Scene]] | medium | conditional code defect |
| 10 | [[Work/Issues/Equipment Picker Does Not Filter Exact Slot Compatibility|Equipment Picker Does Not Filter Exact Slot Compatibility]] | medium | conditional content integration defect |
| 11 | [[Work/Issues/Architecture And Roll Issue Notes Contain Superseded Evidence|Architecture And Roll Issue Notes Contain Superseded Evidence]] | medium | documentation defect |

Related performance evidence is recorded separately in [[Work/Issues/DefaultLocation Memory and Rendering Issues]]. Its measurements were not repeated here. Loading all configured dependencies within one transition is distinct from overlapping multiple top-level transition requests, and deliberate scene residency is not itself an asset-reference leak.

Existing issue: [[Work/Issues/Roll Interruption Issue]]. It was not duplicated or marked resolved. The new roll issue concerns metadata cleared at startup; the existing report concerns chained interruption and contains some already-implemented recommendations.

### Options and Tradeoffs

1. Prioritize the two high-priority current-content defects: locked-roll state and unreachable inventory categories.
2. Correct the selected-slot comparison and settle the stance recovery contract.
3. Give scene transitions and Addressables loads explicit lifecycle ownership. Measure retained memory separately before proposing a performance redesign.
4. Address persistence failure reporting and last-good-file preservation together, since a reliable save boundary needs both.
5. Use the two conditional notes as regression/content gates: armor/ammunition compatibility and non-Workshop first-death/cross-scene respawn.
6. Reconcile documentation before using old issue/research code blocks as implementation instructions.

These are recommendations, not an executable or approved implementation plan.

### Risks, Unknowns, and Open Questions

- All gameplay and timing reproduction remains unperformed. Use a separate bounded `unity_test_runner` follow-up, with scene preflight and asynchronous test safety. Normal validation must not run Play Mode tests.
- Addressables reference ownership is source-confirmed. No allocation total, leak rate, crash attribution, frame timing, or performance improvement was measured.
- Armor/arrow/bolt incompatibility requires content not currently present in the shipped catalog. It is explicitly conditional.
- The default fresh MainMenu path loads Workshop. The respawn issue instead concerns direct non-Workshop entry or a checkpoint/scene mismatch.
- Stance code conflicts with advisory documented behavior. Confirm the intended rule before changing balance.
- Ground-item persistence across scene loads has no sufficiently established design requirement in the inspected notes. Unused save identity and respawning loot were not promoted to a verified defect.
- An additive ladder scene disappearing while its player survives remains a lifecycle question. Ordinary player disposal cancels InteractionController's lifetime token; a normal scene-unload failure was not established.
- An empty platform-arrival synchronization callback and invalid targeting capability paths were not filed as current defects: required behavioral evidence or a valid production trigger was missing.
- Direct equipment/inventory hotkeys while paused were checked and ruled out: OpenRouteFromGameplay already rejects non-Idle state.
- Pickup's token-independent completion after awarding the item and the guaranteed player collection capability were not mislabeled as cancellation/null-handling defects.
- No new confirmed elevator defect was established in the bounded review. This does not constitute exhaustive asset or runtime validation.

### Recommended Review Questions

- Should stance recovery remain independent of poise, as the architecture note says?
- Which direct scene-entry workflows must support a first death before touching a grace?
- Should repeated scene requests be rejected, coalesced, or queued?
- Which scope should own each Addressables load, and what remains deliberately cached?
- What should the settings flow display when persistence fails after a live preview?

### Handoff

Select specific issue notes for implementation, preserve their stated scope, and resolve conditional design questions where needed. A reviewed implementation plan belongs separately in Work/Plans. This research note is an audit record and must not become an execution log.

The issue list is also linked from [[Work/Work Queue]].

## Evidence Rules

- Live code, inspected serialized content, and the current source commit outrank generated Graphify output and stale notes.
- Graphify's existing 4,704-node graph was used for navigation; it was not rebuilt. Initial query vocabulary: character, combat, inventory, settings, scene, orchestrator, entity, interaction. Findings were verified against source rather than inferred graph edges.
- Context was resolved through [[Agent Guide/Agent Context Registry]] and the registered headings of [[Agent Guide/Vault Guide]], [[Architecture/Systems/Character System]], [[Architecture/Systems/Hitbox System]], [[Architecture/Systems/Jump and Roll System]], [[Architecture/Systems/Entity Locator System]], [[Architecture/Systems/Layer Service]], [[Guides/Animation/Animator Sub-State Machine Guide]], [[Guides/UI/UI Code Build Guide]], [[Architecture/UI/Pause Navigation]], [[Architecture/UI/UI Route Navigation]], and [[Architecture/UI/Grace Navigation]] as applicable.
- [[Architecture/Systems/Settings System]] and [[Research/Interaction System Audit]] were treated as evidence with their registry staleness limitations. Proposed future features were not treated as mandatory current behavior.
- Addressables ownership was checked against Context7 and the installed package's `Documentation~/memory-assets.md`, which requires balancing loads and releases. No external recommendation was used to infer a measured performance problem.
