---
name: soulslike-context
description: Resolve curated SoulsLike project knowledge by exact registry key. Use for role handoffs or tasks that require verified vault context, project policy, or feature guidance with an offline Markdown fallback.
---

# SoulsLike Context

Use `SoulsLikeGameVault/ai/Skill_Context_Index.md` as the only registry of vault context that agents may treat as project policy.

1. Resolve the exact context key requested by the task or domain skill.
2. Read the registered note through Obsidian MCP: obtain its document map, then read only the required headings.
3. If Obsidian MCP is unavailable, read the registered disk fallback directly.
4. Treat `required` entries as project constraints and `advisory` entries as feature guidance.
5. Report a missing path, missing heading, or conflict with repository evidence. Do not broaden the search or guess.

Do not treat unregistered or unverified vault notes as policy. Do not load unrelated notes or the whole vault.
