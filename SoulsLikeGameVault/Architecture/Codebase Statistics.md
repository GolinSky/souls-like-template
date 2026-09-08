---
title: Codebase Statistics
type: architecture
domains:
  - project-structure
  - code-metrics
status: current
authority: advisory
updated: 2026-09-08
aliases:
  - CODEBASE_STATISTICS
  - CODE_METRICS
tags:
  - architecture/metrics
  - status/current
---

# Codebase Statistics & Metrics

This note documents the codebase size, class counts, type definitions, and line-of-code distributions across the **SoulsLikeTemplate** project.

## Summary

| Metric Scope | File Count | Classes | All Types (Classes/Interfaces/Structs/Enums) | Raw LOC | Source LOC (SLOC) |
|---|---|---|---|---|---|
| **Project Code (`Assets/Scripts`)** | **384** | **291** | **470** | **39,626** | **34,203** |
| **All Assets C# (`Assets/`)** | **470** | **388** | **588** | **57,709** | **46,585** |
| **Total Tracked Code & Text** | **719** | — | — | **440,156** | — |

---

## 1. Project Scripts Breakdown (`Assets/Scripts`)

The primary source code is modularized by architectural domain:

| Module Folder | Files | Classes | Interfaces | Structs | Enums | Raw LOC | Code SLOC | Comments | Blank Lines |
|---|---|---|---|---|---|---|---|---|---|
| `Entities` | 99 | 62 | 9 | 15 | 28 | 10,470 | 9,275 | 114 | 1,081 |
| `Editor` | 25 | 41 | 0 | 1 | 0 | 7,412 | 6,447 | 109 | 856 |
| `Services` | 78 | 58 | 23 | 5 | 10 | 7,289 | 5,987 | 204 | 1,098 |
| `Ui` | 84 | 59 | 24 | 2 | 7 | 6,272 | 5,408 | 91 | 773 |
| `Components` | 43 | 30 | 7 | 13 | 11 | 4,746 | 4,117 | 113 | 516 |
| `Tests` | 10 | 10 | 0 | 0 | 0 | 1,123 | 955 | 13 | 155 |
| `Items` | 15 | 14 | 0 | 3 | 7 | 1,090 | 952 | 12 | 126 |
| `Interactions` | 6 | 3 | 2 | 1 | 0 | 473 | 411 | 9 | 53 |
| `Orchestrators` | 12 | 4 | 7 | 0 | 2 | 450 | 393 | 6 | 51 |
| `Utilities` | 9 | 7 | 2 | 0 | 0 | 214 | 185 | 4 | 25 |
| `Rendering` | 1 | 1 | 0 | 0 | 0 | 71 | 60 | 1 | 10 |
| `Model` | 2 | 2 | 0 | 0 | 0 | 16 | 13 | 0 | 3 |
| **Total** | **384** | **291** | **74** | **40** | **65** | **39,626** | **34,203** | **676** | **4,747** |

---

## 2. All Assets C# Distribution

Comparing core project code versus third-party/external dependencies located in `Assets/`:

| Scope | Files | Classes | Raw LOC | Code SLOC |
|---|---|---|---|---|
| Project Scripts (`Assets/Scripts`) | 384 | 291 | 39,626 | 34,203 |
| Third-Party Packages (`Assets/ThirdParty`) | 75 | 77 | 15,687 | 10,757 |
| Plugins & Extras (`Assets/Plugins`, etc.) | 11 | 20 | 2,396 | 1,625 |
| **Total C# in Assets** | **470** | **388** | **57,709** | **46,585** |

---

## 3. Project File Types & Languages Breakdown

Distribution of all tracked code, shader, layout, data, and documentation files across the repository (excluding generated files, build outputs, and `.git` / `Library`):

| File Extension | Language / Type | File Count | Line Count |
|---|---|---|---|
| `.xml` | XML Data / Configuration | 49 | 339,300 |
| `.cs` | C# Source Code | 470 | 57,709 |
| `.md` | Markdown Documentation (Vault & Guides) | 121 | 29,369 |
| `.json` | JSON Data / Package Manifests | 20 | 7,178 |
| `.shader` | Unity Shader Source | 17 | 4,363 |
| `.cginc` | Cg/HLSL Include Files | 5 | 541 |
| `.toml` | TOML Configuration (Codex / Agents) | 9 | 454 |
| `.yml` / `.yaml` | YAML Configuration | 13 | 430 |
| `.uss` | UI Toolkit Stylesheet | 7 | 338 |
| `.uxml` | UI Toolkit Layout XML | 7 | 296 |
| `.hlsl` | HLSL Shader Code | 1 | 178 |
| **Total** | | **719** | **440,156** |

---

## Related Notes

- [[Project Organization]]
- [[Architecture Index]]
- [[../Agent Guide/Agent Context Registry|Agent Context Registry]]
