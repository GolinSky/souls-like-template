# Project agent skills

Project skill packages are sibling directories under `.agents/skills`. Custom Codex roles remain under `.codex/agents`, and their instructions compose the required workflow skill with only the domain skills named by the parent handoff.

## Ownership

- `AGENTS.md` defines repository policy and the authority order.
- `.codex/config.toml` is the canonical Codex MCP server and multi-agent-defaults file.
- `.codex/agents/*.toml` narrows a role's model, sandbox, workflow, and tools.
- `.agents/skills/*` contains reusable workflows. Upstream Graphify references remain upstream workflow material, not SoulsLike policy.
- `SoulsLikeGameVault/ai/Skill_Context_Index.md` is a narrow exact-key allow-list, not a vault directory index.
