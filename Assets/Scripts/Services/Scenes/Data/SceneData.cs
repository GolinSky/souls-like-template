using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Services.Scenes.Data
{
    [CreateAssetMenu(fileName = "SceneData", menuName = "Data/SceneData")]
    public class SceneData : Model.Data
    {
        [SerializeField] private SerializedDictionary<SceneType, SceneReference> scenes;

        public Dictionary<SceneType, SceneReference> Scenes => scenes.Dictionary; //todo: better create get by id api
    }
}