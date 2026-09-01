---
name: soulslike-csharp-change
description: Implement one bounded production C# change in the SoulsLike Unity project. Use after the execution path and exact files or symbols have been assigned to a single writer.
---

# SoulsLike CSharp Change

1. Confirm the assigned files, symbols, call sites, and writer ownership.
2. Inspect the minimum live source needed, including affected serialized fields and public APIs.
3. Implement the smallest coherent change in the repository's existing style.
4. Remove only artifacts made obsolete by this change and report compatibility risks.
5. Use `$soulslike-context` only for an applicable registered key; add `$soulslike-ui-workflow` for UI code.

Do not mutate Unity assets or Editor state unless that scope is explicitly assigned. Return changed files and symbols, checks performed, assumptions, and remaining validation.
