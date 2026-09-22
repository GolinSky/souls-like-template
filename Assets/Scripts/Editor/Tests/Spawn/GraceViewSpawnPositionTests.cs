#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoulsLike.Interactions;
using SoulsLike.Services.Layer;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Spawn
{
    public sealed class GraceViewSpawnPositionTests
    {
        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in _objects)
            {
                if (gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }
            }

            _objects.Clear();
        }

        [Test]
        public void GetGroundSpawnPosition_ElevatedGraceOverWalkableFloor_ReturnsFloorHeight()
        {
            GraceView grace = Track(new GameObject("Grace")).AddComponent<GraceView>();
            grace.transform.position = new Vector3(2f, 5f, 3f);

            GameObject floor = Track(new GameObject("Walkable Floor"));
            floor.layer = LayerMask.NameToLayer(nameof(LayerName.Walkable));
            floor.transform.position = new Vector3(2f, 1f, 3f);
            BoxCollider collider = floor.AddComponent<BoxCollider>();
            collider.size = new Vector3(4f, 0.5f, 4f);
            Physics.SyncTransforms();

            Vector3 spawnPosition = grace.GetGroundSpawnPosition();

            Assert.That(spawnPosition, Is.EqualTo(new Vector3(2f, 1.25f, 3f)));
        }

        [Test]
        public void GetGroundSpawnPosition_MissingWalkableFloor_Throws()
        {
            GraceView grace = Track(new GameObject("Grace")).AddComponent<GraceView>();
            grace.transform.position = new Vector3(10000f, 10000f, 10000f);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => grace.GetGroundSpawnPosition());

            Assert.That(exception.Message, Does.Contain("no Walkable ground"));
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
#endif
