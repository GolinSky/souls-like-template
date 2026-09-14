---
title: Game Design Document
type: game-design
domains:
  - game-design
status: draft
authority: advisory
verified: 2026-09-07
context_keys:
  - game-design
tags:
  - game-design/index
  - status/needs-review
---

# Game Design Document

This is the root of the divided GDD. It links to focused feature notes instead of becoming one oversized document. Existing notes contain a mixture of implemented behavior and proposals, so this index does not silently promote them to accepted design.

## Design Pillars

- Deliberate, animation-led character actions with explicit commitment and interruption windows.
- Readable free-movement and target-lock modes.
- Consistent player feedback for movement, combat, inventory, equipment, and world interaction.
- Data-driven tuning where the current architecture supports it.

These pillars are inferred from existing documentation and require owner confirmation.

## Feature Map

### Player Movement

- [[Knowledge/Game Design/Mechanics/Movement Mechanics|Movement Mechanics]] — explanatory feature note; verify tuning values before treating them as current.
- [[Knowledge/Game Design/Systems/Locomotion and Camera|Locomotion and Camera]] — proposed behavior specification.
- [[Knowledge/Game Design/Technical Specifications/Locomotion Architecture|Locomotion Architecture]] — proposed technical design, not implementation authority.
- [[Knowledge/Game Design/Technical Specifications/Roll and Backstep Vectoring|Roll and Backstep Vectoring]] — proposed detailed behavior.

### Combat and Enemies

- [[Knowledge/Game Design/Systems/Enemy Combat and AI|Enemy Combat and AI]] — current design/reference candidate.

### UI and Equipment

- [[Knowledge/Guides/UI/Inventory UI and UX Guide|Inventory UI and UX Guide]]
- [[Knowledge/Guides/UI/Equipment UI and UX Guide|Equipment UI and UX Guide]]

## Decision Boundary

- GDD notes define intended player-facing behavior; Architecture notes describe live implementation.
- A difference between them is a tracked issue or plan, not a reason to rewrite history.
- Draft or proposed GDD content requires explicit acceptance before it constrains implementation.
- New features use [[Meta/Templates/Game Design Feature Template|Game Design Feature Template]].
