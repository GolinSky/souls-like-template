using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.EnemyHealth
{
    public sealed class EnemyHealthBarUi : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Image trailingBufferBar;

        private const float TRAILING_DELAY = 1f;
        private const float FILL_LERP_SPEED = 10f;

        private readonly Queue<PendingFill> _pendingFills = new();
        private float _elapsedTime;
        private float _currentFill = 1f;
        private float _trailingFill = 1f;
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

        public RectTransform RectTransform => (RectTransform)transform;

        public void SetValue(float currentHealth, float maxHealth)
        {
            _targetFill = Mathf.Clamp01(maxHealth > 0f ? currentHealth / maxHealth : 0f);
        }

        public void ResetValue()
        {
            _pendingFills.Clear();
            _elapsedTime = 0f;
            _currentFill = 1f;
            _trailingFill = 1f;
            _targetFill = 1f;
            fillImage.fillAmount = _currentFill;
            trailingBufferBar.fillAmount = _trailingFill;
        }

        public void SetVisible(bool isVisible)
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            float previousFill = _currentFill;
            _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.deltaTime * FILL_LERP_SPEED);
            if (Mathf.Approximately(_currentFill, _targetFill))
            {
                _currentFill = _targetFill;
            }

            if (_currentFill != previousFill)
            {
                _pendingFills.Enqueue(new PendingFill(_currentFill, _elapsedTime + TRAILING_DELAY));
            }

            while (_pendingFills.Count > 0 && _pendingFills.Peek().ApplyAt <= _elapsedTime)
            {
                _trailingFill = _pendingFills.Dequeue().Value;
            }

            fillImage.fillAmount = _currentFill;
            trailingBufferBar.fillAmount = _trailingFill;
        }
    }
}
