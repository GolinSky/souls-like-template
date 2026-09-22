# OpenAI agent settings: verified guidance and migration boundaries

**Research date:** 13 September 2026  
**Scope:** Codex instruction Markdown, skills, configuration, MCP, subagents, execution rules, and related startup hooks.  
**Status:** Official-source research completed. The user's installed global/project settings have **not** been inventoried, audited, or changed. This document is not an audit result.

## Read this first

The general advice to reorganize instructions is supported by OpenAI's documentation: keep the always-present entry point useful, route specialized work to focused capabilities, and validate the configuration that the client actually loads. This is **not** evidence that every Markdown file must be shortened, every existing skill rewritten, or every older setting immediately removed.

Three categories must remain distinct:

| Label | Meaning | How the audit should treat it |
|---|---|---|
| **Documented behavior** | Discovery, file format, precedence, limits, or configuration semantics described by OpenAI. | Verify applicability to the installed client, then test it. |
| **Official recommendation/example** | Advice or an implementation described by OpenAI. | Evaluate against the user's workflow; do not report a violation merely because the implementation differs. |
| **Proposed audit policy** | A safeguard or organizational choice designed for this user's setup. | Identify it as our recommendation, with its own rationale and acceptance test. |

**Recency boundary:** These are the official pages retrieved on the research date, not a claim that every behavior was introduced that day. Where no publication/update date was established, record the access date rather than inventing a release date. Public documentation and an installed older client can legitimately differ.

## 1. What is actually verified

### R-DOC — Small entry point, detailed supporting knowledge

OpenAI's *Harness engineering* case study describes a short `AGENTS.md` that points into a structured repository knowledge base. Its roughly 100-line entry point is an example from that team, **not a universal limit**. The article also describes keeping documentation useful through automated checks and maintenance. [S1]

**Audit implication:** Identify unnecessarily always-loaded material and give it an explicit retrieval route. Preserve useful reference content and trace every removed instruction to its replacement or an evidence-backed retirement decision. A long supporting document is not intrinsically defective.

### R-LOAD — Instruction discovery has concrete rules

The documented global entry point is in `CODEX_HOME`, normally `~/.codex`: the first nonempty `AGENTS.override.md`, otherwise `AGENTS.md`. Project discovery walks from the project root toward the working directory. Each directory contributes at most one selected file: override, ordinary AGENTS, then configured fallback names. Later, narrower guidance can override earlier guidance. Discovery has a configurable combined byte budget, `project_doc_max_bytes`, defaulting to 32 KiB. [S2]

**Audit implication:** Reconstruct the real startup chain. Do not assume both same-directory files merge, all descendant instructions are loaded, or moving content into another active ancestor file evades the budget. Test new sessions and different working directories.

### R-SKILL — A skill is a discoverable workflow, not just any Markdown file

The skills guide documents `SKILL.md` with `name` and `description`, optional resources, and progressive disclosure. Personal/project authoring uses `.agents/skills` roots; the guide also describes administrative and bundled skills. Same-name skills are not automatically merged. `agents/openai.yaml` supplies optional skill metadata/policy rather than defining a standalone worker. Plugins are a distribution route, not a requirement to abandon local authoring. [S3]

**Audit implication:** Verify each installation's actual discovery roots, trigger behavior, dependencies, and resource links. Avoid blanket removal of legacy or bundled directories. Prefer a focused skill over one that claims unrelated tasks. Test explicit invocation separately from implicit selection.

### R-CONFIG — Audit effective configuration, not isolated TOML

The current basics page documents trusted project configuration and this high-to-low precedence: command-line overrides; project layers, nearest first; selected profile file; user configuration; system configuration; defaults. Its profile example uses `~/.codex/profile-name.config.toml`. Managed requirements constrain permitted configuration separately. Untrusted project layers are skipped. [S4]

**Audit implication:** Discover the installed version and its supported layering first. Include wrappers, selected profiles, working directory, trust, and managed restrictions. Do not rewrite an older supported profile layout simply to resemble the newest website.

### R-VERSION — Some concrete migration details are real, but version-sensitive

The current reference calls `agents.max_threads` a **legacy alias** for `agents.max_concurrent_threads_per_session`. It documents `approvals_reviewer` with `user` and `auto_review` choices. A supported alias is not automatically a broken configuration. [S5]

There is also a documentation discrepancy worth recording: the basics page still discusses direct `approval_policy="untrusted"`, while the current reference marks direct `untrusted` unsupported and `on-failure` deprecated. Resolve applicability against the installed client and its matching schema/help, rather than guessing a replacement. [S4, S5]

**Audit implication:** Separate invalid, deprecated, supported legacy, and merely nonpreferred settings. Preserve the user's preference for manual review through the **actual supported mechanism**, not an assumed feature flag.

### R-AGENT — Custom subagents have their own configuration and inheritance

The subagents guide documents custom TOML definitions in personal/project `agents` directories, with `name`, `description`, and `developer_instructions`. Model, tools, and permissions require inheritance-aware inspection. Live parent permission/sandbox overrides can be reapplied when spawning children; a read-only-looking role file alone is not proof of isolation. Custom names can shadow built-in roles. [S6]

**Audit implication:** Verify what is discovered and what each spawned role actually receives. Do not assume a file named `subagents.toml` has native significance without identifying a supported loader. Do not confuse Codex controls with similarly named Responses API or Agents SDK controls.

### R-MCP — Connectivity, credentials, and tool permissions are separate concerns

