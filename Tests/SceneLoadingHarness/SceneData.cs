using UnityEngine.SceneManagement;
namespace SoulsLike.Services.Scenes.Data;
public sealed class SceneData
{
    public SceneType DefaultScene => SceneType.DefaultLocation;
    public SceneReference GetScene(SceneType type) => new(type.ToString());
    public SceneType GetSceneById(Scene scene) => GetSceneByPath(scene.path);
    public SceneType GetSceneByPath(string path) => Enum.TryParse<SceneType>(path, out var type) ? type : SceneType.Undefined;
    public bool TryGetDependencies(SceneType type, out SceneReference[] scenes)
    {
        scenes = [new("Zone1"), new("Zone2"), new("Zone3")];
        return true;
    }
}
