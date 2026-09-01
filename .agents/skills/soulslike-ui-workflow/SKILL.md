---
name: soulslike-ui-workflow
description: Apply the registered SoulsLike UI architecture workflow to UI controllers, presenters, views, prefabs, Addressables, inventory UI, or equipment UI tasks.
---

# SoulsLike UI Workflow

1. Use `$soulslike-context` with the required `ui-code` key before UI work.
2. Also load `inventory-ui` only for inventory UI tasks and `equipment-ui` only for equipment UI tasks; both are advisory.
3. Apply the retrieved boundaries to C# roles, VContainer registration, prefab organization, navigation, and Addressables work in the assigned scope.
4. Report any mismatch between the guide, live source, serialized assets, or the requested behavior.

Do not load unrelated vault notes or convert advisory visual guidance into mandatory project policy.
