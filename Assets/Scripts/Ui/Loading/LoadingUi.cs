using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.Loading
{
    public sealed class LoadingUi : BaseUi
    {
        [SerializeField] private RectTransform loadingIcon;
        [SerializeField] private float rotationSpeed = 180f;

        private ILoadingPresenter Presenter { get; set; }

        public float Progress => Presenter.Progress;

        public void AssignPresenter(ILoadingPresenter presenter)
        {
            Presenter = presenter;
        }

        private void Update()
        {
            loadingIcon.Rotate(0f, 0f, -rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}
