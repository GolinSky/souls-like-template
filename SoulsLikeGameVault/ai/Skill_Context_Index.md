# Skill Context Index

This registry is the allow-list for project context loaded by Codex skills. Registered notes remain checked-in Markdown; Obsidian MCP is the preferred targeted reader, not the skill holder.

| Context key | Exact vault-relative note path | Applicable roles and task triggers | Required headings | Authority | Verification date/source | Disk fallback path |
|---|---|---|---|---|---|---|
| `ui-code` | `ui/UI_Code_Build_Guide.md` | `unity_architect`, `csharp_worker`, `unity_operator`, `unity_reviewer`, and `unity_test_runner` for UI controllers, presenters, views, prefabs, or Addressables | 1. C# Script Architecture (\`Assets/Scripts/Ui/<FeatureName>/\`), `2. Prefab UI Asset Creation & Organization`, `3. Addressables & AssetMappingData Setup` | required | 2026-09-01; `AGENTS.md` UI Workflow | `SoulsLikeGameVault/ui/UI_Code_Build_Guide.md` |
| `inventory-ui` | `ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` | The same UI roles, only for inventory UI layout, cells, state, focus, or input work | `2. Spatial Layout & Screen Breakdown`, `4. Cell UI Architecture (Item Grid Slots)`, `6. UI/UX View State Machine`, `7. Navigation, Focus Management & Input Mapping` | advisory | 2026-09-01; checked-in inventory UI guide | `SoulsLikeGameVault/ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` |
| `equipment-ui` | `ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` | The same UI roles, only for equipment UI layout, comparison, navigation, or state work | `2. Spatial UI Breakdown (What is Located Where)`, `3. Interactive UX States & Navigation Flow`, `4. Visual UI Layout Hierarchy` | advisory | 2026-09-01; checked-in equipment UI guide | `SoulsLikeGameVault/ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` |
| `animation-code` | `animation/Animator_SubState_Machine_Architecture_Guide.md` | `unity_architect`, `csharp_worker`, `unity_operator`, `unity_reviewer` for Animator Controllers, animation states, transitions, sub-state machines, or ActionExecutor integration | `1. Rule: Group Connected Animations into Sub-State Machines`, `2. Rule: Coordinate and Layout Standards`, `3. Rule: Inert Empty Default State Inside Action Sub-State Machines`, `4. Rule: Runtime CrossFade Resolution Compatibility` | required | 2026-09-02; `AGENTS.md` Animation Workflow | `SoulsLikeGameVault/animation/Animator_SubState_Machine_Architecture_Guide.md` |

## Registry rules

- Resolve context by exact key and read only the registered headings needed for the task.
- A `required` note supplies project constraints. An `advisory` note supplies feature guidance and cannot override live source, serialized assets, or required policy.
- If Obsidian MCP is unavailable, read the exact disk fallback. Missing headings, missing files, or conflicts must be reported.
- This registry intentionally stays narrow. `Markdown_Status_Index.md` classifies the rest of the vault but does not register additional context keys.
- Unregistered and unverified notes are not project policy. In particular, `Architecture/PROJECT_ORGANIZATION.md` and character, locomotion, or hitbox drafts are intentionally excluded pending review.
