using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Movement
{
    public interface IMovementData
    {
        float MoveSpeed { get; }
        float SprintSpeed { get; }
        float RotationSmoothTime { get; }
        AnimationCurve SpeedChangeRate { get; }
        float SpeedChangeMultiplier { get; }
        float StoppingAnimationBlendRate { get; }
        float JumpHeight { get; }
        float Gravity { get; }
        float AirControl { get; }
        float AirAcceleration { get; }
        float AirRotationSmoothTime { get; }
        float JumpGroundIgnoreTime { get; }
        float MinimumAirborneTime { get; }
        float JumpApexThreshold { get; }
        float HardLandingMinFallSpeed { get; }
        float JumpTimeout { get; }
        float FallTimeout { get; }
        bool Grounded { get; }
        float GroundedOffset { get; }
        float GroundedRadius { get; }
        float TerminalVelocity { get; set; }
        LayerMask GroundLayers { get; }
        float RollCooldown { get; }
        float SlideSpeed { get; }
        float SlideDuration { get; }
        float CrouchSpeed { get; }
        float CrouchHeight { get; }
    }

    [CreateAssetMenu(fileName = "MovementData", menuName = "Data/MovementData")]
    public class MovementData : Data, IMovementData
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        [field: SerializeField]
        public float MoveSpeed { get; private set; } = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        [field: SerializeField]
        public float SprintSpeed { get; private set; } = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        [field: SerializeField]
        public float RotationSmoothTime { get; private set; } = 0.12f;

        [Tooltip("Acceleration and deceleration rate over time. X axis = Time since magnitude change, Y axis = change rate factor.")]
        [field: SerializeField]
        public AnimationCurve SpeedChangeRate { get; private set; } = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        [Tooltip("Multiplier for the Speed Change Rate curve.")]
        [field: SerializeField]
        public float SpeedChangeMultiplier { get; private set; } = 10.0f;

        [Tooltip("How fast the animation blend smoothly returns to Idle when movement input stops completely.")]
        [field: SerializeField]
        public float StoppingAnimationBlendRate { get; private set; } = 8.0f;

        [field:Space(10)]
        [Tooltip("The height the player can jump")]
        [field: SerializeField]
        public float JumpHeight { get; private set; } = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [field: SerializeField]
        public float Gravity { get; private set; } = -15.0f;

        [Tooltip("Fraction of ground steering authority available while airborne")]
        [Range(0.0f, 1.0f)]
        [field: SerializeField]
        public float AirControl { get; private set; } = 0.25f;

        [Tooltip("Maximum horizontal acceleration used by airborne steering")]
        [field: SerializeField]
        public float AirAcceleration { get; private set; } = 8.0f;

        [Tooltip("How slowly free-movement facing follows travel direction while airborne")]
        [field: SerializeField]
        public float AirRotationSmoothTime { get; private set; } = 0.25f;

        [Tooltip("Prevents the ground probe from immediately cancelling a jump takeoff")]
        [field: SerializeField]
        public float JumpGroundIgnoreTime { get; private set; } = 0.12f;

        [Tooltip("Minimum airborne duration before a landing can be accepted")]
        [field: SerializeField]
        public float MinimumAirborneTime { get; private set; } = 0.08f;

        [Tooltip("Vertical speed at which jump start becomes the falling phase")]
        [field: SerializeField]
        public float JumpApexThreshold { get; private set; } = 0.35f;

        [Tooltip("Downward impact speed that selects a hard landing")]
        [field: SerializeField]
        public float HardLandingMinFallSpeed { get; private set; } = 12.0f;

        [field:Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [field: SerializeField]
        public float JumpTimeout { get; private set; } = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [field: SerializeField]
        public float FallTimeout { get; private set; } = 0.1f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [field: SerializeField]
        public bool Grounded { get; private set; } = true;

        [Tooltip("Useful for rough ground")]
        [field: SerializeField]
        public float GroundedOffset { get; private set; } = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [field: SerializeField]
        public float GroundedRadius { get; private set; } = 0.28f;

        [field: SerializeField] public float TerminalVelocity { get; set; }

        [Tooltip("What layers the character uses as ground")]
        [field: SerializeField]
        public LayerMask GroundLayers { get; private set; }

        [Header("Rolling")]
        [Tooltip("Time required to pass before being able to roll again")]
        [field: SerializeField]
        public float RollCooldown { get; private set; } = 1.0f;

        [Header("Sliding")]
        [Tooltip("Speed of the character while sliding in m/s")]
        [field: SerializeField]
        public float SlideSpeed { get; private set; } = 8.0f;

        [Tooltip("Duration of the slide action in seconds")]
        [field: SerializeField]
        public float SlideDuration { get; private set; } = 0.8f;

        [Header("Crouching")]
        [Tooltip("Speed of the character while crouching in m/s")]
        [field: SerializeField]
        public float CrouchSpeed { get; private set; } = 2.0f;

        [Tooltip("Height of the character collider while crouching")]
        [field: SerializeField]
        public float CrouchHeight { get; private set; } = 1.0f;
    }
}
