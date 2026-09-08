namespace SoulsLike.Services.Scenes.Data;
public sealed class SceneReference(string path)
{
    public string ScenePath => path;
    public bool IsEmpty => string.IsNullOrEmpty(path);
}
