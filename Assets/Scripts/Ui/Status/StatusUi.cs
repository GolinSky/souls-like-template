using System.Collections.Generic;
using System.Ui.Base;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Status
{
    public sealed class StatusUi : BaseUi
    {
        private const float REFERENCE_WIDTH = 1920f;
        private const float REFERENCE_HEIGHT = 1080f;

        [Header("Reference Layout")]
        [SerializeField] private RectTransform contentRoot;

        [Header("Profile")]
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text heldRunesText;
        [SerializeField] private TMP_Text nextLevelRunesText;

        [Header("Attributes")]
        [SerializeField] private TMP_Text vigorText;
        [SerializeField] private TMP_Text mindText;
        [SerializeField] private TMP_Text enduranceText;
        [SerializeField] private TMP_Text strengthText;
        [SerializeField] private TMP_Text dexterityText;
        [SerializeField] private TMP_Text intelligenceText;
        [SerializeField] private TMP_Text faithText;
        [SerializeField] private TMP_Text arcaneText;

        [Header("Base Stats")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text focusText;
        [SerializeField] private TMP_Text staminaText;
        [SerializeField] private TMP_Text equipLoadText;
        [SerializeField] private Image equipLoadFill;
        [SerializeField] private TMP_Text loadClassText;
        [SerializeField] private TMP_Text poiseText;
        [SerializeField] private TMP_Text discoveryText;
        [SerializeField] private TMP_Text memorySlotsText;
        [SerializeField] private TMP_Text memoryEmptyStateText;
        [SerializeField] private GameObject spellBlockRoot;

        [Header("Combat")]
        [SerializeField] private GameObject combatDetailsRoot;
        [SerializeField] private List<TMP_Text> armamentAttackTexts = new();
        [SerializeField] private List<TMP_Text> defenseTexts = new();
        [SerializeField] private List<TMP_Text> negationTexts = new();
        [SerializeField] private List<TMP_Text> resistanceTexts = new();

        [Header("Help and Footer")]
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private TMP_Text helpText;
        [SerializeField] private CustomButton backButton;
        [SerializeField] private CustomButton simpleViewButton;
        [SerializeField] private CustomButton helpButton;

        private IStatusPresenter _presenter;
        private RectTransform _rootRectTransform;

        public void AssignPresenter(IStatusPresenter presenter)
        {
            _presenter = presenter;
            backButton.onClick.AddListener(_presenter.Back);
            simpleViewButton.onClick.AddListener(_presenter.ToggleSimpleView);
            helpButton.onClick.AddListener(_presenter.ToggleHelp);
        }

        public override void Show()
        {
            base.Show();
            backButton.Select();
        }

        public void DisplayProfile(int heldRunes)
        {
            heldRunesText.text = StatusUiFormatter.FormatWholeNumber(heldRunes);
        }

        public void DisplayAttributes(CharacterAttributeStats attributes)
        {
            vigorText.text = attributes.Vigor.ToString();
            mindText.text = attributes.Mind.ToString();
            enduranceText.text = attributes.Endurance.ToString();
            strengthText.text = attributes.Strength.ToString();
            dexterityText.text = attributes.Dexterity.ToString();
            intelligenceText.text = attributes.Intelligence.ToString();
            faithText.text = attributes.Faith.ToString();
            arcaneText.text = attributes.Arcane.ToString();
        }

        public void DisplayBaseStats(
            HealthStats healthStats,
            float equipmentWeight,
            float equipmentCapacity,
            float currentPoise)
        {
            healthText.text = StatusUiFormatter.FormatCurrentAndMaximum(
                healthStats.CurrentHealth,
                healthStats.MaxHealth);
            focusText.text = StatusUiFormatter.FormatCurrentAndMaximum(
                healthStats.CurrentFocus,
                healthStats.MaxFocus);
            staminaText.text = StatusUiFormatter.FormatCurrentAndMaximum(
                healthStats.DisplayCurrentStamina,
                healthStats.MaxStamina);
            equipLoadText.text = StatusUiFormatter.FormatEquipLoad(
                equipmentWeight,
                equipmentCapacity);
            equipLoadFill.fillAmount = Mathf.Clamp01(equipmentWeight / equipmentCapacity);
            poiseText.text = StatusUiFormatter.FormatWholeNumber(currentPoise);
        }

        public void DisplayPoise(float currentPoise)
        {
            poiseText.text = StatusUiFormatter.FormatWholeNumber(currentPoise);
        }

        public void DisplayArmamentAttacks(IReadOnlyList<int> attacks)
        {
            for (int index = 0; index < armamentAttackTexts.Count; index++)
            {
                armamentAttackTexts[index].text = attacks[index].ToString();
            }
        }

        public void DisplayUnavailableValues()
        {
            characterNameText.text = StatusUiFormatter.UNAVAILABLE_VALUE;
            levelText.text = StatusUiFormatter.UNAVAILABLE_VALUE;
            nextLevelRunesText.text = StatusUiFormatter.UNAVAILABLE_VALUE;
            loadClassText.text = StatusUiFormatter.UNAVAILABLE_VALUE;
            discoveryText.text = StatusUiFormatter.UNAVAILABLE_VALUE;
            memorySlotsText.text = StatusUiFormatter.UNAVAILABLE_VALUE;
            memoryEmptyStateText.text = "Spells unavailable";
            SetUnavailable(defenseTexts);
            SetUnavailable(negationTexts);
            SetUnavailable(resistanceTexts);
            helpText.text = "— means this information is not available yet. "
                + "Poise shows the current poise meter. Armament attack shows total base damage.";
        }

        public void SetSimpleView(bool isSimpleView)
        {
            combatDetailsRoot.SetActive(!isSimpleView);
            spellBlockRoot.SetActive(!isSimpleView);
        }

        public void SetHelpVisible(bool isVisible)
        {
            helpPanel.SetActive(isVisible);
        }

        protected override void Awake()
        {
            base.Awake();
            _rootRectTransform = (RectTransform)transform;
            ConfigureFooterNavigation();
            FitContentToRoot();
            SetSimpleView(false);
            SetHelpVisible(false);
        }

        private void OnDestroy()
        {
            if (_presenter == null)
            {
                return;
            }

            backButton.onClick.RemoveListener(_presenter.Back);
            simpleViewButton.onClick.RemoveListener(_presenter.ToggleSimpleView);
            helpButton.onClick.RemoveListener(_presenter.ToggleHelp);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rootRectTransform != null)
            {
                FitContentToRoot();
            }
        }

        private void ConfigureFooterNavigation()
        {
            ConfigureNavigation(backButton, null, simpleViewButton);
            ConfigureNavigation(simpleViewButton, backButton, helpButton);
            ConfigureNavigation(helpButton, simpleViewButton, null);
        }

        private void FitContentToRoot()
        {
            contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
            contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
            contentRoot.pivot = new Vector2(0.5f, 0.5f);
            contentRoot.anchoredPosition = Vector2.zero;
            contentRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, REFERENCE_WIDTH);
            contentRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, REFERENCE_HEIGHT);

            Rect rootRect = _rootRectTransform.rect;
            float scale = Mathf.Min(
                rootRect.width / REFERENCE_WIDTH,
                rootRect.height / REFERENCE_HEIGHT);
            contentRoot.localScale = Vector3.one * scale;
        }

        private static void ConfigureNavigation(
            CustomButton button,
            Selectable left,
            Selectable right)
        {
            UnityEngine.UI.Navigation navigation = new UnityEngine.UI.Navigation
            {
                mode = UnityEngine.UI.Navigation.Mode.Explicit,
                selectOnLeft = left,
                selectOnRight = right
            };
            button.navigation = navigation;
        }

        private static void SetUnavailable(IEnumerable<TMP_Text> fields)
        {
            foreach (TMP_Text field in fields)
            {
                field.text = StatusUiFormatter.UNAVAILABLE_VALUE;
            }
        }
    }
}
