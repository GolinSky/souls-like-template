# Evidence register

Access date: 14 September 2026. Public sources establish product behavior; local files and installed-client probes establish applicability. The three supplied documents are preserved in sources/.

## User documents

- [01 — Verified OpenAI guidance](https://drive.google.com/file/d/1XXjI7RrOwh_sAetNIcVElQhTnEej1kVp/view)
- [02 — Audit and refactor plan](https://drive.google.com/file/d/1Q0ahD4pV6tveDBdEOIuDODqo7RZemnD1/view)
- [03 — Execution prompts and checklist](https://drive.google.com/file/d/1up7KWiXTz_pS23FWBfsLEGRc9IWXxIqv/view)

## Official material rechecked

| Rule | Source | Verified use in this audit |
|---|---|---|
| R-LOAD | [AGENTS.md discovery](https://learn.chatgpt.com/docs/agent-configuration/agents-md) | Separate global and root-to-cwd discovery; selected files verified with installed prompt rendering |
| R-SKILL | [Build skills](https://learn.chatgpt.com/docs/build-skills) | Progressive disclosure, distinct skill packages and catalog budgeting; no automatic merging assumption |
| R-CONFIG | [Config basics](https://learn.chatgpt.com/docs/config-file/config-basic) | Trusted project, profile, user and system layering; confirmed by config/read |
| R-VERSION | [Configuration reference](https://learn.chatgpt.com/docs/config-file/config-reference) | Per-folder skills.config enablement; MCP per-server/tool controls; current concurrency/reviewer keys |
| R-AGENT | [Subagents](https://learn.chatgpt.com/docs/agent-configuration/subagents) | Parent runtime permission overrides can supersede role defaults; connector permissions separate |
| R-MCP | [MCP configuration](https://learn.chatgpt.com/docs/extend/mcp?surface=cli) | Transport, credentials, enablement and tool restrictions are different surfaces |
| R-EXEC | [Execution rules](https://learn.chatgpt.com/docs/agent-configuration/rules) | Prefix rules and static checker; no command execution needed to test matching |
| R-ASTRA | [Rethinking skills and prompts for GPT-6 Astra](https://learn.chatgpt.com/blog/rethinking-skills-and-prompts-for-gpt-6-astra) | Newly retrieved article, publication date shown as 11 September 2026 |

R-ASTRA recommends clear task-specific descriptions and selective document retrieval, and encourages reassessing unnecessary procedural instructions. It acknowledges mixed-model repositories. These are recommendations, not automatic authorization to remove safety constraints, change models or refactor every skill.

The earlier document's reported discrepancy about untrusted approvals has changed: the current Config basics page points to migration from retired untrusted mode. No such setting was found here. The installed CLI help, protocol types and public docs are not interchangeable; this audit does not migrate a nonexistent setting.

The Harness engineering 100-line example remains background supplied by document 01. It was not needed to establish any finding; no universal line limit was inferred.

## Local evidence

- inventory_manifest.json: selected file hashes, sizes, timestamps and scope.
- config_root/nested/outside.sanitized.json: effective settings and layer origins.
- requirements.sanitized.json: null managed requirements returned by fresh CLI.
- skills_discovery.json: enabled duplicate and legacy skill entries, exact paths.
- prompt_load_checks.json: actual rendered instruction boundary checks.
- client_initialize.json: observed CLI version/home/platform.
- client_stderr_strict.sanitized.json: rejected rmcp_client field.
- client_stderr.sanitized.json: non-strict warnings and vendor metadata warnings.
- patch_checks.json and patches/manifest.json: individual dry-run/parser status and exact baselines.
- Runtime parent/curator metadata: capability observations summarized in report; no production writes.

