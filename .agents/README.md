# Project agent skills

Project skill packages are sibling directories under `.agents/skills`. Custom Codex roles remain under `.codex/agents`, and their instructions compose the required workflow skill with only the domain skills named by the parent handoff.

## Ownership

- `AGENTS.md` defines repository policy and the authority order.
- `.codex/config.toml` is the canonical Codex MCP server and multi-agent-defaults file.
- `.codex/agents/*.toml` narrows a role's model, sandbox, workflow, and tools.
- `.agents/skills/*` contains reusable workflows. Upstream Graphify references remain upstream workflow material, not SoulsLike policy.
- `SoulsLikeGameVault/Agent Guide/Agent Context Registry.md` is the curated allow-list for exact-key and task-signal context discovery; it is not a general vault directory index.

## Generated Graphify state

`graphify-out/` is ignored operational state and must be excluded from broad documentation or source searches. Retain the current graph, report, visualization, cost/manifest files, and the 20 newest query-memory files. Older reports and query snapshots are disposable; rebuild the graph when current source evidence is required.
