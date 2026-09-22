using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using UnityEngine;

namespace SoulsLike.Entities.Elevator
{
    public sealed class ElevatorModel
    {
        public bool IsUnlocked { get; set; }
        public bool IsMoving { get; set; }
        public ElevatorFloor CurrentFloor { get; set; }
        public ElevatorFloor DestinationFloor { get; set; }
        public ElevatorMotion Motion { get; set; }
        public Vector3 MotionStartPosition { get; set; }
        public Vector3 MotionDestinationPosition { get; set; }
        public float MotionElapsed { get; set; }
        public Entity RootEntity { get; set; }
        public List<EndpointRegistration> Endpoints { get; } = new();

        public sealed class EndpointRegistration
        {
            public ElevatorEndpoint Endpoint { get; }
            public Entity Entity { get; }
            public ElevatorInteractCommand Command { get; }

            public EndpointRegistration(
                ElevatorEndpoint endpoint,
                Entity entity,
                ElevatorInteractCommand command)
            {
                Endpoint = endpoint;
                Entity = entity;
                Command = command;
            }
        }
    }
}
