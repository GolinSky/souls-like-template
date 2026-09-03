using SoulsLike.Services.Scenes.Data;
using SoulsLike.Services.Travel.Data;
using UnityEngine;

namespace SoulsLike.Services.Spawn
{
    public class CharacterSpawnData
    {
        public bool HasCurrentPosition { get; set; }
        public SceneType CurrentScene { get; set; } = SceneType.Workshop;
        public Vector3 CurrentPosition { get; set; }
        public GraceId LastGraceId { get; set; } = GraceId.WorkshopGrace01;
    }
}
