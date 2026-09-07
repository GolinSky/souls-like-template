using UnityEngine;

namespace SoulsLike.Entities.Elevator
{
    public readonly struct ElevatorMotion
    {
        public float Distance { get; }
        public float Acceleration { get; }
        public float PeakSpeed { get; }
        public float AccelerationDuration { get; }
        public float CruiseDuration { get; }
        public float Duration { get; }

        public ElevatorMotion(float distance, float maxSpeed, float acceleration)
        {
            Distance = Mathf.Max(0f, distance);
            Acceleration = Mathf.Max(acceleration, Mathf.Epsilon);
            float requestedSpeed = Mathf.Max(0f, maxSpeed);
            float distanceToReachRequestedSpeed = requestedSpeed * requestedSpeed / Acceleration;
            PeakSpeed = Distance <= distanceToReachRequestedSpeed
                ? Mathf.Sqrt(Distance * Acceleration)
                : requestedSpeed;
            AccelerationDuration = PeakSpeed / Acceleration;
            float accelerationDistance = PeakSpeed * PeakSpeed / Acceleration;
            CruiseDuration = PeakSpeed <= Mathf.Epsilon
                ? 0f
                : Mathf.Max(0f, Distance - accelerationDistance) / PeakSpeed;
            Duration = AccelerationDuration * 2f + CruiseDuration;
        }

        public float SampleDistance(float elapsedSeconds)
        {
            if (Distance <= Mathf.Epsilon || elapsedSeconds <= 0f)
            {
                return 0f;
            }

            if (elapsedSeconds >= Duration)
            {
                return Distance;
            }

            if (elapsedSeconds < AccelerationDuration)
            {
                return 0.5f * Acceleration * elapsedSeconds * elapsedSeconds;
            }

            float accelerationDistance = 0.5f * Acceleration * AccelerationDuration * AccelerationDuration;
            float cruiseEnd = AccelerationDuration + CruiseDuration;
            if (elapsedSeconds < cruiseEnd)
            {
                return accelerationDistance + PeakSpeed * (elapsedSeconds - AccelerationDuration);
            }

            float decelerationElapsed = elapsedSeconds - cruiseEnd;
            return Distance - 0.5f * Acceleration
                * (AccelerationDuration - decelerationElapsed)
                * (AccelerationDuration - decelerationElapsed);
        }
    }
}
