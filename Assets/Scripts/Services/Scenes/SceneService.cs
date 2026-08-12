using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace SoulsLike.Services.Scenes
{
    public class SceneService : ISceneService 
    {
        public event Action<float> OnProgressUpdated;// temp solution
        public event Action<SceneType> OnSceneChanged;
        
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

            SceneReference loadingScene = _sceneModel.GetScene(SceneType.Loading);
            await StartSceneLoad(loadingScene, LoadSceneMode.Single).ToUniTask();
            
            TargetScene = sceneType;
            SceneReference targetScene = _sceneModel.GetScene(sceneType);
            var sceneLoadOperations = new List<AsyncOperation>();

            if (_sceneModel.TryGetDependencies(sceneType, out SceneReference[] dependencies))
            {
                foreach (SceneReference dependency in dependencies)
                {
                    sceneLoadOperations.Add(StartSceneLoad(dependency, LoadSceneMode.Additive));
                }
            }

            int totalSceneCount = sceneLoadOperations.Count + 1;
            await WaitForSceneLoads(sceneLoadOperations, totalSceneCount);

            sceneLoadOperations.Add(StartSceneLoad(targetScene, LoadSceneMode.Additive));
            await WaitForSceneLoads(sceneLoadOperations, totalSceneCount);

            OnProgressUpdated?.Invoke(1f);

            Scene loadedTargetScene = SceneManager.GetSceneByPath(targetScene.ScenePath);
            if (!loadedTargetScene.IsValid() || !loadedTargetScene.isLoaded)
            {
                throw new InvalidOperationException($"Scene '{targetScene.ScenePath}' did not finish loading.");
            }

            if (!SceneManager.SetActiveScene(loadedTargetScene))
            {
                throw new InvalidOperationException($"Failed to activate scene '{targetScene.ScenePath}'.");
            }

            AsyncOperation unloadLoadingOperation = SceneManager.UnloadSceneAsync(loadingScene.ScenePath);
            if (unloadLoadingOperation == null)
            {
                throw new InvalidOperationException($"Failed to start unloading scene '{loadingScene.ScenePath}'.");
            }

            await unloadLoadingOperation.ToUniTask();
            OnSceneChanged?.Invoke(sceneType);
        }

        private async UniTask WaitForSceneLoads(IReadOnlyList<AsyncOperation> sceneLoadOperations, int totalSceneCount)
        {
            bool allScenesLoaded = false;
            while (!allScenesLoaded)
            {
                float totalProgress = 0f;
                allScenesLoaded = true;

                foreach (AsyncOperation operation in sceneLoadOperations)
                {
                    totalProgress += operation.progress;
                    allScenesLoaded &= operation.isDone;
                }

                if (allScenesLoaded)
                {
                    break;
                }

                OnProgressUpdated?.Invoke(totalProgress / totalSceneCount);
                await UniTask.Yield();
            }
        }

        private static AsyncOperation StartSceneLoad(SceneReference scene, LoadSceneMode loadSceneMode)
        {
            if (scene == null || scene.IsEmpty)
            {
                throw new InvalidOperationException("A required scene reference is missing.");
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scene.ScenePath, loadSceneMode);
            if (loadOperation == null)
            {
                throw new InvalidOperationException($"Failed to start loading scene '{scene.ScenePath}'.");
            }

            return loadOperation;
        }
    }
}
