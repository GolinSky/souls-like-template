using System;
using System.Ui.Base;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Settings
{
    public sealed class SettingsOptionUi : MonoBehaviour
    {
        [SerializeField] private SettingsTab tab;
        [SerializeField] private SettingsOptionId optionId;
        [SerializeField] private Slider slider;
        [SerializeField] private Toggle toggle;
        [SerializeField] private CustomButton actionButton;
        [SerializeField] private TMP_Text valueText;

        public event Action<SettingsOptionId, float> FloatValueChanged;
        public event Action<SettingsOptionId, bool> BoolValueChanged;
        public event Action<SettingsOptionId> ActionRequested;

        public SettingsTab Tab => tab;
        public SettingsOptionId OptionId => optionId;

        private void Awake()
        {
            if (slider == null && toggle == null && actionButton == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SettingsOptionUi)} '{name}' requires a slider, toggle, or action button.");
            }

            if (slider != null)
            {
                slider.onValueChanged.AddListener(HandleSliderChanged);
            }

            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(HandleToggleChanged);
            }

            if (actionButton != null)
            {
                actionButton.onClick.AddListener(HandleActionRequested);
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetFloat(float value, string displayValue)
        {
            if (slider != null)
            {
                slider.SetValueWithoutNotify(value);
            }

            SetDisplayValue(displayValue);
        }

        public void SetToggle(bool value, string displayValue)
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(value);
            }

            SetDisplayValue(displayValue);
        }

        public void SetActionValue(string displayValue)
        {
            SetDisplayValue(displayValue);
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(HandleSliderChanged);
            }

            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveListener(HandleActionRequested);
            }
        }

        private void HandleSliderChanged(float value)
        {
            FloatValueChanged?.Invoke(optionId, value);
        }

        private void HandleToggleChanged(bool value)
        {
            BoolValueChanged?.Invoke(optionId, value);
        }

        private void HandleActionRequested()
        {
            ActionRequested?.Invoke(optionId);
        }

        private void SetDisplayValue(string displayValue)
        {
            if (valueText != null)
            {
                valueText.text = displayValue;
            }
        }
    }
}
