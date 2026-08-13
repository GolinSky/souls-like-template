using UnityEngine;
using UnityEngine.InputSystem;

namespace SoulsLike.Ui.FpsCounter
{
    /// <summary>
    /// Lightweight OnGUI FPS Counter component positioned in the top-right corner.
    /// Displays current FPS, frame time (ms), and performance stats.
    /// Safe against invalid serialized key values and managed via VContainer ProjectScope.
    /// </summary>
    public class OnGuiFpsCounter : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showFpsCounter = true;
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool showFrameTime = true;
        [SerializeField] private bool showMinMax = false;

        [Header("Layout & Styling (Top-Right Corner)")]
        [SerializeField] private float width = 160f;
        [SerializeField] private float height = 45f;
        [SerializeField] private float paddingRight = 12f;
        [SerializeField] private float paddingTop = 12f;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private bool showBackground = true;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.65f);

        [Header("Performance Color Thresholds")]
        [SerializeField] private bool useColorCoding = true;
        [SerializeField] private float targetFps = 60f;
        [SerializeField] private float lowFpsThreshold = 30f;
        [SerializeField] private Color goodColor = new Color(0.2f, 0.9f, 0.2f, 1f); // Green
        [SerializeField] private Color warningColor = new Color(0.95f, 0.8f, 0.2f, 1f); // Yellow
        [SerializeField] private Color criticalColor = new Color(0.95f, 0.25f, 0.25f, 1f); // Red

        [Header("Toggle Controls (Input System)")]
        [SerializeField] private Key toggleKey = Key.F1;

        // FPS Calculation Fields
        private float _accumulatedDeltaTime = 0f;
        private int _frameCount = 0;
        private float _timeRemaining;

        private float _currentFps;
        private float _currentFrameTimeMs;
        private float _minFps = float.MaxValue;
        private float _maxFps = 0f;

        private GUIStyle _boxStyle;
        private GUIStyle _textStyle;
        private Texture2D _backgroundTexture;

        private void Awake()
        {
            _timeRemaining = updateInterval;
        }

        private void Update()
        {
            // Toggle visibility via safely checked key press
            if (IsToggleKeyPressed())
            {
                showFpsCounter = !showFpsCounter;
            }

            if (!showFpsCounter) return;

            // Accumulate delta time and frame counts using unscaled delta time
            float unscaledDelta = Time.unscaledDeltaTime;
            _accumulatedDeltaTime += unscaledDelta;
            _frameCount++;
            _timeRemaining -= unscaledDelta;

            if (_timeRemaining <= 0f)
            {
                _currentFps = _frameCount / Mathf.Max(_accumulatedDeltaTime, 0.0001f);
                _currentFrameTimeMs = (_accumulatedDeltaTime / Mathf.Max(_frameCount, 1)) * 1000f;

                if (_currentFps < _minFps) _minFps = _currentFps;
                if (_currentFps > _maxFps) _maxFps = _currentFps;

                _accumulatedDeltaTime = 0f;
                _frameCount = 0;
                _timeRemaining = updateInterval;
            }
        }

        private bool IsToggleKeyPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            if (toggleKey != Key.None && System.Enum.IsDefined(typeof(Key), toggleKey))
            {
                try
                {
                    var control = keyboard[toggleKey];
                    if (control != null && control.wasPressedThisFrame)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Fallback if serialized enum index is out of bounds
                    return keyboard.f1Key.wasPressedThisFrame;
                }
            }

            return false;
        }

        private void OnGUI()
        {
            if (!showFpsCounter) return;

            InitializeStylesIfNeeded();

            // Calculate rect dynamically anchored in the TOP-RIGHT corner
            float xPos = Screen.width - width - paddingRight;
            float yPos = paddingTop;
            float actualHeight = showMinMax ? height + 25f : height;

            Rect drawRect = new Rect(xPos, yPos, width, actualHeight);

            // Draw Background Box if enabled
            if (showBackground && _boxStyle != null)
            {
                GUI.Box(drawRect, GUIContent.none, _boxStyle);
            }

            // Determine text color based on FPS thresholds
            _textStyle.normal.textColor = GetFpsColor(_currentFps);

            // Format label string
            string text = FormatFpsString();

            // Draw Label centered inside the box
            Rect labelRect = new Rect(drawRect.x + 5f, drawRect.y + 5f, drawRect.width - 10f, drawRect.height - 10f);
            GUI.Label(labelRect, text, _textStyle);
        }

        private string FormatFpsString()
        {
            if (showMinMax)
            {
                return string.Format(
                    "FPS: {0:F1}\nMS: {1:F1} ms\nMin: {2:F0} | Max: {3:F0}",
                    _currentFps, _currentFrameTimeMs, _minFps, _maxFps
                );
            }

            if (showFrameTime)
            {
                return string.Format("FPS: {0:F1} ({1:F1} ms)", _currentFps, _currentFrameTimeMs);
            }

            return string.Format("FPS: {0:F1}", _currentFps);
        }

        private Color GetFpsColor(float fps)
        {
            if (!useColorCoding) return Color.white;

            if (fps >= targetFps)
            {
                return goodColor;
            }
            if (fps >= lowFpsThreshold)
            {
                return warningColor;
            }
            return criticalColor;
        }

        private void InitializeStylesIfNeeded()
        {
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
            }
            else
            {
                _textStyle.fontSize = fontSize;
            }

            if (showBackground && _boxStyle == null)
            {
                _backgroundTexture = new Texture2D(1, 1);
                _backgroundTexture.SetPixel(0, 0, backgroundColor);
                _backgroundTexture.Apply();

                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = _backgroundTexture }
                };
            }
        }

        private void OnDestroy()
        {
            if (_backgroundTexture != null)
            {
                Destroy(_backgroundTexture);
            }
        }

        /// <summary>
        /// Resets Min/Max FPS stats tracking.
        /// </summary>
        public void ResetMinMax()
        {
            _minFps = float.MaxValue;
            _maxFps = 0f;
        }

        /// <summary>
        /// Show or hide the FPS counter programmatically.
        /// </summary>
        public void SetVisible(bool visible)
        {
            showFpsCounter = visible;
        }
    }
}
