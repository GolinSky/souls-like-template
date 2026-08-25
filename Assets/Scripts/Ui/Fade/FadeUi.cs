using System;
using DG.Tweening;
using MPUIKIT;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.Fade
{
    public class FadeUi : BaseUi
    {
        [SerializeField] private MPImage fadeImage;

        private Tween _fadeTween;

        public void FadeIn(float duration, Action onComplete = null)
        {
            _fadeTween?.Kill();
            Show();
            SetFadeAlpha(0f);

            _fadeTween = fadeImage.DOFade(1f, duration).OnComplete(() =>
            {
                _fadeTween = null;
                onComplete?.Invoke();
            });
        }

        public void FadeOut(float duration, Action onComplete = null)
        {
            _fadeTween?.Kill();
            Show();
            SetFadeAlpha(1f);

            _fadeTween = fadeImage.DOFade(0f, duration).OnComplete(() =>
            {
                _fadeTween = null;
                Hide();
                onComplete?.Invoke();
            });
        }

        public void FadeInOut(float duration, float pauseDuration, Action onComplete = null)
        {
            _fadeTween?.Kill();
            Show();
            SetFadeAlpha(0f);

            _fadeTween = DOTween.Sequence()
                .Append(fadeImage.DOFade(1f, duration))
                .AppendInterval(pauseDuration)
                .Append(fadeImage.DOFade(0f, duration))
                .OnComplete(() =>
                {
                    _fadeTween = null;
                    Hide();
                    onComplete?.Invoke();
                });
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
        }

        private void SetFadeAlpha(float alpha)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
