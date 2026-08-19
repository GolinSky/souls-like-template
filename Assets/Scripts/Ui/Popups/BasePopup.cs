using System;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace SoulsLike.Ui.Popups
{
    public abstract class BasePopup : BaseUi, IInitializable, IDisposable
    {
        [SerializeField] private TextMeshProUGUI tittleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
       
        private Action<bool> _callback;

        public void Initialize()
        {
            yesButton.onClick.AddListener(HandleYesButtonClick);
            noButton.onClick.AddListener(HandleNoButtonClick);
            OnInitialize();
        }
        
        public void Dispose()
        {
            yesButton.onClick.RemoveListener(HandleYesButtonClick);
            noButton.onClick.RemoveListener(HandleNoButtonClick);
            OnDispose();
        }
        
        protected virtual void HandleYesButtonClick()
        {
            _callback?.Invoke(true);
            Hide();
        }
        
        protected virtual void HandleNoButtonClick()
        {
            _callback?.Invoke(false);
            Hide();
        }

        public virtual void Show(Action<bool> callback, string tittle = null, string description = null)
        {
            _callback = callback;
            if (tittle != null)
            {
                tittleText.text = tittle;
            }

            if (description != null)
            {
                descriptionText.text = description;
            }
            
            Show();
        }
        
        protected virtual void OnInitialize() { }
        protected virtual void OnDispose() { }
    }
}
