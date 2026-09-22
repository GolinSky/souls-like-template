---
title: Vault Guide
type: guide
domains:
  - documentation
status: current
authority: required
verified: 2026-09-07
source_commit: fcaf410d
context_keys:
  - vault-usage
tags:
  - agent/context
  - vault/rules
updated: 2026-09-14
---

# Vault Guide

## Agent Retrieval Workflow

1. Infer concrete task signals: domain (`ui`, `character`, `animation`), work kind (`implementation`, `research`, `issue`), and affected asset or symbol.
2. Read [[Meta/Agent Context Registry]]. Resolve an exact key when supplied; otherwise match the task signals column literally.
3. Load all directly matching `required` entries, then only the relevant `advisory` or `evidence` entries. Use the vault outline and read only registered headings.
4. If the task names a plan, issue, research package, or implementation record, load the work-routing context and locate an exact title, alias, or path match only in its lifecycle folder. Execute only an explicitly requested plan with status `ready` or `in-progress`.
5. Validate advisory architecture claims against live source, serialized assets, and current tool output. Report conflicts rather than guessing.
6. Write the resulting artifact to the matching lifecycle folder using a template. After implementation, update the canonical architecture note and create one concise implementation record.

Frontmatter and tags help discovery. They do not grant policy authority; only the registry does.

## Note Lifecycle

```text
Research package -> reviewed plan -> active issue or implementation -> implementation record
                          |                                         |
                          +---------- updates architecture/GDD ------+
```

- Research begins in `Research/` and ends with an evidence-backed handoff suitable for external review.
- Approved executable work lives in `Work/Plans/`. Bugs and unresolved defects live in `Work/Issues/`.
- Completed work produces a small record in `History/Records/` and updates the relevant current architecture note. Move completed issues to `History/Closed Issues/` and completed plans to `History/Completed Plans/`. Preserve validation limitations and explicitly identify user-directed closure; neither missing historical tests nor stale checklists reopen completed work.
- Superseded material moves to `History/Superseded/`; retain distinct historical variants. Delete redundant notes only when authorized.
- `Knowledge/` contains `Architecture/`, `Game Design/`, and `Guides/`. `Meta/` contains vault instructions, the registry, catalogs, and `Templates/`. `Research/` retains investigations and their evidence packages.
- Catalogs derive status lists from frontmatter. Home and index notes explain purpose and decisions; do not maintain duplicate exhaustive status lists.

## Naming Rules

- Use human-readable Title Case filenames with spaces: `Character System.md`, not `CHARACTER_SYSTEM_ARCHITECTURE.md`.
- Put the note kind in frontmatter instead of repeating it in every filename. Add a suffix only when it prevents ambiguity, such as `System Audit`.
- Prefer stable names. Preserve old technical names as YAML `aliases` when they are useful search terms.
- Use one canonical note for current architecture per system. Proposed designs, audits, and implementation history remain separate and link back to it.
- Use vault-relative wikilinks. Do not use `file:///` links or machine-specific absolute paths.

## Properties and Tags

Every maintained note uses these atomic properties:

| Property | Shape | Purpose |
|---|---|---|
| `title` | text | Human title. |
| `type` | text | `architecture`, `game-design`, `guide`, `research`, `plan`, `issue`, `implementation-record`, `index`, `template`, or `archive`. |
| `domains` | list | Systems affected by the note. |
| `status` | text | `current`, `draft`, `open`, `ready`, `in-progress`, `blocked`, `done`, `needs-review`, `proposed`, `superseded`, or `archived`. |
| `authority` | text | `required`, `advisory`, `evidence`, or `historical`. |
| `verified` | date | Last validation against live project evidence. Omit when never verified. |
| `source_commit` | text | Short commit used for validation. Omit when not applicable. |
| `updated` | date | Last meaningful edit to the note. |
| `context_keys` | list | Registry keys supplied by this note. |
| `aliases` | list | Previous names and useful acronyms. |
| `tags` | tags | Sparse cross-cutting workflow markers. |

Use nested tags only for cross-cutting queues: `work/plan`, `work/issue`, `status/blocked`, `status/needs-review`, `agent/context`, and `history/change`. Do not duplicate every folder and property as a tag.

## Diagram Rules

- A diagram must answer one question. Split diagrams that mix ownership, runtime sequence, and implementation detail.
- Prefer top-to-bottom flow. Keep an inline Mermaid diagram to roughly 12 nodes or 15 relationships; use a table for larger relationship matrices.
- Quote labels containing parentheses or punctuation. Avoid reserved identifiers such as `Actor` in sequence diagrams.
- Put the explanation before the diagram so the note remains useful if rendering fails.
- Render-test changed Mermaid blocks. Do not solve clipping by changing `.obsidian` CSS or plugin configuration.

## Tooling Basis

The structure uses ordinary folders, Markdown links, YAML properties, nested tags, templates, and Obsidian Bases. It intentionally requires no community plugin.
