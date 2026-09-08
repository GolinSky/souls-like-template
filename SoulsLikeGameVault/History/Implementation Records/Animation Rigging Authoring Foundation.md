---
title: Animation Rigging Authoring Foundation
type: implementation-record
domains: [animation, equipment]
status: done
authority: historical
updated: 2026-09-08
tags: [history/change]
---

# Animation Rigging Authoring Foundation

## Implementation Record Contract

### Outcome

Partial implementation of the [Animation Rigging integration plan](https://drive.google.com/file/d/1dkYzf3LCBeKXFYL_StpJWWBzpxWaUqCp/view): package-independent support-hand grip authoring groundwork only. Runtime IK is not implemented or activated. The user explicitly authorized partial scope and prohibited tests in this dedicated worktree.

### Why

The worktree uses Unity 6000.3.11f1, has no imported Library, and has no Animation Rigging dependency in its manifest or lockfile. `unity status --json` identified only the main checkout Editor at `F:\Private\SoulsLikeTemplate`. Importing or authoring through that Editor would affect a different checkout. Package installation, prefab changes, graph evaluation, and production rollout are deferred together rather than leaving unresolved package references or unpersisted assets.

### Changed Files and Assets

- `Assets/Scripts/Components/Equipment/WeaponRuntime.cs`: optional supporting-hand authoring metadata.
- `Assets/Scripts/Components/Equipment/SupportHandGrip.cs`: weapon-local grip data.
- `Assets/Scripts/Editor/SupportHandGripEditor.cs`: authoring feedback and selected-object preview.
- New scripts include their own `.meta` files. No prefab, scene, controller, profile asset, package manifest, or lockfile is changed.

### Decisions and Tradeoffs

Grip data belongs to the existing weapon runtime prefab, with no references to character bones or another instance. Existing weapons have the feature off by default. Authoring a grip does not authorize any action to use IK.

Live source audit identified these integration owners:

- `CharacterFactory.CreateCharacter` composes the character scope.
- `Character.ApplyEquipmentLoadout` applies equipment, animation profile/hand-mode changes, and active attack weapons.
- `EquipmentPresentation.ApplyLoadout` creates weapon instances under hand anchors. Left-hand mounting has its own override; the initial future solve remains right-hand dominant.
- `EquipmentLoadout.HandMode` comes from the existing equipment model; two-handed mode suppresses the effective left item.
- `CharacterActionStateMachine` and Character movement/input locks govern action ownership. Future policy must cover ladder early returns, critical actions, Grace, item use, swaps, death, and interruptions.
- `PlayerMeleeCombatRelay` opens/closes `MeleeHitboxController` trigger hitboxes. Bone corrections can change collision outcomes; unchanged timing alone does not prove combat equivalence.
- Third-party ACS contains an optional rigging integration. Its presence on actual character assets and graph ownership need Editor inspection before adding a second builder.

No existing Graphify graph is present in this worktree; the read-only audit used live source. No generated graph was created. This is preparatory work for Phases 1–2, not completion of their gates or the MVP.

### Validation Evidence

- **Passed:** branch creation from the current worktree; source ownership audit; package/version inspection; static diff review and whitespace checks (see commit).
- **Not run:** Unity import/compilation, Editor authoring interaction, automated tests, Play Mode, visual contact accuracy, same-frame grip timing, gameplay comparison, lifecycle and performance validation.
- **Deferred:** package resolution and persisted validation prefab/scene. The live Editor is a different checkout.

### Documentation Updated

This record describes the partial scope. Existing runtime architecture remains unchanged.

### Follow-Up

1. Resolve pinned `com.unity.animation.rigging` 1.4.1 through Unity in the intended checkout and review the generated lockfile. The [version-matched Two Bone IK documentation](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.4/manual/constraints/TwoBoneIKConstraint.html) describes the root/mid/tip, target, and hint contract.
2. Inspect the player skeleton, hand anchors, scale, Avatar/import optimization, graph ownership, and any ACS components. Create and persist an isolated validation prefab through Unity APIs.
3. Calibrate a right-hand-dominant weapon grip. Bind a stable dominant-hand-relative proxy; compose weapon mounting and grip local poses without copying a stale animated world transform in Update.
4. Implement one controller/profile and authoritative state/equipment suppression, initially off, with immediate invalidation and controlled blending. Keep the dominant arm, gameplay root, and attack timing untouched.
5. Validate contact error, fast swings, graph lifecycle, equipment swaps, and combat outcomes before enabling production IK. Head look, feet, enemies, and interactions remain later milestones from the source plan.

Rollback: revert this groundwork commit; no production asset or package rollback is required.
