---
title: Vault Structure Refactor
type: implementation-record
domains:
  - documentation
  - ai
status: done
authority: historical
updated: 2026-09-07
source_commit: fcaf410d
tags:
  - history/change
---

# Vault Structure Refactor

## Outcome

Reorganized the vault around Architecture, Game Design, Guides, Research, Work, History, Templates, Agent Guide, and Archive. Added consistent metadata, task-signal context routing, dynamic Bases, note templates, architecture validation status, and diagram checks.

## Why

The previous folder names mixed document type, lifecycle, and domain. Only four hardcoded agent context keys existed, most notes had no metadata, and duplicate or stale reports could be mistaken for current policy.

## Changed Files and Assets

- Moved and renamed 40 notes without deleting content.
- Added [[Home]], [[Agent Guide/Vault Guide]], [[Agent Guide/Agent Context Registry]], section indexes, six templates, and two Bases dashboards.
- Updated `AGENTS.md` and `$soulslike-context` to support deterministic task-signal lookup through 20 registered contexts, including exact named-work routing.
- Archived two duplicate pairs and the default Welcome note instead of deleting them.
- Repaired Mermaid syntax failures and replaced oversized relationship diagrams with compact text or tables.

## Decisions and Tradeoffs

- Folders express durable document class; frontmatter expresses lifecycle and authority; sparse nested tags expose cross-cutting queues.
- Registry membership—not tags—grants policy authority.
- Stale architecture notes remain available with `needs-review` or `evidence` status instead of being silently rewritten or promoted.

## Validation Evidence

- 54 Markdown notes parse as YAML-frontmatter documents with all required properties.
- All vault wikilinks resolve.
- All 31 remaining Mermaid blocks across 16 files render successfully with Mermaid CLI 11.17.0 and stay within the vault readability limit.
- Every registry path and registered heading resolves.
- Both `.base` files parse as YAML.
- `$soulslike-context` passes its skill validator.

## Documentation Updated

- [[Agent Guide/Vault Guide]]
- [[Agent Guide/Agent Context Registry]]
- [[Agent Guide/Vault Status Index]]
- [[Architecture/Architecture Index]]
- [[Game Design/Game Design Document]]
- [[Research/Research Index]]
- [[Work/Work Queue]]

## Follow-Up

Reconcile notes marked `needs-review`, starting with Settings, Interaction, Grace Navigation, Character, and UI Route Navigation.
