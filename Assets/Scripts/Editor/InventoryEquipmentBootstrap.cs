#if UNITY_EDITOR
using System;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Ui.Equipment;
using SoulsLike.Ui.Inventory;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Editor
{
    public static class InventoryEquipmentBootstrap
    {
        private const string ITEM_FOLDER = "Assets/Settings/Items";
        private const string ITEM_DATABASE_PATH = ITEM_FOLDER + "/ItemDatabase.asset";
        private const string WEAPON_DATABASE_PATH = ITEM_FOLDER + "/WeaponDatabase.asset";
        private const string SHIELD_DATABASE_PATH = ITEM_FOLDER + "/ShieldDatabase.asset";
        private const string CONSUMABLE_DATABASE_PATH = ITEM_FOLDER + "/ConsumableDatabase.asset";
        private const string INVENTORY_DATA_PATH = "Assets/Settings/Data/InventoryData.asset";
        private const string ASSET_MAPPING_PATH = "Assets/Settings/Data/AssetMappingData.asset";
        private const string CHARACTER_PREFAB_PATH = "Assets/Prefabs/Character/Character.prefab";
        private const string INVENTORY_UI_PREFAB_PATH = "Assets/Prefabs/Ui/Inventory/InventoryUi.prefab";
        private const string INVENTORY_SLOT_PREFAB_PATH = "Assets/Prefabs/Ui/Inventory/InventorySlot.prefab";
        private const string EQUIPMENT_UI_PREFAB_PATH = "Assets/Prefabs/Ui/Equipment/EquipmentUi.prefab";
        private const string SWORD_PREFAB_PATH = "Assets/Prefabs/Swords/LongSword.prefab";
        private const string SHIELD_PREFAB_PATH = "Assets/Prefabs/Shields/WoodenShield.prefab";
        private const string SWORD_ICON_PATH = "Assets/Art/Textures/ItemIcons/GreatSwordIcon.png";
        private const string SHIELD_ICON_PATH = "Assets/Art/Textures/ItemIcons/ShieldIcon.png";
        private const string FLASK_ICON_PATH = "Assets/Art/Textures/ItemIcons/CrimsonFlaskIcon.png";
        private const string GREASE_ICON_PATH = "Assets/Art/Textures/ItemIcons/LightningGreaseIcon.png";
        private const string RUNE_ICON_PATH = "Assets/Art/Textures/ItemIcons/GoldenRuneSmallIcon.png";
        private const string NO_WEAPON_CONTROLLER_PATH = "Assets/Art/Animation/NoWeaponAnimator.controller";
        private const string WEAPON_CONTROLLER_PATH = "Assets/Art/Animation/CharacterGreatSwordAnimator.controller";
        private const string LEFT_WEAPON_CONTROLLER_PATH =
            "Assets/Art/Animation/CharacterGreatSwordLeftHandAnimator.controller";
        private const string ADDRESSABLE_PACKED_BUILD_PATH =
            "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";

        [MenuItem("Tools/SoulsLike/Bootstrap Inventory and Equipment")]
        public static void Execute()
        {
            EnsureFolder(ITEM_FOLDER);

            AnimationProfile animationProfile = CreateAnimationProfile();
            CombatProfile combatProfile = CreateCombatProfile();
            ItemDatabase itemDatabase = LoadOrCreate<ItemDatabase>(ITEM_DATABASE_PATH);
            WeaponDatabase weaponDatabase = LoadOrCreate<WeaponDatabase>(WEAPON_DATABASE_PATH);
            ShieldDatabase shieldDatabase = LoadOrCreate<ShieldDatabase>(SHIELD_DATABASE_PATH);
            ConsumableDatabase consumableDatabase =
                LoadOrCreate<ConsumableDatabase>(CONSUMABLE_DATABASE_PATH);
            ConfigureItemDatabase(itemDatabase);
            ConfigureWeaponDatabase(weaponDatabase, animationProfile, combatProfile);
            ConfigureShieldDatabase(shieldDatabase);
            ConfigureConsumableDatabase(consumableDatabase);
            itemDatabase.ValidateDatabase();
            weaponDatabase.ValidateDatabase();
            shieldDatabase.ValidateDatabase();
            consumableDatabase.ValidateDatabase();

            ConfigureInitialInventory();
            ConfigureCharacterPrefab();
            ConfigureEquipmentUiPrefab();
            ConfigureAddressables(
                itemDatabase,
                weaponDatabase,
                shieldDatabase,
                consumableDatabase);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildAddressableContent();
            Debug.Log("Inventory and Equipment bootstrap completed successfully.");
        }

        [MenuItem("Tools/SoulsLike/Migrate Item Databases")]
        public static void MigrateItemDatabases()
        {
            EnsureFolder(ITEM_FOLDER);

            AnimationProfile animationProfile = LoadRequiredAsset<AnimationProfile>(
                ITEM_FOLDER + "/StraightSwordAnimationProfile.asset");
            CombatProfile combatProfile = LoadRequiredAsset<CombatProfile>(
                ITEM_FOLDER + "/StraightSwordCombatProfile.asset");
            ItemDatabase itemDatabase = LoadOrCreate<ItemDatabase>(ITEM_DATABASE_PATH);
            WeaponDatabase weaponDatabase = LoadOrCreate<WeaponDatabase>(WEAPON_DATABASE_PATH);
            ShieldDatabase shieldDatabase = LoadOrCreate<ShieldDatabase>(SHIELD_DATABASE_PATH);
            ConsumableDatabase consumableDatabase =
                LoadOrCreate<ConsumableDatabase>(CONSUMABLE_DATABASE_PATH);

            ConfigureItemDatabase(itemDatabase);
            ConfigureWeaponDatabase(weaponDatabase, animationProfile, combatProfile);
            ConfigureShieldDatabase(shieldDatabase);
            ConfigureConsumableDatabase(consumableDatabase);
            new ItemCatalog(
                    itemDatabase,
                    weaponDatabase,
                    shieldDatabase,
                    consumableDatabase)
                .ValidateCatalog();
            ConfigureAddressables(
                itemDatabase,
                weaponDatabase,
                shieldDatabase,
                consumableDatabase);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildAddressableContent();
            Debug.Log("Item database migration completed successfully.");
        }

        private static AnimationProfile CreateAnimationProfile()
        {
            AnimationProfile profile = LoadOrCreate<AnimationProfile>(
                ITEM_FOLDER + "/StraightSwordAnimationProfile.asset");
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                WEAPON_CONTROLLER_PATH);
            RuntimeAnimatorController leftController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(LEFT_WEAPON_CONTROLLER_PATH);
            RequireAsset(controller, WEAPON_CONTROLLER_PATH);
            RequireAsset(leftController, LEFT_WEAPON_CONTROLLER_PATH);
            SetObject(new SerializedObject(profile), "<Controller>k__BackingField", controller);
            SetObject(
                new SerializedObject(profile),
                "<LeftHandController>k__BackingField",
                leftController);
            return profile;
        }

        private static CombatProfile CreateCombatProfile()
        {
            CombatProfile profile = LoadOrCreate<CombatProfile>(
                ITEM_FOLDER + "/StraightSwordCombatProfile.asset");
            var serialized = new SerializedObject(profile);
            SetFloat(serialized, "<LightAttackMultiplier>k__BackingField", 1f, false);
            SetFloat(serialized, "<HeavyAttackMultiplier>k__BackingField", 1.5f, false);
            SetFloat(serialized, "<StaminaCostMultiplier>k__BackingField", 1f);
            return profile;
        }

        private static void ConfigureItemDatabase(ItemDatabase database)
        {
            var serialized = new SerializedObject(database);
            SerializedProperty items = RequireProperty(serialized, "items");
            items.arraySize = 5;
            SetCommonItem(
                items.GetArrayElementAtIndex(0),
                ItemId.GreatSword,
                ItemType.Weapon,
                "Long Sword",
                "A dependable straight sword suited to one- or two-handed combat.",
                "A plain sword carried by soldiers across the realm.",
                SWORD_ICON_PATH,
                3.5f,
                1,
                EquipmentGroup.Armament);
            SetCommonItem(
                items.GetArrayElementAtIndex(1),
                ItemId.WoodenShield,
                ItemType.Shield,
                "Wooden Shield",
                "A light wooden shield with modest physical protection.",
                "Common protection for travelers and militia.",
                SHIELD_ICON_PATH,
                2f,
                1,
                EquipmentGroup.Armament);
            SetCommonItem(
                items.GetArrayElementAtIndex(2),
                ItemId.CrimsonFlask,
                ItemType.Consumable,
                "Crimson Flask",
                "Restores HP.",
                "Restores HP.",
                FLASK_ICON_PATH,
                0f,
                10,
                EquipmentGroup.QuickItem);
            SetCommonItem(
                items.GetArrayElementAtIndex(3),
                ItemId.LightningGrease,
                ItemType.Consumable,
                "Lightning Grease",
                "Temporarily coats the active weapon in lightning.",
                "Temporarily coats the active weapon in lightning.",
                GREASE_ICON_PATH,
                0f,
                10,
                EquipmentGroup.QuickItem);
            SetCommonItem(
                items.GetArrayElementAtIndex(4),
                ItemId.GoldenRuneSmall,
                ItemType.Consumable,
                "Golden Rune [1]",
                "Grants a small number of runes.",
                "Grants a small number of runes.",
                RUNE_ICON_PATH,
                0f,
                99,
                EquipmentGroup.QuickItem);
            Apply(serialized, true);
        }

        private static void ConfigureWeaponDatabase(
            WeaponDatabase database,
            AnimationProfile animationProfile,
            CombatProfile combatProfile)
        {
            var serialized = new SerializedObject(database);
            SerializedProperty items = RequireProperty(serialized, "items");
            items.arraySize = 1;
            SerializedProperty sword = items.GetArrayElementAtIndex(0);
            SetRelativeInt(sword, "itemId", (int)ItemId.GreatSword);
            SetRelativeObject(sword, "animationProfile", animationProfile);
            SetRelativeObject(sword, "combatProfile", combatProfile);
            SetRelativeObject(
                sword,
                "equippedPrefab",
                LoadRequiredAsset<GameObject>(SWORD_PREFAB_PATH));
            SetRelativeBool(sword, "canTwoHand", true);
            SetRelativeInt(sword, "physicalAttack", 110);
            SetRelativeInt(sword, "critical", 100);
            SetRelativeInt(sword, "requirements.Strength", 10);
            SetRelativeInt(sword, "requirements.Dexterity", 10);
            SetRelativeInt(sword, "scaling.Strength", (int)ScalingGrade.D);
            SetRelativeInt(sword, "scaling.Dexterity", (int)ScalingGrade.D);
            SetRelativeString(sword, "skillName", "Square Off");
            SetRelativeInt(sword, "skillFocusCost", 8);
            Apply(serialized, true);
        }

        private static void ConfigureShieldDatabase(ShieldDatabase database)
        {
            var serialized = new SerializedObject(database);
            SerializedProperty items = RequireProperty(serialized, "items");
            items.arraySize = 1;
            SerializedProperty shield = items.GetArrayElementAtIndex(0);
            SetRelativeInt(shield, "itemId", (int)ItemId.WoodenShield);
            SetRelativeObject(
                shield,
                "equippedPrefab",
                LoadRequiredAsset<GameObject>(SHIELD_PREFAB_PATH));
            SetRelativeFloat(shield, "physicalGuard", 60f);
            SetRelativeFloat(shield, "magicGuard", 30f);
            SetRelativeFloat(shield, "fireGuard", 20f);
            SetRelativeFloat(shield, "lightningGuard", 35f);
            SetRelativeFloat(shield, "holyGuard", 30f);
            SetRelativeFloat(shield, "guardBoost", 40f);
            SetRelativeInt(shield, "requirements.Strength", 8);
            Apply(serialized, true);
        }

        private static void ConfigureConsumableDatabase(ConsumableDatabase database)
        {
            var serialized = new SerializedObject(database);
            SerializedProperty items = RequireProperty(serialized, "items");
            items.arraySize = 3;
            SetConsumable(
                items.GetArrayElementAtIndex(0),
                ItemId.CrimsonFlask,
                ItemUseType.Heal,
                60f,
                0f);
            SetConsumable(
                items.GetArrayElementAtIndex(1),
                ItemId.LightningGrease,
                ItemUseType.InfuseActiveWeapon,
                40f,
                60f);
            SetConsumable(
                items.GetArrayElementAtIndex(2),
                ItemId.GoldenRuneSmall,
                ItemUseType.GrantCurrency,
                200f,
                0f);
            Apply(serialized, true);
        }

        private static void ConfigureInitialInventory()
        {
            InventoryData inventoryData = AssetDatabase.LoadAssetAtPath<InventoryData>(INVENTORY_DATA_PATH);
            RequireAsset(inventoryData, INVENTORY_DATA_PATH);
            var serialized = new SerializedObject(inventoryData);
            SerializedProperty entries = RequireProperty(serialized, "initialEntries");
            entries.arraySize = 5;
            SetInitialEntry(entries.GetArrayElementAtIndex(0), ItemId.GreatSword, 1);
            SetInitialEntry(entries.GetArrayElementAtIndex(1), ItemId.WoodenShield, 1);
            SetInitialEntry(entries.GetArrayElementAtIndex(2), ItemId.CrimsonFlask, 5);
            SetInitialEntry(entries.GetArrayElementAtIndex(3), ItemId.LightningGrease, 3);
            SetInitialEntry(entries.GetArrayElementAtIndex(4), ItemId.GoldenRuneSmall, 2);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventoryData);
        }

        private static void ConfigureCharacterPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(CHARACTER_PREFAB_PATH);
            try
            {
                Character character = RequireComponent<Character>(root);
                AnimatorComponent animatorComponent = RequireComponent<AnimatorComponent>(root);
                EquipmentPresentation presentation = root.GetComponent<EquipmentPresentation>();
                if (presentation == null)
                {
                    presentation = root.AddComponent<EquipmentPresentation>();
                }

                Transform rightHandAnchor = FindRequiredChild(
                    root.transform,
                    "mixamorig:RightHand");
                Transform leftHandAnchor = FindRequiredChild(
                    root.transform,
                    "mixamorig:LeftHand");

                var presentationSerialized = new SerializedObject(presentation);
                SetObject(presentationSerialized, "rightHandAnchor", rightHandAnchor, false);
                SetObject(presentationSerialized, "leftHandAnchor", leftHandAnchor);

                var characterSerialized = new SerializedObject(character);
                SetObject(characterSerialized, "equipmentPresentation", presentation, false);
                SetCharacterAttributes(characterSerialized, 10);

                var animatorSerialized = new SerializedObject(animatorComponent);
                SetObject(
                    animatorSerialized,
                    "<RightHandAnchor>k__BackingField",
                    rightHandAnchor,
                    false);
                Animator animator = RequireProperty(
                    animatorSerialized,
                    "animator").objectReferenceValue as Animator;
                if (animator == null)
                {
                    throw new InvalidOperationException("AnimatorComponent requires its Animator reference.");
                }

                RuntimeAnimatorController noWeaponController =
                    AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(NO_WEAPON_CONTROLLER_PATH);
                RequireAsset(noWeaponController, NO_WEAPON_CONTROLLER_PATH);
                animator.runtimeAnimatorController = noWeaponController;
                EditorUtility.SetDirty(animator);

                PrefabUtility.SaveAsPrefabAsset(root, CHARACTER_PREFAB_PATH);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureEquipmentUiPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(EQUIPMENT_UI_PREFAB_PATH);
            try
            {
                EquipmentUi view = RequireComponent<EquipmentUi>(root);
                var serialized = new SerializedObject(view);
                Transform pickerModal = FindRequiredChild(root.transform, "PickerModalWindow");
                Transform pickerGrid = pickerModal.Find("PickerGridContent");
                if (pickerGrid == null)
                {
                    var pickerGridObject = new GameObject(
                        "PickerGridContent",
                        typeof(RectTransform),
                        typeof(GridLayoutGroup));
                    pickerGrid = pickerGridObject.transform;
                    pickerGrid.SetParent(pickerModal, false);
                    var grid = pickerGridObject.GetComponent<GridLayoutGroup>();
                    grid.cellSize = new Vector2(72f, 72f);
                    grid.spacing = new Vector2(8f, 8f);
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 5;
                }

                SetObject(serialized, "inventoryPickerGridContainer", pickerGrid, false);
                GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    INVENTORY_SLOT_PREFAB_PATH);
                RequireAsset(slotPrefab, INVENTORY_SLOT_PREFAB_PATH);
                SetObject(
                    serialized,
                    "inventoryPickerSlotPrefab",
                    RequireComponent<InventorySlotUI>(slotPrefab),
                    false);

                Transform inspector = FindRequiredChild(root.transform, "ItemInspectorPanel");
                EnsureText(serialized, "inspectorAttackSummary", inspector, "InspectorAttackSummary");
                EnsureText(serialized, "inspectorReqDex", inspector, "InspectorReqDex");
                EnsureText(serialized, "inspectorReqInt", inspector, "InspectorReqInt");
                EnsureText(serialized, "inspectorReqFth", inspector, "InspectorReqFth");
                EnsureText(serialized, "inspectorReqArc", inspector, "InspectorReqArc");

                Transform status = FindRequiredChild(root.transform, "CharacterStatusPanel");
                EnsureImage(serialized, "equipLoadFillBar", status, "EquipLoadFillBar");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, EQUIPMENT_UI_PREFAB_PATH);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureAddressables(
            ItemDatabase itemDatabase,
            WeaponDatabase weaponDatabase,
            ShieldDatabase shieldDatabase,
            ConsumableDatabase consumableDatabase)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                throw new InvalidOperationException("AddressableAssetSettings are unavailable.");
            }

            AddAddressable(settings, "Data", ITEM_DATABASE_PATH, nameof(ItemDatabase));
            AddAddressable(settings, "Data", WEAPON_DATABASE_PATH, nameof(WeaponDatabase));
            AddAddressable(settings, "Data", SHIELD_DATABASE_PATH, nameof(ShieldDatabase));
            AddAddressable(settings, "Data", CONSUMABLE_DATABASE_PATH, nameof(ConsumableDatabase));
            AddAddressable(settings, "Ui", INVENTORY_UI_PREFAB_PATH, nameof(InventoryUi));
            AddAddressable(settings, "Ui", EQUIPMENT_UI_PREFAB_PATH, nameof(EquipmentUi));

            AssetMappingData mapping = AssetDatabase.LoadAssetAtPath<AssetMappingData>(ASSET_MAPPING_PATH);
            RequireAsset(mapping, ASSET_MAPPING_PATH);
            SetMapping(mapping, "scriptableObjectMappings", nameof(ItemDatabase), ITEM_DATABASE_PATH);
            SetMapping(
                mapping,
                "scriptableObjectMappings",
                nameof(WeaponDatabase),
                WEAPON_DATABASE_PATH);
            SetMapping(
                mapping,
                "scriptableObjectMappings",
                nameof(ShieldDatabase),
                SHIELD_DATABASE_PATH);
            SetMapping(
                mapping,
                "scriptableObjectMappings",
                nameof(ConsumableDatabase),
                CONSUMABLE_DATABASE_PATH);
            SetMapping(mapping, "uiMappings", nameof(InventoryUi), INVENTORY_UI_PREFAB_PATH);
            SetMapping(mapping, "uiMappings", nameof(EquipmentUi), EQUIPMENT_UI_PREFAB_PATH);
            EditorUtility.SetDirty(itemDatabase);
            EditorUtility.SetDirty(weaponDatabase);
            EditorUtility.SetDirty(shieldDatabase);
            EditorUtility.SetDirty(consumableDatabase);
            EditorUtility.SetDirty(mapping);
            EditorUtility.SetDirty(settings);
        }

        private static void BuildAddressableContent()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                throw new InvalidOperationException("AddressableAssetSettings are unavailable.");
            }

            ScriptableObject packedBuild = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                ADDRESSABLE_PACKED_BUILD_PATH);
            RequireAsset(packedBuild, ADDRESSABLE_PACKED_BUILD_PATH);
            int packedBuildIndex = settings.DataBuilders.IndexOf(packedBuild);
            if (packedBuildIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Addressables builder '{ADDRESSABLE_PACKED_BUILD_PATH}' is not registered.");
            }

            int playModeBuilderIndex = settings.ActivePlayerDataBuilderIndex;
            try
            {
                settings.ActivePlayerDataBuilderIndex = packedBuildIndex;
                AddressableAssetSettings.BuildPlayerContent(
                    out AddressablesPlayerBuildResult result);
                if (!string.IsNullOrEmpty(result.Error))
                {
                    throw new InvalidOperationException(
                        $"Addressables content build failed: {result.Error}");
                }
            }
            finally
            {
                settings.ActivePlayerDataBuilderIndex = playModeBuilderIndex;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        private static void AddAddressable(
            AddressableAssetSettings settings,
            string groupName,
            string assetPath,
            string address)
        {
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group == null)
            {
                throw new InvalidOperationException($"Addressables group '{groupName}' was not found.");
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new InvalidOperationException($"Asset '{assetPath}' has no GUID.");
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;
        }

        private static void SetMapping(
            AssetMappingData mapping,
            string mappingProperty,
            string key,
            string assetPath)
        {
            var serialized = new SerializedObject(mapping);
            SerializedProperty pairs = RequireProperty(serialized, mappingProperty)
                .FindPropertyRelative("keyValue");
            if (pairs == null)
            {
                throw new InvalidOperationException($"Mapping '{mappingProperty}' has no keyValue list.");
            }

            SerializedProperty target = null;
            for (int index = 0; index < pairs.arraySize; index++)
            {
                SerializedProperty pair = pairs.GetArrayElementAtIndex(index);
                if (pair.FindPropertyRelative("key").stringValue == key)
                {
                    target = pair;
                    break;
                }
            }

            if (target == null)
            {
                int index = pairs.arraySize;
                pairs.InsertArrayElementAtIndex(index);
                target = pairs.GetArrayElementAtIndex(index);
            }

            target.FindPropertyRelative("key").stringValue = key;
            SerializedProperty value = target.FindPropertyRelative("value");
            SerializedProperty guid = value.FindPropertyRelative("m_AssetGUID");
            if (guid == null)
            {
                throw new InvalidOperationException($"Mapping value for '{key}' has no AssetGUID field.");
            }

            guid.stringValue = AssetDatabase.AssetPathToGUID(assetPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetCommonItem(
            SerializedProperty property,
            ItemId itemId,
            ItemType itemType,
            string displayName,
            string description,
            string lore,
            string iconPath,
            float weight,
            int maxStack,
            params EquipmentGroup[] groups)
        {
            SetRelativeInt(property, "itemId", (int)itemId);
            SetRelativeInt(property, "itemType", (int)itemType);
            SetRelativeString(property, "displayName", displayName);
            SetRelativeString(property, "description", description);
            SetRelativeString(property, "loreDescription", lore);
            SetRelativeObject(property, "icon", LoadRequiredAsset<Sprite>(iconPath));
            SetRelativeFloat(property, "weight", weight);
            SetRelativeInt(property, "maxStack", maxStack);
            SerializedProperty equipmentGroups = RequireRelative(property, "equipmentGroups");
            equipmentGroups.arraySize = groups.Length;
            for (int index = 0; index < groups.Length; index++)
            {
                equipmentGroups.GetArrayElementAtIndex(index).enumValueIndex = (int)groups[index];
            }
        }

        private static void SetConsumable(
            SerializedProperty property,
            ItemId itemId,
            ItemUseType useType,
            float effectAmount,
            float durationSeconds)
        {
            SetRelativeInt(property, "itemId", (int)itemId);
            SetRelativeInt(property, "useType", (int)useType);
            SetRelativeFloat(property, "effectAmount", effectAmount);
            SetRelativeFloat(property, "durationSeconds", durationSeconds);
        }

        private static void SetRelativeObject(
            SerializedProperty property,
            string propertyName,
            UnityEngine.Object value)
        {
            RequireRelative(property, propertyName).objectReferenceValue = value;
        }

        private static void SetRelativeString(
            SerializedProperty property,
            string propertyName,
            string value)
        {
            RequireRelative(property, propertyName).stringValue = value;
        }

        private static void SetRelativeFloat(
            SerializedProperty property,
            string propertyName,
            float value)
        {
            RequireRelative(property, propertyName).floatValue = value;
        }

        private static void SetRelativeInt(
            SerializedProperty property,
            string propertyName,
            int value)
        {
            SerializedProperty target = RequireRelative(property, propertyName);
            if (target.propertyType == SerializedPropertyType.Enum)
            {
                target.enumValueIndex = value;
            }
            else
            {
                target.intValue = value;
            }
        }

        private static void SetRelativeBool(
            SerializedProperty property,
            string propertyName,
            bool value)
        {
            RequireRelative(property, propertyName).boolValue = value;
        }

        private static SerializedProperty RequireRelative(
            SerializedProperty property,
            string propertyPath)
        {
            SerializedProperty current = property;
            foreach (string segment in propertyPath.Split('.'))
            {
                current = current.FindPropertyRelative(segment);
                if (current == null)
                {
                    throw new InvalidOperationException(
                        $"Serialized row has no property '{propertyPath}'.");
                }
            }

            return current;
        }

        private static void SetInitialEntry(
            SerializedProperty property,
            ItemId itemId,
            int quantity)
        {
            SerializedProperty itemIdProperty = property.FindPropertyRelative("<ItemId>k__BackingField");
            SerializedProperty quantityProperty = property.FindPropertyRelative("<Quantity>k__BackingField");
            if (itemIdProperty == null || quantityProperty == null)
            {
                throw new InvalidOperationException("InitialInventoryEntry serialization layout changed.");
            }

            itemIdProperty.enumValueIndex = (int)itemId;
            quantityProperty.intValue = quantity;
        }

        private static void SetCharacterAttributes(SerializedObject character, int value)
        {
            string[] names =
            {
                "Vigor",
                "Mind",
                "Endurance",
                "Strength",
                "Dexterity",
                "Intelligence",
                "Faith",
                "Arcane"
            };
            foreach (string name in names)
            {
                SetInt(character, $"attributes.<{name}>k__BackingField", value, false);
            }

            character.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureText(
            SerializedObject serialized,
            string propertyName,
            Transform parent,
            string objectName)
        {
            SerializedProperty property = RequireProperty(serialized, propertyName);
            if (property.objectReferenceValue != null)
            {
                return;
            }

            var gameObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parent, false);
            var text = gameObject.GetComponent<TextMeshProUGUI>();
            text.text = "-";
            text.fontSize = 18f;
            property.objectReferenceValue = text;
        }

        private static void EnsureImage(
            SerializedObject serialized,
            string propertyName,
            Transform parent,
            string objectName)
        {
            SerializedProperty property = RequireProperty(serialized, propertyName);
            if (property.objectReferenceValue != null)
            {
                return;
            }

            var gameObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            property.objectReferenceValue = image;
        }

        private static void SetObject(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object value,
            bool apply = true)
        {
            RequireProperty(serialized, propertyName).objectReferenceValue = value;
            Apply(serialized, apply);
        }

        private static void SetString(
            SerializedObject serialized,
            string propertyName,
            string value,
            bool apply = true)
        {
            RequireProperty(serialized, propertyName).stringValue = value;
            Apply(serialized, apply);
        }

        private static void SetFloat(
            SerializedObject serialized,
            string propertyName,
            float value,
            bool apply = true)
        {
            RequireProperty(serialized, propertyName).floatValue = value;
            Apply(serialized, apply);
        }

        private static void SetInt(
            SerializedObject serialized,
            string propertyName,
            int value,
            bool apply = true)
        {
            SerializedProperty property = RequireProperty(serialized, propertyName);
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                property.enumValueIndex = value;
            }
            else
            {
                property.intValue = value;
            }

            Apply(serialized, apply);
        }

        private static void Apply(SerializedObject serialized, bool apply)
        {
            if (apply)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(serialized.targetObject);
            }
        }

        private static SerializedProperty RequireProperty(
            SerializedObject serialized,
            string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"'{serialized.targetObject.GetType().Name}' has no serialized property '{propertyName}'.");
            }

            return property;
        }

        private static T LoadOrCreate<T>(string path)
            where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static T LoadRequiredAsset<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            RequireAsset(asset, path);
            return asset;
        }

        private static T RequireComponent<T>(GameObject gameObject)
            where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"GameObject '{gameObject.name}' requires {typeof(T).Name}.");
            }

            return component;
        }

        private static void RequireAsset(UnityEngine.Object asset, string path)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"Required asset '{path}' was not found.");
            }
        }

        private static Transform FindRequiredChild(Transform root, string childName)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            throw new InvalidOperationException(
                $"Prefab '{root.name}' requires child '{childName}'.");
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }
    }
}
#endif
