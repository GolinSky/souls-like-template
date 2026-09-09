---
title: Work Queue
type: index
domains:
  - project-management
status: current
authority: advisory
verified: 2026-09-07
context_keys:
  - work-routing
tags:
  - vault/index
---

# Work Queue

Plans become executable only after review. Issues describe defects or open problems. Status lives in frontmatter so agents and Obsidian can filter it consistently. An agent executes only an explicitly named plan whose status is `ready` or `in-progress`.

## Draft Plans

- [[Work/Plans/Architecture Lifecycle Remediation and Local Validation]] — `draft`; local Unity follow-up to the September 8 architecture comparison.
- [[Work/Plans/Lighting Bake Plan|Lighting Bake Plan]] — `draft`
- [[Work/Plans/Project Organization Remediation Plan|Project Organization Remediation Plan]] — `draft`
- [[Work/Plans/Settings System Plan|Settings System Plan]] — `draft`

## Executable Plans

- [[Work/Plans/ProBuilder Integration and Agent Skills|ProBuilder Integration and Agent Skills]] — `in-progress`; dedicated branch, tests skipped by user request.
- [[Work/Plans/DefaultLocation Memory Optimization|DefaultLocation Memory Optimization]] — `in-progress`

## Issues

- [[Work/Issues/Character Lifecycle Can Stall When Animation Notifications Are Missing]] — high; missing-event lifecycle recovery.
- [[Work/Issues/Animation Completion Is Not Correlated To The Owning Action]] — medium; stale/missing/duplicate completion ownership.
- [[Work/Issues/Scene And Spawn Failures Have No Recovery Transaction]] — medium; transition failure and spawn commit ordering.
- [[Work/Issues/Interrupted Fades Leave Lifecycle Awaiters Unsettled]] — medium; conditional interruption/cancellation gap.

Current comparison: [[Research/Architecture and Systems Audit 2026-09-08]] — four additional architecture findings; Unity validation deferred to the dedicated local plan.

- [[Work/Issues/Roll Interruption Issue|Roll Interruption Issue]]

Architecture audit: [[Research/Architecture and Systems Audit 2026-09-07]] — static evidence and validation gaps.

- [[Work/Issues/Locked Roll State Is Cleared Before Root Motion]] — done; original trace corrected, minimal cleanup verified in Edit Mode; see [[History/Implementation Records/Roll Movement Lock Cleanup]].
- [[Work/Issues/Stance Recovery Is Gated By Poise Delay]] — done; independent stance recovery verified in Edit Mode; see [[History/Implementation Records/Stance Recovery Independence]].
- [[Work/Issues/Inventory Category Controls Are Not Connected]] — high; code and prefab defect
- [[Work/Issues/Equipment Picker Compares Against The Wrong Slot]] — medium; code defect
- [[Work/Issues/Scene Transitions Allow Concurrent Load Operations]] — medium; code defect
- [[Work/Issues/Addressable Asset Loads Have No Release Owner]] — medium; resource lifetime defect
- [[Work/Issues/Settings Apply Hides Persistence Failures]] — medium; error propagation defect
- [[Work/Issues/Save Writes Can Replace The Last Valid File With Partial Data]] — medium; resilience gap
- [[Work/Issues/Respawn Assumes The Last Grace Is In The Current Scene]] — medium; conditional code defect
- [[Work/Issues/Equipment Picker Does Not Filter Exact Slot Compatibility]] — medium; conditional content integration defect
- [[Work/Issues/Architecture And Roll Issue Notes Contain Superseded Evidence]] — medium; documentation defect

## Lifecycle

`draft -> ready -> in-progress -> blocked or done`

`ready` means reviewed and executable. `in-progress` means an agent has started the explicitly requested work. A blocked plan or issue must name its dependency before work resumes.

Create new work from [[../Templates/Plan Template|Plan Template]] or [[../Templates/Issue Template|Issue Template]]. Completed implementation produces a note in [[../History/Implementation History|Implementation History]].

![[Work Queue.base]]
