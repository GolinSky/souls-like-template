# Rollback and repair safety

No live repair occurred, so no live rollback is currently necessary. The proposal manifest records baseline SHA-256 for every target.

## Before any approved batch

1. Re-read exact targets and compare hashes against patches/manifest.json and inventory_manifest.json. A mismatch requires reconciliation with newer edits.
2. Create prechange copies in a restricted local backup directory outside repository/Drive/sync roots. Personal config and project config contain credentials: never commit or upload their full backups.
3. Record byte encoding and line endings. Do not replace a whole config from an old template.
4. Identify the actual source/installed mapping or record an explicitly accepted Windows local-only pilot.

## Exact proposed targets

| Batch | Target | Reversal |
|---|---|---|
| 01 | C:/Users/golin/.codex/config.toml | Restore only removed rmcp_client line if the tested batch must be reverted; retain all newer unrelated settings |
| 02 | F:/Private/SoulsLikeTemplate/.agents/skills/soulslike-probuilder/SKILL.md | Reverse only command-discovery text replacement |
| 03 | F:/Private/SoulsLikeTemplate/.codex/config.toml | Remove only the two approved skills.config entries |
| 04 | Same project config | Remove only the approved cv_filesystem override |
| 05 | F:/Private/SoulsLikeTemplate/.codex/agents/README.md | Reverse only capability-description replacement |

03 and 04 share a baseline and need a combined reviewed diff if applied together. Do not restore the entire config to undo one block.

## Verification after repair or reversal

Restart only affected Codex sessions so instruction/skill/MCP discovery is fresh. Re-run config/read and skills/list from root/nested/outside, plus strict startup after batch 01. Confirm intended model/effort/reviewer remain unchanged. For tool-scoping changes, verify tool inventory and use only harmless reads.

Do not launch competing Unity Editors or reload/save scenes as part of configuration rollback. Existing Unity/Vault integration should still answer read-only checks. No service credentials, Obsidian state, Git history or unrelated project files should change.

If a patch introduces a regression, reverse only that batch and document evidence. Never git reset --hard, auto-stash user work, restore the entire home directory, or rotate credentials as a rollback shortcut.

macOS rollback is not specified as tested: it needs actual host paths, installation version and source mapping first.

