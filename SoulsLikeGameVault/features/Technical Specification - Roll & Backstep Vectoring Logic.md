---
name: roll-backstep-spec
description: Technical specification for dodge roll and backstep vectoring across target-lock and free-aim modes.
version: 1.0.0
---

# TECHNICAL SPECIFICATION: Roll & Backstep Vectoring Logic

## 1. Dodge Roll Directional Logic

### 1.1 Free-Aim Mode (Unlocked)
- **Vector Evaluation**: The dodge displacement vector ($\vec{D}$) is derived from the camera-relative 2D joystick input ($\vec{I}$).
  $$\vec{D}_{\text{world}} = \text{Normalize}(\vec{I}_{\text{cam\_space}})$$
- **Omnidirectional Cardinal/Ordinal Freedom**: Full 360° rotational freedom. Frame 0 locks the roll direction precisely to $\text{atan2}(I_y, I_x)$.
- **Orientation Matching**: The character transform ($T_{\text{char}}$) immediately rotates its forward vector ($\vec{F}$) to align with $\vec{D}_{\text{world}}$ on Frame 0 of the roll initialization.
- **Root Motion Scaling**: Unlocked rolls maintain a fixed 100% linear translation curve along $\vec{D}_{\text{world}}$ regardless of angle relative to camera or enemies.

### 1.2 Target Lock-On Mode
- **Vector Quantization / Angle Snapping**: While lock-on is active, rolls evaluate movement relative to the Target Transform Vector ($\vec{T}$).
- **4-Way / 8-Way Directional Locking**: Input is clamped into discrete directional bins relative to $\vec{T}$:
  - Forward ($0^\circ$)
  - Lateral Left/Right ($\pm 90^\circ$)
  - Rearward ($180^\circ$)
  - Diagonal Quadrants ($\pm 45^\circ$, $\pm 135^\circ$)
- **Facing Vector Locking**: Unlike unlocked rolls where $\vec{F}$ rotates to match displacement, lock-on strafe rolls maintain character facing toward the target transform ($\vec{F} \rightarrow \vec{T}$) throughout the animation arc, altering the animation clip to specialized strafe-roll variants (e.g., side-flips or back-rolls).
- **Spatial Orbit Mechanics**: Lateral locked rolls apply an angular velocity component ($\omega$), creating a circular displacement arc around the locked target rather than a strictly linear Euclidean ray:
  $$r = \|\vec{P}_{\text{player}} - \vec{P}_{\text{target}}\|$$

---

## 2. Backstep Mechanics & Vector Logic

### 2.1 Trigger Rules & Input Evaluation
- **Neutral Key Release**: Executed when the Dodge/Sprint key is released ($t_{\text{hold}} < T_{\text{threshold}}$) while joystick vector magnitude $\|\vec{I}\| \approx 0$.
- **Directional Vector**: Forced along $-\vec{F}$ (opposite of current character facing vector). No forward or lateral backstep exists natively.

### 2.2 Unlocked vs. Lock-On Backstep Behavior
- **Free-Aim (Unlocked)**:
  - Displaces directly opposite to where the character was facing the frame prior to input.
  - Allows quick-turn tech: Rapidly flicking stick $\vec{I}$ and releasing dodge causes character to flip $180^\circ$ and backstep *towards* the enemy/camera ("Rave Step" / Reverse Backstep).
- **Target Lock-On**:
  - Because $\vec{F}$ is clamped to face the locked target transform $\vec{T}$, backstepping *always* results in pure linear spatial retreat away from the target along the vector $-\vec{T}$.

### 2.3 Frame Data & Cancel Windows
- **i-Frames**: $0\text{ frames}$ (Base layer contains no invincibility).
- **Hyperarmor / Poise**: $0\text{ frames}$ by default.
- **Attack Cancel Window**:
  - Cancels recovery at Frame 8–10 into a unique `Backstep Attack` (Feint/Lunge) index.
  - Bypasses standard light/heavy attack startup chains, executing an accelerated horizontal/piercing attack vector.