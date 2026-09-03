#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoulsLike.Services.Layer;
using SoulsLike.Services.Layer.Data;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Layer
{
    public sealed class LayerConfigurationTests
    {
        [Test]
        public void CanonicalLayerDataAsset_Exists()
        {
            var data = AssetDatabase.LoadAssetAtPath<LayerData>(LayerDataEditorProvider.LAYER_DATA_PATH);
            Assert.That(data, Is.Not.Null, $"Canonical LayerData asset must exist at '{LayerDataEditorProvider.LAYER_DATA_PATH}'.");
        }

        [Test]
        public void UnityLayers_PreviewAndInteraction_ExistAtExpectedIndices()
        {
            int previewIndex = LayerMask.NameToLayer("Preview");
            Assert.That(previewIndex, Is.EqualTo(10), "Unity layer 'Preview' must be at index 10.");

            int interactionIndex = LayerMask.NameToLayer("Interaction");
            Assert.That(interactionIndex, Is.EqualTo(11), "Unity layer 'Interaction' must be at index 11.");
        }

        [Test]
        public void CanonicalLayerData_AllLayerNameEntriesAreConfiguredAndOneHot()
        {
            var data = LayerDataEditorProvider.LoadLayerData();
            IReadOnlyDictionary<LayerName, LayerMask> singleLayers = data.SingleLayers;
            Assert.That(singleLayers, Is.Not.Null);

            foreach (LayerName name in Enum.GetValues(typeof(LayerName)))
            {
                Assert.That(singleLayers.ContainsKey(name), Is.True, $"Missing LayerName key '{name}' in LayerData.");
                LayerMask mask = singleLayers[name];
                uint bits = unchecked((uint)mask.value);
                Assert.That(bits, Is.Not.Zero, $"LayerName '{name}' mask must not be zero.");
                Assert.That((bits & (bits - 1)), Is.EqualTo(0), $"LayerName '{name}' mask (0x{bits:X8}) must have exactly one bit set.");

                int bitIndex = -1;
                for (int i = 0; i < 32; i++)
                {
                    if ((bits & (1u << i)) != 0)
                    {
                        bitIndex = i;
                        break;
                    }
                }

                string unityName = LayerMask.LayerToName(bitIndex);
                Assert.That(unityName, Is.EqualTo(name.ToString()).IgnoreCase,
                    $"LayerName '{name}' maps to layer {bitIndex} ('{unityName}'), expected matching Unity layer name.");
            }
        }

        [Test]
        public void CanonicalLayerData_AllSharedMaskEntriesAreConfiguredAndNonzero()
        {
            var data = LayerDataEditorProvider.LoadLayerData();
            IReadOnlyDictionary<LayerMaskName, LayerMask> sharedMasks = data.SharedMasks;
            Assert.That(sharedMasks, Is.Not.Null);

            foreach (LayerMaskName name in Enum.GetValues(typeof(LayerMaskName)))
            {
                Assert.That(sharedMasks.ContainsKey(name), Is.True, $"Missing LayerMaskName key '{name}' in LayerData.");
                LayerMask mask = sharedMasks[name];
                Assert.That(mask.value, Is.Not.Zero, $"LayerMaskName '{name}' mask must not be zero.");
            }
        }

        [Test]
        public void CanonicalLayerData_PreviewCameraMask_ContainsPreviewLayerOnly()
        {
            LayerMask mask = LayerDataEditorProvider.GetMask(LayerMaskName.PreviewCamera);
            int previewLayer = LayerMask.NameToLayer("Preview");
            Assert.That(mask.value, Is.EqualTo(1 << previewLayer));
        }

        [Test]
        public void CanonicalLayerData_InteractionProbeMask_ContainsDefaultAndInteraction()
        {
            LayerMask mask = LayerDataEditorProvider.GetMask(LayerMaskName.InteractionProbe);
            int defaultBit = 1 << LayerMask.NameToLayer("Default");
            int interactionBit = 1 << LayerMask.NameToLayer("Interaction");
            int expected = defaultBit | interactionBit;

            Assert.That(mask.value & defaultBit, Is.EqualTo(defaultBit), "InteractionProbe must include Default during migration.");
            Assert.That(mask.value & interactionBit, Is.EqualTo(interactionBit), "InteractionProbe must include Interaction.");
            Assert.That(mask.value, Is.EqualTo(expected));
        }

        [Test]
        public void CanonicalLayerData_NavigationBakeMask_ContainsDefaultWalkableStairs()
        {
            LayerMask mask = LayerDataEditorProvider.GetMask(LayerMaskName.NavigationBake);
            int defaultBit = 1 << LayerMask.NameToLayer("Default");
            int walkableBit = 1 << LayerMask.NameToLayer("Walkable");
            int stairsBit = 1 << LayerMask.NameToLayer("Stairs");
            int expected = defaultBit | walkableBit | stairsBit;

            Assert.That(mask.value & defaultBit, Is.EqualTo(defaultBit));
            Assert.That(mask.value & walkableBit, Is.EqualTo(walkableBit));
            Assert.That(mask.value & stairsBit, Is.EqualTo(stairsBit));
            Assert.That(mask.value, Is.EqualTo(expected));
        }
    }
}
#endif
