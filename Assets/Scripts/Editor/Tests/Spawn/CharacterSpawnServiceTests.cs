using NUnit.Framework;
using SoulsLike.Services.Save;
using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using SoulsLike.Services.Spawn;
using SoulsLike.Services.Travel.Data;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Spawn
{
    public sealed class CharacterSpawnServiceTests
    {
        private LocationData _locationData;

        [SetUp]
        public void SetUp()
        {
            _locationData = ScriptableObject.CreateInstance<LocationData>();
            SerializedObject serializedLocationData = new SerializedObject(_locationData);
            SerializedProperty locations = serializedLocationData.FindProperty("locations");
            locations.arraySize = 1;
            SerializedProperty location = locations.GetArrayElementAtIndex(0);
            location.FindPropertyRelative("id").intValue = (int)SceneType.Workshop;
            SerializedProperty graces = location.FindPropertyRelative("graces");
            graces.arraySize = 1;
            graces.GetArrayElementAtIndex(0).FindPropertyRelative("id").intValue =
                (int)GraceId.WorkshopGrace01;
            serializedLocationData.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_locationData);
        }

        [Test]
        public void PrepareResume_ExistingSavedPosition_UsesWorldPositionArrival()
        {
            var save = new FakeSaveService
            {
                Data = new CharacterSpawnData
                {
                    HasCurrentPosition = true,
                    CurrentScene = SceneType.Sandbox,
                    CurrentPosition = new Vector3(1f, 2f, 3f)
                }
            };
            CharacterSpawnService service = CreateService(save);

            Assert.That(service.PrepareResume(), Is.EqualTo(SceneType.Sandbox));
            Assert.That(service.TryConsumeSpawn(out Vector3 position, out bool startsOnGrace), Is.True);
            Assert.That(position, Is.EqualTo(new Vector3(1f, 2f, 3f)));
            Assert.That(startsOnGrace, Is.False);
        }

        [Test]
        public void SaveLastGrace_PrepareResume_UsesGraceRestArrival()
        {
            var save = new FakeSaveService();
            CharacterSpawnService service = CreateService(save);

            service.SaveLastGrace(GraceId.WorkshopGrace01);

            Assert.That(service.PrepareResume(), Is.EqualTo(SceneType.Workshop));
            Assert.That(service.TryGetPendingGrace(out GraceId graceId), Is.True);
            Assert.That(graceId, Is.EqualTo(GraceId.WorkshopGrace01));

            service.ResolvePendingGrace(new Vector3(4f, 5f, 6f));

            Assert.That(service.TryConsumeSpawn(out Vector3 position, out bool startsOnGrace), Is.True);
            Assert.That(position, Is.EqualTo(new Vector3(4f, 5f, 6f)));
            Assert.That(startsOnGrace, Is.True);
        }

        [Test]
        public void SaveCurrentPosition_ClearsGraceResumeIntent()
        {
            var save = new FakeSaveService();
            CharacterSpawnService service = CreateService(save);
            service.SaveLastGrace(GraceId.WorkshopGrace01);
            service.SaveCurrentPosition(new Vector3(7f, 8f, 9f));

            Assert.That(service.PrepareResume(), Is.EqualTo(SceneType.Workshop));
            Assert.That(service.TryConsumeSpawn(out Vector3 position, out bool startsOnGrace), Is.True);
            Assert.That(position, Is.EqualTo(new Vector3(7f, 8f, 9f)));
            Assert.That(startsOnGrace, Is.False);
        }

        private CharacterSpawnService CreateService(FakeSaveService save)
        {
            return new CharacterSpawnService(
                save,
                _locationData,
                new FakeSceneService { CurrentScene = SceneType.Workshop, DefaultScene = SceneType.DefaultLocation });
        }

        private sealed class FakeSaveService : ISaveService
        {
            public CharacterSpawnData Data;

            public bool Exists(string fileName) => Data != null;

            public void Save<T>(string fileName, T data)
            {
                Data = (CharacterSpawnData)(object)data;
            }

            public T Load<T>(string fileName)
            {
                return (T)(object)Data;
            }

            public void Delete(string fileName)
            {
                Data = null;
            }

            public void DeleteAll()
            {
                Data = null;
            }
        }

        private sealed class FakeSceneService : ISceneService
        {
            public event System.Action<float> OnProgressUpdated;
            public event System.Action<SceneType> OnSceneChanged;
            public SceneType CurrentScene { get; set; }
            public SceneType DefaultScene { get; set; }

            public Cysharp.Threading.Tasks.UniTask LoadScene(SceneType sceneType) => default;

            public SceneType GetSceneType(string scenePathOrName) => default;
        }
    }
}
