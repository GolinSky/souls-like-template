# Skill Context Index

This registry is the allow-list for project context loaded by Codex skills. Registered notes remain checked-in Markdown; Obsidian MCP is the preferred targeted reader, not the skill holder.

| Context key | Exact vault-relative note path | Applicable roles and task triggers | Required headings | Authority | Verification date/source | Disk fallback path |
|---|---|---|---|---|---|---|
| `ui-code` | `ui/UI_Code_Build_Guide.md` | `unity_architect`, `csharp_worker`, `unity_operator`, `unity_reviewer`, and `unity_test_runner` for UI controllers, presenters, views, prefabs, or Addressables | 1. C# Script Architecture (\`Assets/Scripts/Ui/<FeatureName>/\`), `2. Prefab UI Asset Creation & Organization`, `3. Addressables & AssetMappingData Setup` | required | 2026-09-01; `AGENTS.md` UI Workflow | `SoulsLikeGameVault/ui/UI_Code_Build_Guide.md` |
| `inventory-ui` | `ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` | The same UI roles, only for inventory UI layout, cells, state, focus, or input work | `2. Spatial Layout & Screen Breakdown`, `4. Cell UI Architecture (Item Grid Slots)`, `6. UI/UX View State Machine`, `7. Navigation, Focus Management & Input Mapping` | advisory | 2026-09-01; checked-in inventory UI guide | `SoulsLikeGameVault/ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` |
| `equipment-ui` | `ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` | The same UI roles, only for equipment UI layout, comparison, navigation, or state work | `2. Spatial UI Breakdown (What is Located Where)`, `3. Interactive UX States & Navigation Flow`, `4. Visual UI Layout Hierarchy` | advisory | 2026-09-01; checked-in equipment UI guide | `SoulsLikeGameVault/ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` |

## Registry rules

- Resolve context by exact key and read only the registered headings needed for the task.
- A `required` note supplies project constraints. An `advisory` note supplies feature guidance and cannot override live source, serialized assets, or required policy.
- If Obsidian MCP is unavailable, read the exact disk fallback. Missing headings, missing files, or conflicts must be reported.
- Unregistered and unverified notes are not project policy. In particular, `Arhitecture/PROJECT_ORGANIZATION.md` and character or locomotion drafts are intentionally excluded pending review.
