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
        SceneType GetSceneType(string scenePathOrName);
        SceneType CurrentScene { get; }
        SceneType DefaultScene { get; }
    }
}
