using UnityEngine.AddressableAssets;
namespace UnityEngine.SceneManagement;
public readonly struct Scene(string scenePath)
{
    public string path => scenePath;
    public bool isLoaded => Addressables.Loaded.Contains(scenePath);
    public bool IsValid() => !string.IsNullOrEmpty(scenePath);
}
