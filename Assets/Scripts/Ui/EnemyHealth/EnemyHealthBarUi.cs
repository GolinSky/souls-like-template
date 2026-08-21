using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.EnemyHealth
{
    public sealed class EnemyHealthBarUi : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        public RectTransform RectTransform => (RectTransform)transform;

        public void SetValue(float currentHealth, float maxHealth)
        {
            fillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }

        public void SetVisible(bool isVisible)
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}
