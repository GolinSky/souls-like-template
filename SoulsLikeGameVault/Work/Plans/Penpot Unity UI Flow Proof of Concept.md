---
title: "Penpot Unity UI Flow Proof of Concept"
type: plan
domains:
  - ui
  - tooling
status: in-progress
authority: advisory
updated: "2026-09-09"
aliases:
  - PENPOT_CODEX_UNITY_UI_FLOW_SETUP_PLAN
tags:
  - work/plan
  - status/in-progress
---

# Penpot Unity UI Flow Proof of Concept

## Plan Contract

### Goal

Execute the user-approved external plan at `C:\Users\golin\Documents\PENPOT_CODEX_UNITY_UI_FLOW_SETUP_PLAN.md` to determine whether Penpot, Penpot MCP, Codex, and the project's existing Unity CLI/Pipeline bridge form a repeatable UI workflow.

### Source Research and Decisions

- The external plan is the execution checklist and feature scope.
- `AGENTS.md`, the registered `ui-code` context, and live repository/Unity evidence remain authoritative.
- The user confirmed that pre-existing dirty worktree files are expected and must not be touched or included.
- The experiment uses the existing uGUI, TMP, VContainer, Addressables, Controller-Presenter-View, Unity CLI, and Unity MCP architecture.

### Assumptions and Non-Goals

- Work is isolated on `experiment/penpot-ui-flow`.
- Existing dirty files remain untouched.
- No production UI replacement, alternate Unity MCP, automated importer, or broad refactor is in scope.
- Human approval is required after the three Penpot design variants and before Unity implementation.

### Success Criteria

- Penpot MCP can read and write an editable Penpot design.
- The approved design can be implemented through the existing Unity UI architecture.
- Unity compiles with no new errors and the prefab has no missing references.
- Deterministic screenshots can be captured and compared at the target resolutions.
- The final results report gives evidence-backed answers to the six decisions in the external plan.

## Execution Plan

- [ ] Phases 0-2 — audit the project and verify Unity CLI/MCP; verify with live Editor queries.
- [ ] Phases 3-4 — configure Penpot Cloud/MCP and install the official AI Kit; verify structured read/write probe.
- [ ] Phases 5-8 — document the design contract and create three Penpot variants; verify then stop for human selection.
- [ ] Phases 9-12 — extract the approved contract, implement isolated Unity UI, compile, capture, compare, and correct.
- [ ] Phases 13-14 — document the repeatable workflow and final recommendation.

## Risks and Rollback

- Penpot requires a user-owned account, MCP key, open file, and connected plugin session.
- Penpot/Unity MCP capability gaps are explicit stop conditions.
- Rollback is deletion of files created on this experiment branch; pre-existing user changes are excluded.

## Validation

- Use the Unity test-safety preflight before any test run.
- Run Edit Mode tests asynchronously only where relevant; Play Mode coverage is a separate validation gap.
- Persist and inspect every Unity asset mutation through the live Editor.

## Execution Handoff

- Required context keys: `ui-code`, `plan-workflow`, `work-routing`.
- Likely new scope: `docs/ui-ai/`, `Assets/Scripts/Ui/PenpotFlow/`, `Assets/Prefabs/Ui/PenpotFlow/`, and optionally an isolated test scene.
- Remaining user decisions: supply the private Penpot MCP key during setup and select one of the three generated designs.
