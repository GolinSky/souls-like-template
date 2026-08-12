using UnityEngine.SceneManagement;

namespace SoulsLike.Services.Scenes.Data
{
    public class SceneModel: Model.Model
    {
        private readonly SceneData _sceneData;

        public SceneModel(SceneData sceneData)
        {
            _sceneData = sceneData;
        }

        public SceneReference GetScene(SceneType sceneType)
        {
            return _sceneData.GetScene(sceneType);
        }

        public SceneType GetSceneById(Scene activeScene)
        {
            return _sceneData.GetSceneById(activeScene);
        }

        public SceneType GetSceneByPath(string scenePath)
        {
            return _sceneData.GetSceneByPath(scenePath);
        }

        public bool TryGetDependencies(SceneType sceneType, out SceneReference[] scenesToLoad) =>
            _sceneData.TryGetDependencies(sceneType, out scenesToLoad);
    }
}
