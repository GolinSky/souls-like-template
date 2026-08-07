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
        float JumpTimeout { get; }
        float FallTimeout { get; }
        bool Grounded { get; }
        float GroundedOffset { get; }
        float GroundedRadius { get; }
        float TerminalVelocity { get; set; }
        LayerMask GroundLayers { get; }
        float RollSpeed { get; }
        float RollDuration { get; }
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

        [field:Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [field: SerializeField]
        public float JumpTimeout { get; private set; } = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [field: SerializeField]
        public float FallTimeout { get; private set; } = 0.15f;

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
        [Tooltip("Speed while rolling")]
        [field: SerializeField]
        public float RollSpeed { get; private set; } = 6.0f;

        [Tooltip("Duration of the roll animation/action")]
        [field: SerializeField]
        public float RollDuration { get; private set; } = 0.8f;

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
