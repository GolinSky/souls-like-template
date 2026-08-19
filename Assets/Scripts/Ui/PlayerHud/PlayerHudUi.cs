using System;
using MPUIKIT;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.PlayerHud
{
    public class PlayerHudUi : BaseUi
    {
        [System.Serializable]
        public class StatBar
        {
            [Header("Container & Components")]
            public RectTransform container;
            public MPImage primaryBar;
            public MPImage trailingBufferBar;

            [Header("Scaling Settings")]
            public float baseWidth = 200f;
            public float scaleFactor = 0.25f;

            [Header("Colors")]
            public Color primaryColor = Color.red;
            public Color bufferColor = Color.yellow;

            // Runtime state
            [HideInInspector] public float currentFill = 1f;
            [HideInInspector] public float trailingFill = 1f;
            [HideInInspector] public float targetFill = 1f;
            [HideInInspector] public float holdTimer = 0f;
            [HideInInspector] public float animateTimer = 0f;
            [HideInInspector] public float bufferStartFill = 1f;
            [HideInInspector] public bool isHolding = false;
            [HideInInspector] public bool isAnimating = false;

            private const float HOLD_DURATION = 0.4f;
            private const float SLIDE_DURATION = 0.6f;

            public void Initialize(Color defaultPrimary, Color defaultBuffer)
            {
                if (primaryBar != null && primaryColor == default)
                {
                    primaryBar.color = defaultPrimary;
                }
                if (trailingBufferBar != null && bufferColor == default)
                {
                    trailingBufferBar.color = defaultBuffer;
                }
            }

            public void UpdateScaling(float maxStatValue)
            {
                float targetWidth = baseWidth + (maxStatValue * scaleFactor);
                if (container != null)
                {
                    container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
                }
            }

            public void UpdateValue(float current, float max)
            {
                float newTargetFill = Mathf.Clamp01(max > 0f ? current / max : 0f);

                if (newTargetFill < targetFill)
                {
                    // Value decreased: Start hold timer for trailing buffer
                    bufferStartFill = trailingFill;
                    targetFill = newTargetFill;
                    holdTimer = HOLD_DURATION;
                    animateTimer = 0f;
                    isHolding = true;
                    isAnimating = false;
                }
                else if (newTargetFill > targetFill)
                {
                    // Value increased (healing/regen): Instantly bump buffer up to new target
                    targetFill = newTargetFill;
                    trailingFill = Mathf.Max(trailingFill, targetFill);
                    bufferStartFill = trailingFill;
                    isHolding = false;
                    isAnimating = false;
                }

                // Immediate primary bar update
                currentFill = newTargetFill;
                if (primaryBar != null)
                {
                    primaryBar.fillAmount = currentFill;
                }
            }

            public void TickAnimation(float deltaTime)
            {
                if (isHolding)
                {
                    holdTimer -= deltaTime;
                    if (holdTimer <= 0f)
                    {
                        isHolding = false;
                        isAnimating = true;
                        animateTimer = 0f;
                    }
                }

                if (isAnimating)
                {
                    animateTimer += deltaTime;
                    float progress = Mathf.Clamp01(animateTimer / SLIDE_DURATION);
                    // Smooth step interpolation for trailing buffer
                    float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                    trailingFill = Mathf.Lerp(bufferStartFill, targetFill, smoothProgress);

                    if (progress >= 1f)
                    {
                        trailingFill = targetFill;
                        isAnimating = false;
                    }
                }

                if (trailingBufferBar != null)
                {
                    trailingBufferBar.fillAmount = trailingFill;
                }
            }
        }

        [Header("Stat Bars")]
        [SerializeField] private StatBar hpBar = new StatBar
        {
            baseWidth = 200f,
            scaleFactor = 0.25f
        };

        [SerializeField] private StatBar fpBar = new StatBar
        {
            baseWidth = 150f,
            scaleFactor = 0.35f
        };

        [SerializeField] private StatBar staminaBar = new StatBar
        {
            baseWidth = 180f,
            scaleFactor = 0.80f
        };

        [Header("Equipment HUD")]
        [SerializeField] private RectTransform equipmentHudContainer;
        [SerializeField] private EquipmentSlotHud topSlot = new EquipmentSlotHud();
        [SerializeField] private EquipmentSlotHud leftSlot = new EquipmentSlotHud();
        [SerializeField] private EquipmentSlotHud rightSlot = new EquipmentSlotHud();
        [SerializeField] private EquipmentSlotHud bottomSlot = new EquipmentSlotHud();

        [Header("HUD Visibility State Machine")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float autoHideDelay = 3.0f;

        private IPlayerHudPresenter Presenter { get; set; }
        private float _inactivityTimer = 0f;
        private float _targetAlpha = 1f;

        // Dark Atmospheric Color Token Constants
        private static readonly Color HpPrimaryColor = HexToColor("#801414"); // Dark Crimson Red
        private static readonly Color HpBufferColor = HexToColor("#B85C00");  // Dark Burnt Amber
        private static readonly Color FpPrimaryColor = HexToColor("#134488"); // Dark Royal Blue
        private static readonly Color FpBufferColor = HexToColor("#3B72A8");  // Dark Muted Blue
        private static readonly Color StaminaPrimaryColor = HexToColor("#1E5E3A"); // Dark Forest Green
        private static readonly Color StaminaBufferColor = HexToColor("#4D9E6E"); // Dark Muted Sage Green

        protected override void Awake()
        {
            base.Awake();
            hpBar.primaryColor = HpPrimaryColor;
            hpBar.bufferColor = HpBufferColor;
            fpBar.primaryColor = FpPrimaryColor;
            fpBar.bufferColor = FpBufferColor;
            staminaBar.primaryColor = StaminaPrimaryColor;
            staminaBar.bufferColor = StaminaBufferColor;

            hpBar.Initialize(HpPrimaryColor, HpBufferColor);
            fpBar.Initialize(FpPrimaryColor, FpBufferColor);
            staminaBar.Initialize(StaminaPrimaryColor, StaminaBufferColor);
        }

        public void AssignPresenter(IPlayerHudPresenter presenter)
        {
            Presenter = presenter;
        }

        public void UpdateStats(HealthStats stats)
        {
            // Dynamic bar scaling
            hpBar.UpdateScaling(stats.MaxHealth);
            fpBar.UpdateScaling(stats.MaxFocus);
            staminaBar.UpdateScaling(stats.MaxStamina);

            // Update fill values
            hpBar.UpdateValue(stats.CurrentHealth, stats.MaxHealth);
            fpBar.UpdateValue(stats.CurrentFocus, stats.MaxFocus);
            staminaBar.UpdateValue(stats.CurrentStamina, stats.MaxStamina);

            _targetAlpha = 1f;
        }

        public void UpdateEquipment(
            Sprite rightIcon,
            Sprite leftIcon,
            Sprite quickItemIcon,
            int quickItemQuantity,
            bool isTwoHanded)
        {
            // Right Hand Armament (Main Weapon)
            if (rightIcon != null)
            {
                rightSlot.SetItem(rightIcon);
            }
            else
            {
                rightSlot.SetEmpty();
            }

            // Left Hand Armament (Shield / Offhand)
            if (leftIcon != null)
            {
                leftSlot.SetItem(leftIcon, 0, isTwoHanded);
            }
            else
            {
                leftSlot.SetEmpty(isTwoHanded);
            }

            // Top Slot (Reserved for Magic / Spells)
            topSlot.SetEmpty();

            // Bottom Slot (Quick Item / Consumable)
            if (quickItemIcon != null)
            {
                bottomSlot.SetItem(quickItemIcon, quickItemQuantity);
            }
            else
            {
                bottomSlot.SetEmpty();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Tick buffer animations
            hpBar.TickAnimation(dt);
            fpBar.TickAnimation(dt);
            staminaBar.TickAnimation(dt);

            // HUD is active all the time (100% opacity)
            if (canvasGroup != null && !Mathf.Approximately(canvasGroup.alpha, 1f))
            {
                canvasGroup.alpha = 1f;
            }
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
