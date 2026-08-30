using System;
using System.Collections.Generic;
using System.Linq;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;

namespace SoulsLike.Services.Travel.Data
{
    [Serializable]
    public sealed class LocationEntry
    {
        [SerializeField] private SceneType id;
        [SerializeField] private string displayName;
        [SerializeField] private GraceData[] graces;

        public SceneType Id => id;
        public string DisplayName => displayName;
        public IReadOnlyList<GraceData> Graces => graces;

        public GraceData GetGrace(GraceId graceId) => graces.Single(grace => grace.Id == graceId);
    }
}
