# **Movement Mechanics Explained**

[https://gemini.google.com/app/4c9340b20769f746](https://gemini.google.com/app/4c9340b20769f746)

*User prompt: moving locked move and free not locked moves jump logic roll logic crouch logic speed difference i need more technical details. less game related details(rings, equipment, etc..)*

### **Response: Input Handling & Button Buffer Engine**

> * **Roll on Release**: Because Dodge/Sprint share a single keybind, the Engine fires the Roll state transition on **Button Up** (Key Release). If the key remains held past a fixed threshold (≈15–20 frames at 60 FPS / ∼250 ms), the system suppresses the roll event and transitions the movement state directly into Sprint.  
> * **Action Queuing (Input Buffer)**: Action requests (Roll, Jump, Crouch, Attack) register into a ≈15–30 frame sliding buffer during un-cancelable recovery windows. Queued inputs automatically execute on the first valid state transition frame.

### **Locomotion: Free-Aim (Unlocked) vs. Locked-On Mechanics**

| Locomotion Parameter | Free-Aim (Unlocked Movement) | Locked-On (Target-Relative Strafe) |
| :---- | :---- | :---- |
| **Coordinate Framework** | World-Space relative to Camera View Vector. | Target-Relative Polar Coordinates (Distance *r*, Angle *θ*). |
| **Facing Vector (*F*)** | Smoothly rotates to match the direction of the 2D joystick input vector (*I*). | Clamped directly toward the target transform (*T*). |
| **Velocity Vector (*V*)** | Uniform 100% vector speed in all 360° directions. | Non-uniform: Forward (100%), Lateral Arc (≈80–85%), Backward (≈70–75%). |
| **Turning Radius & Pivot** | Sharp directional changes (\>90∘) trigger pivot acceleration curves and speed decay. | Turning is eliminated for movement; character strafes laterally/backpedals with constant facing. |

### **Movement Tiers & Speed Values**

Locomotion uses root-motion animations blended with a character controller velocity modifier:

> * **Walk**: ≈2.5 m/s (Low noise footprint for stealth detection AI).  
> * **Run (Default Stick Tilt)**: ≈4.8 m/s.  
> * **Sprint (Hold Dodge)**: ≈6.7 m/s (Drains stamina continuously in combat states; 0 stamina drain out-of-combat).  
> * **Crouch Walk**: ≈2.0 m/s (Minimal AI perception radius).  
> * **Crouch Run (Fast Crouch)**: ≈3.6 m/s (Achieved by toggling crouch during sprint momentum).

### **Roll / Dodge Mechanics (Frame Data @ 60 FPS)**

Rolling generates invincibility frames (i-frames) by temporarily toggling the character's collision/hurtbox layer to ignore incoming attack hitboxes.

`[ Frame 0 ] ---> [ Active i-Frames ] ---> [ Recovery / Cancel Window ] ---> [ Control Restored ]`

| Weight Tier | Weight Ratio | Invincibility (i-Frames) | Recovery Frames | Vector Displacement |
| :---- | :---- | :---- | :---- | :---- |
| **Light Load** | \<30.0% | **13 frames** (Frames 0–12) | **8 frames** | Max distance (≈+20% travel range). |
| **Medium Load** | 30.0% to 69.9% | **13 frames** (Frames 0–12) | **8 frames** | Standard travel range. |
| **Heavy Load** | 70.0% to 99.9% | **12 frames** (Frames 0–11) | **16 frames** | Reduced travel range; long recovery penalty. |
| **Overloaded** | ≥100.0% | *Disabled* | *Disabled* | Roll replaced with a stumble animation. |

> * **Backstep**: Provides **0 i-frames** natively (11 i-frames with specific talismans) and ≈8–10 recovery frames. Serves as a rapid positional decay with instant access to backstep-specific light attack animation trees.

### **Jump Logic & Aerial Physics**

> * **Hurtbox Deactivation (Lower-Body Invincibility)**:  
  * Jumping does **not** grant full-body i-frames like a roll.  
  * From **Frame 1 to the apex of the jump** (≈20–25 frames), the lower-body hurtbox (hips down to feet) is disabled / flagged to ignore ground-level attack hitboxes.  
  * Attacks designed with Ground Sweep or Shockwave collision tags pass through the character without registering damage. Upper-torso hurtboxes remain fully active.  
> * **In-Air Velocity & Trajectory**:  
  * Launch trajectory combines ground momentum vector *V*ground​ with vertical impulse vector *V*jump​.  
  * **Air Control**: In-air trajectory allows limited user-steering influence (≈20–30% vector correction angle). Neutral jumps retain pure vertical lift with minimal horizontal drifting.

### **Crouch Logic & State Machine Tricks**

> * **Capsule Compression**: Toggling crouch reduces the player character's physical collision capsule height by ≈45%. The upper torso and head hurtboxes descend immediately, causing high horizontal weapon swings to whiff above the player ("crouch-dodging").  
> * **Animation Interrupts & Attack State Sharing**:  
  * The crouch state transition can cancel the ending recovery frames of specific weapon swings or rolls earlier than a standard standing idle transition.  
  * The engine maps **Crouch Attack** directly to the **Roll Attack** animation index (crouch\_attack \== roll\_attack). Initiating an attack while crouched executes the rolling attack immediately without spending stamina on a dodge roll or undergoing roll recovery frames.

**Sources**

> 1. [Dodging Tips and Tricks | Wiki](https://fextralife.com/Dodging)  
> 2. [Combat Mechanics | Reforged Wiki \- Fandom](https://err.fandom.com/wiki/Combat_Mechanics)

---

*User prompt: export it as md file \- as promt styled md file for ai agent*

`Response:               +-----------------------------------+`  
              `|      2D Joystick Input Vector     |`  
              `+-----------------------------------+`  
                                `|`  
                `+---------------+---------------+`  
                `|                               |`  
       `[ Target-Lock OFF ]             [ Target-Lock ON ]`  
                `|                               |`  
        `World-Space Cam Rel             Target-Relative Polar`  
        `360° Uniform Speed              Non-uniform Lateral Decay`  
        `Facing Rotates to Dir           Facing Fixed to Target`

`### 2.1 Unlocked Locomotion (Free-Aim)`  
`- **Coordinate System**: World-Space relative to Camera View Vector $\\vec{V}_{\\text{cam}}$.`  
`- **Facing Vector ($\\vec{F}$)**: Rotates dynamically to match the 2D joystick input direction $\\vec{I}$.`  
`- **Velocity Vector ($\\vec{V}$)**: Uniform $100\\%$ speed scalar regardless of direction angle.`  
`- **Pivot Decay**: Turn angles $> 90^\\circ$ trigger turn-pivot animation clips, introducing dynamic deceleration curves and momentary speed penalties.`

`### 2.2 Target Lock-On Locomotion`  
`- **Coordinate System**: Target-Relative Polar Coordinates (Distance $r$, Angle $\\theta$).`  
`- **Facing Vector ($\\vec{F}$)**: Fixed toward target transform $\\vec{T}$.`  
`- **Velocity Vector Scale**:`  
  `- Forward ($0^\\circ$): $100\\%$ base velocity.`  
  `- Lateral Arc ($\\pm 90^\\circ$): $\\approx 80\\text{--}85\\%$ base velocity.`  
  `- Backward ($180^\\circ$): $\\approx 70\\text{--}75\\%$ base velocity.`  
`- **Pivot Decay**: Eliminated; character strafes and backpedals seamlessly without directional turn animation stalls.`

`---`

`## 3. Movement Tiers & Velocity Metrics`

`| Locomotion Tier | Speed Value (m/s) | Stamina Drain Rate | AI Perception Noise Radius |`  
`| :--- | :--- | :--- | :--- |`  
`| **Crouch Walk** | $\\approx 2.0\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Minimal ($\approx 20\\%$) |`  
`| **Walk** | $\\approx 2.5\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Low ($\approx 40\\%$) |`  
`| **Crouch Run (Fast Crouch)**| $\\approx 3.6\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Low-Medium ($\approx 50\\%$) |`  
`| **Run (Default Stick)** | $\\approx 4.8\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Standard ($100\\%$) |`  
`| **Sprint** | $\\approx 6.7\\text{ m/s}$ | Combat: $\\sim 10.0\\text{ pts/s}$ / Out-of-Combat: $0.0$ | High ($150\\%$) |`

`---`

`## 4. Roll / Dodge Engine & Frame Data ($60\\text{ FPS}$)`

`Rolling toggles the player character's hurtbox layer collision flags to ignore enemy attack hitboxes.`

Frame 0 Frame 12/13 Frame 20/28 |-----------------------|---------------------| | Active i-Frames | Recovery Phase | | (Invulnerable Layer) | (Vulnerable Layer) |

`### 4.1 Weight Load Classifications`

0.0% 30.0% 70.0% 100.0% |--- Light \---|-- Medium \--|--- Heavy \---|-- Overloaded (Stumble) \--|

`- **Light Load ($< 30.0\\%$)**:`  
  `- **i-Frames**: $13\\text{ frames}$ (Frames 0–12).`  
  `- **Recovery**: $8\\text{ frames}$ (Total animation: $21\\text{ frames}$).`  
  `- **Displacement**: Maximum distance vector ($+20\\%$ modifier).`  
`- **Medium Load ($30.0\\% \\text{ to } 69.9\\%$)**:`  
  `- **i-Frames**: $13\\text{ frames}$ (Frames 0–12).`  
  `- **Recovery**: $8\\text{ frames}$ (Total animation: $21\\text{ frames}$).`  
  `- **Displacement**: Standard distance vector.`  
`- **Heavy Load ($70.0\\% \\text{ to } 99.9\\%$)**:`  
  `- **i-Frames**: $12\\text{ frames}$ (Frames 0–11).`  
  `- **Recovery**: $16\\text{ frames}$ (Total animation: $28\\text{ frames}$).`  
  `- **Displacement**: Reduced distance vector.`  
`- **Overloaded ($\\ge 100.0\\%$)**:`  
  `- **i-Frames**: $0\\text{ frames}$.`  
  `- **Behavior**: Replaced by a high-recovery stumble animation with zero invincibility window.`

`### 4.2 Backstep Mechanics`  
`- **i-Frames**: $0\\text{ frames}$ (default state).`  
`- **Recovery**: $8\\text{--}10\\text{ frames}$.`  
`- **Utility**: Rapid rearward displacement allowing immediate transition into backstep-unique attack animation trees.`

`---`

`## 5. Jump Logic & Aerial Physics`

`### 5.1 Lower-Body Hurtbox Deactivation`  
`- **Invincibility Classification**: Selective spatial hurtbox toggling (NOT global i-Frames).`  
`- **Active Window**: Frame 1 to Jump Apex ($\\approx 20\\text{--}25\\text{ frames}$).`  
``- **Mechanism**: The collision layer for the lower body (hips down to feet) is disabled or set to pass-through for attack hitboxes flagged with `GroundSweep` or `Shockwave` tags.``  
`- **Upper Torso**: Remains fully vulnerable throughout the entire jump arc.`

`### 5.2 Aerial Steering & Trajectory`  
`- **Initial Velocity Vector**: $\\vec{V}_{\\text{air}} = \\vec{V}_{\\text{ground}} + \\vec{V}_{\\text{jump}}$.`  
`- **Air Control Influence**: Directional input allows a maximum of $\\approx 20\\text{--}30\\%$ vector alteration angle mid-air. Neutral jumps maintain vertical momentum without horizontal drift.`

`---`

`## 6. Crouch Logic & Animation Interrupts`

`### 6.1 Capsule Compression`  
`- **Collision Capsule Height**: Reduced by $\\approx 45\\%$ upon entering Crouch state.`  
`- **High-Hitbox Evasion**: Head and upper torso hurtboxes lower instantly, allowing high-horizontal attack hitboxes to pass over the character model without triggering hit registration ("crouch dodging").`

`### 6.2 Animation Recovery Cancels & State Aliasing`  
`- **Recovery Interruption**: Toggling Crouch during the terminal recovery window of specific weapon attack clips cancels recovery frames earlier than standard Idle transitions.`  
`- **Attack Index Mapping**:`  
  ```` ```text ````  
  `crouch_attack_id == rolling_attack_id`

Executing an attack while in the Crouch state directly triggers the Rolling Attack animation script, bypassing stamina consumption and roll recovery requirements. """

file\_path \= "\_ring\_locomotion\_spec.md" with open(file\_path, "w", encoding="utf-8") as f: f.write(content.strip())

print(f"File saved successfully as {file\_path}")

```` ```text?code_stdout&code_event_index=1 ````  
`File saved successfully as locomotion_spec.md`

Your Markdown file formatted as a system prompt / technical specification for AI agents is ready:

\[file-tag: code-generated-file-7f9fb2b8-e861-4988-b3ed-145c991e3e43\]

### **Summary of Prompt Specifications Included in \_ring\_locomotion\_spec.md**

> * **Frontmatter Metadata**: System prompt identity, name, description, and versioning.  
> * **Input Engine Logic**: Key-release timer thresholds (15–20 frames), Dodge vs. Sprint registration, and the 15–30 frame sliding input buffer.  
> * **Locomotion Coordinate Frameworks**: Mathematical differentiation between Camera-Relative Free-Aim and Target-Relative Polar coordinates with speed decay matrices.  
> * **Velocity Metrics**: Ground speed metrics (2.0 m/s to 6.7 m/s), stamina drain logic, and AI stealth perception multipliers.  
> * **Roll Engine & Weight Tiers**: Light/Medium/Heavy/Overloaded i-frame matrices, recovery windows, and displacement scaling.  
> * **Jump Mechanics**: Lower-body selective hurtbox disabling (Frame 1 to Jump Apex) and air-steering trajectory limits (20–30%).  
> * **Crouch Architecture**: 45% capsule height compression, recovery cancels, and crouch\_attack \== rolling\_attack state aliasing.

---

*User prompt: add cases with how handles stairs logic also block movement \- so movement must have api to block entire movement when some animations played \- attacks, hit, roll, jump, fall also investigate the case that movement systems relays a lot for animation*

Response: \+-------------------+ Extract Vector \+------------------------+ | Animation Track | \------------------------\> | Delta Root Position | | (Clip Root Bone) | | Delta Root Rotation | \+-------------------+ \+------------------------+ | | | Modulated by Input v \+-----------------------------------\> \+------------------------+ | Character Capsule | | Transform Delta | \+------------------------+

`### 2.1 Motion Extraction & Blending`  
`- **Root Motion Primacy**: The primary velocity ($\vec{V}$) and yaw rotation ($\Delta \\theta$) are extracted directly from the root bone's translation vector ($\vec{\\Delta P}_{\\text{root}}$) and rotation quaternion ($\\mathbf{Q}_{\\text{root}}$) frame-by-frame:`  
  `$$\\vec{V}_{\\text{frame}} = \\frac{\\vec{\\Delta P}_{\\text{root}}}{\\Delta t}, \\quad \\Delta \\theta_{\\text{frame}} = \\text{Yaw}(\\mathbf{Q}_{\\text{root}})$$`  
`- **Velocity Decoupling**: Standard kinematic controller acceleration is zeroed out during root-motion driven actions (attacks, rolls, staggers). The capsule translation is governed strictly by the keyframed delta in the animation asset.`  
`- **Directional Blending**: For standard locomotion (Walk/Run), root-motion clips are blended across 2D parametric blend spaces based on input direction relative to camera/target, aligning the capsule forward vector with input while maintaining realistic foot-placement velocity.`

`---`

`## 3. Movement Blocking & Action Locking System`

`To enforce weighty, commitment-based combat, the movement system exposes an Action State Locking API driven by bitwise flag parameters per animation frame.`

`### 3.1 Movement Lock State Flags`

``| Flag Bit | State Flag Name | Effect When Active (`true`) |``  
`| :--- | :--- | :--- |`  
``| `0x01` | `LOCK_MOVE_INPUT` | Ignores directional stick input ($\vec{I}$). Vector speed drops to zero or relies purely on Root Motion. |``  
``| `0x02` | `LOCK_TURN_INPUT` | Fixes character facing vector ($\vec{F}$). Rotational input is suppressed. |``  
``| `0x04` | `USE_ROOT_MOTION` | Overrides character capsule movement exclusively with root-motion translation/rotation curves. |``  
``| `0x08` | `ALLOW_ACTION_CANCEL`| Enables input buffer evaluation for recovery cancels (Crouch cancel, Roll cancel, Attack chaining). |``

`### 3.2 Action State Matrix`

\[ Attack / Skill \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION \[ Hit Reaction \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION (Knockback) \[ Dodge Roll \] \----\> LOCK\_MOVE\_INPUT | USE\_ROOT\_MOTION (Unlocked at Recovery) \[ Fall / Landing \] \----\> LOCK\_MOVE\_INPUT (Scale duration by fall height / impact force)

`- **Attacks & Weapon Skills**:`  
  ``- Sets `LOCK_MOVE_INPUT`, `LOCK_TURN_INPUT`, and `USE_ROOT_MOTION`.``  
  `- Player cannot steer manually; positional lunge or step-forward is governed strictly by the attack clip's root displacement.`  
`- **Hit Reactions & Stagger**:`  
  ``- Hard lock on all user inputs (`LOCK_MOVE_INPUT | LOCK_TURN_INPUT | USE_ROOT_MOTION`).``  
  `- Character velocity is driven by the stagger recoil root motion animation curve corresponding to the Poise Damage tier.`  
`- **Roll / Backstep**:`  
  ``- `LOCK_MOVE_INPUT` active during dodge execution. Direction vector is locked at Frame 0 based on input angle and cannot be steered mid-flight.``  
  ``- Switches to `ALLOW_ACTION_CANCEL` on Frame 13 (Medium load), enabling early exit into crouch/attack/block.``  
`- **Landing Recovery / Impact Stuns**:`  
  `- **Light Fall ($< 4\\text{m}$)**: No movement lock; root motion blends smoothly into landing-run transition.`  
  ``- **Medium Fall ($4\\text{m} - 12\\text{m}$)**: Sets `LOCK_MOVE_INPUT` for $12\\text{--}25\\text{ frames}$ based on vertical velocity ($v_z$).``  
  `- **Hard Landing ($12\\text{m} - 16\\text{m}$)**: Forces heavy knee-bend recovery animation with absolute input lock ($\sim 45\\text{ frames}$).`

`---`

`## 4. Ground Alignment & Stairs Logic`

`Handling uneven geometry and stairways without capsule snagging or floating feet relies on **Raycast Probes, Inverse Kinematics (IK), and Root Motion Surface Snapping**.`

          `Capsule Raycast Center`  
                    `|`  
        `+-----------+-----------+`  
        `|                       |`  
  `Left Foot Probe         Right Foot Probe`  
        `|                       |`  
        `v                       v`  
  `+-----------+           +-----------+`  
  `|  Step IK  |           |  Step IK  |`  
  `+-----------+           +-----------+`

`### 4.1 Foot-Placement Inverse Kinematics (2-Bone IK)`  
`- **Raycast Down Probes**: Dual downward raycasts originate from the hip/ankle bone transforms during ground-contact animation frames.`  
`- **Offset Calculation**: Calculates height delta $\\Delta z_i = z_{\\text{hit}} - z_{\\text{root\_plane}}$.`  
`- **Leg Adjustment**: Adjusts ankle end-effectors via 2-Bone IK (Hip-Knee-Ankle) to match stair treads, while bending knee joints analytically.`  
`- **Pelvis Adaptation**: Drops or raises the root pelvis bone transform height by $\\min(\\Delta z_{\\text{left}}, \\Delta z_{\\text{right}})$ to keep leg extensions within realistic biomechanical limits on steep stairs.`

`### 4.2 Stair Stepping & Slope Physics`  
`- **Maximum Step Height**: $h_{\\text{step}} \\approx 0.3\\text{m}$. Obstacles below $h_{\\text{step}}$ allow smooth capsule step-up without triggering jump or fall state machines.`  
`- **Slope Angle Thresholds**:`  
  `- $\\theta \\le 45^\\circ$: Walkable ground. Velocity vector is projected onto the surface plane vector $\\vec{V}_{\\text{surface}} = \\vec{V} - (\\vec{V} \\cdot \\vec{N}) \\vec{N}$.`  
  `- $45^\\circ < \\theta \\le 60^\\circ$: Sliding slope. Adds downward gravity vector acceleration while scaling player control input down.`  
  ``- $\\theta > 60^\\circ$: Non-walkable wall. Capsule collision bounces horizontal velocity; character detaches from ground state machine into `Falling`.``  
`- **Root Motion Stair Slope Matching**: When traversing stairs using root motion, horizontal root displacement ($\\Delta x, \\Delta y$) is aligned with the surface plane tangent, preventing the character model from clipping inside stair geometry or hovering off step edges.`

`---`

`## 5. Locomotion Modes: Free-Aim (Unlocked) vs. Target Lock-On`

`### 5.1 Unlocked Locomotion (Free-Aim)`  
`- **Coordinate System**: World-Space relative to Camera View Vector $\\vec{V}_{\\text{cam}}$.`  
`- **Facing Vector ($\\vec{F}$)**: Rotates dynamically to match the 2D joystick input direction $\vec{I}$.`  
`- **Velocity Vector ($\\vec{V}$)**: Uniform $100\\%$ speed scalar regardless of direction angle.`  
`- **Pivot Decay**: Turn angles $> 90^\\circ$ trigger turn-pivot animation clips, introducing dynamic deceleration curves and momentary speed penalties.`

`### 5.2 Target Lock-On Locomotion`  
`- **Coordinate System**: Target-Relative Polar Coordinates (Distance $r$, Angle $\\theta$).`  
`- **Facing Vector ($\\vec{F}$)**: Fixed toward target transform $\\vec{T}$.`  
`- **Velocity Vector Scale**:`  
  `- Forward ($0^\\circ$): $100\\%$ base velocity.`  
  `- Lateral Arc ($\\pm 90^\\circ$): $\\approx 80\\text{--}85\\%$ base velocity.`  
  `- Backward ($180^\\circ$): $\\approx 70\\text{--}75\\%$ base velocity.`  
`- **Pivot Decay**: Eliminated; character strafes and backpedals seamlessly without directional turn animation stalls.`

`---`

`## 6. Movement Tiers & Velocity Metrics`

`| Locomotion Tier | Speed Value (m/s) | Stamina Drain Rate | AI Perception Noise Radius |`  
`| :--- | :--- | :--- | :--- |`  
`| **Crouch Walk** | $\\approx 2.0\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Minimal ($\approx 20\\%$) |`  
`| **Walk** | $\\approx 2.5\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Low ($\approx 40\\%$) |`  
`| **Crouch Run (Fast Crouch)**| $\\approx 3.6\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Low-Medium ($\approx 50\\%$) |`  
`| **Run (Default Stick)** | $\\approx 4.8\\text{ m/s}$ | $0.0\\text{ pts/s}$ | Standard ($100\\%$) |`  
`| **Sprint** | $\\approx 6.7\\text{ m/s}$ | Combat: $\\sim 10.0\\text{ pts/s}$ / Out-of-Combat: $0.0$ | High ($150\\%$) |`

`---`

`## 7. Roll / Dodge Engine & Frame Data ($60\\text{ FPS}$)`

`Rolling toggles the player character's hurtbox layer collision flags to ignore enemy attack hitboxes.`

Frame 0 Frame 12/13 Frame 20/28 |-----------------------|---------------------| | Active i-Frames | Recovery Phase | | (Invulnerable Layer) | (Vulnerable Layer) |

`### 7.1 Weight Load Classifications`

`- **Light Load ($< 30.0\\%$)**: $13\\text{ i-frames}$ (Frames 0–12), $8\\text{ recovery frames}$, $+20\\%$ displacement distance.`  
`- **Medium Load ($30.0\\% \\text{ to } 69.9\\%$)**: $13\\text{ i-frames}$ (Frames 0–12), $8\\text{ recovery frames}$, standard displacement.`  
`- **Heavy Load ($70.0\\% \\text{ to } 99.9\\%$)**: $12\\text{ i-frames}$ (Frames 0–11), $16\\text{ recovery frames}$, reduced displacement.`  
`- **Overloaded ($\\ge 100.0\\%$)**: $0\\text{ i-frames}$, high recovery stumble animation.`

`### 7.2 Backstep Mechanics`  
`- **i-Frames**: $0\\text{ frames}$ (default state).`  
`- **Recovery**: $8\\text{--}10\\text{ frames}$.`  
`- **Utility**: Rapid rearward displacement allowing immediate transition into backstep-unique attack animation trees.`

`---`

`## 8. Jump Logic & Aerial Physics`

`### 8.1 Lower-Body Hurtbox Deactivation`  
`- **Invincibility Classification**: Selective spatial hurtbox toggling (NOT global i-Frames).`  
`- **Active Window**: Frame 1 to Jump Apex ($\\approx 20\\text{--}25\\text{ frames}$).`  
``- **Mechanism**: The collision layer for the lower body (hips down to feet) is disabled or set to pass-through for attack hitboxes flagged with `GroundSweep` or `Shockwave` tags.``  
`- **Upper Torso**: Remains fully vulnerable throughout the entire jump arc.`

`### 8.2 Aerial Steering & Trajectory`  
`- **Initial Velocity Vector**: $\\vec{V}_{\\text{air}} = \\vec{V}_{\\text{ground}} + \\vec{V}_{\\text{jump}}$.`  
`- **Air Control Influence**: Directional input allows a maximum of $\\approx 20\\text{--}30\\%$ vector alteration angle mid-air. Neutral jumps maintain vertical momentum without horizontal drift.`

`---`

`## 9. Crouch Logic & Animation Interrupts`

`### 9.1 Capsule Compression`  
`- **Collision Capsule Height**: Reduced by $\\approx 45\\%$ upon entering Crouch state.`  
`- **High-Hitbox Evasion**: Head and upper torso hurtboxes lower instantly, allowing high-horizontal attack hitboxes to pass over the character model without triggering hit registration ("crouch dodging").`

`### 9.2 Animation Recovery Cancels & State Aliasing`  
`- **Recovery Interruption**: Toggling Crouch during the terminal recovery window of specific weapon attack clips cancels recovery frames earlier than standard Idle transitions.`  
`- **Attack Index Mapping**:`  
  ```` ```text ````  
  `crouch_attack_id == rolling_attack_id`

Executing an attack while in the Crouch state directly triggers the Rolling Attack animation script, bypassing stamina consumption and roll recovery requirements. """

file\_path \= "\_ring\_locomotion\_spec.md" with open(file\_path, "w", encoding="utf-8") as f: f.write(content.strip())

print(f"File updated successfully as {file\_path}")

```` ```text?code_stdout&code_event_index=1 ````  
`File updated successfully as locomotion_spec.md`

The updated Markdown specification file \_ring\_locomotion\_spec.md is generated and saved:

\[file-tag: code-generated-file-3f44baaf-1ac9-42b6-b2dc-676d3ffa00b7\]

### **What Was Added & System Architecture Overview**

#### **1\. Root Motion-Centric Locomotion Architecture**

> * **Animation-Driven Displacement**: Translates root-bone animation keyframes (Δ*P*root​, Qroot​) directly into capsule velocity, decoupling standard kinematic controller acceleration during dynamic actions.  
> * **Non-Linear Speed Profiles**: Explains how attack lunges, roll initial boosts/decelerations, and stagger knockbacks are encoded directly into the animation assets rather than coded via physics forces.

#### **2\. Movement Blocking & Action Locking System (API)**

> * **Bitwise State Lock Flags**:  
  * LOCK\_MOVE\_INPUT (0x01): Ignores joystick directional vectors (*I*).  
  * LOCK\_TURN\_INPUT (0x02): Prevents character yaw rotation adjustments.  
  * USE\_ROOT\_MOTION (0x04): Forces capsule movement to rely strictly on root-motion animation curves.  
  * ALLOW\_ACTION\_CANCEL (0x08): Opens input buffer evaluation window for recovery cancels.  
> * **Action State Locking Matrix**: Standardizes input locks across **Attacks/Skills**, **Hit Reactions/Stagger**, **Dodge Rolls/Backsteps**, and **Fall/Landing Recovery Stuns** (12–45 frames based on impact velocity).

#### **3\. Ground Alignment & Stairs Logic**

> * **Raycast Probes & 2-Bone Foot IK**: Dual downward raycasts from feet adjust ankle end-effectors and knee rotations via 2-Bone IK, while adjusting pelvis height (min(Δ*z*left​,Δ*z*right​)) on stair treads.  
> * **Slope Thresholds & Surface Normal Projection**:  
  * **≤45∘**: Velocity projects onto the stair/slope surface plane vector *V*surface​.  
  * **45∘−60∘**: Triggers downward slide gravity acceleration.  
  * **\>60∘**: Detaches character from grounded state into Falling.  
> * **Step Limit**: Step height threshold capped at *h*step​≈0.3m.

### **File Content (\_ring\_locomotion\_spec.md)**

`---`  
`name: locomotion-spec`  
`description: Technical specification and system prompt for character locomotion, frame data, root motion architecture, stairs handling, and state machine locking.`  
`version: 1.1.0`  
`---`

`# SYSTEM PROMPT / TECHNICAL SPECIFICATION: Locomotion Architecture`

`You are an expert Game Physics & Animation Systems Engineer specializing in FromSoftware-style action RPG locomotion architecture. Utilize the precise mathematical, algorithmic, and frame-accurate rules outlined below for state transitions, movement vectors, input buffer handling, root-motion processing, stairs handling, and movement blocking flags.`

`---`

`## 1. Input Engine & Buffer Management`

`### 1.1 Key-Release Action Mapping (Sprint vs. Roll)`  
`- **Key-Down Event**: Starts an internal timer ($t_{\text{hold}}$).`  
``- **Key-Up Event ($t_{\text{hold}} < T_{\text{threshold}}$)**: Triggers the `Roll` state transition on key release.``  
  `- Threshold $T_{\text{threshold}} \approx 15\text{--}20\text{ frames}$ ($250\text{ ms}$ at $60\text{ FPS}$).`  
``- **Hold Event ($t_{\text{hold}} \ge T_{\text{threshold}}$)**: Cancels the `Roll` event registration and transitions locomotion directly into `Sprint`.``

`### 1.2 Input Buffer (Sliding Window)`  
`- **Buffer Size**: $15\text{--}30\text{ frames}$ ($250\text{--}500\text{ ms}$).`  
``- **Behavior**: Any action command (`Roll`, `Jump`, `Crouch`, `Attack`) pressed during non-cancelable action recovery windows is cached. The queued action executes on frame 1 of the earliest cancel window (`CanCancel` flag = `true`).``

`---`

`## 2. Root-Motion Centric Locomotion Architecture`

`Unlike standard dynamic arcade movement systems that rely purely on kinematic velocity vectors applied directly to a capsule, FromSoftware's locomotion engine heavily relies on **Root-Motion Animation Curves** augmented by dynamic input blending.`

\+-------------------+ Extract Vector \+------------------------+ | Animation Track | \------------------------\> | Delta Root Position | | (Clip Root Bone) | | Delta Root Rotation | \+-------------------+ \+------------------------+ | | | Modulated by Input v \+-----------------------------------\> \+------------------------+ | Character Capsule | | Transform Delta | \+------------------------+

`### 2.1 Motion Extraction & Blending`  
`- **Root Motion Primacy**: The primary velocity ($\vec{V}$) and yaw rotation ($\Delta \theta$) are extracted directly from the root bone's translation vector ($\vec{\Delta P}_{\text{root}}$) and rotation quaternion ($\mathbf{Q}_{\text{root}}$) frame-by-frame:`  
  `$$\vec{V}_{\text{frame}} = \frac{\vec{\Delta P}_{\text{root}}}{\Delta t}, \quad \Delta \theta_{\text{frame}} = \text{Yaw}(\mathbf{Q}_{\text{root}})$$`  
`- **Velocity Decoupling**: Standard kinematic controller acceleration is zeroed out during root-motion driven actions (attacks, rolls, staggers). The capsule translation is governed strictly by the keyframed delta in the animation asset.`  
`- **Directional Blending**: For standard locomotion (Walk/Run), root-motion clips are blended across 2D parametric blend spaces based on input direction relative to camera/target, aligning the capsule forward vector with input while maintaining realistic foot-placement velocity.`

`---`

`## 3. Movement Blocking & Action Locking System`

`To enforce weighty, commitment-based combat, the movement system exposes an Action State Locking API driven by bitwise flag parameters per animation frame.`

`### 3.1 Movement Lock State Flags`

``| Flag Bit | State Flag Name | Effect When Active (`true`) |``  
`| :--- | :--- | :--- |`  
``| `0x01` | `LOCK_MOVE_INPUT` | Ignores directional stick input ($\vec{I}$). Vector speed drops to zero or relies purely on Root Motion. |``  
``| `0x02` | `LOCK_TURN_INPUT` | Fixes character facing vector ($\vec{F}$). Rotational input is suppressed. |``  
``| `0x04` | `USE_ROOT_MOTION` | Overrides character capsule movement exclusively with root-motion translation/rotation curves. |``  
``| `0x08` | `ALLOW_ACTION_CANCEL`| Enables input buffer evaluation for recovery cancels (Crouch cancel, Roll cancel, Attack chaining). |``

`### 3.2 Action State Matrix`

\[ Attack / Skill \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION \[ Hit Reaction \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION (Knockback) \[ Dodge Roll \] \----\> LOCK\_MOVE\_INPUT | USE\_ROOT\_MOTION (Unlocked at Recovery) \[ Fall / Landing \] \----\> LOCK\_MOVE\_INPUT (Scale duration by fall height / impact force)

`- **Attacks & Weapon Skills**:`  
  ``- Sets `LOCK_MOVE_INPUT`, `LOCK_TURN_INPUT`, and `USE_ROOT_MOTION`.``  
  `- Player cannot steer manually; positional lunge or step-forward is governed strictly by the attack clip's root displacement.`  
`- **Hit Reactions & Stagger**:`  
  ``- Hard lock on all user inputs (`LOCK_MOVE_INPUT | LOCK_TURN_INPUT | USE_ROOT_MOTION`).``  
  `- Character velocity is driven by the stagger recoil root motion animation curve corresponding to the Poise Damage tier.`  
`- **Roll / Backstep**:`  
  ``- `LOCK_MOVE_INPUT` active during dodge execution. Direction vector is locked at Frame 0 based on input angle and cannot be steered mid-flight.``  
  ``- Switches to `ALLOW_ACTION_CANCEL` on Frame 13 (Medium load), enabling early exit into crouch/attack/block.``  
`- **Landing Recovery / Impact Stuns**:`  
  `- **Light Fall ($< 4\text{m}$)**: No movement lock; root motion blends smoothly into landing-run transition.`  
  ``- **Medium Fall ($4\text{m} - 12\text{m}$)**: Sets `LOCK_MOVE_INPUT` for $12\text{--}25\text{ frames}$ based on vertical velocity ($v_z$).``  
  `- **Hard Landing ($12\text{m} - 16\text{m}$)**: Forces heavy knee-bend recovery animation with absolute input lock ($\sim 45\text{ frames}$).`

`---`

`## 4. Ground Alignment & Stairs Logic`

`Handling uneven geometry and stairways without capsule snagging or floating feet relies on **Raycast Probes, Inverse Kinematics (IK), and Root Motion Surface Snapping**.`

          `Capsule Raycast Center`  
                    `|`  
        `+-----------+-----------+`  
        `|                       |`  
  `Left Foot Probe         Right Foot Probe`  
        `|                       |`  
        `v                       v`  
  `+-----------+           +-----------+`  
  `|  Step IK  |           |  Step IK  |`  
  `+-----------+           +-----------+`

`### 4.1 Foot-Placement Inverse Kinematics (2-Bone IK)`  
`- **Raycast Down Probes**: Dual downward raycasts originate from the hip/ankle bone transforms during ground-contact animation frames.`  
`- **Offset Calculation**: Calculates height delta $\Delta z_i = z_{\text{hit}} - z_{\text{root\_plane}}$.`  
`- **Leg Adjustment**: Adjusts ankle end-effectors via 2-Bone IK (Hip-Knee-Ankle) to match stair treads, while bending knee joints analytically.`  
`- **Pelvis Adaptation**: Drops or raises the root pelvis bone transform height by $\min(\Delta z_{\text{left}}, \Delta z_{\text{right}})$ to keep leg extensions within realistic biomechanical limits on steep stairs.`

`### 4.2 Stair Stepping & Slope Physics`  
`- **Maximum Step Height**: $h_{\text{step}} \approx 0.3\text{m}$. Obstacles below $h_{\text{step}}$ allow smooth capsule step-up without triggering jump or fall state machines.`  
`- **Slope Angle Thresholds**:`  
  `- $\theta \le 45^\circ$: Walkable ground. Velocity vector is projected onto the surface plane vector $\vec{V}_{\text{surface}} = \vec{V} - (\vec{V} \cdot \vec{N}) \vec{N}$.`  
  `- $45^\circ < \theta \le 60^\circ$: Sliding slope. Adds downward gravity vector acceleration while scaling player control input down.`  
  ``- $\theta > 60^\circ$: Non-walkable wall. Capsule collision bounces horizontal velocity; character detaches from ground state machine into `Falling`.``  
`- **Root Motion Stair Slope Matching**: When traversing stairs using root motion, horizontal root displacement ($\Delta x, \Delta y$) is aligned with the surface plane tangent, preventing the character model from clipping inside stair geometry or hovering off step edges.`

`---`

`## 5. Locomotion Modes: Free-Aim (Unlocked) vs. Target Lock-On`

`### 5.1 Unlocked Locomotion (Free-Aim)`  
`- **Coordinate System**: World-Space relative to Camera View Vector $\vec{V}_{\text{cam}}$.`  
`- **Facing Vector ($\vec{F}$)**: Rotates dynamically to match the 2D joystick input direction $\vec{I}$.`  
`- **Velocity Vector ($\vec{V}$)**: Uniform $100\%$ speed scalar regardless of direction angle.`  
`- **Pivot Decay**: Turn angles $> 90^\circ$ trigger turn-pivot animation clips, introducing dynamic deceleration curves and momentary speed penalties.`

`### 5.2 Target Lock-On Locomotion`  
`- **Coordinate System**: Target-Relative Polar Coordinates (Distance $r$, Angle $\theta$).`  
`- **Facing Vector ($\vec{F}$)**: Fixed toward target transform $\vec{T}$.`  
`- **Velocity Vector Scale**:`  
  `- Forward ($0^\circ$): $100\%$ base velocity.`  
  `- Lateral Arc ($\pm 90^\circ$): $\approx 80\text{--}85\%$ base velocity.`  
  `- Backward ($180^\circ$): $\approx 70\text{--}75\%$ base velocity.`  
`- **Pivot Decay**: Eliminated; character strafes and backpedals seamlessly without directional turn animation stalls.`

`---`

`## 6. Movement Tiers & Velocity Metrics`

`| Locomotion Tier | Speed Value (m/s) | Stamina Drain Rate | AI Perception Noise Radius |`  
`| :--- | :--- | :--- | :--- |`  
`| **Crouch Walk** | $\approx 2.0\text{ m/s}$ | $0.0\text{ pts/s}$ | Minimal ($\approx 20\%$) |`  
`| **Walk** | $\approx 2.5\text{ m/s}$ | $0.0\text{ pts/s}$ | Low ($\approx 40\%$) |`  
`| **Crouch Run (Fast Crouch)**| $\approx 3.6\text{ m/s}$ | $0.0\text{ pts/s}$ | Low-Medium ($\approx 50\%$) |`  
`| **Run (Default Stick)** | $\approx 4.8\text{ m/s}$ | $0.0\text{ pts/s}$ | Standard ($100\%$) |`  
`| **Sprint** | $\approx 6.7\text{ m/s}$ | Combat: $\sim 10.0\text{ pts/s}$ / Out-of-Combat: $0.0$ | High ($150\%$) |`

`---`

`## 7. Roll / Dodge Engine & Frame Data ($60\text{ FPS}$)`

`Rolling toggles the player character's hurtbox layer collision flags to ignore enemy attack hitboxes.`

Frame 0 Frame 12/13 Frame 20/28 |-----------------------|---------------------| | Active i-Frames | Recovery Phase | | (Invulnerable Layer) | (Vulnerable Layer) |

`### 7.1 Weight Load Classifications`

`- **Light Load ($< 30.0\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, $+20\%$ displacement distance.`  
`- **Medium Load ($30.0\% \text{ to } 69.9\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, standard displacement.`  
`- **Heavy Load ($70.0\% \text{ to } 99.9\%$)**: $12\text{ i-frames}$ (Frames 0–11), $16\text{ recovery frames}$, reduced displacement.`  
`- **Overloaded ($\ge 100.0\%$)**: $0\text{ i-frames}$, high recovery stumble animation.`

`### 7.2 Backstep Mechanics`  
`- **i-Frames**: $0\text{ frames}$ (default state).`  
`- **Recovery**: $8\text{--}10\text{ frames}$.`  
`- **Utility**: Rapid rearward displacement allowing immediate transition into backstep-unique attack animation trees.`

`---`

`## 8. Jump Logic & Aerial Physics`

`### 8.1 Lower-Body Hurtbox Deactivation`  
`- **Invincibility Classification**: Selective spatial hurtbox toggling (NOT global i-Frames).`  
`- **Active Window**: Frame 1 to Jump Apex ($\approx 20\text{--}25\text{ frames}$).`  
``- **Mechanism**: The collision layer for the lower body (hips down to feet) is disabled or set to pass-through for attack hitboxes flagged with `GroundSweep` or `Shockwave` tags.``  
`- **Upper Torso**: Remains fully vulnerable throughout the entire jump arc.`

`### 8.2 Aerial Steering & Trajectory`  
`- **Initial Velocity Vector**: $\vec{V}_{\text{air}} = \vec{V}_{\text{ground}} + \vec{V}_{\text{jump}}$.`  
`- **Air Control Influence**: Directional input allows a maximum of $\approx 20\text{--}30\%$ vector alteration angle mid-air. Neutral jumps maintain vertical momentum without horizontal drift.`

`---`

`## 9. Crouch Logic & Animation Interrupts`

`### 9.1 Capsule Compression`  
`- **Collision Capsule Height**: Reduced by $\approx 45\%$ upon entering Crouch state.`  
`- **High-Hitbox Evasion**: Head and upper torso hurtboxes lower instantly, allowing high-horizontal attack hitboxes to pass over the character model without triggering hit registration ("crouch dodging").`

`### 9.2 Animation Recovery Cancels & State Aliasing`  
`- **Recovery Interruption**: Toggling Crouch during the terminal recovery window of specific weapon attack clips cancels recovery frames earlier than standard Idle transitions.`  
`- **Attack Index Mapping**:`  
  ```` ```text ````  
  `crouch_attack_id == rolling_attack_id`

Executing an attack while in the Crouch state directly triggers the Rolling Attack animation script, bypassing stamina consumption and roll recovery requirements.

---

*User prompt: now extract this all as md file*

`Response: ---`  
`name: locomotion-spec`  
`description: Technical specification and system prompt for character locomotion, frame data, root motion architecture, stairs handling, and state machine locking.`  
`version: 1.1.0`  
`---`

`# SYSTEM PROMPT / TECHNICAL SPECIFICATION: Locomotion Architecture`

`You are an expert Game Physics & Animation Systems Engineer specializing in FromSoftware-style action RPG locomotion architecture[cite: 2]. Utilize the precise mathematical, algorithmic, and frame-accurate rules outlined below for state transitions, movement vectors, input buffer handling, root-motion processing, stairs handling, and movement blocking flags[cite: 2].`

`---`

`## 1. Input Engine & Buffer Management`

`### 1.1 Key-Release Action Mapping (Sprint vs. Roll)`  
`- **Key-Down Event**: Starts an internal timer ($t_{\text{hold}}$)[cite: 2].`  
``- **Key-Up Event ($t_{\text{hold}} < T_{\text{threshold}}$)**: Triggers the `Roll` state transition on key release[cite: 2].``  
  `- Threshold $T_{\text{threshold}} \approx 15\text{--}20\text{ frames}$ ($250\text{ ms}$ at $60\text{ FPS}$)[cite: 2].`  
``- **Hold Event ($t_{\text{hold}} \ge T_{\text{threshold}}$)**: Cancels the `Roll` event registration and transitions locomotion directly into `Sprint`[cite: 2].``

`### 1.2 Input Buffer (Sliding Window)`  
`- **Buffer Size**: $15\text{--}30\text{ frames}$ ($250\text{--}500\text{ ms}$)[cite: 2].`  
``- **Behavior**: Any action command (`Roll`, `Jump`, `Crouch`, `Attack`) pressed during non-cancelable action recovery windows is cached[cite: 2]. The queued action executes on frame 1 of the earliest cancel window (`CanCancel` flag = `true`)[cite: 2].``

`---`

`## 2. Root-Motion Centric Locomotion Architecture`

`Unlike standard dynamic arcade movement systems that rely purely on kinematic velocity vectors applied directly to a capsule, FromSoftware's locomotion engine heavily relies on **Root-Motion Animation Curves** augmented by dynamic input blending[cite: 2].`

\+-------------------+ Extract Vector \+------------------------+ | Animation Track | \------------------------\> | Delta Root Position | | (Clip Root Bone) | | Delta Root Rotation | \+-------------------+ \+------------------------+ | | | Modulated by Input v \+-----------------------------------\> \+------------------------+ | Character Capsule | | Transform Delta | \+------------------------+

`### 2.1 Motion Extraction & Blending`  
`- **Root Motion Primacy**: The primary velocity ($\vec{V}$) and yaw rotation ($\Delta \theta$) are extracted directly from the root bone's translation vector ($\vec{\Delta P}_{\text{root}}$) and rotation quaternion ($\mathbf{Q}_{\text{root}}$) frame-by-frame[cite: 2]:`  
  `$$\vec{V}_{\text{frame}} = \frac{\vec{\Delta P}_{\text{root}}}{\Delta t}, \quad \Delta \theta_{\text{frame}} = \text{Yaw}(\mathbf{Q}_{\text{root}})$$`  
`[cite: 2]`  
`- **Velocity Decoupling**: Standard kinematic controller acceleration is zeroed out during root-motion driven actions (attacks, rolls, staggers)[cite: 2]. The capsule translation is governed strictly by the keyframed delta in the animation asset[cite: 2].`  
`- **Directional Blending**: For standard locomotion (Walk/Run), root-motion clips are blended across 2D parametric blend spaces based on input direction relative to camera/target, aligning the capsule forward vector with input while maintaining realistic foot-placement velocity[cite: 2].`

`---`

`## 3. Movement Blocking & Action Locking System`

`To enforce weighty, commitment-based combat, the movement system exposes an Action State Locking API driven by bitwise flag parameters per animation frame[cite: 2].`

`### 3.1 Movement Lock State Flags`

``| Flag Bit | State Flag Name | Effect When Active (`true`) |``  
`| :--- | :--- | :--- |`  
``| `0x01` | `LOCK_MOVE_INPUT` | Ignores directional stick input ($\vec{I}$). Vector speed drops to zero or relies purely on Root Motion.[cite: 2] |``  
``| `0x02` | `LOCK_TURN_INPUT` | Fixes character facing vector ($\vec{F}$). Rotational input is suppressed.[cite: 2] |``  
``| `0x04` | `USE_ROOT_MOTION` | Overrides character capsule movement exclusively with root-motion translation/rotation curves.[cite: 2] |``  
``| `0x08` | `ALLOW_ACTION_CANCEL`| Enables input buffer evaluation for recovery cancels (Crouch cancel, Roll cancel, Attack chaining).[cite: 2] |``

`### 3.2 Action State Matrix`

\[ Attack / Skill \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION \[ Hit Reaction \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION (Knockback) \[ Dodge Roll \] \----\> LOCK\_MOVE\_INPUT | USE\_ROOT\_MOTION (Unlocked at Recovery) \[ Fall / Landing \] \----\> LOCK\_MOVE\_INPUT (Scale duration by fall height / impact force)

`- **Attacks & Weapon Skills**:`  
  ``- Sets `LOCK_MOVE_INPUT`, `LOCK_TURN_INPUT`, and `USE_ROOT_MOTION`[cite: 2].``  
  `- Player cannot steer manually; positional lunge or step-forward is governed strictly by the attack clip's root displacement[cite: 2].`  
`- **Hit Reactions & Stagger**:`  
  ``- Hard lock on all user inputs (`LOCK_MOVE_INPUT | LOCK_TURN_INPUT | USE_ROOT_MOTION`)[cite: 2].``  
  `- Character velocity is driven by the stagger recoil root motion animation curve corresponding to the Poise Damage tier[cite: 2].`  
`- **Roll / Backstep**:`  
  ``- `LOCK_MOVE_INPUT` active during dodge execution[cite: 2]. Direction vector is locked at Frame 0 based on input angle and cannot be steered mid-flight[cite: 2].``  
  ``- Switches to `ALLOW_ACTION_CANCEL` on Frame 13 (Medium load), enabling early exit into crouch/attack/block[cite: 2].``  
`- **Landing Recovery / Impact Stuns**:`  
  `- **Light Fall ($< 4\text{m}$)**: No movement lock; root motion blends smoothly into landing-run transition[cite: 2].`  
  ``- **Medium Fall ($4\text{m} - 12\text{m}$)**: Sets `LOCK_MOVE_INPUT` for $12\text{--}25\text{ frames}$ based on vertical velocity ($v_z$)[cite: 2].``  
  `- **Hard Landing ($12\text{m} - 16\text{m}$)**: Forces heavy knee-bend recovery animation with absolute input lock ($\sim 45\text{ frames}$)[cite: 2].`

`---`

`## 4. Ground Alignment & Stairs Logic`

`Handling uneven geometry and stairways without capsule snagging or floating feet relies on **Raycast Probes, Inverse Kinematics (IK), and Root Motion Surface Snapping**[cite: 2].`

          `Capsule Raycast Center`  
                    `|`  
        `+-----------+-----------+`  
        `|                       |`  
  `Left Foot Probe         Right Foot Probe`  
        `|                       |`  
        `v                       v`  
  `+-----------+           +-----------+`  
  `|  Step IK  |           |  Step IK  |`  
  `+-----------+           +-----------+`

`### 4.1 Foot-Placement Inverse Kinematics (2-Bone IK)`  
`- **Raycast Down Probes**: Dual downward raycasts originate from the hip/ankle bone transforms during ground-contact animation frames[cite: 2].`  
`- **Offset Calculation**: Calculates height delta $\Delta z_i = z_{\text{hit}} - z_{\text{root\_plane}}$[cite: 2].`  
`- **Leg Adjustment**: Adjusts ankle end-effectors via 2-Bone IK (Hip-Knee-Ankle) to match stair treads, while bending knee joints analytically[cite: 2].`  
`- **Pelvis Adaptation**: Drops or raises the root pelvis bone transform height by $\min(\Delta z_{\text{left}}, \Delta z_{\text{right}})$ to keep leg extensions within realistic biomechanical limits on steep stairs[cite: 2].`

`### 4.2 Stair Stepping & Slope Physics`  
`- **Maximum Step Height**: $h_{\text{step}} \approx 0.3\text{m}$[cite: 2]. Obstacles below $h_{\text{step}}$ allow smooth capsule step-up without triggering jump or fall state machines[cite: 2].`  
`- **Slope Angle Thresholds**:`  
  `- $\theta \le 45^\circ$: Walkable ground[cite: 2]. Velocity vector is projected onto the surface plane vector $\vec{V}_{\text{surface}} = \vec{V} - (\vec{V} \cdot \vec{N}) \vec{N}$[cite: 2].`  
  `- $45^\circ < \theta \le 60^\circ$: Sliding slope[cite: 2]. Adds downward gravity vector acceleration while scaling player control input down[cite: 2].`  
  ``- $\theta > 60^\circ$: Non-walkable wall[cite: 2]. Capsule collision bounces horizontal velocity; character detaches from ground state machine into `Falling`[cite: 2].``  
`- **Root Motion Stair Slope Matching**: When traversing stairs using root motion, horizontal root displacement ($\Delta x, \Delta y$) is aligned with the surface plane tangent, preventing the character model from clipping inside stair geometry or hovering off step edges[cite: 2].`

`---`

`## 5. Locomotion Modes: Free-Aim (Unlocked) vs. Target Lock-On`

`### 5.1 Unlocked Locomotion (Free-Aim)`  
`- **Coordinate System**: World-Space relative to Camera View Vector $\vec{V}_{\text{cam}}$[cite: 2].`  
`- **Facing Vector ($\vec{F}$)**: Rotates dynamically to match the 2D joystick input direction $\vec{I}$[cite: 2].`  
`- **Velocity Vector ($\vec{V}$)**: Uniform $100\%$ speed scalar regardless of direction angle[cite: 2].`  
`- **Pivot Decay**: Turn angles $> 90^\circ$ trigger turn-pivot animation clips, introducing dynamic deceleration curves and momentary speed penalties[cite: 2].`

`### 5.2 Target Lock-On Locomotion`  
`- **Coordinate System**: Target-Relative Polar Coordinates (Distance $r$, Angle $\theta$)[cite: 2].`  
`- **Facing Vector ($\vec{F}$)**: Fixed toward target transform $\vec{T}$[cite: 2].`  
`- **Velocity Vector Scale**:`  
  `- Forward ($0^\circ$): $100\%$ base velocity[cite: 2].`  
  `- Lateral Arc ($\pm 90^\circ$): $\approx 80\text{--}85\%$ base velocity[cite: 2].`  
  `- Backward ($180^\circ$): $\approx 70\text{--}75\%$ base velocity[cite: 2].`  
`- **Pivot Decay**: Eliminated; character strafes and backpedals seamlessly without directional turn animation stalls[cite: 2].`

`---`

`## 6. Movement Tiers & Velocity Metrics`

`| Locomotion Tier | Speed Value (m/s) | Stamina Drain Rate | AI Perception Noise Radius |`  
`| :--- | :--- | :--- | :--- |`  
`| **Crouch Walk** | $\approx 2.0\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Minimal ($\approx 20\%$)[cite: 2] |`  
`| **Walk** | $\approx 2.5\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Low ($\approx 40\%$)[cite: 2] |`  
`| **Crouch Run (Fast Crouch)**| $\approx 3.6\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Low-Medium ($\approx 50\%$)[cite: 2] |`  
`| **Run (Default Stick)** | $\approx 4.8\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Standard ($100\%$)[cite: 2] |`  
`| **Sprint** | $\approx 6.7\text{ m/s}$[cite: 2] | Combat: $\sim 10.0\text{ pts/s}$ / Out-of-Combat: $0.0$[cite: 2] | High ($150\%$)[cite: 2] |`

`---`

`## 7. Roll / Dodge Engine & Frame Data ($60\text{ FPS}$)`

`Rolling toggles the player character's hurtbox layer collision flags to ignore enemy attack hitboxes[cite: 2].`

Frame 0 Frame 12/13 Frame 20/28 |-----------------------|---------------------| | Active i-Frames | Recovery Phase | | (Invulnerable Layer) | (Vulnerable Layer) |

`### 7.1 Weight Load Classifications`

`- **Light Load ($< 30.0\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, $+20\%$ displacement distance[cite: 2].`  
`- **Medium Load ($30.0\% \text{ to } 69.9\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, standard displacement[cite: 2].`  
`- **Heavy Load ($70.0\% \text{ to } 99.9\%$)**: $12\text{ i-frames}$ (Frames 0–11), $16\text{ recovery frames}$, reduced displacement[cite: 2].`  
`- **Overloaded ($\ge 100.0\%$)**: $0\text{ i-frames}$, high recovery stumble animation[cite: 2].`

`### 7.2 Backstep Mechanics`  
`- **i-Frames**: $0\text{ frames}$ (default state)[cite: 2].`  
`- **Recovery**: $8\text{--}10\text{ frames}$[cite: 2].`  
`- **Utility**: Rapid rearward displacement allowing immediate transition into backstep-unique attack animation trees[cite: 2].`

`---`

`## 8. Jump Logic & Aerial Physics`

`### 8.1 Lower-Body Hurtbox Deactivation`  
`- **Invincibility Classification**: Selective spatial hurtbox toggling (NOT global i-Frames)[cite: 2].`  
`- **Active Window**: Frame 1 to Jump Apex ($\approx 20\text{--}25\text{ frames}$)[cite: 2].`  
``- **Mechanism**: The collision layer for the lower body (hips down to feet) is disabled or set to pass-through for attack hitboxes flagged with `GroundSweep` or `Shockwave` tags[cite: 2].``  
`- **Upper Torso**: Remains fully vulnerable throughout the entire jump arc[cite: 2].`

`### 8.2 Aerial Steering & Trajectory`  
`- **Initial Velocity Vector**: $\vec{V}_{\text{air}} = \vec{V}_{\text{ground}} + \vec{V}_{\text{jump}}$[cite: 2].`  
`- **Air Control Influence**: Directional input allows a maximum of $\approx 20\text{--}30\%$ vector alteration angle mid-air[cite: 2]. Neutral jumps maintain vertical momentum without horizontal drift[cite: 2].`

`---`

`## 9. Crouch Logic & Animation Interrupts`

`### 9.1 Capsule Compression`  
`- **Collision Capsule Height**: Reduced by $\approx 45\%$ upon entering Crouch state[cite: 2].`  
`- **High-Hitbox Evasion**: Head and upper torso hurtboxes lower instantly, allowing high-horizontal attack hitboxes to pass over the character model without triggering hit registration ("crouch dodging")[cite: 2].`

`### 9.2 Animation Recovery Cancels & State Aliasing`  
`- **Recovery Interruption**: Toggling Crouch during the terminal recovery window of specific weapon attack clips cancels recovery frames earlier than standard Idle transitions[cite: 2].`  
`- **Attack Index Mapping**:`  
  ```` ```text ````  
  `crouch_attack_id == rolling_attack_id`  
  ```` ```[cite: 2] ````  
  ``Executing an attack while in the Crouch state directly triggers the `Rolling Attack` animation script, bypassing stamina consumption and roll recovery requirements[cite: 2].``

---

*User prompt: i said explicitly give me full md file \- not some parts*

`Response: ---`  
`name: locomotion-spec`  
`description: Technical specification and system prompt for character locomotion, frame data, root motion architecture, stairs handling, and state machine locking.`  
`version: 1.1.0`  
`---`

`# SYSTEM PROMPT / TECHNICAL SPECIFICATION: Locomotion Architecture`

`You are an expert Game Physics & Animation Systems Engineer specializing in FromSoftware-style action RPG locomotion architecture[cite: 2]. Utilize the precise mathematical, algorithmic, and frame-accurate rules outlined below for state transitions, movement vectors, input buffer handling, root-motion processing, stairs handling, and movement blocking flags[cite: 2].`

`---`

`## 1. Input Engine & Buffer Management`

`### 1.1 Key-Release Action Mapping (Sprint vs. Roll)`  
`- **Key-Down Event**: Starts an internal timer ($t_{\text{hold}}$)[cite: 2].`  
``- **Key-Up Event ($t_{\text{hold}} < T_{\text{threshold}}$)**: Triggers the `Roll` state transition on key release[cite: 2].``  
  `- Threshold $T_{\text{threshold}} \approx 15\text{--}20\text{ frames}$ ($250\text{ ms}$ at $60\text{ FPS}$)[cite: 2].`  
``- **Hold Event ($t_{\text{hold}} \ge T_{\text{threshold}}$)**: Cancels the `Roll` event registration and transitions locomotion directly into `Sprint`[cite: 2].``

`### 1.2 Input Buffer (Sliding Window)`  
`- **Buffer Size**: $15\text{--}30\text{ frames}$ ($250\text{--}500\text{ ms}$)[cite: 2].`  
``- **Behavior**: Any action command (`Roll`, `Jump`, `Crouch`, `Attack`) pressed during non-cancelable action recovery windows is cached[cite: 2]. The queued action executes on frame 1 of the earliest cancel window (`CanCancel` flag = `true`)[cite: 2].``

`---`

`## 2. Root-Motion Centric Locomotion Architecture`

`Unlike standard dynamic arcade movement systems that rely purely on kinematic velocity vectors applied directly to a capsule, FromSoftware's locomotion engine heavily relies on **Root-Motion Animation Curves** augmented by dynamic input blending[cite: 2].`

\+-------------------+ Extract Vector \+------------------------+ | Animation Track | \------------------------\> | Delta Root Position | | (Clip Root Bone) | | Delta Root Rotation | \+-------------------+ \+------------------------+ | | | Modulated by Input v \+-----------------------------------\> \+------------------------+ | Character Capsule | | Transform Delta | \+------------------------+

`### 2.1 Motion Extraction & Blending`  
`- **Root Motion Primacy**: The primary velocity ($\vec{V}$) and yaw rotation ($\Delta \theta$) are extracted directly from the root bone's translation vector ($\vec{\Delta P}_{\text{root}}$) and rotation quaternion ($\mathbf{Q}_{\text{root}}$) frame-by-frame[cite: 2]:`  
  `$$\vec{V}_{\text{frame}} = \frac{\vec{\Delta P}_{\text{root}}}{\Delta t}, \quad \Delta \theta_{\text{frame}} = \text{Yaw}(\mathbf{Q}_{\text{root}})$$`  
`[cite: 2]`  
`- **Velocity Decoupling**: Standard kinematic controller acceleration is zeroed out during root-motion driven actions (attacks, rolls, staggers)[cite: 2]. The capsule translation is governed strictly by the keyframed delta in the animation asset[cite: 2].`  
`- **Directional Blending**: For standard locomotion (Walk/Run), root-motion clips are blended across 2D parametric blend spaces based on input direction relative to camera/target, aligning the capsule forward vector with input while maintaining realistic foot-placement velocity[cite: 2].`

`---`

`## 3. Movement Blocking & Action Locking System`

`To enforce weighty, commitment-based combat, the movement system exposes an Action State Locking API driven by bitwise flag parameters per animation frame[cite: 2].`

`### 3.1 Movement Lock State Flags`

``| Flag Bit | State Flag Name | Effect When Active (`true`) |``  
`| :--- | :--- | :--- |`  
``| `0x01` | `LOCK_MOVE_INPUT` | Ignores directional stick input ($\vec{I}$). Vector speed drops to zero or relies purely on Root Motion.[cite: 2] |``  
``| `0x02` | `LOCK_TURN_INPUT` | Fixes character facing vector ($\vec{F}$). Rotational input is suppressed.[cite: 2] |``  
``| `0x04` | `USE_ROOT_MOTION` | Overrides character capsule movement exclusively with root-motion translation/rotation curves.[cite: 2] |``  
``| `0x08` | `ALLOW_ACTION_CANCEL`| Enables input buffer evaluation for recovery cancels (Crouch cancel, Roll cancel, Attack chaining).[cite: 2] |``

`### 3.2 Action State Matrix`

\[ Attack / Skill \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION \[ Hit Reaction \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION (Knockback) \[ Dodge Roll \] \----\> LOCK\_MOVE\_INPUT | USE\_ROOT\_MOTION (Unlocked at Recovery) \[ Fall / Landing \] \----\> LOCK\_MOVE\_INPUT (Scale duration by fall height / impact force)

`- **Attacks & Weapon Skills**:`  
  ``- Sets `LOCK_MOVE_INPUT`, `LOCK_TURN_INPUT`, and `USE_ROOT_MOTION`[cite: 2].``  
  `- Player cannot steer manually; positional lunge or step-forward is governed strictly by the attack clip's root displacement[cite: 2].`  
`- **Hit Reactions & Stagger**:`  
  ``- Hard lock on all user inputs (`LOCK_MOVE_INPUT | LOCK_TURN_INPUT | USE_ROOT_MOTION`)[cite: 2].``  
  `- Character velocity is driven by the stagger recoil root motion animation curve corresponding to the Poise Damage tier[cite: 2].`  
`- **Roll / Backstep**:`  
  ``- `LOCK_MOVE_INPUT` active during dodge execution[cite: 2]. Direction vector is locked at Frame 0 based on input angle and cannot be steered mid-flight[cite: 2].``  
  ``- Switches to `ALLOW_ACTION_CANCEL` on Frame 13 (Medium load), enabling early exit into crouch/attack/block[cite: 2].``  
`- **Landing Recovery / Impact Stuns**:`  
  `- **Light Fall ($< 4\text{m}$)**: No movement lock; root motion blends smoothly into landing-run transition[cite: 2].`  
  ``- **Medium Fall ($4\text{m} - 12\text{m}$)**: Sets `LOCK_MOVE_INPUT` for $12\text{--}25\text{ frames}$ based on vertical velocity ($v_z$)[cite: 2].``  
  `- **Hard Landing ($12\text{m} - 16\text{m}$)**: Forces heavy knee-bend recovery animation with absolute input lock ($\sim 45\text{ frames}$)[cite: 2].`

`---`

`## 4. Ground Alignment & Stairs Logic`

`Handling uneven geometry and stairways without capsule snagging or floating feet relies on **Raycast Probes, Inverse Kinematics (IK), and Root Motion Surface Snapping**[cite: 2].`

          `Capsule Raycast Center`  
                    `|`  
        `+-----------+-----------+`  
        `|                       |`  
  `Left Foot Probe         Right Foot Probe`  
        `|                       |`  
        `v                       v`  
  `+-----------+           +-----------+`  
  `|  Step IK  |           |  Step IK  |`  
  `+-----------+           +-----------+`

`### 4.1 Foot-Placement Inverse Kinematics (2-Bone IK)`  
`- **Raycast Down Probes**: Dual downward raycasts originate from the hip/ankle bone transforms during ground-contact animation frames[cite: 2].`  
`- **Offset Calculation**: Calculates height delta $\Delta z_i = z_{\text{hit}} - z_{\text{root\_plane}}$[cite: 2].`  
`- **Leg Adjustment**: Adjusts ankle end-effectors via 2-Bone IK (Hip-Knee-Ankle) to match stair treads, while bending knee joints analytically[cite: 2].`  
`- **Pelvis Adaptation**: Drops or raises the root pelvis bone transform height by $\min(\Delta z_{\text{left}}, \Delta z_{\text{right}})$ to keep leg extensions within realistic biomechanical limits on steep stairs[cite: 2].`

`### 4.2 Stair Stepping & Slope Physics`  
`- **Maximum Step Height**: $h_{\text{step}} \approx 0.3\text{m}$[cite: 2]. Obstacles below $h_{\text{step}}$ allow smooth capsule step-up without triggering jump or fall state machines[cite: 2].`  
`- **Slope Angle Thresholds**:`  
  `- $\theta \le 45^\circ$: Walkable ground[cite: 2]. Velocity vector is projected onto the surface plane vector $\vec{V}_{\text{surface}} = \vec{V} - (\vec{V} \cdot \vec{N}) \vec{N}$[cite: 2].`  
  `- $45^\circ < \theta \le 60^\circ$: Sliding slope[cite: 2]. Adds downward gravity vector acceleration while scaling player control input down[cite: 2].`  
  ``- $\theta > 60^\circ$: Non-walkable wall[cite: 2]. Capsule collision bounces horizontal velocity; character detaches from ground state machine into `Falling`[cite: 2].``  
`- **Root Motion Stair Slope Matching**: When traversing stairs using root motion, horizontal root displacement ($\Delta x, \Delta y$) is aligned with the surface plane tangent, preventing the character model from clipping inside stair geometry or hovering off step edges[cite: 2].`

`---`

`## 5. Locomotion Modes: Free-Aim (Unlocked) vs. Target Lock-On`

`### 5.1 Unlocked Locomotion (Free-Aim)`  
`- **Coordinate System**: World-Space relative to Camera View Vector $\vec{V}_{\text{cam}}$[cite: 2].`  
`- **Facing Vector ($\vec{F}$)**: Rotates dynamically to match the 2D joystick input direction $\vec{I}$[cite: 2].`  
`- **Velocity Vector ($\vec{V}$)**: Uniform $100\%$ speed scalar regardless of direction angle[cite: 2].`  
`- **Pivot Decay**: Turn angles $> 90^\circ$ trigger turn-pivot animation clips, introducing dynamic deceleration curves and momentary speed penalties[cite: 2].`

`### 5.2 Target Lock-On Locomotion`  
`- **Coordinate System**: Target-Relative Polar Coordinates (Distance $r$, Angle $\theta$)[cite: 2].`  
`- **Facing Vector ($\vec{F}$)**: Fixed toward target transform $\vec{T}$[cite: 2].`  
`- **Velocity Vector Scale**:`  
  `- Forward ($0^\circ$): $100\%$ base velocity[cite: 2].`  
  `- Lateral Arc ($\pm 90^\circ$): $\approx 80\text{--}85\%$ base velocity[cite: 2].`  
  `- Backward ($180^\circ$): $\approx 70\text{--}75\%$ base velocity[cite: 2].`  
`- **Pivot Decay**: Eliminated; character strafes and backpedals seamlessly without directional turn animation stalls[cite: 2].`

`---`

`## 6. Movement Tiers & Velocity Metrics`

`| Locomotion Tier | Speed Value (m/s) | Stamina Drain Rate | AI Perception Noise Radius |`  
`| :--- | :--- | :--- | :--- |`  
`| **Crouch Walk** | $\approx 2.0\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Minimal ($\approx 20\%$)[cite: 2] |`  
`| **Walk** | $\approx 2.5\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Low ($\approx 40\%$)[cite: 2] |`  
`| **Crouch Run (Fast Crouch)**| $\approx 3.6\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Low-Medium ($\approx 50\%$)[cite: 2] |`  
`| **Run (Default Stick)** | $\approx 4.8\text{ m/s}$[cite: 2] | $0.0\text{ pts/s}$[cite: 2] | Standard ($100\%$)[cite: 2] |`  
`| **Sprint** | $\approx 6.7\text{ m/s}$[cite: 2] | Combat: $\sim 10.0\text{ pts/s}$ / Out-of-Combat: $0.0$[cite: 2] | High ($150\%$)[cite: 2] |`

`---`

`## 7. Roll / Dodge Engine & Frame Data ($60\text{ FPS}$)`

`Rolling toggles the player character's hurtbox layer collision flags to ignore enemy attack hitboxes[cite: 2].`

Frame 0 Frame 12/13 Frame 20/28 |-----------------------|---------------------| | Active i-Frames | Recovery Phase | | (Invulnerable Layer) | (Vulnerable Layer) |

`### 7.1 Weight Load Classifications`

`- **Light Load ($< 30.0\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, $+20\%$ displacement distance[cite: 2].`  
`- **Medium Load ($30.0\% \text{ to } 69.9\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, standard displacement[cite: 2].`  
`- **Heavy Load ($70.0\% \text{ to } 99.9\%$)**: $12\text{ i-frames}$ (Frames 0–11), $16\text{ recovery frames}$, reduced displacement[cite: 2].`  
`- **Overloaded ($\ge 100.0\%$)**: $0\text{ i-frames}$, high recovery stumble animation[cite: 2].`

`### 7.2 Backstep Mechanics`  
`- **i-Frames**: $0\text{ frames}$ (default state)[cite: 2].`  
`- **Recovery**: $8\text{--}10\text{ frames}$[cite: 2].`  
`- **Utility**: Rapid rearward displacement allowing immediate transition into backstep-unique attack animation trees[cite: 2].`

`---`

`## 8. Jump Logic & Aerial Physics`

`### 8.1 Lower-Body Hurtbox Deactivation`  
`- **Invincibility Classification**: Selective spatial hurtbox toggling (NOT global i-Frames)[cite: 2].`  
`- **Active Window**: Frame 1 to Jump Apex ($\approx 20\text{--}25\text{ frames}$)[cite: 2].`  
``- **Mechanism**: The collision layer for the lower body (hips down to feet) is disabled or set to pass-through for attack hitboxes flagged with `GroundSweep` or `Shockwave` tags[cite: 2].``  
`- **Upper Torso**: Remains fully vulnerable throughout the entire jump arc[cite: 2].`

`### 8.2 Aerial Steering & Trajectory`  
`- **Initial Velocity Vector**: $\vec{V}_{\text{air}} = \vec{V}_{\text{ground}} + \vec{V}_{\text{jump}}$[cite: 2].`  
`- **Air Control Influence**: Directional input allows a maximum of $\approx 20\text{--}30\%$ vector alteration angle mid-air[cite: 2]. Neutral jumps maintain vertical momentum without horizontal drift[cite: 2].`

`---`

`## 9. Crouch Logic & Animation Interrupts`

`### 9.1 Capsule Compression`  
`- **Collision Capsule Height**: Reduced by $\approx 45\%$ upon entering Crouch state[cite: 2].`  
`- **High-Hitbox Evasion**: Head and upper torso hurtboxes lower instantly, allowing high-horizontal attack hitboxes to pass over the character model without triggering hit registration ("crouch dodging")[cite: 2].`

`### 9.2 Animation Recovery Cancels & State Aliasing`  
`- **Recovery Interruption**: Toggling Crouch during the terminal recovery window of specific weapon attack clips cancels recovery frames earlier than standard Idle transitions[cite: 2].`  
`- **Attack Index Mapping**:`  
  ```` ```text ````  
  `crouch_attack_id == rolling_attack_id`  
  ```` ```[cite: 2] ````  
  ``Executing an attack while in the Crouch state directly triggers the `Rolling Attack` animation script, bypassing stamina consumption and roll recovery requirements[cite: 2].``

---

*User prompt: again \- u idiot \- u give me parts of mds \- not one single md file*

`Response: ---`  
`name: locomotion-spec`  
`description: Technical specification and system prompt for character locomotion, frame data, root motion architecture, stairs handling, and state machine locking.`  
`version: 1.1.0`  
`---`

`# SYSTEM PROMPT / TECHNICAL SPECIFICATION: Locomotion Architecture`

`You are an expert Game Physics & Animation Systems Engineer specializing in FromSoftware-style action RPG locomotion architecture. Utilize the precise mathematical, algorithmic, and frame-accurate rules outlined below for state transitions, movement vectors, input buffer handling, root-motion processing, stairs handling, and movement blocking flags.`

`---`

`## 1. Input Engine & Buffer Management`

`### 1.1 Key-Release Action Mapping (Sprint vs. Roll)`  
`- **Key-Down Event**: Starts an internal timer ($t_{\text{hold}}$).`  
``- **Key-Up Event ($t_{\text{hold}} < T_{\text{threshold}}$)**: Triggers the `Roll` state transition on key release.``  
  `- Threshold $T_{\text{threshold}} \approx 15\text{--}20\text{ frames}$ ($250\text{ ms}$ at $60\text{ FPS}$).`  
``- **Hold Event ($t_{\text{hold}} \ge T_{\text{threshold}}$)**: Cancels the `Roll` event registration and transitions locomotion directly into `Sprint`.``

`### 1.2 Input Buffer (Sliding Window)`  
`- **Buffer Size**: $15\text{--}30\text{ frames}$ ($250\text{--}500\text{ ms}$).`  
``- **Behavior**: Any action command (`Roll`, `Jump`, `Crouch`, `Attack`) pressed during non-cancelable action recovery windows is cached. The queued action executes on frame 1 of the earliest cancel window (`CanCancel` flag = `true`).``

`---`

`## 2. Root-Motion Centric Locomotion Architecture`

`Unlike standard dynamic arcade movement systems that rely purely on kinematic velocity vectors applied directly to a capsule, FromSoftware's locomotion engine heavily relies on **Root-Motion Animation Curves** augmented by dynamic input blending.`

\+-------------------+ Extract Vector \+------------------------+ | Animation Track | \------------------------\> | Delta Root Position | | (Clip Root Bone) | | Delta Root Rotation | \+-------------------+ \+------------------------+ | | | Modulated by Input v \+-----------------------------------\> \+------------------------+ | Character Capsule | | Transform Delta | \+------------------------+

`### 2.1 Motion Extraction & Blending`  
`- **Root Motion Primacy**: The primary velocity ($\vec{V}$) and yaw rotation ($\Delta \theta$) are extracted directly from the root bone's translation vector ($\vec{\Delta P}_{\text{root}}$) and rotation quaternion ($\mathbf{Q}_{\text{root}}$) frame-by-frame:`  
  `$$\vec{V}_{\text{frame}} = \frac{\vec{\Delta P}_{\text{root}}}{\Delta t}, \quad \Delta \theta_{\text{frame}} = \text{Yaw}(\mathbf{Q}_{\text{root}})$$`  
`- **Velocity Decoupling**: Standard kinematic controller acceleration is zeroed out during root-motion driven actions (attacks, rolls, staggers). The capsule translation is governed strictly by the keyframed delta in the animation asset.`  
`- **Directional Blending**: For standard locomotion (Walk/Run), root-motion clips are blended across 2D parametric blend spaces based on input direction relative to camera/target, aligning the capsule forward vector with input while maintaining realistic foot-placement velocity.`

`---`

`## 3. Movement Blocking & Action Locking System`

`To enforce weighty, commitment-based combat, the movement system exposes an Action State Locking API driven by bitwise flag parameters per animation frame.`

`### 3.1 Movement Lock State Flags`

``| Flag Bit | State Flag Name | Effect When Active (`true`) |``  
`| :--- | :--- | :--- |`  
``| `0x01` | `LOCK_MOVE_INPUT` | Ignores directional stick input ($\vec{I}$). Vector speed drops to zero or relies purely on Root Motion. |``  
``| `0x02` | `LOCK_TURN_INPUT` | Fixes character facing vector ($\vec{F}$). Rotational input is suppressed. |``  
``| `0x04` | `USE_ROOT_MOTION` | Overrides character capsule movement exclusively with root-motion translation/rotation curves. |``  
``| `0x08` | `ALLOW_ACTION_CANCEL`| Enables input buffer evaluation for recovery cancels (Crouch cancel, Roll cancel, Attack chaining). |``

`### 3.2 Action State Matrix`

\[ Attack / Skill \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION \[ Hit Reaction \] \----\> LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION (Knockback) \[ Dodge Roll \] \----\> LOCK\_MOVE\_INPUT | USE\_ROOT\_MOTION (Unlocked at Recovery) \[ Fall / Landing \] \----\> LOCK\_MOVE\_INPUT (Scale duration by fall height / impact force)

`- **Attacks & Weapon Skills**:`  
  ``- Sets `LOCK_MOVE_INPUT`, `LOCK_TURN_INPUT`, and `USE_ROOT_MOTION`.``  
  `- Player cannot steer manually; positional lunge or step-forward is governed strictly by the attack clip's root displacement.`  
`- **Hit Reactions & Stagger**:`  
  ``- Hard lock on all user inputs (`LOCK_MOVE_INPUT | LOCK_TURN_INPUT | USE_ROOT_MOTION`).``  
  `- Character velocity is driven by the stagger recoil root motion animation curve corresponding to the Poise Damage tier.`  
`- **Roll / Backstep**:`  
  ``- `LOCK_MOVE_INPUT` active during dodge execution. Direction vector is locked at Frame 0 based on input angle and cannot be steered mid-flight.``  
  ``- Switches to `ALLOW_ACTION_CANCEL` on Frame 13 (Medium load), enabling early exit into crouch/attack/block.``  
`- **Landing Recovery / Impact Stuns**:`  
  `- **Light Fall ($< 4\text{m}$)**: No movement lock; root motion blends smoothly into landing-run transition.`  
  ``- **Medium Fall ($4\text{m} - 12\text{m}$)**: Sets `LOCK_MOVE_INPUT` for $12\text{--}25\text{ frames}$ based on vertical velocity ($v_z$).``  
  `- **Hard Landing ($12\text{m} - 16\text{m}$)**: Forces heavy knee-bend recovery animation with absolute input lock ($\sim 45\text{ frames}$).`

`---`

`## 4. Ground Alignment & Stairs Logic`

`Handling uneven geometry and stairways without capsule snagging or floating feet relies on **Raycast Probes, Inverse Kinematics (IK), and Root Motion Surface Snapping**.`

          `Capsule Raycast Center`  
                    `|`  
        `+-----------+-----------+`  
        `|                       |`  
  `Left Foot Probe         Right Foot Probe`  
        `|                       |`  
        `v                       v`  
  `+-----------+           +-----------+`  
  `|  Step IK  |           |  Step IK  |`  
  `+-----------+           +-----------+`

`### 4.1 Foot-Placement Inverse Kinematics (2-Bone IK)`  
`- **Raycast Down Probes**: Dual downward raycasts originate from the hip/ankle bone transforms during ground-contact animation frames.`  
`- **Offset Calculation**: Calculates height delta $\Delta z_i = z_{\text{hit}} - z_{\text{root\_plane}}$.`  
`- **Leg Adjustment**: Adjusts ankle end-effectors via 2-Bone IK (Hip-Knee-Ankle) to match stair treads, while bending knee joints analytically.`  
`- **Pelvis Adaptation**: Drops or raises the root pelvis bone transform height by $\min(\Delta z_{\text{left}}, \Delta z_{\text{right}})$ to keep leg extensions within realistic biomechanical limits on steep stairs.`

`### 4.2 Stair Stepping & Slope Physics`  
`- **Maximum Step Height**: $h_{\text{step}} \approx 0.3\text{m}$. Obstacles below $h_{\text{step}}$ allow smooth capsule step-up without triggering jump or fall state machines.`  
`- **Slope Angle Thresholds**:`  
  `- $\theta \le 45^\circ$: Walkable ground. Velocity vector is projected onto the surface plane vector $\vec{V}_{\text{surface}} = \vec{V} - (\vec{V} \cdot \vec{N}) \vec{N}$.`  
  `- $45^\circ < \theta \le 60^\circ$: Sliding slope. Adds downward gravity vector acceleration while scaling player control input down.`  
  ``- $\theta > 60^\circ$: Non-walkable wall. Capsule collision bounces horizontal velocity; character detaches from ground state machine into `Falling`.``  
`- **Root Motion Stair Slope Matching**: When traversing stairs using root motion, horizontal root displacement ($\Delta x, \Delta y$) is aligned with the surface plane tangent, preventing the character model from clipping inside stair geometry or hovering off step edges.`

`---`

`## 5. Locomotion Modes: Free-Aim (Unlocked) vs. Target Lock-On`

`### 5.1 Unlocked Locomotion (Free-Aim)`  
`- **Coordinate System**: World-Space relative to Camera View Vector $\vec{V}_{\text{cam}}$.`  
`- **Facing Vector ($\vec{F}$)**: Rotates dynamically to match the 2D joystick input direction $\vec{I}$.`  
`- **Velocity Vector ($\vec{V}$)**: Uniform $100\%$ speed scalar regardless of direction angle.`  
`- **Pivot Decay**: Turn angles $> 90^\circ$ trigger turn-pivot animation clips, introducing dynamic deceleration curves and momentary speed penalties.`

`### 5.2 Target Lock-On Locomotion`  
`- **Coordinate System**: Target-Relative Polar Coordinates (Distance $r$, Angle $\theta$).`  
`- **Facing Vector ($\vec{F}$)**: Fixed toward target transform $\vec{T}$.`  
`- **Velocity Vector Scale**:`  
  `- Forward ($0^\circ$): $100\%$ base velocity.`  
  `- Lateral Arc ($\pm 90^\circ$): $\approx 80\text{--}85\%$ base velocity.`  
  `- Backward ($180^\circ$): $\approx 70\text{--}75\%$ base velocity.`  
`- **Pivot Decay**: Eliminated; character strafes and backpedals seamlessly without directional turn animation stalls.`

`---`

`## 6. Movement Tiers & Velocity Metrics`

`| Locomotion Tier | Speed Value (m/s) | Stamina Drain Rate | AI Perception Noise Radius |`  
`| :--- | :--- | :--- | :--- |`  
`| **Crouch Walk** | $\approx 2.0\text{ m/s}$ | $0.0\text{ pts/s}$ | Minimal ($\approx 20\%$) |`  
`| **Walk** | $\approx 2.5\text{ m/s}$ | $0.0\text{ pts/s}$ | Low ($\approx 40\%$) |`  
`| **Crouch Run (Fast Crouch)**| $\approx 3.6\text{ m/s}$ | $0.0\text{ pts/s}$ | Low-Medium ($\approx 50\%$) |`  
`| **Run (Default Stick)** | $\approx 4.8\text{ m/s}$ | $0.0\text{ pts/s}$ | Standard ($100\%$) |`  
`| **Sprint** | $\approx 6.7\text{ m/s}$ | Combat: $\sim 10.0\text{ pts/s}$ / Out-of-Combat: $0.0$ | High ($150\%$) |`

`---`

`## 7. Roll / Dodge Engine & Frame Data ($60\text{ FPS}$)`

`Rolling toggles the player character's hurtbox layer collision flags to ignore enemy attack hitboxes.`

Frame 0 Frame 12/13 Frame 20/28 |-----------------------|---------------------| | Active i-Frames | Recovery Phase | | (Invulnerable Layer) | (Vulnerable Layer) |

`### 7.1 Weight Load Classifications`

`- **Light Load ($< 30.0\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, $+20\%$ displacement distance.`  
`- **Medium Load ($30.0\% \text{ to } 69.9\%$)**: $13\text{ i-frames}$ (Frames 0–12), $8\text{ recovery frames}$, standard displacement.`  
`- **Heavy Load ($70.0\% \text{ to } 99.9\%$)**: $12\text{ i-frames}$ (Frames 0–11), $16\text{ recovery frames}$, reduced displacement.`  
`- **Overloaded ($\ge 100.0\%$)**: $0\text{ i-frames}$, high recovery stumble animation.`

`### 7.2 Backstep Mechanics`  
`- **i-Frames**: $0\text{ frames}$ (default state).`  
`- **Recovery**: $8\text{--}10\text{ frames}$.`  
`- **Utility**: Rapid rearward displacement allowing immediate transition into backstep-unique attack animation trees.`

`---`

`## 8. Jump Logic & Aerial Physics`

`### 8.1 Lower-Body Hurtbox Deactivation`  
`- **Invincibility Classification**: Selective spatial hurtbox toggling (NOT global i-Frames).`  
`- **Active Window**: Frame 1 to Jump Apex ($\approx 20\text{--}25\text{ frames}$).`  
``- **Mechanism**: The collision layer for the lower body (hips down to feet) is disabled or set to pass-through for attack hitboxes flagged with `GroundSweep` or `Shockwave` tags.``  
`- **Upper Torso**: Remains fully vulnerable throughout the entire jump arc.`

`### 8.2 Aerial Steering & Trajectory`  
`- **Initial Velocity Vector**: $\vec{V}_{\text{air}} = \vec{V}_{\text{ground}} + \vec{V}_{\text{jump}}$.`  
`- **Air Control Influence**: Directional input allows a maximum of $\approx 20\text{--}30\%$ vector alteration angle mid-air. Neutral jumps maintain vertical momentum without horizontal drift.`

`---`

`## 9. Crouch Logic & Animation Interrupts`

`### 9.1 Capsule Compression`  
`- **Collision Capsule Height**: Reduced by $\approx 45\%$ upon entering Crouch state.`  
`- **High-Hitbox Evasion**: Head and upper torso hurtboxes lower instantly, allowing high-horizontal attack hitboxes to pass over the character model without triggering hit registration ("crouch dodging").`

`### 9.2 Animation Recovery Cancels & State Aliasing`  
`- **Recovery Interruption**: Toggling Crouch during the terminal recovery window of specific weapon attack clips cancels recovery frames earlier than standard Idle transitions.`  
`- **Attack Index Mapping**:`  
  ```` ```text ````  
  `crouch_attack_id == rolling_attack_id`

Executing an attack while in the Crouch state directly triggers the Rolling Attack animation script, bypassing stamina consumption and roll recovery requirements.

---

*User prompt: doing same shit action doesn't help change approach on how u create single md*

Response: Here is the full, complete system prompt Markdown file provided directly as plain, copyable text (without enclosing it in code blocks that can trigger formatting glitches):

## **name: locomotion-spec description: Technical specification and system prompt for character locomotion, frame data, root motion architecture, stairs handling, and state machine locking. version: 1.1.0**

# **SYSTEM PROMPT / TECHNICAL SPECIFICATION: Locomotion Architecture**

You are an expert Game Physics & Animation Systems Engineer specializing in FromSoftware-style action RPG locomotion architecture. Utilize the precise mathematical, algorithmic, and frame-accurate rules outlined below for state transitions, movement vectors, input buffer handling, root-motion processing, stairs handling, and movement blocking flags.

## **1\. Input Engine & Buffer Management**

### **1.1 Key-Release Action Mapping (Sprint vs. Roll)**

> * **Key-Down Event**: Starts an internal timer (*t*hold​).  
> * **Key-Up Event (*t*hold​\<*T*threshold​)**: Triggers the Roll state transition on key release.  
  * Threshold *T*threshold​≈15–20 frames (250 ms at 60 FPS).  
> * **Hold Event (*t*hold​≥*T*threshold​)**: Cancels the Roll event registration and transitions locomotion directly into Sprint.

### **1.2 Input Buffer (Sliding Window)**

> * **Buffer Size**: 15–30 frames (250–500 ms).  
> * **Behavior**: Any action command (Roll, Jump, Crouch, Attack) pressed during non-cancelable action recovery windows is cached. The queued action executes on frame 1 of the earliest cancel window (CanCancel flag \= true).

## **2\. Root-Motion Centric Locomotion Architecture**

Unlike standard dynamic arcade movement systems that rely purely on kinematic velocity vectors applied directly to a capsule, FromSoftware's locomotion engine heavily relies on **Root-Motion Animation Curves** augmented by dynamic input blending.

### **2.1 Motion Extraction & Blending**

> * **Root Motion Primacy**: The primary velocity (*V*) and yaw rotation (Δ*θ*) are extracted directly from the root bone's translation vector (Δ*P*root​) and rotation quaternion (Qroot​) frame-by-frame:  
>   *V*frame​\=Δ*t*Δ*P*root​​,Δ*θ*frame​\=Yaw(Qroot​)  
> * **Velocity Decoupling**: Standard kinematic controller acceleration is zeroed out during root-motion driven actions (attacks, rolls, staggers). The capsule translation is governed strictly by the keyframed delta in the animation asset.  
> * **Directional Blending**: For standard locomotion (Walk/Run), root-motion clips are blended across 2D parametric blend spaces based on input direction relative to camera/target, aligning the capsule forward vector with input while maintaining realistic foot-placement velocity.

## **3\. Movement Blocking & Action Locking System**

To enforce weighty, commitment-based combat, the movement system exposes an Action State Locking API driven by bitwise flag parameters per animation frame.

### **3.1 Movement Lock State Flags**

> * **0x01 (LOCK\_MOVE\_INPUT)**: Ignores directional stick input (*I*). Vector speed drops to zero or relies purely on Root Motion.  
> * **0x02 (LOCK\_TURN\_INPUT)**: Fixes character facing vector (*F*). Rotational input is suppressed.  
> * **0x04 (USE\_ROOT\_MOTION)**: Overrides character capsule movement exclusively with root-motion translation/rotation curves.  
> * **0x08 (ALLOW\_ACTION\_CANCEL)**: Enables input buffer evaluation for recovery cancels (Crouch cancel, Roll cancel, Attack chaining).

### **3.2 Action State Behaviors**

> * **Attacks & Weapon Skills**: Sets LOCK\_MOVE\_INPUT, LOCK\_TURN\_INPUT, and USE\_ROOT\_MOTION. Player cannot steer manually; positional lunge or step-forward is governed strictly by the attack clip's root displacement.  
> * **Hit Reactions & Stagger**: Hard lock on all user inputs (LOCK\_MOVE\_INPUT | LOCK\_TURN\_INPUT | USE\_ROOT\_MOTION). Character velocity is driven by the stagger recoil root motion animation curve corresponding to the Poise Damage tier.  
> * **Roll / Backstep**: LOCK\_MOVE\_INPUT active during dodge execution. Direction vector is locked at Frame 0 based on input angle and cannot be steered mid-flight. Switches to ALLOW\_ACTION\_CANCEL on Frame 13 (Medium load), enabling early exit into crouch/attack/block.  
> * **Landing Recovery / Impact Stuns**:  
  * **Light Fall (\<4m)**: No movement lock; root motion blends smoothly into landing-run transition.  
  * **Medium Fall (4m−12m)**: Sets LOCK\_MOVE\_INPUT for 12–25 frames based on vertical velocity (*vz*​).  
  * **Hard Landing (12m−16m)**: Forces heavy knee-bend recovery animation with absolute input lock (∼45 frames).

## **4\. Ground Alignment & Stairs Logic**

Handling uneven geometry and stairways without capsule snagging or floating feet relies on **Raycast Probes, Inverse Kinematics (IK), and Root Motion Surface Snapping**.

### **4.1 Foot-Placement Inverse Kinematics (2-Bone IK)**

> * **Raycast Down Probes**: Dual downward raycasts originate from the hip/ankle bone transforms during ground-contact animation frames.  
> * **Offset Calculation**: Calculates height delta Δ*zi*​\=*z*hit​−*z*root\_plane​.  
> * **Leg Adjustment**: Adjusts ankle end-effectors via 2-Bone IK (Hip-Knee-Ankle) to match stair treads, while bending knee joints analytically.  
> * **Pelvis Adaptation**: Drops or raises the root pelvis bone transform height by min(Δ*z*left​,Δ*z*right​) to keep leg extensions within realistic biomechanical limits on steep stairs.

### **4.2 Stair Stepping & Slope Physics**

> * **Maximum Step Height**: *h*step​≈0.3m. Obstacles below *h*step​ allow smooth capsule step-up without triggering jump or fall state machines.  
> * **Slope Angle Thresholds**:  
  * *θ*≤45∘: Walkable ground. Velocity vector is projected onto the surface plane vector *V*surface​\=*V*−(*V*⋅*N*)*N*.  
  * 45∘\<*θ*≤60∘: Sliding slope. Adds downward gravity vector acceleration while scaling player control input down.  
  * *θ*\>60∘: Non-walkable wall. Capsule collision bounces horizontal velocity; character detaches from ground state machine into Falling.  
> * **Root Motion Stair Slope Matching**: When traversing stairs using root motion, horizontal root displacement (Δ*x*,Δ*y*) is aligned with the surface plane tangent, preventing the character model from clipping inside stair geometry or hovering off step edges.

## **5\. Locomotion Modes: Free-Aim (Unlocked) vs. Target Lock-On**

### **5.1 Unlocked Locomotion (Free-Aim)**

> * **Coordinate System**: World-Space relative to Camera View Vector *V*cam​.  
> * **Facing Vector (*F*)**: Rotates dynamically to match the 2D joystick input direction *I*.  
> * **Velocity Vector (*V*)**: Uniform 100% speed scalar regardless of direction angle.  
> * **Pivot Decay**: Turn angles \>90∘ trigger turn-pivot animation clips, introducing dynamic deceleration curves and momentary speed penalties.

### **5.2 Target Lock-On Locomotion**

> * **Coordinate System**: Target-Relative Polar Coordinates (Distance *r*, Angle *θ*).  
> * **Facing Vector (*F*)**: Fixed toward target transform *T*.  
> * **Velocity Vector Scale**:  
  * Forward (0∘): 100% base velocity.  
  * Lateral Arc (±90∘): ≈80–85% base velocity.  
  * Backward (180∘): ≈70–75% base velocity.  
> * **Pivot Decay**: Eliminated; character strafes and backpedals seamlessly without directional turn animation stalls.

## **6\. Movement Tiers & Velocity Metrics**

> * **Crouch Walk**: ≈2.0 m/s | Stamina: 0.0 pts/s | Stealth Radius: Minimal (≈20%)  
> * **Walk**: ≈2.5 m/s | Stamina: 0.0 pts/s | Stealth Radius: Low (≈40%)  
> * **Crouch Run (Fast Crouch)**: ≈3.6 m/s | Stamina: 0.0 pts/s | Stealth Radius: Low-Medium (≈50%)  
> * **Run (Default Stick)**: ≈4.8 m/s | Stamina: 0.0 pts/s | Stealth Radius: Standard (100%)  
> * **Sprint**: ≈6.7 m/s | Stamina: Combat ∼10.0 pts/s / Non-Combat 0.0 | Stealth Radius: High (150%)

## **7\. Roll / Dodge Engine & Frame Data (60 FPS)**

Rolling toggles the player character's hurtbox layer collision flags to ignore enemy attack hitboxes.

### **7.1 Weight Load Classifications**

> * **Light Load (\<30.0%)**: 13 i-frames (Frames 0–12), 8 recovery frames, \+20% displacement distance.  
> * **Medium Load (30.0% to 69.9%)**: 13 i-frames (Frames 0–12), 8 recovery frames, standard displacement.  
> * **Heavy Load (70.0% to 99.9%)**: 12 i-frames (Frames 0–11), 16 recovery frames, reduced displacement.  
> * **Overloaded (≥100.0%)**: 0 i-frames, high recovery stumble animation.

### **7.2 Backstep Mechanics**

> * **i-Frames**: 0 frames (default state).  
> * **Recovery**: 8–10 frames.  
> * **Utility**: Rapid rearward displacement allowing immediate transition into backstep-unique attack animation trees.

## **8\. Jump Logic & Aerial Physics**

### **8.1 Lower-Body Hurtbox Deactivation**

> * **Invincibility Classification**: Selective spatial hurtbox toggling (NOT global i-Frames).  
> * **Active Window**: Frame 1 to Jump Apex (≈20–25 frames).  
> * **Mechanism**: The collision layer for the lower body (hips down to feet) is disabled or set to pass-through for attack hitboxes flagged with GroundSweep or Shockwave tags.  
> * **Upper Torso**: Remains fully vulnerable throughout the entire jump arc.

### **8.2 Aerial Steering & Trajectory**

> * **Initial Velocity Vector**: *V*air​\=*V*ground​\+*V*jump​.  
> * **Air Control Influence**: Directional input allows a maximum of ≈20–30% vector alteration angle mid-air. Neutral jumps maintain vertical momentum without horizontal drift.

## **9\. Crouch Logic & Animation Interrupts**

### **9.1 Capsule Compression**

> * **Collision Capsule Height**: Reduced by ≈45% upon entering Crouch state.  
> * **High-Hitbox Evasion**: Head and upper torso hurtboxes lower instantly, allowing high-horizontal attack hitboxes to pass over the character model without triggering hit registration ("crouch dodging").

### **9.2 Animation Recovery Cancels & State Aliasing**

> * **Recovery Interruption**: Toggling Crouch during the terminal recovery window of specific weapon attack clips cancels recovery frames earlier than standard Idle transitions.  
> * **Attack Index Mapping**: crouch\_attack\_id \== rolling\_attack\_id  
>   Executing an attack while in the Crouch state directly triggers the Rolling Attack animation script, bypassing stamina consumption and roll recovery requirements.

---

