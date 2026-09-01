using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services
{
    public class GameOrchestrator: IGameOrchestrator, IInitializable
    {
        private readonly ISceneService _sceneService;
        private readonly IInputService _inputService;

        public GameOrchestrator(ISceneService sceneService, IInputService inputService)
        {
            _sceneService = sceneService;
            _inputService = inputService;
        }

        public void Initialize()
        {
            LoadMenu().Forget();
        }

        public async UniTaskVoid LoadMenu()
        {
            await _sceneService.LoadScene(SceneType.MainMenu);
        }

        public async UniTask LoadLevel(SceneType sceneType)
        {
            await _sceneService.LoadScene(sceneType);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
