using System;
using UnityEngine;
using UnityEngine.AI;
using SoulsLike.Entities.Elevator;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CharacterController))]
    public sealed class EnemyNavigationMotor : MonoBehaviour, IInitializable, IPlatformRiderMotor
    {
        private const float VELOCITY_EPSILON = 0.0001f;
        private const float GROUNDING_SPEED = -2f;
        private const int GROUND_PROBE_HIT_CAPACITY = 8;

        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private CharacterController controller;

        private bool _rootMotionActive;
        private bool _hasDestination;
        private readonly RaycastHit[] _groundProbeHits = new RaycastHit[GROUND_PROBE_HIT_CAPACITY];

        public Vector3 WorldVelocity { get; private set; }
        public Vector3 LocalVelocity => transform.InverseTransformDirection(WorldVelocity);

        public void Initialize()
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
            if (!agent.Warp(transform.position))
            {
                throw new InvalidOperationException(
                    $"Enemy '{name}' must spawn on a baked NavMesh.");
            }

            Stop();
        }

        public void SetDestination(Vector3 position)
        {
            _hasDestination = true;
            agent.nextPosition = transform.position;
            agent.isStopped = false;
            if (!agent.SetDestination(position))
            {
                Stop();
            }
        }

        public void Stop()
        {
            _hasDestination = false;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.nextPosition = transform.position;
            }

            WorldVelocity = Vector3.zero;
        }

        public void SuspendForTraversal()
        {
            Stop();
            agent.enabled = false;
        }

        public void ResumeAfterTraversal()
        {
            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            if (agent.isOnNavMesh && !agent.Warp(transform.position))
            {
                throw new InvalidOperationException(
                    $"Enemy '{name}' could not resume on the NavMesh after ladder traversal.");
            }

            Stop();
        }

        public void SetRootMotion(bool active)
        {
            _rootMotionActive = active;
            if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            {
                return;
            }

            agent.isStopped = active || !_hasDestination;
            agent.nextPosition = transform.position;
        }

        public void ApplyRootMotion(Vector3 deltaPosition)
        {
            deltaPosition.y = GROUNDING_SPEED * Time.deltaTime;
            Vector3 before = transform.position;
            controller.Move(deltaPosition);
            agent.nextPosition = transform.position;
            WorldVelocity = Time.deltaTime > 0f
                ? (transform.position - before) / Time.deltaTime
                : Vector3.zero;
        }

        public void ApplyPlatformDisplacement(Vector3 displacement)
        {
            controller.Move(displacement);
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.nextPosition = transform.position;
            }
        }

        public bool IsSupportedBy(Collider supportCollider)
        {
            if (!controller.enabled || !controller.isGrounded)
            {
                return false;
            }

            float lowerSphereOffset = Mathf.Max(controller.height * 0.5f - controller.radius, 0f);
            Vector3 castOrigin = transform.TransformPoint(controller.center)
                - Vector3.up * lowerSphereOffset;
            int hitCount = Physics.SphereCastNonAlloc(
                castOrigin,
                controller.radius * 0.9f,
                Vector3.down,
                _groundProbeHits,
                controller.skinWidth + 0.05f,
                ~0,
                QueryTriggerInteraction.Ignore);
            for (int index = 0; index < hitCount; index++)
            {
                if (_groundProbeHits[index].collider == supportCollider)
                {
                    return true;
                }
            }

            return false;
        }

        public void SynchronizeAfterPlatformRide()
        {
            if (!agent.isActiveAndEnabled)
            {
                return;
            }

            if (!agent.Warp(transform.position))
            {
                throw new InvalidOperationException(
                    $"Enemy '{name}' could not synchronize with the elevator NavMesh position.");
            }

            agent.nextPosition = transform.position;
        }

        public void Tick(float deltaTime, bool faceMovement)
        {
            if (_rootMotionActive)
            {
                agent.nextPosition = transform.position;
                return;
            }

            agent.nextPosition = transform.position;
            Vector3 desiredVelocity = agent.isStopped
                ? Vector3.zero
                : agent.desiredVelocity;
            if (faceMovement && desiredVelocity.sqrMagnitude > VELOCITY_EPSILON)
            {
                Face(
                    transform.position + desiredVelocity,
                    agent.angularSpeed,
                    deltaTime);
            }

            Vector3 motion = desiredVelocity * deltaTime;
            motion.y = GROUNDING_SPEED * deltaTime;

            Vector3 before = transform.position;
            controller.Move(motion);
            agent.nextPosition = transform.position;
            WorldVelocity = deltaTime > 0f
                ? (transform.position - before) / deltaTime
                : Vector3.zero;
        }

        public void Face(Vector3 position, float degreesPerSecond, float deltaTime)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= VELOCITY_EPSILON)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                degreesPerSecond * deltaTime);
        }

        public void FaceImmediately(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= VELOCITY_EPSILON)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(
                direction.normalized,
                Vector3.up);
        }

        public void Rotate(float degrees, float deltaTime)
        {
            transform.Rotate(0f, degrees * deltaTime, 0f, Space.World);
        }

        public bool IsWithin(Vector3 position, float distance) =>
            (transform.position - position).sqrMagnitude <= distance * distance;
    }
}
