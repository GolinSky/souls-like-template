using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulsLike.Services.Scenes.Data
{
    [CreateAssetMenu(fileName = "SceneData", menuName = "Data/SceneData")]
    public class SceneData : Model.Data
    {
        [SerializeField] private SerializedDictionary<SceneType, SceneReference> scenes;
        [SerializeField] private SerializedDictionary<SceneType, SceneDependency> dependencies;

        public bool TryGetDependencies(SceneType sceneType, out SceneReference[] scenesToLoad)
        {
            scenesToLoad = null;
            if (dependencies.Dictionary.TryGetValue(sceneType, out SceneDependency sceneDependency))
            {
                scenesToLoad = sceneDependency.Dependencies;
                return scenesToLoad != null && scenesToLoad.Length > 0;
            }

            return false;
        }

        public SceneReference GetScene(SceneType sceneType)
        {
            return scenes.Dictionary[sceneType];
        }

        public SceneType GetSceneById(Scene activeScene)
        {
            foreach (var keyValuePair in scenes.Dictionary)
            {
                if (keyValuePair.Value != null && keyValuePair.Value.BuildIndex == activeScene.buildIndex)
                {
                    return keyValuePair.Key;
                }
            }

            return SceneType.Undefined;
        }

        public SceneType GetSceneByPath(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return SceneType.Undefined;
            }

            foreach (var keyValuePair in scenes.Dictionary)
            {
                SceneReference scene = keyValuePair.Value;
                if (scene == null || scene.IsEmpty)
                {
                    continue;
                }

                if (string.Equals(scene.ScenePath, scenePath, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(scene.SceneName, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return keyValuePair.Key;
                }
            }

            return SceneType.Undefined;
        }
    }
}