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
updated: 2026-09-14
---

# Work Queue

This page shows active work from note properties. Completed issues and plans live in [[History/Implementation History]], so they cannot silently re-enter the active queue through an old manual list.

## Draft Plans

Draft plans require review and explicit execution authorization.

![[Work/Work Queue.base#Draft Plans]]

## Executable Plans

Only reviewed `ready` or already authorized `in-progress` plans are executable. A catalog row does not authorize starting work.

![[Work/Work Queue.base#Executable Plans]]

## Issues

![[Work/Work Queue.base#Open Issues]]

## Lifecycle

`draft -> ready -> in-progress -> blocked or done`

Open issues use `open`; a blocked item names its dependency. Move completed issues to `History/Closed Issues/` and completed plans to `History/Completed Plans/`. Preserve the original evidence and record whether closure rests on validation or an explicit user decision. Do not reopen closed work from historical audits or unchecked old scenarios.

Create work from [[Meta/Templates/Plan Template]] or [[Meta/Templates/Issue Template]]. Record meaningful completed work using [[Meta/Templates/Implementation Record Template]].
