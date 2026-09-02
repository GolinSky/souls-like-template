---
name: soulslike-animation-workflow
description: Apply the registered SoulsLike animation and Animator Controller architecture workflow to Animator Controllers, sub-state machines, transitions, layers, or action executor runtime integration.
---

# SoulsLike Animation Workflow

1. Use `$soulslike-context` with the required `animation-code` key before modifying any Animator Controller or animation integration code.
2. Group connected animations into semantic sub-state machines (`Locomotion`, `Attack`, `Hits`, `Combat`, `Death`).
3. Apply the coordinate and layout standards matching `CharacterGreatSwordAnimator.controller` (sub-state machine column at X = 530, horizontal attack rows at Y = -140, vertical hit/combat columns at X = 440).
4. Ensure all action and reaction sub-state machines contain an inert `Empty` state (`motion = null`) set as `defaultState` so Entry never triggers involuntary animation.
5. Use short state names or hashes for runtime `CrossFadeInFixedTime` / `Play` calls to preserve sub-state machine compatibility.
6. Force reserialize and persist modified controllers through Unity `AssetDatabase`.
