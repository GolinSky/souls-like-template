---
name: soulslike-context
description: Resolve curated SoulsLike project knowledge by context key or task signals. Use for role handoffs or tasks that require verified vault context, project policy, feature guidance, plans, issues, research, or implementation history.
---

# SoulsLike Context

Use `SoulsLikeGameVault/Meta/Agent Context Registry.md` as the only registry of vault context that agents may treat as project policy.

1. If the task supplies an exact context key, resolve it directly. Otherwise match concrete task nouns, systems, file types, and actions against the registry's task signals. Do not use vague semantic similarity.
2. Load every directly matching `required` entry. Load only the matching `advisory` entries needed for the assigned scope; ignore entries marked `stale` or `needs-review` as policy.
3. Read the registered note through Obsidian MCP: obtain its outline, then read only the registered headings.
4. When the task explicitly names a plan, issue, research package, or implementation record, first load the registered workflow and work-queue context. Then search only the corresponding lifecycle folder (`Work/Plans`, `Work/Issues`, `Research`, or `History/Records`) for an exact title, alias, or path match and read that note. For a completed plan or issue, also resolve its exact title or alias in `History/Completed Plans` or `History/Closed Issues`; superseded evidence belongs in `History/Superseded`. Preserve user-directed closure and recorded validation limits; an older audit does not reopen completed work. A named work artifact scopes the task but never becomes project policy.
5. Execute a plan only when the user explicitly directs execution and its frontmatter status is `ready` or `in-progress`. Treat `draft` as unapproved, `blocked` as requiring its stated dependency, and `done` as historical.
6. If Obsidian MCP is unavailable, read the exact disk fallback directly.
7. Treat `required` entries as project constraints, `advisory` entries as guidance, `evidence` entries as investigation material, and `historical` entries as non-current records.
8. Report a missing path, missing heading, ambiguous signal match, ambiguous named-artifact match, stale verification, or conflict with repository evidence. Live source, serialized assets, and current tool output win.

Do not treat unregistered or unverified vault notes as policy. Do not load unrelated notes or the whole vault. Tags and properties improve discovery but never grant authority.
