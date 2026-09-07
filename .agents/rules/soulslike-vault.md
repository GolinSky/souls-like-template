# SoulsLikeGameVault access

These rules apply to the SoulsLikeTemplate project and its vault.

Vault: F:\Private\SoulsLikeTemplate\SoulsLikeGameVault
Primary note server: soulslike-vault (MCPVault)

Use this server for normal note discovery, reading, content search,
creation, patching, frontmatter updates, and tag updates.
Use vault-relative paths in tool calls, for example `Work/Plans/My Plan.md`
or `Work/Issues/My Issue.md`.

Do not start Obsidian, check a REST endpoint, call the Obsidian CLI,
or ask the user to open Obsidian before ordinary note work.

Read existing notes before editing. Preserve unrelated content,
frontmatter, wikilinks, folder layout, and naming conventions.
Do not restructure the vault unless the active user request explicitly asks
for it. Never edit `.obsidian` without an explicit Obsidian configuration request.

For task context, resolve `Agent Guide/Agent Context Registry.md` by exact
context key or concrete task signals. For a named plan, issue, research package,
or implementation record, first load the registered workflow/index context,
then locate the exact named note only inside its lifecycle folder. Execute a plan
only when the user explicitly names it and its status is `ready` or `in-progress`.

Do not assume a file rename updates incoming links. For renames of
existing linked notes, inspect and deliberately update affected links,
or defer the operation rather than silently breaking references.
Do not delete existing user notes without explicit authorization.

Do not claim MCPVault ran an Obsidian plugin command or accessed the
active editor tab. Report such operations as outside this integration.

If the MCP connection fails, inspect its configuration/logs first.
Do not silently substitute shell access and report the MCP as working.
