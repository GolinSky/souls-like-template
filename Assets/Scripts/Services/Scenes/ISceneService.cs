using System;
using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes.Data;

namespace SoulsLike.Services.Scenes
{
    public interface ISceneService 
    {
        event Action<float> OnProgressUpdated;
        event Action<SceneType> OnSceneChanged;
        UniTask LoadScene(SceneType sceneType);
        bool TryGetScenePath(SceneType sceneType, out string scenePath);
        SceneType GetSceneType(string scenePathOrName);
        SceneType CurrentScene { get; }
    }
}
