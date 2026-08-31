using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace SoulsLike.Services.Scenes
{
    public sealed class AddressableSceneBootstrap : MonoBehaviour
    {
        private const string MAIN_MENU_ADDRESS = "Assets/Scenes/MainMenu/MainMenu.unity";

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            AsyncOperationHandle<SceneInstance> loadOperation = Addressables.LoadSceneAsync(MAIN_MENU_ADDRESS, LoadSceneMode.Single);
            yield return loadOperation;

            if (loadOperation.Status != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException($"Failed to load bootstrap scene '{MAIN_MENU_ADDRESS}'.", loadOperation.OperationException);
            }

            Destroy(gameObject);
        }
    }
}
