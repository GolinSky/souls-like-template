# System Specification: Souls-like Locomotion & Camera System

## System Overview
You are tasked with implementing or configuring a 3rd-person character locomotion and camera controller system modeled after . The system strictly toggles between two operational modes: Unlocked (Free Orbit Mode) and Locked-On (Target Anchor Mode).

---

## 1. Unlocked Mode (Free Orbit)

### 1.1 Camera Controller Dynamics
* Control Mode: Free Orbit.
* Pitch / Yaw Control: Driven entirely by manual camera stick / mouse input.
* Target Tracking: Disabled.
* Auto-Snap: Disabled (except optional manual camera-recenter button).
* Collision Handling: Standard camera raycast / spherecast occlusion sweep against environment geometry.

### 1.2 Character Orientation & Facing
* Heading Determination: Character faces the normalized 2D direction of the current Movement Input Vector (relative to screen space).
* Turning Behavior: Immediate or smooth turn toward the input direction.
* Backwards Input Behavior: Pulling backward causes the character model to immediately rotate 180° to face toward the camera screen space and run forward.
* Strafing / Orbiting: Disabled. Character never strafes or side-steps in Unlocked Mode.

### 1.3 Movement Vectors & Speed Multipliers
* Forward Speed: 100% (1.0x baseline velocity).
* Backward Input Speed: 100% (1.0x baseline velocity — character turns around and runs).
* Lateral Input Speed: 100% (1.0x baseline velocity).
* Sprint Behavior: Linear sprint in the movement vector direction (1.5x speed).

### 1.4 Combat & Evade Physics
* Guard Vector: Aligns strictly with the character's forward mesh vector. Retreating exposes the back to incoming attacks.
* Evade / Roll Behavior: 8-directional roll relative to movement stick input / camera direction.
* Attack Aiming: Attack vectors follow character mesh forward vector or camera look directional axis (Free-Aiming enabled).

---

## 2. Locked-On Mode (Target Anchor)

### 2.1 Camera Controller Dynamics
* Control Mode: Target Anchor.
* Target Tracking: Continuously rotates camera transform to keep active Target Lock Node centered in the view frame.
* Target Lock Nodes: Selectable bone anchors on enemies (Head, Torso, Base).
* Pitch Adjustments: Dynamic pitch clamping. Pitch automatically angles higher or lower based on distance and vertical elevation of the lock node.
* Auto-Snap: Rapidly snaps to keep fast-moving targets centered.

### 2.2 Character Orientation & Facing
* Heading Determination: Character's forward torso vector continuously rotates to face the active Target Lock Node (TargetLockVector).
* Strafing Matrix: Active. Pushing left/right forces lateral side-stepping while maintaining target facing.
* Orbiting Dynamics: Movement around the target forms a radial arc centered on the enemy lock node.

### 2.3 Movement Vectors & Speed Multipliers
* Forward Speed: 100% (1.0x baseline velocity).
* Backward Speed: 70% (0.7x baseline velocity — backpedaling penalty applied).
* Lateral (Strafe) Speed: 85% (0.85x baseline velocity).
* Sprint Behavior: Forces a curved orbital trajectory around the target, unless directional input diverges from the target vector by more than 120°, in which case the character breaks orbit into a linear escape sprint.

### 2.4 Combat & Evade Physics
* Guard Vector: Always pointed directly at the target lock vector, blocking incoming frontal attacks while retreating or strafing.
* Evade / Roll Behavior: 8-directional dodge relative to the Target Lock Vector (Forward Dodge, Backstep/Backroll, Left/Right Strafe Dodge).
* Attack Alignment: Attacks, melee swings, and spellcasting automatically home toward the targeted node coordinate (Free-Aiming disabled).

---

## 3. Transition Logic & Break Conditions

### 3.1 Lock-On Acquisition (OnLockOnButtonPressed)
* Perform spherecast / frustum check within max range (e.g., 20m) and line-of-sight check.
* On Success: Acquire nearest target node, switch state to LOCKED_ON.
* On Fail: Recenter camera forward along character look direction, remain in UNLOCKED.

### 3.2 Lock-On Disruption & Break Conditions
* Manual Toggle: User presses lock-on button again.
* Target Death: Target entity health reaches 0.
* Out of Range: Distance between player and lock target exceeds max leash distance (e.g., 25m).
* Line of Sight Loss: Target obscured by geometry for greater than 2.5 consecutive seconds.