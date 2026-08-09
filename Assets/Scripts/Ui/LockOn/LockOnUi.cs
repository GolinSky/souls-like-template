using MPUIKIT;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.LockOn
{
    public class LockOnUi : BaseUi
    {
        [SerializeField] private MPImage reticle;

        public void SetTargetPosition(Transform targetTransform, Camera targetCamera)
        {
            if (targetTransform == null)
            {
                throw new System.ArgumentNullException(nameof(targetTransform));
            }

            if (targetCamera == null)
            {
                throw new System.ArgumentNullException(nameof(targetCamera));
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                throw new System.InvalidOperationException("LockOnUi must be placed below a Canvas.");
            }

            RectTransform parentRect = Transform.parent as RectTransform;
            if (parentRect == null)
            {
                throw new System.InvalidOperationException("LockOnUi must have a RectTransform parent.");
            }

            Vector3 screenPosition = targetCamera.WorldToScreenPoint(targetTransform.position);
            if (!IsVisibleOnScreen(screenPosition))
            {
                Hide();
                return;
            }

            Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvasCamera == null)
            {
                throw new System.InvalidOperationException("The LockOnUi canvas requires a world camera.");
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPosition,
                    canvasCamera,
                    out Vector2 localPosition))
            {
                Hide();
                return;
            }

            RectTransform.anchoredPosition = localPosition;
            Show();
        }

        private bool IsVisibleOnScreen(Vector3 screenPosition)
        {
            return screenPosition.z > 0f
                && screenPosition.x >= 0f
                && screenPosition.x <= Screen.width
                && screenPosition.y >= 0f
                && screenPosition.y <= Screen.height;
        }

        private RectTransform RectTransform => (RectTransform)transform;
    }
}
