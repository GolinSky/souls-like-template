# Validation and evidence

The implementation run on 2026-09-08 explicitly skips tests at the user's
request. Compilation and generated-asset import/persistence inspection are
implementation checks, not evidence that regression tests passed.

For later authorized validation, inspect every loaded scene with
`list_open_scenes` or `assert_test_ready` before a scene reload or test. A dirty
untitled scene blocks the operation; never open another scene to escape it.
Follow the repository's asynchronous and time-boxed test protocol for Edit Mode.
Normal validation does not run Play Mode tests.

Inspect the spec boundary, ownership metadata, element identities, mesh topology,
material references, transform/bounds, collider shapes, and persisted paths.
Verify a doorway's collision opening independently of its rendered opening.
Check repeat generation, targeted updates, manual edits, removed elements,
partial-save recovery, and reload persistence only when that validation is
authorized. An unchanged build must not churn scene or mesh references.

Separate follow-up gameplay work covers the actual controller, rolls, stair
snapping, ramp slopes, camera obstruction, lock-on framing, and enemy navigation.
No navigation bake or duplicate player/bootstrap is implied by a geometry build.
Keep placeholders visibly named as placeholders and report missing integrations.
