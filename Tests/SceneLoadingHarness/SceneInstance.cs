using UnityEngine.SceneManagement;
namespace UnityEngine.ResourceManagement.ResourceProviders;
public readonly struct SceneInstance(Scene scene)
{
    public Scene Scene => scene;
}
