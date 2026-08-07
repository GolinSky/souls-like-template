using MultiPlayerTemplate.Extensions;
using UnityEngine;

namespace MultiPlayerTemplate.Ui.Base
{
    public interface IBaseUi
    {
        void Show();
        void Hide();
        bool IsHidden { get; }
    }
    
    public abstract class BaseUi : MonoBehaviour, IBaseUi
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected bool autoHide = true;

        public bool IsHidden { get; private set;}

        public bool IsActive { get; private set; }
        
        public Transform Transform => transform;
        
        private void Awake()
        {
            if (autoHide)
            {
                InternalHide();
            }
        }

        public virtual void Show()
        {
            SetCanvasState(true);
            IsHidden = false;
        }

        public virtual void Hide()
        {
            InternalHide();
        }

        private void InternalHide()
        {
            SetCanvasState(false);
            IsHidden = true;
        }

        private void SetCanvasState(bool state)
        {
            canvasGroup.SetActive(state);
            IsActive = state;
        }
    }
}