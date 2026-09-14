---
title: Agent Configuration Repair Plan 2026-09-14
type: plan
status: draft
created: 2026-09-14
source_commit: e5180a25173576f4e7219cbe34a8bf93540a8608
tags:
  - agent/configuration
  - plan
---

# Agent Configuration Repair Plan 2026-09-14

## Plan Contract

### Goal

Repair demonstrated configuration/reference defects and make project skill/tool selection explicit, while preserving working Unity integration, user settings and other-client behavior.

### Source Research and Decisions

Source: [[Research/Agent Settings Audit 2026-09-14]] and its evidence directory. The audit read all three supplied documents. Five proposals are staged; no live repair is authorized by the report request.

Official guidance distinguishes discovery, config layering, runtime permissions, skills and command rules. The current installed client, not an unpinned latest schema, determines compatibility.

### Assumptions and Non-Goals

Initial pilot is this Windows repository only. Keep existing model/effort/reviewer, vendor ownership, user changes, Unity conventions and vault protection. No installations, publishing, trust changes, accounts/endpoints, Obsidian configuration changes, broad Markdown rewrite or quota benchmark.

Personal settings source and macOS installation are unresolved. A repair may be called Windows-local only until sync ownership is verified.

### Success Criteria

- Strict startup no longer rejects rmcp_client on the two inspected clients.
- Required ProBuilder discovery no longer points to a missing file.
- Role documentation accurately describes inheritance; any claimed enforced role boundary has direct fixture evidence.
- Fresh root/nested discovery exposes the intended project skills; outside discovery retains personal packages.
- CV service is absent in this project's fresh tool catalog if batch B is approved.
- Models/effort/reviewer and working Unity/vault routes remain unchanged.
- All changed obligations have a preservation entry, verified diff and reversible baseline.
- Inaccessible/unexecuted checks remain explicit gaps.

## Execution Plan

| Batch | Exact scope | Work | Acceptance |
|---|---|---|---|
| A0 — Baseline | Audit manifest and current target files | Reconcile hashes and newer edits; identify settings source; make protected prechange snapshots | No unrelated change lost; no secret in exported diff |
| A1 — Compatibility | C:/Users/golin/.codex/config.toml:257 | Apply patch 01: remove only features.rmcp_client | Both client strict startups succeed; normal config/MCP behavior unchanged |
| A2 — Reference repair | .agents/skills/soulslike-probuilder/SKILL.md:15–16 | Apply patch 02: use live unity command --json schema | Link/discovery check succeeds; no asset mutation |
| A3 — Accurate role docs | .codex/agents/README.md:24 | Apply patch 05 | No claim that undeclared services are automatically excluded; role responsibilities unchanged |
| B — Windows project scope | .codex/config.toml | Merge patches 03/04 into one reviewed edit if both approved: disable two personal skills and inherited CV service | Fresh root/nested/outside discovery and harmless tool inventory match intent |
| C — Policy decision | AGENTS.md:64; role README:22; possibly project model default | Resolve intended parent model, then edit only the chosen authority surface | Fresh task shows chosen behavior; benchmarking remains separate |
| D — Enforced role boundaries | Exact selected role TOMLs and supported server/plugin policy | Define allowed tools per role; choose effective parent permissions; validate with disposable fixtures | Read-only claim demonstrated; no unneeded connector writes; no production probe |
| E — Owning-source maintenance | Legacy Unity skill, agy source manifest, personal rules and sync source | Repair only individually approved items; preserve other clients; reinstall only with authorization | Resource checks, plugin metadata acceptance, rule fixtures and source-sync proof |

Batch D must not change Obsidian configuration or MCP authentication without an explicit Obsidian configuration request. If tool policy needs such an edit, split that component out and document the dependency. Do not “solve” inherited full-access permissions by assuming a read-only TOML line is enforcement.

Batch E is deferred until source ownership is known. Do not patch plugin caches or apply Windows absolute paths on macOS without an actual host mapping.

## Risks and Rollback

Follow the audit's 06_ROLLBACK.md. Keep secret-bearing backups local with restrictive access. Restore only edited lines/blocks and preserve subsequent user changes. Patches 03 and 04 target the same baseline; merge before applying.

No reset, automatic stash, whole-config replacement, Git history rewrite or credential rotation. Restart affected Codex sessions and rerun the narrow checks after repair/reversal.

## Validation

Use V01–V14 as applicable from the audit. Minimum for A: TOML parse, strict app-server startup, root/nested/outside prompt/config inspection, broken-link check and read-only Unity/vault smoke.

For B: fresh skills/list and tool inventory; two positive, two negative and explicit invocations for the adjusted skills. For D: actual role runtime plus a disposable boundary fixture. For E: source-to-install regeneration and rollback on each real platform.

Do not run Unity gameplay/EditMode tests for configuration/document-only batches. Do not save/reload scenes. If future integration work actually requires UTF, resolve unity-testing and follow the project protocol separately.

Do not count CLI request success alone as a test pass. No savings claim until comparable repeated measurements preserve task quality.

## Execution Handoff

Status remains draft until reviewed. Execution requires a separate user request and ready/in-progress status under project policy.

Required context keys: vault-usage and plan-workflow; research-workflow for report updates. No UI or animation context is necessary for these config-only batches. Any actual Unity asset/test work requires its own assigned workflow and context.

Writer: one coordinator for all shared settings. Independent review can inspect the proposed diff and semantic map without editing the same files. No new permanent roles.

Unity asset implications: none intended; official Pipeline project routing and dirty-scene state must remain unchanged.

Remaining decisions: approval of batches; parent model intent; required role isolation; settings-sync source/macOS scope; separately authorized credential, Obsidian, installation or account changes. The smallest runnable repair proposal is A0–A3.

