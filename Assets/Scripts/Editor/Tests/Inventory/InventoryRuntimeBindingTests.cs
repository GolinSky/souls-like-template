#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SoulsLike.Entities.Character;
using SoulsLike.Ui.Inventory;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Inventory
{
    public sealed class InventoryRuntimeBindingTests
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
        public void GrantCurrency_RaisesCurrentHeldCurrency()
        {
            Character character = CreateCharacter();
            int changedCurrency = -1;
            character.CurrencyChanged += heldCurrency => changedCurrency = heldCurrency;

            character.GrantCurrency(1250);

            Assert.That(character.HeldCurrency, Is.EqualTo(1250));
            Assert.That(changedCurrency, Is.EqualTo(1250));
        }

        [Test]
        public void ViewStates_ControlVisibilityAndInteractionForEachColumn()
        {
            InventoryViewStateController controller = CreateViewStateController(
                out CanvasGroup grid,
                out CanvasGroup details,
                out CanvasGroup lore,
                out CanvasGroup stats);

            controller.SetState(InventoryViewState.DualPanel);
            AssertGroupState(grid, true);
            AssertGroupState(details, true);
            AssertGroupState(lore, false);
            AssertGroupState(stats, true);

            controller.SetState(InventoryViewState.LoreView);
            AssertGroupState(grid, true);
            AssertGroupState(details, false);
            AssertGroupState(lore, true);
            AssertGroupState(stats, true);

            controller.SetState(InventoryViewState.SimpleView);
            AssertGroupState(grid, true);
            AssertGroupState(details, false);
            AssertGroupState(lore, false);
            AssertGroupState(stats, false);
        }

        private Character CreateCharacter()
        {
            return Track(new GameObject("Character")).AddComponent<Character>();
        }

        private InventoryViewStateController CreateViewStateController(
            out CanvasGroup grid,
            out CanvasGroup details,
            out CanvasGroup lore,
            out CanvasGroup stats)
        {
            GameObject root = Track(new GameObject("Inventory View State Controller"));
            root.SetActive(false);
            InventoryViewStateController controller = root.AddComponent<InventoryViewStateController>();
            grid = CreateCanvasGroup("Grid");
            details = CreateCanvasGroup("Details");
            lore = CreateCanvasGroup("Lore");
            stats = CreateCanvasGroup("Stats");
            SetField(controller, "gridColumnGroup", grid);
            SetField(controller, "detailsColumnGroup", details);
            SetField(controller, "loreCardGroup", lore);
            SetField(controller, "statsColumnGroup", stats);
            root.SetActive(true);
            return controller;
        }

        private CanvasGroup CreateCanvasGroup(string name)
        {
            return Track(new GameObject(name)).AddComponent<CanvasGroup>();
        }

        private static void SetField(
            InventoryViewStateController controller,
            string fieldName,
            CanvasGroup value)
        {
            typeof(InventoryViewStateController)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(controller, value);
        }

        private static void AssertGroupState(CanvasGroup group, bool active)
        {
            Assert.That(group.alpha, Is.EqualTo(active ? 1f : 0f));
            Assert.That(group.interactable, Is.EqualTo(active));
            Assert.That(group.blocksRaycasts, Is.EqualTo(active));
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
#endif
