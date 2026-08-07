using System;
using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes.Data;
using UnityEngine.SceneManagement;


namespace SoulsLike.Services.Scenes
{
    public class SceneService : ISceneService 
    {
        public event Action<float> OnProgressUpdated;// temp solution
        
        private readonly SceneModel _sceneModel;
        public SceneType TargetScene { get; private set; }
        public SceneType CurrentScene
        {
            get
            {
                if (_sceneModel == null)
                {
                    UnityEngine.Debug.LogError("[SceneService] _sceneModel is null when getting CurrentScene!");
                    return SceneType.Undefined;
                }
                return _sceneModel.GetSceneById(SceneManager.GetActiveScene());
            }
        }

        public SceneService(SceneModel sceneModel)
        {
            _sceneModel = sceneModel;
            if (_sceneModel == null)
            {
                UnityEngine.Debug.LogError("[SceneService] SceneModel dependency injected is null!");
            }
        }

        public async UniTask LoadScene(SceneType sceneType)
        {
            await LoadSceneAsync(sceneType);
        }

        public bool TryGetScenePath(SceneType sceneType, out string scenePath)
        {
            scenePath = null;

            if (_sceneModel == null)
            {
                UnityEngine.Debug.LogError("[SceneService] _sceneModel is null in TryGetScenePath!");
                return false;
            }

            if (!_sceneModel.TryGetScene(sceneType, out var scene))
            {
                return false;
            }

            scenePath = scene.ScenePath;
            return !string.IsNullOrWhiteSpace(scenePath);
        }

        public SceneType GetSceneType(string scenePathOrName)
        {
            if (_sceneModel == null)
            {
                UnityEngine.Debug.LogError("[SceneService] _sceneModel is null in GetSceneType!");
                return SceneType.Undefined;
            }
            return _sceneModel.GetSceneByPath(scenePathOrName);
        }

        private async UniTask LoadSceneAsync(SceneType sceneType)
        {
            if (_sceneModel == null)
            {
                UnityEngine.Debug.LogError("[SceneService] _sceneModel is null in LoadSceneAsync!");
                return;
            }

            string loadingSceneName = _sceneModel.GetScene(SceneType.Loading).SceneName;
            await SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Single).ToUniTask();
            
            TargetScene = sceneType;
            string sceneName = _sceneModel.GetScene(sceneType).SceneName;
            var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            asyncOp.allowSceneActivation = false;

            // Scene loading progress (0 → 0.9)
            while (asyncOp.progress < 0.9f)
            {
                OnProgressUpdated?.Invoke(asyncOp.progress);
                await UniTask.Yield();
            }

            // Fully loaded (0.9 → 1)
            OnProgressUpdated?.Invoke(1f);

            await UniTask.Delay(300); // small smooth delay

            asyncOp.allowSceneActivation = true;
        }
    }
}
