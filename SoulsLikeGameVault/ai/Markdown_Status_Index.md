# Vault Markdown Status Index

This index classifies checked-in vault Markdown so drafts and historical artifacts cannot be mistaken for project policy. It does not register skill context; only `Skill_Context_Index.md` can do that. Live source, serialized assets, `AGENTS.md`, and registered required context take precedence.

| Path | Classification | Use |
|---|---|---|
| `ai/Skill_Context_Index.md` | required | Exact-key context registry and authority boundary. |
| `ai/Markdown_Status_Index.md` | advisory | Classification index only; it does not extend the authority order. |
| `ai/GPT_PRO_REVIEW_PACKAGE.md` | historical | Time-bounded audit evidence; verify every claim against live files. |
| `Architecture/CHARACTER_SYSTEM_ARCHITECTURE.md` | advisory | Authoritative character system architecture, runtime protocol, and rules. |
| `Architecture/HITBOX_SYSTEM_ARCHITECTURE.md` | advisory | Authoritative hitbox system architecture, hit resolution cascade, defense, and critical runtime guide. |
| `Architecture/PROJECT_ORGANIZATION.md` | advisory | Asset organization guidance; live project structure wins on conflict. |
| `Architecture/SETTINGS_SYSTEM_ARCHITECTURE.md` | advisory | Settings system architecture, segregation model, and observer flow. |
| `Architecture/LAYER_SERVICE_SYSTEM_ARCHITECTURE.md` | advisory | Authoritative layer service architecture, layer identity vs query masks, and fail-fast rules. |
| `Artifact/elden_ring_inventory_equipment_architecture.md` | draft | Design artifact; not registered project context. |
| `ToDo/ELDEN_RING_STYLE_SETTINGS_SYSTEM_PLAN.md` | draft | Settings system implementation plan artifact; not registered project context. |
| `Artifact/Roll System & Interruption Bug.md` | draft | Roll interruption lockout and stamina investigation artifact; not registered project context. |
| `features/Advanced Locomotion Architecture Prompt Specification.md` | draft | Imported prompt artifact; not implementation authority. |
| `features/Current Jump and Roll System.md` | advisory | Implementation note that must be checked against live source. |
| `features/Locomotion Architecture Technical Specification.md` | draft | Design specification; not implementation authority. |
| `features/Enemy Combat & AI Systems.md` | advisory | Enemy combat and AI systems specification, lifecycles, and authoring contracts. |
| `features/Movement Mechanics Explained.md` | advisory | Feature explanation; verify against live source before use. |
| `features/System Specification - Souls-like Locomotion & Camera System.md` | draft | Proposed system specification. |
| `features/Technical Specification - Roll & Backstep Vectoring Logic.md` | draft | Proposed vectoring specification. |
| `ToDo/First Steps.md` | historical | Early placeholder note. |
| `Artifact/Hitbox System Implementation Plan.md` | historical | Implemented design plan; superseded by `HITBOX_SYSTEM_ARCHITECTURE.md`. |
| `Artifact/Hitbox System.md` | historical | Implemented design note; superseded by `HITBOX_SYSTEM_ARCHITECTURE.md`. |
| `ToDo/LIGHTING_BAKE_PLAN.md` | draft | Operational plan; validate against current Unity tooling before execution. |
| `ui/UI_Code_Build_Guide.md` | required | Registered `ui-code` context. |
| `ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` | advisory | Registered `inventory-ui` context. |
| `ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` | advisory | Registered `equipment-ui` context. |
| `ui/UI_Route_Navigation_Architecture.md` | advisory | Foundational UI route stack and navigation architecture guide. |
| `ui/Pause_Navigation_Route_Architecture.md` | advisory | Pause navigation hub, sub-route flow, and hotkey architecture. |
| `ui/Grace_Route_Navigation_Architecture.md` | advisory | Grace navigation hub, fade coordination, and travel architecture. |
| `ToDo/Refactor_Pause_Navigation_Naming.md` | advisory | ToDo tracking for refactoring IPauseNavigationRouteNavigation naming (Completed). |
| `ToDo/Project_Organization_Analysis_And_Fix_Plan.md` | advisory | Audit analysis of project structure against PROJECT_ORGANIZATION.md and phased remediation plan. |
| `ToDo/FLASK_HEALING_SYSTEM_RESEARCH.md` | advisory | Reference research and architectural survey of Elden Ring flask mechanics and SoulsLike codebase integration. |
| `Done/LAYER_SERVICE_FIX_PLAN.md` | historical | Completed remediation plan for layer service configuration, fail-fast rules, and query mask ownership. |
| `ToDo/ENEMY_ENCOUNTER_SYSTEM_ANALYSIS_REPORT.md` | advisory | Architectural analysis, scalability audit, and modernization roadmap for enemy spawning and encounter systems. |
| `ToDo/ROLL_INVESTIGATION_REPORT.md` | advisory | Character roll interruption bug investigation report, Mecanim desync root cause, and remediation plan. |
| `animation/Animator_SubState_Machine_Architecture_Guide.md` | advisory | Authoritative animator controller sub-state machine rules, layout coordinates, and empty default state standards. |
| `Welcome.md` | historical | Obsidian starter note; no policy value. |

Generated files under repository-root `graphify-out/` are local operational output, not vault documentation. They are ignored, excluded from broad searches, and governed by the retention rule in `AGENTS.md`.
