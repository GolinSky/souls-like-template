#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Entities.Character.Components.Targeting;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Targeting
{
    public sealed class TargetLockComponentTests
    {
        [Test]
        public void TargetTransform_DefaultAnchor_ResolvesComponentTransform()
        {
            GameObject target = new("Target");

            try
            {
                TargetLockComponent component = target.AddComponent<TargetLockComponent>();

                Assert.That(component.TargetTransform, Is.SameAs(target.transform));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void TargetTransform_CustomAnchor_ResolvesAssignedTransform()
        {
            GameObject target = new("Target");
            GameObject customAnchor = new("CustomAnchor");

            try
            {
                TargetLockComponent component = target.AddComponent<TargetLockComponent>();
                using (var serializedComponent = new SerializedObject(component))
                {
                    serializedComponent.FindProperty("anchorType").enumValueIndex =
                        (int)TargetLockAnchorType.Custom;
                    serializedComponent.FindProperty("customTargetPoint").objectReferenceValue =
                        customAnchor.transform;
                    serializedComponent.ApplyModifiedPropertiesWithoutUndo();
                }

                Assert.That(component.TargetTransform, Is.SameAs(customAnchor.transform));
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(customAnchor);
            }
        }

        [TestCase("Assets/Prefabs/Models/Character/Character.prefab")]
        [TestCase("Assets/Prefabs/Models/Enemy/ErikaMeleeEnemy.prefab")]
        public void AuthoredPrefab_ResolvesTargetLockComponent(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.That(prefab, Is.Not.Null, path);
            Assert.That(prefab.GetComponentInChildren<TargetLockComponent>(true), Is.Not.Null, path);
        }
    }
}
#endif
