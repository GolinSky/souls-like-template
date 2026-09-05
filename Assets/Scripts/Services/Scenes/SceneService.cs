using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
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

        public SceneType DefaultScene => _sceneModel.DefaultScene;

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
            AsyncOperationHandle<SceneInstance> loadingSceneLoadOperation = StartSceneLoad(loadingScene, LoadSceneMode.Single);
            await WaitForSceneLoads(new[] { loadingSceneLoadOperation }, 1);
            EnsureSucceeded(loadingSceneLoadOperation, loadingScene);
            
            TargetScene = sceneType;
            SceneReference targetScene = _sceneModel.GetScene(sceneType);
            var sceneLoadOperations = new List<AsyncOperationHandle<SceneInstance>>();

            if (_sceneModel.TryGetDependencies(sceneType, out SceneReference[] dependencies))
            {
                foreach (SceneReference dependency in dependencies)
                {
                    sceneLoadOperations.Add(StartSceneLoad(dependency, LoadSceneMode.Additive));
                }
            }

            int totalSceneCount = sceneLoadOperations.Count + 1;
            await WaitForSceneLoads(sceneLoadOperations, totalSceneCount);
            EnsureSucceeded(sceneLoadOperations);

            AsyncOperationHandle<SceneInstance> targetSceneLoadOperation = StartSceneLoad(targetScene, LoadSceneMode.Additive);
            sceneLoadOperations.Add(targetSceneLoadOperation);
            await WaitForSceneLoads(sceneLoadOperations, totalSceneCount);
            EnsureSucceeded(sceneLoadOperations);

            OnProgressUpdated?.Invoke(1f);

            Scene loadedTargetScene = targetSceneLoadOperation.Result.Scene;
            if (!loadedTargetScene.IsValid() || !loadedTargetScene.isLoaded)
            {
                throw new InvalidOperationException($"Scene '{targetScene.ScenePath}' did not finish loading.");
            }

            if (!SceneManager.SetActiveScene(loadedTargetScene))
            {
                throw new InvalidOperationException($"Failed to activate scene '{targetScene.ScenePath}'.");
            }

            AsyncOperationHandle<SceneInstance> unloadLoadingOperation = Addressables.UnloadSceneAsync(loadingSceneLoadOperation, autoReleaseHandle: false);
            while (!unloadLoadingOperation.IsDone)
            {
                await UniTask.Yield();
            }

            AsyncOperationStatus unloadStatus = unloadLoadingOperation.Status;
            Exception unloadException = unloadLoadingOperation.OperationException;
            Addressables.Release(unloadLoadingOperation);
            if (unloadStatus != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException($"Failed to unload scene '{loadingScene.ScenePath}'.", unloadException);
            }

            OnSceneChanged?.Invoke(sceneType);
        }

        private async UniTask WaitForSceneLoads(IReadOnlyList<AsyncOperationHandle<SceneInstance>> sceneLoadOperations, int totalSceneCount)
        {
            bool allScenesLoaded = false;
            while (!allScenesLoaded)
            {
                float totalProgress = 0f;
                allScenesLoaded = true;

                foreach (AsyncOperationHandle<SceneInstance> operation in sceneLoadOperations)
                {
                    totalProgress += operation.PercentComplete;
                    allScenesLoaded &= operation.IsDone;
                }

                if (allScenesLoaded)
                {
                    break;
                }

                OnProgressUpdated?.Invoke(totalProgress / totalSceneCount);
                await UniTask.Yield();
            }
        }

        private static void EnsureSucceeded(IReadOnlyList<AsyncOperationHandle<SceneInstance>> sceneLoadOperations)
        {
            foreach (AsyncOperationHandle<SceneInstance> operation in sceneLoadOperations)
            {
                if (operation.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new InvalidOperationException("A scene failed to load.", operation.OperationException);
                }
            }
        }

        private static void EnsureSucceeded(AsyncOperationHandle<SceneInstance> operation, SceneReference scene)
        {
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException($"Scene '{scene.ScenePath}' failed to load.", operation.OperationException);
            }
        }

        private static AsyncOperationHandle<SceneInstance> StartSceneLoad(SceneReference scene, LoadSceneMode loadSceneMode)
        {
            if (scene == null || scene.IsEmpty)
            {
                throw new InvalidOperationException("A required scene reference is missing.");
            }

            return Addressables.LoadSceneAsync(scene.ScenePath, loadSceneMode);
        }
    }
}
