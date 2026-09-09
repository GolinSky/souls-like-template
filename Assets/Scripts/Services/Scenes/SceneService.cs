using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes.Data;
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
                return _sceneModel.GetSceneById(SceneManager.GetActiveScene());
            }
        }

        public SceneType DefaultScene => _sceneModel.DefaultScene;

        public SceneService(SceneModel sceneModel)
        {
            _sceneModel = sceneModel;
        }

        public async UniTask LoadScene(SceneType sceneType)
        {
            if (_sceneModel.IsLoadingScene)
            {
                throw new InvalidOperationException("A scene transition is already in progress.");
            }

            _sceneModel.IsLoadingScene = true;
            try
            {
                SceneReference loadingScene = _sceneModel.GetScene(SceneType.Loading);
                var loadingSceneOperation = await LoadLoadingScene(loadingScene);

                TargetScene = sceneType;
                SceneReference targetScene = _sceneModel.GetScene(sceneType);
                var targetSceneOperation = await LoadSceneWithDependencies(sceneType, targetScene);

                OnProgressUpdated?.Invoke(1f);
                ActivateScene(targetSceneOperation, targetScene);
                await UnloadSceneAsync(loadingSceneOperation, loadingScene.ScenePath);
                OnSceneChanged?.Invoke(sceneType);
            }
            finally
            {
                _sceneModel.IsLoadingScene = false;
            }
        }


        public SceneType GetSceneType(string scenePathOrName)
        {
            return _sceneModel.GetSceneByPath(scenePathOrName);
        }

        private async UniTask<AsyncOperationHandle<SceneInstance>> LoadLoadingScene(SceneReference loadingScene)
        {
            var operation = StartSceneLoad(loadingScene, LoadSceneMode.Single);
            await WaitForSceneLoads(new[] { operation }, 1);
            return operation;
        }

        private async UniTask<AsyncOperationHandle<SceneInstance>> LoadSceneWithDependencies(SceneType sceneType, SceneReference targetScene)
        {
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

            var targetSceneOperation = StartSceneLoad(targetScene, LoadSceneMode.Additive);
            sceneLoadOperations.Add(targetSceneOperation);
            await WaitForSceneLoads(sceneLoadOperations, totalSceneCount);
            return targetSceneOperation;
        }

        private static void ActivateScene(AsyncOperationHandle<SceneInstance> sceneLoadOperation, SceneReference targetScene)
        {
            Scene loadedTargetScene = sceneLoadOperation.Result.Scene;
            if (!loadedTargetScene.IsValid() || !loadedTargetScene.isLoaded)
            {
                throw new InvalidOperationException($"Scene '{targetScene.ScenePath}' did not finish loading.");
            }

            if (!SceneManager.SetActiveScene(loadedTargetScene))
            {
                throw new InvalidOperationException($"Failed to activate scene '{targetScene.ScenePath}'.");
            }
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
                    if (operation.Status == AsyncOperationStatus.Failed)
                    {
                        throw new InvalidOperationException("A scene failed to load.", operation.OperationException);
                    }
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
