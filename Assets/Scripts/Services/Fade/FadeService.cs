using System;
using SoulsLike.Ui.Fade;

namespace SoulsLike.Services.Fade
{
    public interface IFadeService
    {
        void FadeIn(float duration, Action onComplete = null);
        void FadeOut(float duration, Action onComplete = null);
        void FadeInOut(float duration, float pauseDuration, Action onComplete = null);
    }

    public class FadeService : IFadeService
    {
        private readonly IUiService _uiService;
        private FadeUi _fadeUi;

        public FadeService(IUiService uiService)
        {
            _uiService = uiService;
        }

        public void FadeIn(float duration, Action onComplete = null)
        {
            GetFadeUi().FadeIn(duration, onComplete);
        }

        public void FadeOut(float duration, Action onComplete = null)
        {
            GetFadeUi().FadeOut(duration, onComplete);
        }

        public void FadeInOut(float duration, float pauseDuration, Action onComplete = null)
        {
            GetFadeUi().FadeInOut(duration, pauseDuration, onComplete);
        }

        private FadeUi GetFadeUi()
        {
            if (_fadeUi == null)
            {
                _fadeUi = _uiService.CreateUi<FadeUi>();
                _uiService.MarkUiAsOverlay(_fadeUi);
            }

            return _fadeUi;
        }
    }
}
