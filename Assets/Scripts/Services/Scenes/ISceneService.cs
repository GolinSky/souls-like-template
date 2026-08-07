using System;
using Cysharp.Threading.Tasks;
using MultiPlayerTemplate.Services.Scenes.Data;

namespace MultiPlayerTemplate.Services.Scenes
{
    public interface ISceneService 
    {
        event Action<float> OnProgressUpdated;
        UniTask LoadScene(SceneType sceneType);
        bool TryGetScenePath(SceneType sceneType, out string scenePath);
        SceneType GetSceneType(string scenePathOrName);
        SceneType CurrentScene { get; }
    }
}
