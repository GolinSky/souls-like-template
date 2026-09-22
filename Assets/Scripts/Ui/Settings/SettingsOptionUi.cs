using System;
using System.Collections.Generic;
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
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TMP_Text valueText;

        public event Action<SettingsOptionId, float> FloatValueChanged;
        public event Action<SettingsOptionId, bool> BoolValueChanged;
        public event Action<SettingsOptionId, int> DropdownValueChanged;
        public event Action<SettingsOptionId> ActionRequested;

        public SettingsTab Tab => tab;
        public SettingsOptionId OptionId => optionId;

        private void Awake()
        {
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

            if (dropdown != null)
            {
                dropdown.onValueChanged.AddListener(HandleDropdownChanged);
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

        public void SetDropdown(int selectedIndex, List<string> options)
        {
            if (dropdown != null)
            {
                bool optionsChanged = dropdown.options.Count != options.Count;
                if (!optionsChanged)
                {
                    for (int i = 0; i < options.Count; i++)
                    {
                        if (dropdown.options[i].text != options[i])
                        {
                            optionsChanged = true;
                            break;
                        }
                    }
                }

                if (optionsChanged)
                {
                    dropdown.ClearOptions();
                    dropdown.AddOptions(options);
                }

                dropdown.SetValueWithoutNotify(selectedIndex);
                dropdown.RefreshShownValue();
            }
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

            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveListener(HandleDropdownChanged);
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

        private void HandleDropdownChanged(int value)
        {
            DropdownValueChanged?.Invoke(optionId, value);
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
