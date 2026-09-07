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

- [[Work/Plans/Lighting Bake Plan|Lighting Bake Plan]] — `draft`
- [[Work/Plans/Project Organization Remediation Plan|Project Organization Remediation Plan]] — `draft`
- [[Work/Plans/Settings System Plan|Settings System Plan]] — `draft`

## Executable Plans

No plan is currently marked `ready` or `in-progress`.

## Issues

- [[Work/Issues/Roll Interruption Issue|Roll Interruption Issue]]

## Lifecycle

`draft -> ready -> in-progress -> blocked or done`

`ready` means reviewed and executable. `in-progress` means an agent has started the explicitly requested work. A blocked plan or issue must name its dependency before work resumes.

Create new work from [[../Templates/Plan Template|Plan Template]] or [[../Templates/Issue Template|Issue Template]]. Completed implementation produces a note in [[../History/Implementation History|Implementation History]].

![[Work Queue.base]]
