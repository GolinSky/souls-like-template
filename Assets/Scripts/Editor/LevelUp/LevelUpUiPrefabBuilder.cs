#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Ui.Base;
using SoulsLike.Ui.LevelUp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Editor.LevelUp
{
    public static class LevelUpUiPrefabBuilder
    {
        private const string PREFAB_DIR = "Assets/Prefabs/Ui/LevelUp";
        private const string PREFAB_PATH = PREFAB_DIR + "/LevelUpUi.prefab";

        private const string ATMOSPHERE_PATH = "Assets/Art/Textures/InventoryUI/InventoryAtmosphereOverlay.png";
        private const string PANEL_SOFT_PATH = "Assets/Art/Textures/StatusUI/StatusPanelSoft.png";
        private const string FLAT_FRAME_PATH = "Assets/Art/Textures/MenuIcons/FlatIconFrame.png";
        private const string GRACE_ICON_PATH = "Assets/Art/Textures/MenuIcons/GraceIcon.png";

        private const string EMPTY_ARMS_PATH = "Assets/Art/Textures/EquipmentUI/EmptySlots/EquipmentEmptyArms.png";
        private const string EMPTY_CHEST_PATH = "Assets/Art/Textures/EquipmentUI/EmptySlots/EquipmentEmptyChest.png";
        private const string EMPTY_WEAPON_PATH = "Assets/Art/Textures/EquipmentUI/EmptySlots/EquipmentEmptyWeapon.png";
        private const string EMPTY_SHIELD_PATH = "Assets/Art/Textures/EquipmentUI/EmptySlots/EquipmentEmptyShield.png";
        private const string EMPTY_HEAD_PATH = "Assets/Art/Textures/EquipmentUI/EmptySlots/EquipmentEmptyHead.png";

        private const string FONT_CINZEL_PATH = "Assets/Art/Fonts/Cinzel/Cinzel[wght] SDF.asset";
        private const string FONT_GARAMOND_PATH = "Assets/Art/Fonts/EBGaramond/EBGaramond SDF.asset";
        private const string FONT_INTER_PATH = "Assets/Art/Fonts/Inter/Inter SDF.asset";

        private static readonly Color GoldTextColor = new Color(0.867f, 0.839f, 0.784f, 1f);
        private static readonly Color SubtitleColor = new Color(0.686f, 0.655f, 0.608f, 1f);
        private static readonly Color ArrowColor = new Color(0.55f, 0.53f, 0.48f, 0.9f);
        private static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.08f, 0.88f);
        private static readonly Color StepperBtnColor = new Color(0.18f, 0.20f, 0.17f, 0.95f);

        [MenuItem("Tools/SoulsLike/Build LevelUp UI Prefab")]
        public static GameObject BuildPrefab()
        {
            if (!Directory.Exists(PREFAB_DIR))
            {
                Directory.CreateDirectory(PREFAB_DIR);
            }

            Sprite atmosphereSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ATMOSPHERE_PATH);
            Sprite panelSoftSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PANEL_SOFT_PATH);
            Sprite flatFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FLAT_FRAME_PATH);
            Sprite graceIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GRACE_ICON_PATH);

            Sprite emptyArmsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EMPTY_ARMS_PATH);
            Sprite emptyChestSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EMPTY_CHEST_PATH);
            Sprite emptyWeaponSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EMPTY_WEAPON_PATH);
            Sprite emptyShieldSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EMPTY_SHIELD_PATH);
            Sprite emptyHeadSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EMPTY_HEAD_PATH);

            TMP_FontAsset fontCinzel = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_CINZEL_PATH);
            TMP_FontAsset fontGaramond = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_GARAMOND_PATH);
            TMP_FontAsset fontInter = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_INTER_PATH);

            GameObject rootGo = new GameObject("LevelUpUi", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(LevelUpUi));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(1920f, 1080f);

            Canvas canvas = rootGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = rootGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            LevelUpUi levelUpUi = rootGo.GetComponent<LevelUpUi>();

            // Background
            GameObject bgGo = CreateUiElement("Background", rootGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.sprite = atmosphereSprite;
            bgImg.color = new Color(0.04f, 0.04f, 0.04f, 0.85f);
            bgImg.raycastTarget = false;

            // ContentRoot (1920 x 1080)
            GameObject contentRootGo = CreateUiElement("ContentRoot", rootGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920f, 1080f));
            RectTransform contentRootRt = contentRootGo.GetComponent<RectTransform>();

            // Header (top left)
            GameObject headerGo = CreateUiElement("Header", contentRootGo.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, -60f), new Vector2(400f, 60f));
            headerGo.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            GameObject iconFrameGo = CreateUiElement("EmblemFrame", headerGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(44f, 44f));
            Image frameImg = iconFrameGo.AddComponent<Image>();
            frameImg.sprite = flatFrameSprite;
            frameImg.raycastTarget = false;

            GameObject iconGo = CreateUiElement("GraceIcon", iconFrameGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 30f));
            Image iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = graceIconSprite;
            iconImg.color = new Color(1f, 0.85f, 0.5f, 1f);
            iconImg.raycastTarget = false;

            GameObject titleTextGo = CreateUiElement("TitleText", headerGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(60f, 0f), new Vector2(300f, 40f));
            titleTextGo.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            TMP_Text titleText = titleTextGo.AddComponent<TextMeshProUGUI>();
            titleText.font = fontCinzel;
            titleText.fontSize = 28f;
            titleText.text = "Level Up";
            titleText.color = GoldTextColor;
            titleText.alignment = TextAlignmentOptions.Left;

            // Columns Container (below header)
            GameObject mainGo = CreateUiElement("Main", contentRootGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(1760f, 820f));

            // ==========================================
            // COLUMN 1: Profile, Attribute Points, Confirm (Width 440, X: -640)
            // ==========================================
            GameObject col1Go = CreateUiElement("Column1", mainGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(460f, 800f));
            col1Go.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);

            // Profile Panel
            GameObject profilePanelGo = CreatePanel("ProfilePanel", col1Go.transform, panelSoftSprite, PanelColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -10f), new Vector2(0f, 150f));
            profilePanelGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            TMP_Text levelCurrText, levelNextText;
            CreateComparisonRow(profilePanelGo.transform, fontGaramond, "Level", -25f, out levelCurrText, out levelNextText);

            TMP_Text runesHeldCurrText, runesHeldNextText;
            CreateComparisonRow(profilePanelGo.transform, fontGaramond, "Runes Held", -65f, out runesHeldCurrText, out runesHeldNextText);

            TMP_Text runesNeededText;
            CreateSingleStatRow(profilePanelGo.transform, fontGaramond, "Runes Needed", -105f, out runesNeededText);

            // Attribute Points Panel
            GameObject attrPanelGo = CreatePanel("AttributePanel", col1Go.transform, panelSoftSprite, PanelColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -175f), new Vector2(0f, 520f));
            attrPanelGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            CreateSectionHeader(attrPanelGo.transform, emptyArmsSprite, fontCinzel, "Attribute Points", -20f);

            string[] attrNames = { "Vigor", "Mind", "Endurance", "Strength", "Dexterity", "Intelligence", "Faith", "Arcane" };
            var attrRowList = new List<LevelUpUi.AttributeRowView>();

            float attrRowY = -60f;
            for (int i = 0; i < attrNames.Length; i++)
            {
                var rowView = CreateAttributeStepperRow(attrPanelGo.transform, fontGaramond, fontInter, attrNames[i], attrRowY, panelSoftSprite);
                attrRowList.Add(rowView);
                attrRowY -= 55f;
            }

            // Confirm Button
            GameObject confirmBtnGo = CreateButton("ConfirmButton", col1Go.transform, panelSoftSprite, StepperBtnColor, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(220f, 46f));
            CustomButton confirmBtn = confirmBtnGo.GetComponent<CustomButton>();
            TMP_Text confirmBtnText = confirmBtnGo.GetComponentInChildren<TMP_Text>();
            confirmBtnText.font = fontCinzel;
            confirmBtnText.fontSize = 20f;
            confirmBtnText.text = "Confirm";
            confirmBtnText.color = GoldTextColor;

            // ==========================================
            // COLUMN 2: Base Stats & Attack Power (Width 520, X: 520)
            // ==========================================
            GameObject col2Go = CreateUiElement("Column2", mainGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(530f, 0f), new Vector2(540f, 800f));
            col2Go.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);

            // Base Stats Panel
            GameObject baseStatsPanelGo = CreatePanel("BaseStatsPanel", col2Go.transform, panelSoftSprite, PanelColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -10f), new Vector2(0f, 360f));
            baseStatsPanelGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            CreateSectionHeader(baseStatsPanelGo.transform, emptyChestSprite, fontCinzel, "Base Stats", -20f);

            TMP_Text hpCurrText, hpNextText;
            CreateComparisonRow(baseStatsPanelGo.transform, fontGaramond, "HP", -60f, out hpCurrText, out hpNextText);

            TMP_Text fpCurrText, fpNextText;
            CreateComparisonRow(baseStatsPanelGo.transform, fontGaramond, "FP", -110f, out fpCurrText, out fpNextText);

            TMP_Text staCurrText, staNextText;
            CreateComparisonRow(baseStatsPanelGo.transform, fontGaramond, "Stamina", -160f, out staCurrText, out staNextText);

            TMP_Text eqCurrText, eqNextText;
            CreateComparisonRow(baseStatsPanelGo.transform, fontGaramond, "Max Equip Load", -210f, out eqCurrText, out eqNextText);

            TMP_Text poiseCurrText, poiseNextText;
            CreateComparisonRow(baseStatsPanelGo.transform, fontGaramond, "Poise", -260f, out poiseCurrText, out poiseNextText);

            TMP_Text discCurrText, discNextText;
            CreateComparisonRow(baseStatsPanelGo.transform, fontGaramond, "Discovery", -310f, out discCurrText, out discNextText);

            // Attack Power Panel
            GameObject attackPanelGo = CreatePanel("AttackPanel", col2Go.transform, panelSoftSprite, PanelColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -385f), new Vector2(0f, 410f));
            attackPanelGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            CreateSectionHeader(attackPanelGo.transform, emptyWeaponSprite, fontCinzel, "Attack Power", -20f);

            string[] attackNames = { "R Armament 1", "R Armament 2", "R Armament 3", "L Armament 1", "L Armament 2", "L Armament 3" };
            var armCurrList = new List<TMP_Text>();
            var armNextList = new List<TMP_Text>();

            float attackY = -60f;
            for (int i = 0; i < attackNames.Length; i++)
            {
                TMP_Text cText, nText;
                CreateComparisonRow(attackPanelGo.transform, fontGaramond, attackNames[i], attackY, out cText, out nText);
                armCurrList.Add(cText);
                armNextList.Add(nText);
                attackY -= 50f;
            }

            // ==========================================
            // COLUMN 3: Defense Power & Body (Width 580, X: 1080)
            // ==========================================
            GameObject col3Go = CreateUiElement("Column3", mainGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(1100f, 0f), new Vector2(600f, 800f));
            col3Go.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);

            // Defense Power Panel
            GameObject defensePanelGo = CreatePanel("DefensePanel", col3Go.transform, panelSoftSprite, PanelColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -10f), new Vector2(0f, 460f));
            defensePanelGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            CreateSectionHeader(defensePanelGo.transform, emptyShieldSprite, fontCinzel, "Defense Power", -20f);

            string[] defenseNames = { "Physical", "VS Strike", "VS Slash", "VS Pierce", "Magic", "Fire", "Lightning", "Holy" };
            var defCurrList = new List<TMP_Text>();
            var defNextList = new List<TMP_Text>();

            float defY = -60f;
            for (int i = 0; i < defenseNames.Length; i++)
            {
                TMP_Text cText, nText;
                CreateComparisonRow(defensePanelGo.transform, fontGaramond, defenseNames[i], defY, out cText, out nText);
                defCurrList.Add(cText);
                defNextList.Add(nText);
                defY -= 48f;
            }

            // Body Panel
            GameObject bodyPanelGo = CreatePanel("BodyPanel", col3Go.transform, panelSoftSprite, PanelColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -485f), new Vector2(0f, 310f));
            bodyPanelGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            CreateSectionHeader(bodyPanelGo.transform, emptyHeadSprite, fontCinzel, "Body", -20f);

            string[] bodyNames = { "Immunity", "Robustness", "Focus", "Vitality" };
            var bodyCurrList = new List<TMP_Text>();
            var bodyNextList = new List<TMP_Text>();

            float bodyY = -60f;
            for (int i = 0; i < bodyNames.Length; i++)
            {
                TMP_Text cText, nText;
                CreateComparisonRow(bodyPanelGo.transform, fontGaramond, bodyNames[i], bodyY, out cText, out nText);
                bodyCurrList.Add(cText);
                bodyNextList.Add(nText);
                bodyY -= 52f;
            }

            // ==========================================
            // Footer (Prompt & Back / Hotkeys)
            // ==========================================
            GameObject footerGo = CreateUiElement("Footer", contentRootGo.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 30f), new Vector2(-160f, 50f));
            footerGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);

            GameObject promptGo = CreateUiElement("PromptText", footerGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(400f, 30f));
            promptGo.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            TMP_Text promptText = promptGo.AddComponent<TextMeshProUGUI>();
            promptText.font = fontGaramond;
            promptText.fontSize = 20f;
            promptText.text = "Choose attribute to level up";
            promptText.color = SubtitleColor;

            GameObject backBtnGo = CreateButton("BackButton", footerGo.transform, null, Color.clear, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(140f, 36f));
            backBtnGo.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
            CustomButton backBtn = backBtnGo.GetComponent<CustomButton>();
            TMP_Text backBtnText = backBtnGo.GetComponentInChildren<TMP_Text>();
            backBtnText.font = fontInter;
            backBtnText.fontSize = 18f;
            backBtnText.text = "[Q] :Back";
            backBtnText.color = GoldTextColor;

            // ==========================================
            // Connect Serialized Fields on LevelUpUi
            // ==========================================
            SerializedObject so = new SerializedObject(levelUpUi);
            so.FindProperty("contentRoot").objectReferenceValue = contentRootRt;

            so.FindProperty("currentLevelText").objectReferenceValue = levelCurrText;
            so.FindProperty("nextLevelText").objectReferenceValue = levelNextText;
            so.FindProperty("currentRunesText").objectReferenceValue = runesHeldCurrText;
            so.FindProperty("projectedRunesText").objectReferenceValue = runesHeldNextText;
            so.FindProperty("runesNeededText").objectReferenceValue = runesNeededText;

            so.FindProperty("confirmButton").objectReferenceValue = confirmBtn;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("promptText").objectReferenceValue = promptText;

            so.FindProperty("hpCurrentText").objectReferenceValue = hpCurrText;
            so.FindProperty("hpNextText").objectReferenceValue = hpNextText;
            so.FindProperty("fpCurrentText").objectReferenceValue = fpCurrText;
            so.FindProperty("fpNextText").objectReferenceValue = fpNextText;
            so.FindProperty("staminaCurrentText").objectReferenceValue = staCurrText;
            so.FindProperty("staminaNextText").objectReferenceValue = staNextText;
            so.FindProperty("equipLoadCurrentText").objectReferenceValue = eqCurrText;
            so.FindProperty("equipLoadNextText").objectReferenceValue = eqNextText;
            so.FindProperty("poiseCurrentText").objectReferenceValue = poiseCurrText;
            so.FindProperty("poiseNextText").objectReferenceValue = poiseNextText;
            so.FindProperty("discoveryCurrentText").objectReferenceValue = discCurrText;
            so.FindProperty("discoveryNextText").objectReferenceValue = discNextText;

            SerializedProperty attrRowsProp = so.FindProperty("attributeRows");
            attrRowsProp.arraySize = attrRowList.Count;
            for (int i = 0; i < attrRowList.Count; i++)
            {
                SerializedProperty elem = attrRowsProp.GetArrayElementAtIndex(i);
                SetObjectRef(elem, "labelText", GetField<TMP_Text>(attrRowList[i], "labelText"));
                SetObjectRef(elem, "currentValueText", GetField<TMP_Text>(attrRowList[i], "currentValueText"));
                SetObjectRef(elem, "nextValueText", GetField<TMP_Text>(attrRowList[i], "nextValueText"));
                SetObjectRef(elem, "decrementButton", attrRowList[i].DecrementButton);
                SetObjectRef(elem, "incrementButton", attrRowList[i].IncrementButton);
                SetObjectRef(elem, "selectionHighlight", GetField<GameObject>(attrRowList[i], "selectionHighlight"));
            }

            SetTextListProperty(so, "armamentCurrentTexts", armCurrList);
            SetTextListProperty(so, "armamentNextTexts", armNextList);

            SetTextListProperty(so, "defenseCurrentTexts", defCurrList);
            SetTextListProperty(so, "defenseNextTexts", defNextList);

            SetTextListProperty(so, "bodyCurrentTexts", bodyCurrList);
            SetTextListProperty(so, "bodyNextTexts", bodyNextList);

            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PREFAB_PATH);
            Object.DestroyImmediate(rootGo);
            AssetDatabase.SaveAssets();

            Debug.Log($"LevelUpUi prefab created successfully at {PREFAB_PATH}");
            return prefab;
        }

        private static GameObject CreateUiElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Sprite sprite, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = CreateUiElement(name, parent, anchorMin, anchorMax, anchoredPos, sizeDelta);
            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            return go;
        }

        private static void CreateSectionHeader(Transform parent, Sprite icon, TMP_FontAsset font, string title, float yPos)
        {
            GameObject headerGo = CreateUiElement("SectionHeader", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos), new Vector2(-40f, 32f));
            headerGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            GameObject iconGo = CreateUiElement("Icon", headerGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(24f, 24f));
            Image img = iconGo.AddComponent<Image>();
            img.sprite = icon;
            img.color = new Color(0.85f, 0.82f, 0.75f, 0.9f);
            img.raycastTarget = false;

            GameObject titleGo = CreateUiElement("Title", headerGo.transform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(24f, 0f), new Vector2(-48f, 28f));
            titleGo.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            TMP_Text text = titleGo.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = 20f;
            text.text = title;
            text.color = GoldTextColor;
            text.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateComparisonRow(Transform parent, TMP_FontAsset font, string label, float yPos, out TMP_Text currentText, out TMP_Text nextText)
        {
            GameObject rowGo = CreateUiElement("Row_" + label, parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos), new Vector2(-40f, 36f));
            rowGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            // Label
            GameObject labelGo = CreateUiElement("Label", rowGo.transform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10f, 0f), new Vector2(-20f, 30f));
            labelGo.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            TMP_Text labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.font = font;
            labelTmp.fontSize = 21f;
            labelTmp.text = label;
            labelTmp.color = GoldTextColor;
            labelTmp.alignment = TextAlignmentOptions.Left;

            // Current value
            GameObject currGo = CreateUiElement("CurrentValue", rowGo.transform, new Vector2(0.55f, 0.5f), new Vector2(0.72f, 0.5f), Vector2.zero, new Vector2(0f, 30f));
            currentText = currGo.AddComponent<TextMeshProUGUI>();
            currentText.font = font;
            currentText.fontSize = 21f;
            currentText.text = "10";
            currentText.color = GoldTextColor;
            currentText.alignment = TextAlignmentOptions.Right;

            // Arrow
            GameObject arrowGo = CreateUiElement("Arrow", rowGo.transform, new Vector2(0.76f, 0.5f), new Vector2(0.82f, 0.5f), Vector2.zero, new Vector2(0f, 30f));
            TMP_Text arrowTmp = arrowGo.AddComponent<TextMeshProUGUI>();
            arrowTmp.font = font;
            arrowTmp.fontSize = 20f;
            arrowTmp.text = "→";
            arrowTmp.color = ArrowColor;
            arrowTmp.alignment = TextAlignmentOptions.Center;

            // Next value
            GameObject nextGo = CreateUiElement("NextValue", rowGo.transform, new Vector2(0.84f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 0f), new Vector2(0f, 30f));
            nextText = nextGo.AddComponent<TextMeshProUGUI>();
            nextText.font = font;
            nextText.fontSize = 21f;
            nextText.text = "10";
            nextText.color = GoldTextColor;
            nextText.alignment = TextAlignmentOptions.Right;
        }

        private static void CreateSingleStatRow(Transform parent, TMP_FontAsset font, string label, float yPos, out TMP_Text valueText)
        {
            GameObject rowGo = CreateUiElement("Row_" + label, parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos), new Vector2(-40f, 36f));
            rowGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            GameObject labelGo = CreateUiElement("Label", rowGo.transform, new Vector2(0f, 0.5f), new Vector2(0.6f, 0.5f), new Vector2(10f, 0f), new Vector2(-20f, 30f));
            labelGo.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            TMP_Text labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.font = font;
            labelTmp.fontSize = 21f;
            labelTmp.text = label;
            labelTmp.color = GoldTextColor;
            labelTmp.alignment = TextAlignmentOptions.Left;

            GameObject valGo = CreateUiElement("Value", rowGo.transform, new Vector2(0.65f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 0f), new Vector2(0f, 30f));
            valueText = valGo.AddComponent<TextMeshProUGUI>();
            valueText.font = font;
            valueText.fontSize = 21f;
            valueText.text = "0";
            valueText.color = GoldTextColor;
            valueText.alignment = TextAlignmentOptions.Right;
        }

        private static LevelUpUi.AttributeRowView CreateAttributeStepperRow(
            Transform parent,
            TMP_FontAsset fontGaramond,
            TMP_FontAsset fontInter,
            string label,
            float yPos,
            Sprite panelSprite)
        {
            GameObject rowGo = CreateUiElement("Row_" + label, parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos), new Vector2(-24f, 44f));
            rowGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            // Selection Highlight
            GameObject highlightGo = CreateUiElement("SelectionHighlight", rowGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image highlightImg = highlightGo.AddComponent<Image>();
            highlightImg.color = new Color(0.35f, 0.38f, 0.32f, 0.25f);
            highlightGo.SetActive(false);

            // Label
            GameObject labelGo = CreateUiElement("Label", rowGo.transform, new Vector2(0f, 0.5f), new Vector2(0.42f, 0.5f), new Vector2(8f, 0f), new Vector2(-16f, 32f));
            labelGo.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
            TMP_Text labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.font = fontGaramond;
            labelTmp.fontSize = 21f;
            labelTmp.text = label;
            labelTmp.color = GoldTextColor;
            labelTmp.alignment = TextAlignmentOptions.Left;

            // Current value
            GameObject currGo = CreateUiElement("CurrentValue", rowGo.transform, new Vector2(0.44f, 0.5f), new Vector2(0.58f, 0.5f), Vector2.zero, new Vector2(0f, 32f));
            TMP_Text currTmp = currGo.AddComponent<TextMeshProUGUI>();
            currTmp.font = fontGaramond;
            currTmp.fontSize = 21f;
            currTmp.text = "10";
            currTmp.color = GoldTextColor;
            currTmp.alignment = TextAlignmentOptions.Right;

            // Arrow
            GameObject arrowGo = CreateUiElement("Arrow", rowGo.transform, new Vector2(0.60f, 0.5f), new Vector2(0.68f, 0.5f), Vector2.zero, new Vector2(0f, 32f));
            TMP_Text arrowTmp = arrowGo.AddComponent<TextMeshProUGUI>();
            arrowTmp.font = fontGaramond;
            arrowTmp.fontSize = 20f;
            arrowTmp.text = "→";
            arrowTmp.color = ArrowColor;
            arrowTmp.alignment = TextAlignmentOptions.Center;

            // Stepper Container (< value >)
            GameObject stepperGo = CreateUiElement("Stepper", rowGo.transform, new Vector2(0.70f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-4f, 0f), new Vector2(0f, 36f));

            // Decrement Button (<)
            GameObject decBtnGo = CreateButton("DecButton", stepperGo.transform, panelSprite, StepperBtnColor, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(28f, 32f));
            CustomButton decBtn = decBtnGo.GetComponent<CustomButton>();
            TMP_Text decText = decBtnGo.GetComponentInChildren<TMP_Text>();
            decText.font = fontInter;
            decText.fontSize = 18f;
            decText.text = "<";
            decText.color = GoldTextColor;

            // Next Value
            GameObject nextValGo = CreateUiElement("NextValue", stepperGo.transform, new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f), Vector2.zero, new Vector2(0f, 32f));
            TMP_Text nextTmp = nextValGo.AddComponent<TextMeshProUGUI>();
            nextTmp.font = fontGaramond;
            nextTmp.fontSize = 21f;
            nextTmp.text = "10";
            nextTmp.color = GoldTextColor;
            nextTmp.alignment = TextAlignmentOptions.Center;

            // Increment Button (>)
            GameObject incBtnGo = CreateButton("IncButton", stepperGo.transform, panelSprite, StepperBtnColor, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(28f, 32f));
            CustomButton incBtn = incBtnGo.GetComponent<CustomButton>();
            TMP_Text incText = incBtnGo.GetComponentInChildren<TMP_Text>();
            incText.font = fontInter;
            incText.fontSize = 18f;
            incText.text = ">";
            incText.color = GoldTextColor;

            var rowView = new LevelUpUi.AttributeRowView();
            SetField(rowView, "labelText", labelTmp);
            SetField(rowView, "currentValueText", currTmp);
            SetField(rowView, "nextValueText", nextTmp);
            SetField(rowView, "decrementButton", decBtn);
            SetField(rowView, "incrementButton", incBtn);
            SetField(rowView, "selectionHighlight", highlightGo);

            return rowView;
        }

        private static GameObject CreateButton(string name, Transform parent, Sprite sprite, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = CreateUiElement(name, parent, anchorMin, anchorMax, anchoredPos, sizeDelta);
            Image img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
            }
            img.color = color;

            CustomButton btn = go.AddComponent<CustomButton>();
            btn.targetGraphic = img;

            GameObject textGo = CreateUiElement("Text", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TMP_Text tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;

            return go;
        }

        private static void SetTextListProperty(SerializedObject so, string propName, List<TMP_Text> list)
        {
            SerializedProperty prop = so.FindProperty(propName);
            prop.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            }
        }

        private static void SetObjectRef(SerializedProperty parent, string childName, Object obj)
        {
            SerializedProperty child = parent.FindPropertyRelative(childName);
            if (child != null)
            {
                child.objectReferenceValue = obj;
            }
        }

        private static void SetField(object obj, string fieldName, object val)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            field?.SetValue(obj, val);
        }

        private static T GetField<T>(object obj, string fieldName) where T : class
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return field?.GetValue(obj) as T;
        }
    }
}
#endif
