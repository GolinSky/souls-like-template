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

- [[Architecture/Systems/Character System|Character System]] — partial verification.
- [[Architecture/Systems/Entity Locator System|Entity Locator System]] — verified required boundary for cross-entity gameplay communication.
- [[Architecture/Systems/Hitbox System|Hitbox System]] — mostly verified.
- [[Architecture/Systems/Jump and Roll System|Jump and Roll System]] — current locomotion implementation candidate.
- [[Architecture/Systems/Layer Service|Layer Service]] — mostly verified; serialized values still need Unity inspection.
- [[Architecture/Systems/Settings System|Settings System]] — needs review; its overview reflects the live coordinator, but later sections retain older future-state material.

## UI

- [[Architecture/UI/UI Route Navigation|UI Route Navigation]] — partial verification.
- [[Architecture/UI/Pause Navigation|Pause Navigation]] — mostly verified.
- [[Architecture/UI/Grace Navigation|Grace Navigation]] — needs review.

## Project Structure

- [[Project Organization]] — advisory asset organization guidance.

Interaction and enemy encounter documents currently live in [[../Research/Research Index|Research]] because they are audits or proposed migrations, not verified current architecture.
