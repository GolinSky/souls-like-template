using System;
using MultiPlayerTemplate.Ui.Base;
using UI.Base;
using UnityEngine;
using VContainer.Unity;

namespace MultiPlayerTemplate.Ui.MainMenu
{
    public class LobbyNavigationUi: BaseUi, IStartable, IDisposable
    {
        [SerializeField] private Transform uiWindowParent;
        
        [SerializeField] private CustomButtonToggle deploymentToggle;
        [SerializeField] private CustomButtonToggle skinToggle;
        private ILobbyNavigationPresenter Presenter { get; set; }


        void IStartable.Start()
        {
           // Corrected AddListener calls to pass uiWindowParent using lambda expressions
           skinToggle.onValueChanged.AddListener((show) => Presenter.RequestSkinSelection(show, uiWindowParent));
           deploymentToggle.onValueChanged.AddListener((show) => Presenter.RequestDeployment(show, uiWindowParent));
           deploymentToggle.isOn = true;
        }

        public void Dispose()
        {
            // Remove listeners to prevent memory leaks
            skinToggle.onValueChanged.RemoveAllListeners();
            deploymentToggle.onValueChanged.RemoveAllListeners();
        }


        public void AssignPresenter(ILobbyNavigationPresenter presenter)
        {
            Presenter = presenter;
        }
    }
}