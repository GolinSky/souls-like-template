#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using SoulsLike.Services.Layer;
using SoulsLike.Services.Layer.Data;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Layer
{
    public sealed class LayerServiceTests
    {
        private LayerData _data;
        private LayerService _service;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<LayerData>();
            _service = new LayerService(_data);
        }

        [TearDown]
        public void TearDown()
        {
            if (_data != null)
            {
                UnityEngine.Object.DestroyImmediate(_data);
            }
        }

        [Test]
        public void EveryOneBitMask_ReturnsCorrectUnityIndex()
        {
            for (int index = 0; index < 32; index++)
            {
                _data.SetLayerMaskForTest(LayerName.Player, 1 << index);

                int layer = _service.GetLayer(LayerName.Player);
                LayerMask mask = _service.GetLayerMask(LayerName.Player);

                Assert.That(layer, Is.EqualTo(index));
                Assert.That(mask.value, Is.EqualTo(1 << index));
            }
        }

        [Test]
        public void Layer31OneBitMask_IsAcceptedAndReturns31()
        {
            _data.SetLayerMaskForTest(LayerName.Default, 1 << 31);

            int layer = _service.GetLayer(LayerName.Default);
            Assert.That(layer, Is.EqualTo(31));
        }

        [Test]
        public void MissingLayerName_FailsDictionaryLookup()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetLayer(LayerName.Player));
            Assert.Throws<KeyNotFoundException>(() => _service.GetLayerMask(LayerName.Player));
        }

        [Test]
        public void MissingSharedMask_FailsDictionaryLookup()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetMask(LayerMaskName.PreviewCamera));
        }

        [Test]
        public void ValidSharedMask_ReturnsUnchanged()
        {
            LayerMask expectedMask = (1 << 0) | (1 << 8) | (1 << 9);
            _data.SetSharedMaskForTest(LayerMaskName.NavigationBake, expectedMask);

            LayerMask actual = _service.GetMask(LayerMaskName.NavigationBake);

            Assert.That(actual.value, Is.EqualTo(expectedMask.value));
        }

        [Test]
        public void SetLayer_NonRecursive_ChangesOnlyRoot()
        {
            _data.SetLayerMaskForTest(LayerName.Preview, 1 << 10);

            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            root.layer = 0;
            child.layer = 0;

            try
            {
                _service.SetLayer(root, LayerName.Preview, recursive: false);

                Assert.That(root.layer, Is.EqualTo(10));
                Assert.That(child.layer, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SetLayer_Recursive_ChangesRootAndAllDescendants()
        {
            _data.SetLayerMaskForTest(LayerName.Preview, 1 << 10);

            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");
            GameObject grandChild = new GameObject("GrandChild");
            child.transform.SetParent(root.transform);
            grandChild.transform.SetParent(child.transform);
            root.layer = 0;
            child.layer = 0;
            grandChild.layer = 0;

            try
            {
                _service.SetLayer(root, LayerName.Preview, recursive: true);

                Assert.That(root.layer, Is.EqualTo(10));
                Assert.That(child.layer, Is.EqualTo(10));
                Assert.That(grandChild.layer, Is.EqualTo(10));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

    }
}
#endif
