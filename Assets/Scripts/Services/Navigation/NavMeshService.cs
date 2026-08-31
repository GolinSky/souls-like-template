using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SoulsLike.Services.Navigation
{
    public sealed class NavMeshService : INavMeshService
    {
        private const float NEAREST_POSITION_SEARCH_EPSILON = 0.01f;

        private readonly List<NavMeshSurface> _surfaces = new();
        private readonly List<NavMeshData> _navMeshes = new();

        public IReadOnlyList<NavMeshSurface> Surfaces
        {
            get
            {
                RefreshSurfaces();
                return _surfaces;
            }
        }

        public IReadOnlyList<NavMeshData> NavMeshes
        {
            get
            {
                RefreshSurfaces();
                return _navMeshes;
            }
        }

        public bool TrySamplePosition(
            Vector3 position,
            float maxDistance,
            NavMeshQueryFilter queryFilter,
            out NavMeshHit hit)
        {
            RefreshSurfaces();
            return NavMesh.SamplePosition(position, out hit, maxDistance, queryFilter);
        }

        public bool TrySampleNearestPosition(
            Vector3 position,
            NavMeshQueryFilter queryFilter,
            out NavMeshHit hit)
        {
            RefreshSurfaces(queryFilter.agentTypeID);
            if (_surfaces.Count == 0)
            {
                hit = default;
                return false;
            }

            float maxDistance = 0f;
            foreach (NavMeshSurface surface in _surfaces)
            {
                maxDistance = Mathf.Max(
                    maxDistance,
                    GetMaxDistanceToSourceBounds(position, surface));
            }

            return NavMesh.SamplePosition(
                position,
                out hit,
                maxDistance + NEAREST_POSITION_SEARCH_EPSILON,
                queryFilter);
        }

        private static float GetMaxDistanceToSourceBounds(
            Vector3 position,
            NavMeshSurface surface)
        {
            Bounds bounds = surface.navMeshData.sourceBounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 surfacePosition = surface.transform.position;
            Quaternion surfaceRotation = surface.transform.rotation;
            float maxDistance = 0f;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 corner = new(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 worldCorner = surfacePosition + surfaceRotation * corner;
                        maxDistance = Mathf.Max(
                            maxDistance,
                            Vector3.Distance(position, worldCorner));
                    }
                }
            }

            return maxDistance;
        }

        private void RefreshSurfaces(int? agentTypeId = null)
        {
            _surfaces.Clear();
            _navMeshes.Clear();

            foreach (NavMeshSurface surface in NavMeshSurface.activeSurfaces)
            {
                if (!surface.isActiveAndEnabled
                    || surface.navMeshData == null
                    || (agentTypeId.HasValue && surface.agentTypeID != agentTypeId.Value))
                {
                    continue;
                }

                _surfaces.Add(surface);
                _navMeshes.Add(surface.navMeshData);
            }
        }
    }
}
