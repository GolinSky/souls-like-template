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
        public float JumpTimeout { get; }
        public float FallTimeout { get; }
        public bool Grounded { get; set; }
        public float GroundedOffset { get; }
        public float GroundedRadius { get; }
        public LayerMask GroundLayers { get; }
        public float TerminalVelocity { get; set; }
        public float RollCooldown { get; }
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
            JumpTimeout = movementData.JumpTimeout;
            FallTimeout = movementData.FallTimeout;
            Grounded = movementData.Grounded;
            GroundedOffset = movementData.GroundedOffset;
            GroundedRadius = movementData.GroundedRadius;
            GroundLayers = movementData.GroundLayers;
            TerminalVelocity = movementData.TerminalVelocity;
            RollCooldown = movementData.RollCooldown;
            SlideSpeed = movementData.SlideSpeed;
            SlideDuration = movementData.SlideDuration;
            CrouchSpeed = movementData.CrouchSpeed;
            CrouchHeight = movementData.CrouchHeight;
        }

    }
}
