using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulsLike.Services.Scenes.Data
{
    public class SceneModel: Model.Model
    {
        private Dictionary<SceneType, SceneReference> SceneDictionary { get; }
        
        public SceneModel(SceneData sceneData)
        {
            SceneDictionary = sceneData != null ? sceneData.Scenes : new Dictionary<SceneType, SceneReference>();
        }

        public SceneReference GetScene(SceneType sceneType)
        {
            if (TryGetScene(sceneType, out var scene))
            {
                return scene;
            }

            Debug.LogWarning($"No scene found with type {sceneType}");
            return null;
        }

        public bool TryGetScene(SceneType sceneType, out SceneReference scene)
        {
            scene = null;

            return SceneDictionary != null &&
                   SceneDictionary.TryGetValue(sceneType, out scene) &&
                   scene != null &&
                   !scene.IsEmpty;
        }

        public SceneType GetSceneById(Scene activeScene)
        {
            if (SceneDictionary == null)
            {
                return SceneType.Undefined;
            }

            foreach (var keyValuePair in SceneDictionary)
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
            if (SceneDictionary == null)
            {
                return SceneType.Undefined;
            }

            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return SceneType.Undefined;
            }

            foreach (var keyValuePair in SceneDictionary)
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
