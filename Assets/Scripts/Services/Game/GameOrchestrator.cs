using System;
using Cysharp.Threading.Tasks;
using MultiPlayerTemplate.Services.Scenes;
using MultiPlayerTemplate.Services.Scenes.Data;
using UnityEngine;
using VContainer.Unity;

namespace MultiPlayerTemplate.Services
{
    public class GameOrchestrator: IGameOrchestrator, ITickable
    {
        private const float DISCOVERY_TIMEOUT_SECONDS = 5f;
        private const float RETRY_DELAY_SECONDS = 1f;
        private const int   MAX_HOST_RETRIES = 2;

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
