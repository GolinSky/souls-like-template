---
name: soulslike-unity-architecture
description: Design high-risk or ambiguous SoulsLike Unity systems, refactors, and migrations. Use when a bounded implementation needs architecture, ownership, lifetime, serialization, or asset decisions first.
---

# SoulsLike Unity Architecture

Design the smallest coherent change supported by the current execution path.

- Establish ownership, lifetime, state, dependency-injection, serialization, asset, and Unity lifecycle boundaries.
- Identify exact files, symbols, assets, migration steps, invariants, and rollback points.
- Compare only viable alternatives and state concrete tradeoffs.
- Define acceptance criteria and non-overlapping writer scopes.
- Load `$soulslike-context` only for an applicable registered context key; add `$soulslike-ui-workflow` for UI work.

Produce an implementation-ready plan. Do not mutate the project.
