using SoulsLike.Entities.Character.Components;
using UnityEngine.UI;

namespace SoulsLike.Services
{
    public interface IPreviewRenderService
    {
        void SetupPreview(RawImage targetImage, AnimatorComponent animatorComponent);

        /// <summary>
        /// Clears the current preview.
        /// </summary>
        void ClearPreview();
    }
}
