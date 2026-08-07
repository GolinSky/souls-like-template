using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services
{
    //TODO: MOVE IPauseMenuPresenter OUT OF HERE - CREATE UI CONTROLLER FOR THAT 
    public class CoreGameOrchestrator: IInitializable, IStartable, IDisposable, IPauseMenuPresenter, ITickable, IGameStateNotifier
    {
        private readonly IUiService _uiService;
        private readonly IInputService _inputService;

        // private PauseMenuUi _pauseMenuUi;
        
        public GameState CurrentGameState { get; private set; }
        
        private readonly List<IGameStateObserver> _observers = new();

        public CoreGameOrchestrator(IUiService uiService, IInputService inputService)
        {
            _uiService = uiService;
            _inputService = inputService;
        }

        public void Initialize()
        {
            SetGameState(GameState.Initialized);
        }
        
        public void Dispose()
        {
            SetGameState(GameState.Ended);
        }

        public void Start()
        {
            // _pauseMenuUi = _uiService.CreateUi<PauseMenuUi>();
            // _pauseMenuUi.Initialize(this);
            
            SetGameState(GameState.Idle);
        }

     
        public void SetGameState(GameState newState)
        {
            if (CurrentGameState == newState) return;

            Cursor.lockState = newState != GameState.Idle
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            CurrentGameState = newState;
            NotifyObservers();
        }

        public void ResumeGame()
        {
            // _pauseMenuUi.Hide();
            SetGameState(GameState.Idle);
        }
        
        private void PauseGame()
        {
            // _pauseMenuUi.Show();
            SetGameState(GameState.Paused);
        }

        public void OpenOptions()
        {
            // TODO: Implement options
        }

        public void QuitGame()
        {
            
        }

        public void Tick()
        {
            if (_inputService.CharacterActions.Pause.WasPressedThisFrame())
            {
                if (CurrentGameState == GameState.Idle)
                {
                    PauseGame();
                }
                else if (CurrentGameState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }

        public void RegisterObserver(IGameStateObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void UnregisterObserver(IGameStateObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        public void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnGameStateChanged(CurrentGameState);
            }
        }
    }
}