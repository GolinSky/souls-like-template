#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SoulsLike.Editor
{
    public static class DefaultLocationNavMeshBakeTool
    {
        private const string DEFAULT_LOCATION_FOLDER = "Assets/Scenes/DefaultLocation";
        private const string MAIN_SCENE_PATH =
            DEFAULT_LOCATION_FOLDER + "/DefaultLocation.unity";
        private const string NAVIGATION_FOLDER =
            DEFAULT_LOCATION_FOLDER + "/Navigation";
        private const string NAVIGATION_ROOT_NAME = "Navigation";
        private const string NAVIGATION_LINKS_ROOT_NAME = "NavMeshLinks";
        private const string LEGACY_NAVIGATION_ROOT_NAME = "EnemyNavigation";
        private const string LEGACY_NAVIGATION_DATA_PATH =
            NAVIGATION_FOLDER + "/EnemyNavigation.asset";
        private const float LINK_MAX_DISTANCE = 8f;
        private const float LINK_WIDTH = 1f;
        private const int NAVIGATION_LAYER_MASK = 55;

        private static readonly string[] _excludedSceneNames =
        {
            "Rocks",
            "Zone_03",
            "Zone_05"
        };

        public static string[] NavigationScenePaths =>
            GetDefaultLocationScenes()
                .Where(IsNavigationScene)
                .ToArray();

        [MenuItem("Tools/SoulsLike/Bake DefaultLocation NavMeshes")]
        public static void BakeNavigation()
        {
            EnsureFolder(NAVIGATION_FOLDER);

            string[] allScenePaths = GetDefaultLocationScenes();
            string[] navigationScenePaths = allScenePaths
                .Where(IsNavigationScene)
                .ToArray();
            if (navigationScenePaths.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No navigation scenes were found under '{DEFAULT_LOCATION_FOLDER}'.");
            }

            var changedScenePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var changedAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string scenePath in allScenePaths)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    throw new InvalidOperationException(
                        $"Scene '{scenePath}' must be loaded for the navigation bake.");
                }

                bool changed = ClearNavigationObjects(scene);
                if (!IsNavigationScene(scenePath))
                {
                    DeleteNavigationData(scene, changedAssetPaths);
                    if (changed)
                    {
                        SaveScene(scene);
                        changedScenePaths.Add(scenePath);
                    }

                    continue;
                }

                NavMeshSurface surface = CreateSurface(scene);
                surface.BuildNavMesh();
                PersistSurfaceData(surface, changedAssetPaths);
                SaveScene(scene);
                changedScenePaths.Add(scenePath);
            }

            DeleteLegacyNavigationData(changedAssetPaths);

            Scene mainScene = OpenDefaultLocationScenes(allScenePaths);
            NavMeshSurface[] surfaces = GetNavigationSurfaces(navigationScenePaths);
            int linkCount = RebuildNavigationLinks(mainScene, surfaces);
            SaveScene(mainScene);
            changedScenePaths.Add(mainScene.path);

            AssetDatabase.SaveAssets();
            ForceReserialize(changedScenePaths, changedAssetPaths);

            Debug.Log(
                $"[DefaultLocationNavMeshBakeTool] Baked {surfaces.Length} navmesh scene(s) "
                + $"and created {linkCount} link(s) in {Path.GetFileName(MAIN_SCENE_PATH)}.");
        }

        public static void FinalizeNavigationBake()
        {
            BakeNavigation();
        }

        private static string[] GetDefaultLocationScenes()
        {
            return AssetDatabase.FindAssets("t:Scene", new[] { DEFAULT_LOCATION_FOLDER })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                .OrderBy(GetSceneSortKey)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static int GetSceneSortKey(string scenePath)
        {
            if (scenePath.Equals(MAIN_SCENE_PATH, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            return Path.GetFileNameWithoutExtension(scenePath).StartsWith(
                    "Zone_",
                    StringComparison.OrdinalIgnoreCase)
                ? 1
                : 2;
        }

        private static bool IsNavigationScene(string scenePath)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            return !_excludedSceneNames.Contains(sceneName, StringComparer.OrdinalIgnoreCase);
        }

        private static Scene OpenDefaultLocationScenes(IReadOnlyList<string> scenePaths)
        {
            Scene mainScene = default;
            for (int index = 0; index < scenePaths.Count; index++)
            {
                OpenSceneMode mode = index == 0
                    ? OpenSceneMode.Single
                    : OpenSceneMode.Additive;
                Scene scene = EditorSceneManager.OpenScene(scenePaths[index], mode);
                if (scenePaths[index].Equals(MAIN_SCENE_PATH, StringComparison.OrdinalIgnoreCase))
                {
                    mainScene = scene;
                }
            }

            if (!mainScene.IsValid() || !mainScene.isLoaded)
            {
                throw new InvalidOperationException(
                    $"Main navigation scene '{MAIN_SCENE_PATH}' must be loaded.");
            }

            EditorSceneManager.SetActiveScene(mainScene);
            return mainScene;
        }

        private static bool ClearNavigationObjects(Scene scene)
        {
            bool changed = false;
            foreach (GameObject root in scene.GetRootGameObjects().ToArray())
            {
                if (root.name.Equals(LEGACY_NAVIGATION_ROOT_NAME, StringComparison.Ordinal)
                    || root.name.Equals(NAVIGATION_ROOT_NAME, StringComparison.Ordinal))
                {
                    Object.DestroyImmediate(root);
                    changed = true;
                    continue;
                }

                foreach (NavMeshSurface surface in root.GetComponentsInChildren<NavMeshSurface>(true)
                             .ToArray())
                {
                    Object.DestroyImmediate(surface);
                    changed = true;
                }

                foreach (NavMeshLink link in root.GetComponentsInChildren<NavMeshLink>(true)
                             .ToArray())
                {
                    Object.DestroyImmediate(link);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            return changed;
        }

        private static NavMeshSurface CreateSurface(Scene scene)
        {
            GameObject navigation = new(NAVIGATION_ROOT_NAME);
            SceneManager.MoveGameObjectToScene(navigation, scene);

            GameObject surfaceObject = new($"{scene.name}_NavMeshSurface");
            SceneManager.MoveGameObjectToScene(surfaceObject, scene);
            surfaceObject.transform.SetParent(navigation.transform, false);

            NavMeshSurface surface = surfaceObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = NAVIGATION_LAYER_MASK;
            surface.ignoreNavMeshAgent = true;
            surface.ignoreNavMeshObstacle = true;

            EditorUtility.SetDirty(surface);
            EditorSceneManager.MarkSceneDirty(scene);
            return surface;
        }

        private static void PersistSurfaceData(
            NavMeshSurface surface,
            ISet<string> changedAssetPaths)
        {
            NavMeshData navigationData = surface.navMeshData
                ?? throw new InvalidOperationException(
                    $"Scene '{surface.gameObject.scene.path}' produced no NavMeshData.");

            string dataPath = GetNavigationDataPath(surface.gameObject.scene);
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(dataPath) != null)
            {
                AssetDatabase.DeleteAsset(dataPath);
            }

            navigationData.name = Path.GetFileNameWithoutExtension(dataPath);
            AssetDatabase.CreateAsset(navigationData, dataPath);
            surface.navMeshData = navigationData;
            EditorUtility.SetDirty(surface);
            changedAssetPaths.Add(dataPath);
        }

        private static string GetNavigationDataPath(Scene scene) =>
            $"{NAVIGATION_FOLDER}/{scene.name}NavMesh.asset";

        private static NavMeshSurface[] GetNavigationSurfaces(
            IEnumerable<string> scenePaths)
        {
            var surfaces = new List<NavMeshSurface>();
            foreach (string scenePath in scenePaths)
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    throw new InvalidOperationException(
                        $"Scene '{scenePath}' must be loaded before links are generated.");
                }

                GameObject navigation = FindRoot(scene, NAVIGATION_ROOT_NAME)
                    ?? throw new InvalidOperationException(
                        $"Scene '{scenePath}' does not contain a '{NAVIGATION_ROOT_NAME}' root.");
                NavMeshSurface surface =
                    navigation.GetComponentInChildren<NavMeshSurface>(true)
                    ?? throw new InvalidOperationException(
                        $"Scene '{scenePath}' does not contain a NavMeshSurface.");

                if (surface.navMeshData == null)
                {
                    throw new InvalidOperationException(
                        $"Scene '{scenePath}' has no baked NavMeshData.");
                }

                surfaces.Add(surface);
            }

            return surfaces.ToArray();
        }

        private static int RebuildNavigationLinks(
            Scene mainScene,
            IReadOnlyList<NavMeshSurface> surfaces)
        {
            GameObject navigation = FindRoot(mainScene, NAVIGATION_ROOT_NAME);
            if (navigation == null)
            {
                navigation = new GameObject(NAVIGATION_ROOT_NAME);
                SceneManager.MoveGameObjectToScene(navigation, mainScene);
            }

            GameObject linksRoot = new(NAVIGATION_LINKS_ROOT_NAME);
            SceneManager.MoveGameObjectToScene(linksRoot, mainScene);
            linksRoot.transform.SetParent(navigation.transform, false);

            SurfaceRecord[] records = CreateSurfaceRecords(surfaces);
            int linkCount = 0;

            for (int i = 0; i < records.Length; i++)
            {
                for (int j = i + 1; j < records.Length; j++)
                {
                    if (TryCreateLink(records[i], records[j], linksRoot.transform))
                    {
                        linkCount++;
                    }
                }
            }

            if (linkCount == 0)
            {
                Debug.LogWarning(
                    "[DefaultLocationNavMeshBakeTool] No NavMeshLink objects were created.");
            }

            EditorUtility.SetDirty(linksRoot);
            EditorSceneManager.MarkSceneDirty(mainScene);
            return linkCount;
        }

        private static bool TryCreateLink(
            SurfaceRecord first,
            SurfaceRecord second,
            Transform parent)
        {
            if (first.Surface.agentTypeID != second.Surface.agentTypeID)
            {
                return false;
            }

            if (!TryGetClosestConnection(first, second, out Vector3 firstPoint,
                    out Vector3 secondPoint, out float distance)
                || distance > LINK_MAX_DISTANCE)
            {
                return false;
            }

            GameObject linkObject = new(
                $"NavMeshLink_{first.SceneName}_To_{second.SceneName}");
            SceneManager.MoveGameObjectToScene(linkObject, parent.gameObject.scene);
            linkObject.transform.SetParent(parent, false);
            Vector3 origin = (firstPoint + secondPoint) * 0.5f;
            linkObject.transform.position = origin;

            NavMeshLink link = linkObject.AddComponent<NavMeshLink>();
            link.agentTypeID = first.Surface.agentTypeID;
            link.startPoint = firstPoint - origin;
            link.endPoint = secondPoint - origin;
            link.width = LINK_WIDTH;
            link.bidirectional = true;
            link.area = 0;
            link.activated = true;

            EditorUtility.SetDirty(linkObject);
            EditorUtility.SetDirty(link);
            return true;
        }

        private static SurfaceRecord[] CreateSurfaceRecords(
            IReadOnlyList<NavMeshSurface> surfaces)
        {
            var records = new List<SurfaceRecord>();
            try
            {
                foreach (NavMeshSurface surface in surfaces)
                {
                    Vector3[] vertices = GetSurfaceVertices(surface);
                    if (vertices.Length == 0)
                    {
                        if (!surface.gameObject.scene.path.Equals(
                                MAIN_SCENE_PATH,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.LogWarning(
                                $"[DefaultLocationNavMeshBakeTool] Scene '{surface.gameObject.scene.name}' "
                                + "has no navmesh vertices and will not receive links.");
                        }

                        continue;
                    }

                    records.Add(new SurfaceRecord(surface, vertices));
                }
            }
            finally
            {
                RestoreActiveNavMeshData(surfaces);
            }

            return records.ToArray();
        }

        private static Vector3[] GetSurfaceVertices(NavMeshSurface surface)
        {
            NavMesh.RemoveAllNavMeshData();
            NavMesh.AddNavMeshData(
                surface.navMeshData,
                surface.transform.position,
                surface.transform.rotation);
            return NavMesh.CalculateTriangulation().vertices;
        }

        private static void RestoreActiveNavMeshData(
            IEnumerable<NavMeshSurface> surfaces)
        {
            NavMesh.RemoveAllNavMeshData();
            foreach (NavMeshSurface surface in surfaces)
            {
                if (surface.isActiveAndEnabled && surface.navMeshData != null)
                {
                    NavMesh.AddNavMeshData(
                        surface.navMeshData,
                        surface.transform.position,
                        surface.transform.rotation);
                }
            }
        }

        private static bool TryGetClosestConnection(
            SurfaceRecord first,
            SurfaceRecord second,
            out Vector3 firstPoint,
            out Vector3 secondPoint,
            out float distance)
        {
            firstPoint = default;
            secondPoint = default;
            distance = float.PositiveInfinity;
            if (first.Vertices.Count == 0 || second.Vertices.Count == 0)
            {
                return false;
            }

            float bestSqrDistance = float.PositiveInfinity;
            foreach (Vector3 firstVertex in first.Vertices)
            {
                foreach (Vector3 secondVertex in second.Vertices)
                {
                    float sqrDistance = (firstVertex - secondVertex).sqrMagnitude;
                    if (sqrDistance < bestSqrDistance)
                    {
                        bestSqrDistance = sqrDistance;
                        firstPoint = firstVertex;
                        secondPoint = secondVertex;
                    }
                }
            }

            distance = Mathf.Sqrt(bestSqrDistance);
            return !float.IsPositiveInfinity(distance);
        }

        private static void DeleteLegacyNavigationData(ISet<string> changedAssetPaths)
        {
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(LEGACY_NAVIGATION_DATA_PATH) != null)
            {
                AssetDatabase.DeleteAsset(LEGACY_NAVIGATION_DATA_PATH);
                changedAssetPaths.Add(LEGACY_NAVIGATION_DATA_PATH);
            }
        }

        private static void DeleteNavigationData(Scene scene, ISet<string> changedAssetPaths)
        {
            string dataPath = GetNavigationDataPath(scene);
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(dataPath) != null)
            {
                AssetDatabase.DeleteAsset(dataPath);
                changedAssetPaths.Add(dataPath);
            }
        }

        private static GameObject FindRoot(Scene scene, string name) =>
            scene.GetRootGameObjects().FirstOrDefault(root => root.name == name);

        private static void SaveScene(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ForceReserialize(
            IEnumerable<string> changedScenePaths,
            IEnumerable<string> changedAssetPaths)
        {
            string[] paths = changedScenePaths
                .Concat(changedAssetPaths)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(path => AssetDatabase.LoadMainAssetAtPath(path) != null)
                .ToArray();

            if (paths.Length > 0)
            {
                AssetDatabase.ForceReserializeAssets(
                    paths,
                    ForceReserializeAssetsOptions.ReserializeAssets);
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private readonly struct SurfaceRecord
        {
            public SurfaceRecord(
                NavMeshSurface surface,
                IReadOnlyList<Vector3> vertices)
            {
                Surface = surface;
                Vertices = vertices;
                SceneName = surface.gameObject.scene.name;
            }

            public NavMeshSurface Surface { get; }
            public IReadOnlyList<Vector3> Vertices { get; }
            public string SceneName { get; }
        }
    }
}
#endif
