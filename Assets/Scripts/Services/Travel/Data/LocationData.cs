using System.Collections.Generic;
using System.Linq;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;

namespace SoulsLike.Services.Travel.Data
{
    [CreateAssetMenu(fileName = "LocationData", menuName = "Data/LocationData")]
    public sealed class LocationData : SoulsLike.Model.Data
    {
        [SerializeField] private LocationEntry[] locations;

        public IReadOnlyList<LocationEntry> Locations => locations;

        public LocationEntry GetLocation(SceneType locationId) =>
            locations.Single(location => location.Id == locationId);

        public GraceData GetGrace(GraceId graceId) =>
            locations.SelectMany(location => location.Graces).Single(grace => grace.Id == graceId);
    }
}
