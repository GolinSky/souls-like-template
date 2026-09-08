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
        private bool _isLoadingScene;
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
            if (_isLoadingScene)
            {
                throw new InvalidOperationException("A scene transition is already in progress.");
            }

            _isLoadingScene = true;
            try
            {
                await LoadSceneAsync(sceneType);
            }
            finally
            {
                _isLoadingScene = false;
            }
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
            AsyncOperationHandle<SceneInstance> loadingSceneLoadOperation = default;
            var sceneLoadOperations = new List<AsyncOperationHandle<SceneInstance>>();

            try
            {
                loadingSceneLoadOperation = StartSceneLoad(loadingScene, LoadSceneMode.Single);
                await WaitForSceneLoads(new[] { loadingSceneLoadOperation }, 1);
                EnsureSucceeded(loadingSceneLoadOperation, loadingScene);

                TargetScene = sceneType;
                SceneReference targetScene = _sceneModel.GetScene(sceneType);

                int totalSceneCount = 1;
                if (_sceneModel.TryGetDependencies(sceneType, out SceneReference[] dependencies))
                {
                    totalSceneCount = dependencies.Length + 1;
                    foreach (SceneReference dependency in dependencies)
                    {
                        AsyncOperationHandle<SceneInstance> dependencyLoadOperation = StartSceneLoad(dependency, LoadSceneMode.Additive);
                        sceneLoadOperations.Add(dependencyLoadOperation);
                        await WaitForSceneLoads(sceneLoadOperations, totalSceneCount);
                        EnsureSucceeded(dependencyLoadOperation, dependency);
                    }
                }

                AsyncOperationHandle<SceneInstance> targetSceneLoadOperation = StartSceneLoad(targetScene, LoadSceneMode.Additive);
                sceneLoadOperations.Add(targetSceneLoadOperation);
                await WaitForSceneLoads(sceneLoadOperations, totalSceneCount);
                EnsureSucceeded(targetSceneLoadOperation, targetScene);

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

                await UnloadSceneAsync(loadingSceneLoadOperation, loadingScene.ScenePath);
            }
            catch
            {
                await CleanupFailedSceneLoads(sceneLoadOperations);
                await ReleaseFailedLoadingSceneLoad(loadingSceneLoadOperation);
                throw;
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

        private static async UniTask UnloadSceneAsync(AsyncOperationHandle<SceneInstance> sceneLoadOperation, string scenePath)
        {
            AsyncOperationHandle<SceneInstance> unloadSceneOperation = Addressables.UnloadSceneAsync(sceneLoadOperation, autoReleaseHandle: false);
            if (!unloadSceneOperation.IsValid())
            {
                throw new InvalidOperationException($"Failed to start unloading scene '{scenePath}'.");
            }

            while (!unloadSceneOperation.IsDone)
            {
                await UniTask.Yield();
            }

            AsyncOperationStatus unloadStatus = unloadSceneOperation.Status;
            Exception unloadException = unloadSceneOperation.OperationException;
            Addressables.Release(unloadSceneOperation);

            if (unloadStatus != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException($"Failed to unload scene '{scenePath}'.", unloadException);
            }
        }

        private static void EnsureSucceeded(AsyncOperationHandle<SceneInstance> operation, SceneReference scene)
        {
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException($"Scene '{scene.ScenePath}' failed to load.", operation.OperationException);
            }
        }

        private static async UniTask CleanupFailedSceneLoads(IReadOnlyList<AsyncOperationHandle<SceneInstance>> sceneLoadOperations)
        {
            for (int operationIndex = sceneLoadOperations.Count - 1; operationIndex >= 0; operationIndex--)
            {
                AsyncOperationHandle<SceneInstance> sceneLoadOperation = sceneLoadOperations[operationIndex];
                try
                {
                    while (!sceneLoadOperation.IsDone)
                    {
                        await UniTask.Yield();
                    }

                    if (!sceneLoadOperation.IsValid())
                    {
                        continue;
                    }

                    if (sceneLoadOperation.Status == AsyncOperationStatus.Succeeded)
                    {
                        await UnloadSceneAsync(sceneLoadOperation, "destination scene");
                    }
                    else
                    {
                        Addressables.Release(sceneLoadOperation);
                    }
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }
        }

        private static async UniTask ReleaseFailedLoadingSceneLoad(AsyncOperationHandle<SceneInstance> loadingSceneLoadOperation)
        {
            try
            {
                if (!loadingSceneLoadOperation.IsValid())
                {
                    return;
                }

                while (!loadingSceneLoadOperation.IsDone)
                {
                    await UniTask.Yield();
                }

                if (loadingSceneLoadOperation.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(loadingSceneLoadOperation);
                }
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
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
