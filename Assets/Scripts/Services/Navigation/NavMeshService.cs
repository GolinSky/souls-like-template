using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SoulsLike.Services.Navigation
{
    public sealed class NavMeshService : INavMeshService
    {
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

        private void RefreshSurfaces()
        {
            _surfaces.Clear();
            _navMeshes.Clear();

            foreach (NavMeshSurface surface in NavMeshSurface.activeSurfaces)
            {
                if (!surface.isActiveAndEnabled || surface.navMeshData == null)
                {
                    continue;
                }

                _surfaces.Add(surface);
                _navMeshes.Add(surface.navMeshData);
            }
        }
    }
}
