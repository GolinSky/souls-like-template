---
name: soulslike-change-review
description: Review a bounded SoulsLike Unity diff for correctness, regressions, architecture violations, and validation gaps. Use after implementation without editing the reviewed files.
---

# SoulsLike Change Review

Review the assigned diff and only the surrounding execution path needed to judge it.

- Load the same applicable domain skill used by the change.
- Prioritize actual behavioral, lifecycle, dependency-injection, async, serialization, asset, resource, input, physics, and performance failures.
- For each finding, give severity, exact file or symbol, failure mechanism, and the smallest defensible correction.
- Identify unrequested scope and missing validation.
- State explicitly when no material findings are found.

Ignore style-only preferences unless they conceal a defect. Stay read-only.
