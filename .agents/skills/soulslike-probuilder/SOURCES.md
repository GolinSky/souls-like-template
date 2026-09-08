# Sources and adaptation

This is one project-owned ProBuilder workflow, authored on 2026-09-08. It adapts
inspect-before-edit, semantic face selection, bounded geometry, explicit pivots,
and whole-object versus face-material guidance from the two MIT sources below.
Their server activation, tool names, instance-ID contracts, fixed version pins,
and generic playtest instructions are excluded.

| Source | Reviewed immutable revision | Relevant source |
| --- | --- | --- |
| [Unity Open MCP](https://github.com/AlexeyPerov/Unity-Open-MCP) | `a5c2f520539705bc1bb30193a5071b008f29a793` | `skills/extensions/probuilder/SKILL.md`; blob `4e767fe7d2ae1686f0dcaab47cb597c6e4470033` |
| [unity-mcp-skills](https://github.com/batihandev/unity-mcp-skills) | `090c8e3eaefa23d57a98e237572a4ffbd19df444` | `skills/probuilder/SKILL.md`; blob `889caeedf11935004993b8a1d9429714a9389a7f` |
| [Unity skills](https://github.com/Unity-Technologies/skills) | `e91a6c4f9acf252e542c1228db1fc36b5a22a8d0` | Selected Unity CLI and package-management guidance, adapted in sibling skills under the Unity Companion License |
| [Unity ProBuilder](https://github.com/Unity-Technologies/com.unity.probuilder) | Package `6.1.2`; package repository revision `f070d01a1ecb9e899ee09cbeb17f006b9dfcc8d4` | Official registry tarball SHA-1 `8be9724b23352f64b906f2ec71abb378236b23c9`; public API source inspected rather than copying wrapper recipes |

The complete fetched-date, source-blob, license, and adopted-copy provenance is
in [the source review](../../third-party-notices/probuilder-sources.md).
License notices are preserved in that directory; the Unity-derived files are
not relabeled MIT. No source installer or extra MCP server was activated.

The local command contract must be checked against the actual Pipeline catalog.
Source inspection and compilation do not establish behavior under tests. The
implementation record documents actual executed work and skipped coverage.
