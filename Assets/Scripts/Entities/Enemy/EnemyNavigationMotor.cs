using System;
using UnityEngine;
using UnityEngine.AI;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CharacterController))]
    public sealed class EnemyNavigationMotor : MonoBehaviour, IInitializable
    {
        private const float VELOCITY_EPSILON = 0.0001f;
        private const float GROUNDING_SPEED = -2f;

        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private CharacterController controller;

        private bool _rootMotionActive;
        private bool _hasDestination;

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
