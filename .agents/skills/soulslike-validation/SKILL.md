---
name: soulslike-validation
description: Execute one explicitly assigned SoulsLike validation task and return compact evidence. Use for bounded static checks, Unity Test Framework (UTF) tests, builds, or reproduction steps without attempting repairs.
---

# SoulsLike Validation

1. Confirm the exact validation target and load the same applicable domain skill used by that target.
2. Inspect prerequisite state and run only the assigned check, test, build, or reproduction.
3. Capture pass/fail evidence, relevant errors, and environment conditions.
4. Distinguish pre-existing failures when the evidence permits.
5. State what remains unvalidated.

For Unity tests, resolve `unity-testing` through `$soulslike-context` and follow the registered **Unity Test Framework Test Flow** guide. Use the official Unity CLI/Pipeline bridge and the installed UTF package. Discover the live schemas, confirm clean scenes and an idle runner, then confirm the assigned selection with `list_tests --mode editor`. Run only that selection with explicit `--mode editor`, `--async_tests true`, and a bounded timeout; enforce the wall-clock budget externally because the current async bridge ignores `--timeout`. Poll `test_status` and inspect its nested result. Do not treat zero executed tests or a successful request envelope as a pass. Follow `AGENTS.md` for dirty scenes, cancellation, timeout recovery, and deferred Play Mode coverage.

Do not broaden into test suites that were not requested. Do not edit source, assets, settings, or expectations, and do not attempt repairs.
