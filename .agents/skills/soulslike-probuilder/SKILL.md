---
name: soulslike-probuilder
description: Create or revise editable ProBuilder graybox rooms, corridors, doorways, stairs, ramps, arenas, and traversal prototype geometry in this SoulsLike project through its official Unity CLI/Pipeline bridge. Use for prototype geometry and structural inspection, not unrelated C# fixes, final art, or runtime world generation.
---

# SoulsLike ProBuilder

Compose with `soulslike-unity-assets` for Editor operations and resolve relevant
registered context with `soulslike-context`. For C# command changes, use
`soulslike-csharp-change`. Preserve the existing official `unity` / `unity mcp`
bridge; the source playbooks' other servers are not dependencies.

Before acting, verify the exact worktree and Editor with `unity status --json`.
Pass `--project-path` explicitly when multiple Editors exist. Inspect the live
command schema before invoking an operation. Read
[command-contract.md](references/command-contract.md) for the implemented surface.

## Build loop

1. Turn the brief into a bounded specification with stable element IDs, material
   roles, measured clearances, and explicit entry/exit connections. Read
   [recipes.md](references/recipes.md) for the coordinate and composition contract.
2. Inspect existing owned content and preview the proposed change before writing.
   Treat manual-edit conflicts as requiring reconciliation; never resolve them
   by deleting ownership records or silently regenerating the scene.
3. Build only the scoped prototype. Keep ProBuilder authoring data, shared
   compatible materials, and explicit static colliders. A visible opening must
   also be open in collision geometry.
4. Persist the intended scene/assets inside the operation and inspect the output.
   Read [validation.md](references/validation.md) before a reload or validation.
   Respect explicit user requests to skip tests.

Read [setup.md](references/setup.md) only for package setup. Source attribution
and adaptations are recorded in [SOURCES.md](SOURCES.md).

## Boundaries and report

Never save or discard unrelated dirty scenes. Keep one Editor writer. Do not
add player/camera/service stacks without checking the project bootstrap.
Spawn, camera, shortcut, or enemy markers are placeholders unless backed by
verified gameplay integration. Do not infer traversal or combat success from
generated geometry. Play Mode validation remains a separate authorized phase.

Report changed scene/spec/asset paths and element IDs, actual commands used,
import/persistence state, conflicts, checks performed and skipped, and remaining
gameplay/design decisions. Distinguish generation, structural inspection,
gameplay verification, and design approval.
