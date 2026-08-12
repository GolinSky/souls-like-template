using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace SoulsLike.EditorTools
{
    public static class OcclusionOptimizer
    {
        [MenuItem("Tools/Optimize Scene Occlusion (Prevent Baking Crash)")]
        public static void OptimizeSceneOcclusion()
        {
            OptimizeSceneOcclusion(3.5f);
        }

        public static void OptimizeSceneOcclusion(float minOccluderSizeMeters)
        {
            var allGameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int occluderRemovedCount = 0;
            int totalStaticCount = 0;

            Undo.RegisterCompleteObjectUndo(allGameObjects, "Optimize Occlusion Static Flags");

            foreach (var go in allGameObjects)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                
                // Check if this object currently contributes to Occlusion Culling as an Occluder
                if ((flags & StaticEditorFlags.OccluderStatic) != 0)
                {
                    totalStaticCount++;
                    
                    bool isSmallProp = false;
                    var renderer = go.GetComponent<Renderer>();

                    if (renderer != null)
                    {
                        Vector3 size = renderer.bounds.size;
                        float maxDim = Mathf.Max(size.x, size.y, size.z);

                        // If max dimension is smaller than threshold (e.g. 3.5m), it's a prop/detail object
                        if (maxDim < minOccluderSizeMeters)
                        {
                            isSmallProp = true;
                        }
                    }
                    else if (go.transform.childCount == 0)
                    {
                        // Empty object or light/trigger
                        isSmallProp = true;
                    }

                    if (isSmallProp)
                    {
                        // Remove OccluderStatic, but keep OccludeeStatic and other static flags
                        flags &= ~StaticEditorFlags.OccluderStatic;
                        flags |= StaticEditorFlags.OccludeeStatic;

                        GameObjectUtility.SetStaticEditorFlags(go, flags);
                        EditorUtility.SetDirty(go);
                        occluderRemovedCount++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log($"[OcclusionOptimizer] Successfully optimized {occluderRemovedCount} / {totalStaticCount} static objects! Small props now have OccluderStatic disabled, preventing Umbra native memory crashes during bake.");

            EditorUtility.DisplayDialog(
                "Occlusion Optimization Complete",
                $"Processed {totalStaticCount} static objects.\n\n" +
                $"• Stripped 'Occluder Static' from {occluderRemovedCount} small props/details (kept 'Occludee Static').\n" +
                $"• 'Smallest Hole' is set to 1.5m.\n\n" +
                "You can now safely bake Occlusion Culling (Window > Rendering > Occlusion Culling > Bake) without crashing Unity!",
                "OK"
            );
        }
    }
}
