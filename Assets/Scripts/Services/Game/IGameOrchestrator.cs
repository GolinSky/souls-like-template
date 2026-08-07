using Cysharp.Threading.Tasks;
using MultiPlayerTemplate.Services.Scenes.Data;

namespace MultiPlayerTemplate.Services
{
    /// <summary>
    /// Main Orchestrator
    /// </summary>
    public interface IGameOrchestrator 
    {
        UniTask LoadLevel(SceneType sceneType);
        UniTaskVoid LoadMenu();
        void ExitGame();
    }
}
