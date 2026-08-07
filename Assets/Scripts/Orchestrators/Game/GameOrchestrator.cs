using System;
using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services
{
    public class GameOrchestrator: IGameOrchestrator, ITickable
    {
        private readonly ISceneService _sceneService;
        private readonly IInputService _inputService;

        public GameOrchestrator(ISceneService sceneService, IInputService inputService)
        {
            _sceneService = sceneService;
            _inputService = inputService;
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
        

        // ── tick ──────────────────────────────────────────────────────

        public void Tick()
        {
            // if (_inputService.CharacterActions.Pause.WasPressedThisFrame())
            // {
            //     Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
            //         ? CursorLockMode.None
            //         : CursorLockMode.Locked;
            // }
        }
    }
}
