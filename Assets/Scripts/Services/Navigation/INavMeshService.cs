using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SoulsLike.Services.Navigation
{
    public interface INavMeshService
    {
        IReadOnlyList<NavMeshSurface> Surfaces { get; }
        IReadOnlyList<NavMeshData> NavMeshes { get; }

        bool TrySamplePosition(
            Vector3 position,
            float maxDistance,
            NavMeshQueryFilter queryFilter,
            out NavMeshHit hit);
    }
}
