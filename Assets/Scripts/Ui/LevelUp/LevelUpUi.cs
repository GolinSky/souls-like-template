using System;
using System.Collections.Generic;
using System.Ui.Base;
using SoulsLike.Entities.Character;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.LevelUp
{
    public sealed class LevelUpUi : BaseUi
    {
        private const float REFERENCE_WIDTH = 1920f;
        private const float REFERENCE_HEIGHT = 1080f;

        [Serializable]
        public sealed class AttributeRowView
        {
            [SerializeField] private TMP_Text labelText;
            [SerializeField] private TMP_Text currentValueText;
            [SerializeField] private TMP_Text nextValueText;
            [SerializeField] private CustomButton decrementButton;
            [SerializeField] private CustomButton incrementButton;
            [SerializeField] private GameObject selectionHighlight;

            public CustomButton DecrementButton => decrementButton;
            public CustomButton IncrementButton => incrementButton;

            public void SetValues(int current, int next, bool isSelected)
            {
                currentValueText.text = LevelUpUiFormatter.FormatWholeNumber(current);
                nextValueText.text = LevelUpUiFormatter.FormatNextValue(current, next);
                if (selectionHighlight != null)
                {
                    selectionHighlight.SetActive(isSelected);
                }
            }
        }

        [Header("Reference Layout")]
        [SerializeField] private RectTransform contentRoot;

        [Header("Profile")]
        [SerializeField] private TMP_Text currentLevelText;
        [SerializeField] private TMP_Text nextLevelText;
        [SerializeField] private TMP_Text currentRunesText;
        [SerializeField] private TMP_Text projectedRunesText;
        [SerializeField] private TMP_Text runesNeededText;

        [Header("Attribute Stepper Rows")]
        [SerializeField] private List<AttributeRowView> attributeRows = new();
        [SerializeField] private CustomButton confirmButton;

        [Header("Base Stats")]
        [SerializeField] private TMP_Text hpCurrentText;
        [SerializeField] private TMP_Text hpNextText;
        [SerializeField] private TMP_Text fpCurrentText;
        [SerializeField] private TMP_Text fpNextText;
        [SerializeField] private TMP_Text staminaCurrentText;
        [SerializeField] private TMP_Text staminaNextText;
        [SerializeField] private TMP_Text equipLoadCurrentText;
        [SerializeField] private TMP_Text equipLoadNextText;
        [SerializeField] private TMP_Text poiseCurrentText;
        [SerializeField] private TMP_Text poiseNextText;
        [SerializeField] private TMP_Text discoveryCurrentText;
        [SerializeField] private TMP_Text discoveryNextText;

        [Header("Armament Attack")]
        [SerializeField] private List<TMP_Text> armamentCurrentTexts = new();
        [SerializeField] private List<TMP_Text> armamentNextTexts = new();

        [Header("Defense Power")]
        [SerializeField] private List<TMP_Text> defenseCurrentTexts = new();
        [SerializeField] private List<TMP_Text> defenseNextTexts = new();

        [Header("Body")]
        [SerializeField] private List<TMP_Text> bodyCurrentTexts = new();
        [SerializeField] private List<TMP_Text> bodyNextTexts = new();

        [Header("Footer")]
        [SerializeField] private CustomButton backButton;
        [SerializeField] private TMP_Text promptText;

        private ILevelUpPresenter _presenter;
        private RectTransform _rootRectTransform;

        public void AssignPresenter(ILevelUpPresenter presenter)
        {
            _presenter = presenter;
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(_presenter.Confirm);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(_presenter.Back);
            }

            for (int i = 0; i < attributeRows.Count; i++)
            {
                int index = i;
                if (attributeRows[i].DecrementButton != null)
                {
                    attributeRows[i].DecrementButton.onClick.AddListener(() => _presenter.DecrementAttribute(index));
                }

                if (attributeRows[i].IncrementButton != null)
                {
                    attributeRows[i].IncrementButton.onClick.AddListener(() => _presenter.IncrementAttribute(index));
                }
            }
        }

        public override void Show()
        {
            base.Show();
            if (attributeRows.Count > 0 && attributeRows[0].IncrementButton != null)
            {
                attributeRows[0].IncrementButton.Select();
            }
            else if (confirmButton != null)
            {
                confirmButton.Select();
            }
        }

        public void DisplayProfile(
            int currentLevel,
            int nextLevel,
            int currentRunes,
            int projectedRunes,
            int runesNeeded,
            bool isAffordable)
        {
            currentLevelText.text = LevelUpUiFormatter.FormatWholeNumber(currentLevel);
            nextLevelText.text = LevelUpUiFormatter.FormatNextValue(currentLevel, nextLevel);
            currentRunesText.text = LevelUpUiFormatter.FormatWholeNumber(currentRunes);
            projectedRunesText.text = LevelUpUiFormatter.FormatWholeNumber(projectedRunes);
            runesNeededText.text = LevelUpUiFormatter.FormatRunesNeeded(runesNeeded, isAffordable);
        }

        public void DisplayAttributeRow(int index, int current, int next, bool isSelected)
        {
            if (index >= 0 && index < attributeRows.Count)
            {
                attributeRows[index].SetValues(current, next, isSelected);
            }
        }

        public void DisplayBaseStats(
            float currentHp, float nextHp,
            float currentFp, float nextFp,
            float currentStamina, float nextStamina,
            float currentEquip, float nextEquip,
            float currentPoise, float nextPoise,
            float currentDisc, float nextDisc)
        {
            hpCurrentText.text = LevelUpUiFormatter.FormatWholeNumber(currentHp);
            hpNextText.text = LevelUpUiFormatter.FormatNextValue(currentHp, nextHp);

            fpCurrentText.text = LevelUpUiFormatter.FormatWholeNumber(currentFp);
            fpNextText.text = LevelUpUiFormatter.FormatNextValue(currentFp, nextFp);

            staminaCurrentText.text = LevelUpUiFormatter.FormatWholeNumber(currentStamina);
            staminaNextText.text = LevelUpUiFormatter.FormatNextValue(currentStamina, nextStamina);

            equipLoadCurrentText.text = LevelUpUiFormatter.FormatDecimal(currentEquip);
            equipLoadNextText.text = LevelUpUiFormatter.FormatNextValue(currentEquip, nextEquip, 1);

            poiseCurrentText.text = LevelUpUiFormatter.FormatWholeNumber(currentPoise);
            poiseNextText.text = LevelUpUiFormatter.FormatNextValue(currentPoise, nextPoise);

            discoveryCurrentText.text = LevelUpUiFormatter.FormatDecimal(currentDisc);
            discoveryNextText.text = LevelUpUiFormatter.FormatNextValue(currentDisc, nextDisc, 1);
        }

        public void DisplayArmamentAttacks(IReadOnlyList<int> current, IReadOnlyList<int> next)
        {
            for (int i = 0; i < armamentCurrentTexts.Count && i < current.Count; i++)
            {
                armamentCurrentTexts[i].text = LevelUpUiFormatter.FormatWholeNumber(current[i]);
            }

            for (int i = 0; i < armamentNextTexts.Count && i < next.Count; i++)
            {
                armamentNextTexts[i].text = LevelUpUiFormatter.FormatNextValue(current[i], next[i]);
            }
        }

        public void DisplayDefenses(IReadOnlyList<float> current, IReadOnlyList<float> next)
        {
            for (int i = 0; i < defenseCurrentTexts.Count && i < current.Count; i++)
            {
                defenseCurrentTexts[i].text = LevelUpUiFormatter.FormatWholeNumber(current[i]);
            }

            for (int i = 0; i < defenseNextTexts.Count && i < next.Count; i++)
            {
                defenseNextTexts[i].text = LevelUpUiFormatter.FormatNextValue(current[i], next[i]);
            }
        }

        public void DisplayBodyStats(IReadOnlyList<float> current, IReadOnlyList<float> next)
        {
            for (int i = 0; i < bodyCurrentTexts.Count && i < current.Count; i++)
            {
                bodyCurrentTexts[i].text = LevelUpUiFormatter.FormatWholeNumber(current[i]);
            }

            for (int i = 0; i < bodyNextTexts.Count && i < next.Count; i++)
            {
                bodyNextTexts[i].text = LevelUpUiFormatter.FormatNextValue(current[i], next[i]);
            }
        }

        public void SetConfirmInteractable(bool isInteractable)
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = isInteractable;
            }
        }

        public void SetPrompt(string prompt)
        {
            if (promptText != null)
            {
                promptText.text = prompt;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _rootRectTransform = (RectTransform)transform;
            FitContentToRoot();
        }

        private void OnDestroy()
        {
            if (_presenter == null)
            {
                return;
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(_presenter.Confirm);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(_presenter.Back);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rootRectTransform != null)
            {
                FitContentToRoot();
            }
        }

        private void FitContentToRoot()
        {
            if (contentRoot == null || _rootRectTransform == null)
            {
                return;
            }

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
    }
}
