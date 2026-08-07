using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Base
{
    public class CustomButtonToggle : Toggle
    {
        [Header("Custom Toggle Settings")]
        [SerializeField] private TMP_Text toggleText;
        [SerializeField] private Image additionalIcon;
        
        [Header("Text & Icon Tint Settings")]
        [SerializeField] private bool tintTextAndIcon = true;
        [SerializeField] private ColorBlock textColorBlock = ColorBlock.defaultColorBlock;

        [Header("State Graphics Settings")]
        [SerializeField] private GameObject[] enableWhenOn;
        [SerializeField] private GameObject[] enableWhenOff;

        public bool HasText => toggleText != null;
        public bool HasAdditionalIcon => additionalIcon != null;

        protected override void Awake()
        {
            base.Awake();
            if (onValueChanged != null)
            {
                onValueChanged.AddListener(UpdateStateGraphics);
            }
            UpdateStateGraphics(isOn);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            onValueChanged.RemoveListener(UpdateStateGraphics);
        }

        private void UpdateStateGraphics(bool isOnState)
        {
            if (enableWhenOn != null)
            {
                foreach (var go in enableWhenOn)
                {
                    if (go != null) go.SetActive(isOnState);
                }
            }

            if (enableWhenOff != null)
            {
                foreach (var go in enableWhenOff)
                {
                    if (go != null) go.SetActive(!isOnState);
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // Automatically update graphics in editor if toggled
            if (!Application.isPlaying)
            {
                // Unity sometimes calls OnValidate before everything is initialized, 
                // so we use a delayed call to avoid Unity warnings.
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null && gameObject != null)
                    {
                        UpdateStateGraphics(isOn);
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                };
            }
        }
#endif

        public void SetText(string text)
        {
            if (HasText)
            {
                toggleText.text = text;
            }
        }
        
        public string GetText()
        {
            return HasText ? toggleText.text : string.Empty;
        }

        public void SetAdditionalIcon(Sprite icon)
        {
            if (HasAdditionalIcon)
            {
                additionalIcon.sprite = icon;
            }
        }
        
        public Sprite GetAdditionalIcon()
        {
            return HasAdditionalIcon ? additionalIcon.sprite : null;
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
                toggleText.CrossFadeColor(finalColor, duration, true, true);
            }
            
            if (HasAdditionalIcon && additionalIcon != targetGraphic && additionalIcon != graphic)
            {
                additionalIcon.CrossFadeColor(finalColor, duration, true, true);
            }
        }
    }
}
