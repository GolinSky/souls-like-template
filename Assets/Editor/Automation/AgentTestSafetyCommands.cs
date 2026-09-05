#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Pipeline.Commands;
using UnityEngine.SceneManagement;

public static class AgentTestSafetyCommands
{
    [CliCommand(
        "assert_test_ready",
        "Fails when any loaded scene has unsaved changes.")]
    public static string AssertTestReady()
    {
        var dirtyScenes = new List<string>();
        for (var index = 0; index < SceneManager.sceneCount; index++)
        {
            var scene = SceneManager.GetSceneAt(index);
            if (!scene.isLoaded || !scene.isDirty)
            {
                continue;
            }

            var identifier = string.IsNullOrWhiteSpace(scene.path)
                ? $"{scene.name} [UNTITLED]"
                : scene.path;
            dirtyScenes.Add(identifier);
        }

        if (dirtyScenes.Count > 0)
        {
            throw new InvalidOperationException(
                "BLOCKED_DIRTY_SCENE: " + string.Join("; ", dirtyScenes));
        }

        return "TEST_READY";
    }
}
#endif
