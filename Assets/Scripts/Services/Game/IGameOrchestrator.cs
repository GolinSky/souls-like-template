using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes.Data;

namespace SoulsLike.Services
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
