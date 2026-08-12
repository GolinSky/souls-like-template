using System;
using UnityEngine;

namespace SoulsLike.Services.Scenes.Data
{
    [Serializable]
    public struct SceneDependency
    {
        [field: SerializeField] public SceneReference[] Dependencies { get; private set; }
    }
}