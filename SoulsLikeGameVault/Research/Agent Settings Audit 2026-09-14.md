---
title: Agent Settings Audit 2026-09-14
type: research
status: draft
reviewed: 2026-09-14
source_commit: e5180a25173576f4e7219cbe34a8bf93540a8608
tags:
  - agent/configuration
  - audit
---

# Agent Settings Audit 2026-09-14

Your setup needs a few targeted fixes, not a wholesale rewrite. The existing project ownership, vault routing and official Unity integration are largely sound. The most important gap is that “read-only” role declarations are being treated as stronger isolation than the current runtime actually provides.

This report compares all three supplied documents with the accessible Windows installation. No live configuration, skill, role, credential, gameplay code or Unity asset was changed. Five small patches are staged for review; the repair plan remains draft.

## Question and Desired Decision

Which agent instructions, skills, MCP settings and roles are correct, which demonstrably fail, and what is the smallest safe repair sequence?

Review the proposed compatibility/reference fixes first, then decide the intended role permission boundaries and parent model policy. Do not approve credential or cross-machine changes by implication.

## Scope and Non-Goals

Inspected repository and personal Codex declarations, 26 project skill entry points and selected resources, eight role definitions, two competing personal skill packages, execution rules, relevant plugin manifests and existing client adapters. Read the three Drive documents completely and rechecked official documentation. The dated evidence directory includes environment, inventories, load map, findings, preservation map, validation and rollback.

macOS, upstream settings sync and Gemini/Claude runtime behavior remain unverified. No package upgrade, Unity feature work, Obsidian change or benchmark was performed.

## Current System Map

Global instructions and config supply personal defaults. Repository AGENTS.md routes to project skills, roles and exact vault context. Project config adds official Unity, Serena and vault services and disables legacy Unity for this repository. Most roles inherit unspecified global tools and the parent's current permissions.

Installed CLI diagnostics confirmed the expected root/nested/outside instruction boundary. Raw global plus repository instructions total 24,228 bytes, below the observed 32,768-byte setting; the project end marker appears in rendered prompts. There is no evidence that AGENTS.md needs shortening to meet a limit.

## Entry Points, Dependencies and Consumers

- Personal entry: C:/Users/golin/.codex/AGENTS.md and config.toml.
- Repository entry: AGENTS.md and .codex/config.toml.
- Roles: .codex/agents; reusable workflows: .agents/skills.
- Vault policy: Agent Context Registry plus registered headings.
- Current clients: PATH CLI 0.154.0; app-associated CLI 0.154.0-alpha.6.2.
- Unity: CLI 1.0.0-beta.6, Pipeline 0.5.0-exp.1, Editor 6000.3.11f1, correct project, clean ElevatorDemo scene.

## Evidence and Findings

| Priority | Finding | Recommended treatment |
|---|---|---|
| P1 | Curator read-only declaration does not isolate this task: full-access runtime and write-capable tools are inherited | Correct the README claim, then test an explicit permission/tool policy |
| P2 | features.rmcp_client is unknown and fails strict startup in both installed clients | Remove that single line |
| P2 | Both Graphify copies and a conflicting legacy Unity skill are discoverable/enabled in project inventories; runtime misrouting was not reproduced | Disable the personal competitors only in this repository |
| P2 | ProBuilder links a missing command-contract.md | Route to the already verified live command schema |
| P2 | CV-project filesystem tools are exposed in SoulsLike tasks | Add a project-only service disable |
| P2 | Sol/High parent prose disagrees with Astra/xhigh config | Choose intended behavior before changing either |
| P2 | Inline auth material exists, including in tracked project config | Report only; separate explicit credential/Obsidian decision |
| P2/P3 | Legacy missing resources, agy metadata warning, broad command allowances and Graphify build routing | Repair only the relevant owning surface after the small pilot |

The [full findings](<Agent Settings Audit 2026-09-14/03_FINDINGS.md>) include exact paths/lines, confidence, false-positive checks, smallest fixes, acceptance tests and rollback.

## Options and Tradeoffs

Recommended: preserve the present layout and apply a small compatibility/reference batch, then a separately validated project-scoping batch. This addresses observed failures while retaining other projects' personal skills and integrations.

A broad AGENTS/skills rewrite is not justified by this audit. It would create semantic migration work without evidence of truncation or savings. The newly retrieved [Astra article](https://learn.chatgpt.com/blog/rethinking-skills-and-prompts-for-gpt-6-astra) supports clearer triggers and task-specific reading, not a mandatory rewrite.

## Risks, Unknowns and Open Questions

- Current parent and curator runtime use danger-full-access/never. A role file alone cannot guarantee isolation.
- No actual misuse or remote write was attempted; advertised capabilities do not prove a remote service would authorize them.
- No settings-source checkout/sync contract was identified in bounded discovery.
- Skill routing after the proposed overrides, all-role enforcement, hook replay, macOS and live rollback are not yet tested.
- An early diagnostic exposed a Penpot token in this task's tool output before URL redaction was corrected. Saved report files exclude configured credentials; the raw transcript is not a sanitized export.
- No token/quota saving is claimed.

## Recommended Review Questions

1. Is the desired parent still Sol/High, or should the prose follow the installed Astra/xhigh default?
2. Should read-only roles have enforced isolation, or are they intentionally operating under the parent's full-access permissions?
3. Where is the authoritative settings-sync source, and is macOS currently part of the rollout?
4. Should credential handling and broad MCP add/login allow rules be addressed in a separate authorized batch?

These questions do not block review of the concrete reference and compatibility proposals.

## Handoff

Read the [draft repair plan](<../Work/Plans/Agent Configuration Repair Plan 2026-09-14.md>), [staged patch manifest](<Agent Settings Audit 2026-09-14/patches/manifest.json>), [validation results](<Agent Settings Audit 2026-09-14/05_VALIDATION.md>) and [rollback procedure](<Agent Settings Audit 2026-09-14/06_ROLLBACK.md>).

All five patches pass individual dry-run checks. Patches 03/04 share a config baseline and must be merged into one reviewed diff if approved together. Configuration remains unchanged.
