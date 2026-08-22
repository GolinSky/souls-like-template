using System.Collections.Generic;
using MPUIKIT;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Ui.Base;
using UnityEngine;

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

            private const float TRAILING_DELAY = 1f;
            private const float FILL_LERP_SPEED = 10f;

            private readonly Queue<PendingFill> _pendingFills = new();
            private float _elapsedTime;
            private float _targetFill = 1f;

            private readonly struct PendingFill
            {
                public PendingFill(float value, float applyAt)
                {
                    Value = value;
                    ApplyAt = applyAt;
                }

                public float Value { get; }
                public float ApplyAt { get; }
            }

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
                _targetFill = Mathf.Clamp01(max > 0f ? current / max : 0f);
            }

            public void TickAnimation(float deltaTime)
            {
                _elapsedTime += deltaTime;

                float previousFill = currentFill;
                currentFill = Mathf.Lerp(currentFill, _targetFill, deltaTime * FILL_LERP_SPEED);
                if (Mathf.Approximately(currentFill, _targetFill))
                {
                    currentFill = _targetFill;
                }

                if (currentFill != previousFill)
                {
                    // Replay the primary bar's smoothed fill one second later.
                    _pendingFills.Enqueue(new PendingFill(currentFill, _elapsedTime + TRAILING_DELAY));
                }

                while (_pendingFills.Count > 0 && _pendingFills.Peek().ApplyAt <= _elapsedTime)
                {
                    trailingFill = _pendingFills.Dequeue().Value;
                }

                if (primaryBar != null)
                {
                    primaryBar.fillAmount = currentFill;
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

        [Header("Acquisition Panel")]
        [SerializeField] private ItemAcquisitionPanel acquisitionPanel;

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

        public void ShowAcquisition(string itemName, Sprite icon, int quantity)
        {
            acquisitionPanel.ShowAcquisition(itemName, icon, quantity);
        }

        public void ShowInteractionFailure(string message)
        {
            acquisitionPanel.ShowMessage(message);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Tick stat bar animations
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
