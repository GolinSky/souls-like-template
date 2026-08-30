using System;
using UnityEngine;

namespace SoulsLike.Services.Travel.Data
{
    [Serializable]
    public sealed class GraceData
    {
        [SerializeField] private string name;
        [SerializeField] private GraceId id;
        [SerializeField] private string displayName;

        public string Name => name;
        public GraceId Id => id;
        public string DisplayName => displayName;
    }
}
