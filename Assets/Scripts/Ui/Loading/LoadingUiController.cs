using System;
using SoulsLike.Services;
using SoulsLike.Services.Scenes;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.Loading
{
    public sealed class LoadingUiController : UiController, IInitializable, IDisposable, ILoadingPresenter
    {
        private readonly ISceneService _sceneService;
        private LoadingUi _loadingUi;

        public float Progress { get; private set; }

        public LoadingUiController(IUiService uiService, ISceneService sceneService)
            : base(uiService)
        {
            _sceneService = sceneService;
        }

        public void Initialize()
        {
            Progress = 0f;
            _sceneService.OnProgressUpdated += HandleProgressUpdated;

            _loadingUi = CreateUi<LoadingUi>();
            _loadingUi.AssignPresenter(this);
            _loadingUi.Show();
        }

        public void Dispose()
        {
            _sceneService.OnProgressUpdated -= HandleProgressUpdated;
        }

        private void HandleProgressUpdated(float progress)
        {
            Progress = Mathf.Clamp01(progress);
        }
    }
}
