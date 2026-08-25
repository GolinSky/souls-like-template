using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.Character;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services
{
    public interface ICoreGameOrchestrator
    {
        GameState CurrentGameState { get; }
        void ResumeGame();
        void PauseGame();
        UniTask OnGraceSit();
        void QuitGame();
    }

    public class CoreGameOrchestrator: IInitializable, IStartable, IDisposable, IGameStateNotifier, ICoreGameOrchestrator
    {
        private readonly IGameOrchestrator _gameOrchestrator;
        private readonly CharacterFactory _characterFactory;
        public GameState CurrentGameState { get; private set; }
        
        private readonly List<IGameStateObserver> _observers = new();

        public CoreGameOrchestrator(
            IGameOrchestrator gameOrchestrator,
            CharacterFactory characterFactory)
        {
            _gameOrchestrator = gameOrchestrator;
            _characterFactory = characterFactory;
        }
        
        public void Initialize()
        {
            SetGameState(GameState.Initialized);
            _characterFactory.CreateCharacter();
        }
        
        public void Dispose()
        {
            SetGameState(GameState.Ended);
        }

        public void Start()
        {
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
            SetGameState(GameState.Idle);
        }
        
        public void PauseGame()
        {
            SetGameState(GameState.Paused);
        }

        public UniTask OnGraceSit()
        {
            SetGameState(GameState.OnGraceSit);
            return UniTask.CompletedTask;
        }

        public void QuitGame()
        {
            _gameOrchestrator.LoadLevel(SceneType.MainMenu);
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
            foreach (IGameStateObserver observer in _observers.ToArray())
            {
                observer.OnGameStateChanged(CurrentGameState);
            }
        }
    }
}
