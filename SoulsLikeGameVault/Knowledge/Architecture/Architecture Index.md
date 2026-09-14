---
title: Architecture Index
type: index
domains:
  - architecture
status: current
authority: advisory
verified: 2026-09-07
tags:
  - vault/index
---

# Architecture Index

Architecture notes describe the implementation that exists now. Their frontmatter shows whether they were verified against live code and assets.

## Systems

- [[Knowledge/Architecture/Systems/Character System|Character System]] — partial verification.
- [[Knowledge/Architecture/Systems/Entity Locator System|Entity Locator System]] — verified required boundary for cross-entity gameplay communication.
- [[Knowledge/Architecture/Systems/Hitbox System|Hitbox System]] — mostly verified.
- [[Knowledge/Architecture/Systems/Jump and Roll System|Jump and Roll System]] — current locomotion implementation candidate.
- [[Knowledge/Architecture/Systems/Layer Service|Layer Service]] — mostly verified; serialized values still need Unity inspection.
- [[Knowledge/Architecture/Systems/Settings System|Settings System]] — needs review; its overview reflects the live coordinator, but later sections retain older future-state material.

## UI

- [[Knowledge/Architecture/UI/UI Route Navigation|UI Route Navigation]] — partial verification.
- [[Knowledge/Architecture/UI/Pause Navigation|Pause Navigation]] — mostly verified.
- [[Knowledge/Architecture/UI/Grace Navigation|Grace Navigation]] — needs review.

## Project Structure

- [[Knowledge/Architecture/Project Organization]] — advisory asset organization guidance.
- [[Knowledge/Architecture/Codebase Statistics]] — metrics on classes, types, lines of code, and module distributions.

Interaction and enemy encounter documents currently live in [[Research/Research Index|Research]] because they are audits or proposed migrations, not verified current architecture.
