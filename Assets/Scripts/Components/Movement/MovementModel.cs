using System;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Movement
{
    public class MovementModel: Model.Model,IMovementData
    {
        public float MoveSpeed { get; }
        public float SprintSpeed { get; }
        public float RotationSmoothTime { get; }
        public AnimationCurve SpeedChangeRate { get; }
        public float SpeedChangeMultiplier { get; }
        public float StoppingAnimationBlendRate { get; }
        public float JumpHeight { get; }
        public float Gravity { get; }
        public float AirControl { get; }
        public float AirAcceleration { get; }
        public float AirRotationSmoothTime { get; }
        public float JumpGroundIgnoreTime { get; }
        public float MinimumAirborneTime { get; }
        public float JumpApexThreshold { get; }
        public float HardLandingMinFallSpeed { get; }
        public float JumpTimeout { get; }
        public float FallTimeout { get; }
        public bool Grounded { get; set; }
        public float GroundedOffset { get; }
        public float GroundedRadius { get; }
        public float GroundSnapDistance { get; }
        public LayerMask GroundProbeMask { get; }
        public LayerMask GroundLayers => GroundProbeMask;
        public float TerminalVelocity { get; set; }
        public float RollCooldown { get; }
        public float RollStaminaCost { get; }
        public float RollStaminaStartThreshold { get; }
        public float JumpStaminaCost { get; }
        public float JumpStaminaStartThreshold { get; }
        public float CombatSprintStaminaDrainPerSecond { get; }
        public float CombatSprintStaminaStartThreshold { get; }
        public float SlideSpeed { get; }
        public float SlideDuration { get; }
        public float CrouchSpeed { get; }
        public float CrouchHeight { get; }



        public MovementModel(IMovementData movementData)
        {


            MoveSpeed = movementData.MoveSpeed;
            SprintSpeed = movementData.SprintSpeed;
            RotationSmoothTime = movementData.RotationSmoothTime;
            SpeedChangeRate = movementData.SpeedChangeRate;
            SpeedChangeMultiplier = movementData.SpeedChangeMultiplier;
            StoppingAnimationBlendRate = movementData.StoppingAnimationBlendRate;
            JumpHeight = movementData.JumpHeight;
            Gravity = movementData.Gravity;
            AirControl = movementData.AirControl;
            AirAcceleration = movementData.AirAcceleration;
            AirRotationSmoothTime = movementData.AirRotationSmoothTime;
            JumpGroundIgnoreTime = movementData.JumpGroundIgnoreTime;
            MinimumAirborneTime = movementData.MinimumAirborneTime;
            JumpApexThreshold = movementData.JumpApexThreshold;
            HardLandingMinFallSpeed = movementData.HardLandingMinFallSpeed;
            JumpTimeout = movementData.JumpTimeout;
            FallTimeout = movementData.FallTimeout;
            Grounded = movementData.Grounded;
            GroundedOffset = movementData.GroundedOffset;
            GroundedRadius = movementData.GroundedRadius;
            GroundSnapDistance = movementData.GroundSnapDistance;
            GroundProbeMask = movementData.GroundProbeMask;
            TerminalVelocity = movementData.TerminalVelocity;
            RollCooldown = movementData.RollCooldown;
            RollStaminaCost = Mathf.Max(0f, movementData.RollStaminaCost);
            RollStaminaStartThreshold = movementData.RollStaminaStartThreshold;
            JumpStaminaCost = Mathf.Max(0f, movementData.JumpStaminaCost);
            JumpStaminaStartThreshold = movementData.JumpStaminaStartThreshold;
            CombatSprintStaminaDrainPerSecond = Mathf.Max(0f, movementData.CombatSprintStaminaDrainPerSecond);
            CombatSprintStaminaStartThreshold = movementData.CombatSprintStaminaStartThreshold;
            SlideSpeed = movementData.SlideSpeed;
            SlideDuration = movementData.SlideDuration;
            CrouchSpeed = movementData.CrouchSpeed;
            CrouchHeight = movementData.CrouchHeight;
        }

    }
}
