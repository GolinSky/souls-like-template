namespace UnityEngine.SceneManagement;
public static class SceneManager
{
    public static Scene Active;
    public static bool FailActivation;
    public static Scene GetActiveScene() => Active;
    public static bool SetActiveScene(Scene scene)
    {
        if (FailActivation) return false;
        Active = scene;
        return true;
    }
}
