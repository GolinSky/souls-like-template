using System.Collections.Generic;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.EnemyHealth
{
    public sealed class EnemyHealthUi : BaseUi
    {
        [SerializeField] private RectTransform barContainer;
        [SerializeField] private EnemyHealthBarUi barPrefab;
        [SerializeField] private int initialPoolSize = 32;
        [SerializeField] private Vector3 worldOffset = new(0f, 2f, 0f);

        private readonly Stack<EnemyHealthBarUi> _availableBars = new();

        protected override void Awake()
        {
            base.Awake();
            barPrefab.SetVisible(false);
            //todo: add pool service
            for (int index = 0; index < initialPoolSize; index++)
            {
                ReleaseBar(CreateBar());
            }
        }

        public EnemyHealthBarUi AcquireBar()
        {
            EnemyHealthBarUi bar = _availableBars.Count > 0
                ? _availableBars.Pop()
                : CreateBar();
            bar.SetVisible(true);
            return bar;
        }

        public void ReleaseBar(EnemyHealthBarUi bar)
        {
            bar.SetVisible(false);
            bar.ResetValue();
            _availableBars.Push(bar);
        }

        public bool TrySetBarPosition(
            EnemyHealthBarUi bar,
            Vector3 enemyPosition,
            Camera targetCamera)
        {
            Vector3 screenPosition = targetCamera.WorldToScreenPoint(enemyPosition + worldOffset);
            if (screenPosition.z <= 0f
                || !targetCamera.pixelRect.Contains(new Vector2(screenPosition.x, screenPosition.y)))
            {
                return false;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    barContainer,
                    screenPosition,
                    null,
                    out Vector2 localPosition))
            {
                return false;
            }

            bar.RectTransform.anchoredPosition = localPosition;
            return true;
        }

        private EnemyHealthBarUi CreateBar()
        {
            return Instantiate(barPrefab, barContainer);
        }
    }
}
