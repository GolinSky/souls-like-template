#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using MPUIKIT;
using NUnit.Framework;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Ui.Equipment;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulsLike.Editor.Tests.Equipment
{
    public sealed class EquipmentUiPresentationTests
    {
        private readonly List<UnityEngine.Object> _objects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (UnityEngine.Object instance in _objects)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            _objects.Clear();
        }

        [Test]
        public void Bind_TransitionsBetweenEquippedAndEmptyVisuals()
        {
            EquipmentSlotUI slot = CreateSlot(
                out Image iconImage,
                out GameObject selectionHighlight,
                out GameObject lockOverlay,
                out TMP_Text quantityText,
                out GameObject equippedBadge,
                out Sprite emptyIcon);
            InventoryItemViewData item = CreateItem("Crimson Flask", 3, true);

            slot.Bind(EquipmentSlotId.QuickItem1, item);

            Assert.That(iconImage.sprite, Is.EqualTo(item.Icon));
            Assert.That(iconImage.color, Is.EqualTo(Color.white));
            Assert.That(equippedBadge.activeSelf, Is.True);
            Assert.That(quantityText.gameObject.activeSelf, Is.True);
            Assert.That(quantityText.text, Is.EqualTo("3"));

            slot.Bind(EquipmentSlotId.QuickItem1, null);

            Assert.That(iconImage.sprite, Is.EqualTo(emptyIcon));
            Assert.That(iconImage.color, Is.EqualTo(new Color(1f, 1f, 1f, 0.45f)));
            Assert.That(iconImage.enabled, Is.True);
            Assert.That(equippedBadge.activeSelf, Is.False);
            Assert.That(quantityText.gameObject.activeSelf, Is.False);
            Assert.That(lockOverlay.activeSelf, Is.False);
            Assert.That(selectionHighlight.activeSelf, Is.False);

            slot.Bind(EquipmentSlotId.QuickItem1, item);

            Assert.That(iconImage.sprite, Is.EqualTo(item.Icon));
            Assert.That(iconImage.color, Is.EqualTo(Color.white));
            Assert.That(equippedBadge.activeSelf, Is.True);
            Assert.That(quantityText.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void Bind_LockedSlotSuppressesItemVisuals()
        {
            EquipmentSlotUI slot = CreateSlot(
                out Image iconImage,
                out GameObject selectionHighlight,
                out GameObject lockOverlay,
                out TMP_Text quantityText,
                out GameObject equippedBadge,
                out Sprite emptyIcon);
            InventoryItemViewData item = CreateItem("Crimson Flask", 3, true);

            slot.Bind(EquipmentSlotId.QuickItem1, item, true);

            Assert.That(iconImage.enabled, Is.False);
            Assert.That(equippedBadge.activeSelf, Is.False);
            Assert.That(quantityText.gameObject.activeSelf, Is.False);
            Assert.That(lockOverlay.activeSelf, Is.True);
        }

        [Test]
        public void Bind_PreservesFocusedVisualState()
        {
            EquipmentSlotUI slot = CreateSlot(
                out Image iconImage,
                out GameObject selectionHighlight,
                out GameObject lockOverlay,
                out TMP_Text quantityText,
                out GameObject equippedBadge,
                out Sprite emptyIcon);

            slot.OnSelect(new BaseEventData(null));
            slot.Bind(EquipmentSlotId.RightHand1, CreateItem("Long Sword", 1, false));

            Assert.That(selectionHighlight.activeSelf, Is.True);
        }

        [Test]
        public void Show_WhenReturningToEquipment_RestoresPreviouslySelectedSlot()
        {
            EventSystem previousEventSystem = EventSystem.current;
            EventSystem eventSystem = Track(new GameObject("Event System")).AddComponent<EventSystem>();
            MethodInfo onEnable = typeof(EventSystem).GetMethod(
                "OnEnable",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo onDisable = typeof(EventSystem).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic);
            onEnable.Invoke(eventSystem, null);
            EventSystem.current = eventSystem;
            try
            {
                EquipmentSlotUI selectedSlot = CreateSlot(
                    out Image iconImage,
                    out GameObject selectionHighlight,
                    out GameObject lockOverlay,
                    out TMP_Text quantityText,
                    out GameObject equippedBadge,
                    out Sprite emptyIcon);
                EquipmentUi equipmentUi = CreateEquipmentUi();
                SetField(equipmentUi, "_selectedSlot", selectedSlot);

                equipmentUi.Show();

                Assert.That(eventSystem.currentSelectedGameObject, Is.EqualTo(selectedSlot.gameObject));
                Assert.That(selectionHighlight.activeSelf, Is.True);
            }
            finally
            {
                onDisable.Invoke(eventSystem, null);
                if (previousEventSystem != null)
                {
                    EventSystem.current = previousEventSystem;
                }
            }
        }

        [Test]
        public void DisplayEmpty_ReplacesLoreTextAndArtwork()
        {
            LoreCardUi loreCard = CreateLoreCard(
                out TMP_Text itemName,
                out Image artwork,
                out TMP_Text fullText);
            InventoryItemViewData item = CreateItem("Long Sword", 1, false);
            Sprite emptyIcon = CreateSprite();

            loreCard.Display(item);

            Assert.That(itemName.text, Is.EqualTo("Long Sword"));
            Assert.That(artwork.sprite, Is.EqualTo(item.Icon));
            Assert.That(fullText.text, Is.EqualTo("A test description.\n\nA test lore entry."));

            loreCard.DisplayEmpty("Right Armament 1", emptyIcon);

            Assert.That(itemName.text, Is.EqualTo("Empty slot"));
            Assert.That(artwork.sprite, Is.EqualTo(emptyIcon));
            Assert.That(artwork.enabled, Is.True);
            Assert.That(fullText.text, Is.EqualTo(
                "No Right Armament 1 equipped. Select this slot to choose equipment from your inventory."));
        }

        private EquipmentSlotUI CreateSlot(
            out Image iconImage,
            out GameObject selectionHighlight,
            out GameObject lockOverlay,
            out TMP_Text quantityText,
            out GameObject equippedBadge,
            out Sprite emptyIcon)
        {
            GameObject root = Track(new GameObject("Equipment Slot"));
            root.SetActive(false);
            EquipmentSlotUI slot = root.AddComponent<EquipmentSlotUI>();
            iconImage = CreateComponent<Image>("Icon");
            MPImage borderImage = CreateComponent<MPImage>("Border");
            MPImage highlightImage = CreateComponent<MPImage>("Selection Highlight");
            selectionHighlight = highlightImage.gameObject;
            lockOverlay = Track(new GameObject("Lock Overlay"));
            quantityText = CreateComponent<TextMeshProUGUI>("Quantity");
            equippedBadge = Track(new GameObject("Equipped Badge"));
            emptyIcon = CreateSprite();

            SetField(slot, "iconImage", iconImage);
            SetField(slot, "borderImage", borderImage);
            SetField(slot, "selectionHighlight", highlightImage);
            SetField(slot, "lockOverlay", lockOverlay);
            SetField(slot, "quantityText", quantityText);
            SetField(slot, "emptyIcon", emptyIcon);
            SetField(slot, "equippedBadge", equippedBadge);
            root.SetActive(true);
            return slot;
        }

        private LoreCardUi CreateLoreCard(
            out TMP_Text itemName,
            out Image artwork,
            out TMP_Text fullText)
        {
            GameObject root = Track(new GameObject("Lore Card"));
            LoreCardUi loreCard = root.AddComponent<LoreCardUi>();
            itemName = CreateComponent<TextMeshProUGUI>("Item Name");
            artwork = CreateComponent<Image>("Artwork");
            fullText = CreateComponent<TextMeshProUGUI>("Full Text");

            SetField(loreCard, "loreItemName", itemName);
            SetField(loreCard, "loreItemArtwork", artwork);
            SetField(loreCard, "loreFullText", fullText);
            return loreCard;
        }

        private EquipmentUi CreateEquipmentUi()
        {
            GameObject root = Track(new GameObject("Equipment UI"));
            root.SetActive(false);
            EquipmentUi equipmentUi = root.AddComponent<EquipmentUi>();
            CanvasGroup canvasGroup = CreateComponent<CanvasGroup>("Equipment Canvas Group");
            SetField(equipmentUi, "canvasGroup", canvasGroup);
            return equipmentUi;
        }

        private InventoryItemViewData CreateItem(string displayName, int quantity, bool stackable)
        {
            var definition = new ItemDefinition();
            Sprite icon = CreateSprite();
            SetField(definition, "itemId", ItemId.CrimsonFlask);
            SetField(definition, "itemType", ItemType.Consumable);
            SetField(definition, "displayName", displayName);
            SetField(definition, "description", "A test description.");
            SetField(definition, "loreDescription", "A test lore entry.");
            SetField(definition, "icon", icon);
            SetField(definition, "maxStack", stackable ? 99 : 1);

            ConstructorInfo constructor = typeof(InventoryItemViewData).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(InventoryEntry),
                    typeof(ItemDefinition),
                    typeof(ItemStatSnapshot),
                    typeof(Sprite),
                    typeof(bool),
                    typeof(string),
                    typeof(bool)
                },
                null);
            return (InventoryItemViewData)constructor.Invoke(new object[]
            {
                new InventoryEntry(InventoryEntryId.Create(), ItemId.CrimsonFlask, quantity),
                definition,
                ItemStatSnapshot.Empty,
                null,
                false,
                string.Empty,
                true
            });
        }

        private T CreateComponent<T>(string name) where T : Component
        {
            GameObject gameObject = Track(new GameObject(name, typeof(RectTransform)));
            return gameObject.AddComponent<T>();
        }

        private Sprite CreateSprite()
        {
            Texture2D texture = Track(new Texture2D(2, 2));
            return Track(Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), Vector2.zero));
        }

        private static void SetField(object target, string fieldName, object value)
        {
            Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }

                type = type.BaseType;
            }

            throw new MissingFieldException(target.GetType().Name, fieldName);
        }

        private T Track<T>(T instance) where T : UnityEngine.Object
        {
            _objects.Add(instance);
            return instance;
        }
    }
}
#endif
