using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace System.Ui.Base
{
    [Serializable]
    public class CustomButton : Button, ICustomButton
    {
        [Header("Custom Button Settings")]
        [SerializeField] private InputTypes inputType;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Image innerIcon;
        [SerializeField] private TMP_Text additionalText;
        
        [Header("Text & Icon Tint Settings")]
        [SerializeField] private bool tintTextAndIcon = true;
        [SerializeField] private ColorBlock textColorBlock = ColorBlock.defaultColorBlock;
        
        public InputTypes InputType => inputType;
        
        public bool HasText => buttonText != null;
        public bool HasIcon => innerIcon != null;
        public bool HasAdditionalText => additionalText != null;

        protected override void Awake()
        {
            base.Awake();
            if (additionalText != null) additionalText.gameObject.SetActive(false);
            if (innerIcon != null) innerIcon.gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            if (HasText)
            {
                buttonText.text = text;
            }
            else
            {
                Debug.LogError("CustomButton: Trying to set text, but no buttonText component is assigned.");
            }
        }
        
        public string GetText()
        {
            return HasText ? buttonText.text : string.Empty;
        }

        public void SetIcon(Sprite icon)
        {
            if (HasIcon)
            {
                innerIcon.sprite = icon;
                innerIcon.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("CustomButton: Trying to set icon, but no innerIcon component is assigned.");
            }
        }
        
        public Sprite GetIcon()
        {
            return HasIcon ? innerIcon.sprite : null;
        }

        public void SetAdditionalText(string text)
        {
            if (HasAdditionalText)
            {
                additionalText.text = text;
                additionalText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("CustomButton: Trying to set additional text, but no additionalText component is assigned.");
            }
        }
        
        public string GetAdditionalText()
        {
            return HasAdditionalText ? additionalText.text : string.Empty;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            
            if (!tintTextAndIcon || !gameObject.activeInHierarchy) return;
            
            Color tintColor = state switch
            {
                SelectionState.Normal => textColorBlock.normalColor,
                SelectionState.Highlighted => textColorBlock.highlightedColor,
                SelectionState.Pressed => textColorBlock.pressedColor,
                SelectionState.Selected => textColorBlock.selectedColor,
                SelectionState.Disabled => textColorBlock.disabledColor,
                _ => Color.white
            };
            
            var finalColor = tintColor * textColorBlock.colorMultiplier;
            float duration = instant ? 0f : colors.fadeDuration;
            
            if (HasText)
            {
                buttonText.CrossFadeColor(finalColor, duration, true, true);
            }
            
            if (HasAdditionalText)
            {
                additionalText.CrossFadeColor(finalColor, duration, true, true);
            }
            
            if (HasIcon && innerIcon != targetGraphic)
            {
                innerIcon.CrossFadeColor(finalColor, duration, true, true);
            }
        }
    }
}
