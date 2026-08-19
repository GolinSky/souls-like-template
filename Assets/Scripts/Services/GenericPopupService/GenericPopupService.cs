using System;
using SoulsLike.Ui.Popups;

namespace SoulsLike.Services.GenericPopupService
{
    public interface IGenericPopupService
    {
        void ShowAcceptPopup(string title, string message, Action<bool> callback);
        void ShowAlertPopup(string title, string message, Action<bool> callback);
    }
    
    public class GenericPopupService : IGenericPopupService
    {
        private readonly IUiService _uiService;
        private AcceptPopup _acceptPopup;
        private AlertPopup _alertPopup;
        
        public GenericPopupService(IUiService uiService)
        {
            _uiService = uiService;
        }
        
        public void ShowAcceptPopup(string title, string message, Action<bool> callback)
        {
            if (_acceptPopup == null)
            {
                _acceptPopup = _uiService.CreateUi<AcceptPopup>();
                _uiService.MarkUiAsOverlay(_acceptPopup);
            }

            _acceptPopup.Show(callback, title, message);
        }

        public void ShowAlertPopup(string title, string message, Action<bool> callback)
        {
            if (_alertPopup == null)
            {
                _alertPopup = _uiService.CreateUi<AlertPopup>();
                _uiService.MarkUiAsOverlay(_alertPopup);
            }

            _alertPopup.Show(callback, title, message);
        }
    }
}
