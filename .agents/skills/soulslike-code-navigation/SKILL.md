---
name: soulslike-code-navigation
description: Map a bounded SoulsLike code path and its dependencies for exploration handoffs. Use for architecture discovery, impact analysis, ownership tracing, or locating entry points before a Unity change.
---

# SoulsLike Code Navigation

1. Use `$graphify` first for broad relationships when the existing graph is available.
2. Narrow to candidate files and symbols; verify important graph claims against live source and symbol references.
3. Trace entry points, calls, state ownership, dependency-injection boundaries, and serialized or editor dependencies.
4. Separate verified facts from stale-graph risk and unresolved questions.

Return a compact handoff with relevant files and symbols, the real execution path, direct dependencies, likely impact surface, and unknowns. Stay read-only.