OpenAI documents STDIO and HTTP MCP configurations, reversible disabling, environment-based credential references, and tool allow/deny controls. Plugin-provided servers have their own configuration path and policy boundaries. [S7]

**Audit implication:** Inspect transport, startup, credentials handling, service duplication, and the exact capabilities exposed. A healthy connection is not proof that all its tools are appropriate for every task or subagent. Review launch commands before invoking untrusted servers; test only harmless operations.

### R-EXEC — Markdown instructions are not executable security rules

The execution-rules guide describes Starlark `.rules` files, rule checks, and restrictive conflict resolution: `forbidden` outranks `prompt`, which outranks `allow`. This is different from instruction-file precedence. The guide marks execution rules experimental. [S8]

**Audit implication:** Keep prose guidance, command policy, filesystem sandboxing, approvals, and remote-service permissions distinct. Test rule matching with the supported checker rather than executing a command to discover whether it would be allowed.

## 2. Claims that should not become automatic refactoring rules

| Claim | Verdict for this plan |
|---|---|
| “Every `AGENTS.md` must be at most 100 lines.” | Not established. The cited number belongs to an OpenAI case study. [S1] |
| “Every `.md` over 32 KiB is invalid.” | Incorrect generalization. The cited limit concerns the discovered instruction chain, not all reference files. [S2] |
| “All knowledge should be pasted into the global instructions.” | Rejected as our design choice; it makes project boundaries and selective retrieval harder to maintain. |
| “Every old directory or setting must be deleted.” | Unsupported. Establish whether it is loaded, supported, bundled, custom, or obsolete first. [S3, S5] |
| “A local skill with the same name automatically replaces the global one.” | Do not assume this; the guide explicitly distinguishes duplicate skill discovery from merging. [S3] |
| “`agents/openai.yaml` and a subagent TOML file are interchangeable.” | They are different configuration surfaces. [S3, S6] |
| “OpenAI mandates one shared instruction format for Codex, Gemini, and Claude.” | Not established by the sources reviewed. Preserve existing adapters and verify each other client's own documented behavior before changing them. |
| “A read-only worker cannot perform remote writes.” | Do not infer this from a role label. Local sandbox and remote-tool authorization need separate verification. [S6, S7] |
| “Refactoring instructions guarantees lower quota use.” | No such guarantee is established here. Measure comparable tasks; quality and safety remain hard gates. |
| “Changing the model and restructuring skills together proves the restructuring worked.” | Rejected experimental design. Hold model, reasoning, tools, and task inputs fixed during the comparison. |

## 3. Research limitation: the recent skills/prompts article

Official site navigation exposed a post titled **“Rethinking skills and prompts for GPT-6 Astra.”** The article body could not be reliably retrieved in this research session. Its specific recommendations and publication date are therefore **not treated as verified evidence**. The plan below relies on the independently retrieved documentation and case study listed in the source register, not an inferred summary of that post.

Article link found in the official navigation: <https://learn.chatgpt.com/blog/rethinking-skills-and-prompts-for-gpt-6-astra>

During the local audit, retry that official article and add any newly verified claims to the evidence register, including exact applicability. Do not interrupt the rest of the audit merely because that page remains unavailable.

## 4. What was checked in the user's Drive

The connected Drive folder **BaseSettings** was located and its direct contents were listed. Existing entries include the dark-fantasy UI rules, Unity UI layout/troubleshooting rules, ProBuilder and Animation Rigging plans, Obsidian guide, and the two reasoning-usage measurement plans.

This was a **metadata-level check**, not a review of those documents' contents or proof that any of them is installed as local instructions. No existing files were moved, rewritten, or deleted by this research task.

Folder: <https://drive.google.com/drive/folders/1pIaPEufoCoAqow6urboE84QE2U4acLzI>

## 5. How to use this package

Read the [audit and refactoring plan](2026-09-13_02_AUDIT_AND_REFACTOR_PLAN.md), then use the [execution prompts and checklist](2026-09-13_03_EXECUTION_PROMPTS_AND_CHECKLIST.md) in a local agent with access to the actual machines/repositories.

The recommended first operation is a **read-only audit with staged patches**, not a wholesale configuration replacement. Live repair is a separate, explicitly authorized stage.

## Official source register

All sources below were accessed on **13 September 2026**. No publication date is asserted where it was not separately established. Some older `developers.openai.com/codex/...` routes now resolve to these official ChatGPT Learn pages.

- **[S1] OpenAI — Harness engineering: leveraging Codex in an agent-first world.** <https://openai.com/index/harness-engineering/>
- **[S2] OpenAI — Custom instructions with AGENTS.md.** <https://learn.chatgpt.com/docs/agent-configuration/agents-md>
- **[S3] OpenAI — Build skills.** <https://learn.chatgpt.com/docs/build-skills>
- **[S4] OpenAI — Config basics.** <https://learn.chatgpt.com/docs/config-file/config-basic>
- **[S5] OpenAI — Configuration Reference.** <https://learn.chatgpt.com/docs/config-file/config-reference>
- **[S6] OpenAI — Subagents.** <https://learn.chatgpt.com/docs/agent-configuration/subagents>
- **[S7] OpenAI — Model Context Protocol.** <https://learn.chatgpt.com/docs/extend/mcp?surface=cli>
- **[S8] OpenAI — Execution rules.** <https://learn.chatgpt.com/docs/agent-configuration/rules>

