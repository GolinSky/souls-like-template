using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.LockOn
{
    public class LockOnUi : BaseUi
    {
        [SerializeField] private RectTransform reticleRectTransform;

        public bool TrySetTargetPosition(Vector3 targetPosition, Camera targetCamera)
        {
            Vector3 screenPosition = targetCamera.WorldToScreenPoint(targetPosition);
            if (!IsVisible(targetCamera, screenPosition))
            {
                return false;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    RectTransform,
                    screenPosition,
                    null,
                    out Vector2 localPosition))
            {
                return false;
            }

            reticleRectTransform.anchoredPosition = localPosition;
            return true;
        }

        private static bool IsVisible(Camera targetCamera, Vector3 screenPosition)
        {
            return screenPosition.z > 0f
                && targetCamera.pixelRect.Contains(new Vector2(screenPosition.x, screenPosition.y));
        }

        private RectTransform RectTransform => (RectTransform)transform;
    }
}
