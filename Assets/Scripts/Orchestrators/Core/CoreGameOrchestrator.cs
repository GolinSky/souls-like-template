using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Enemy;
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
        void OpenOptions();
        void QuitGame();
    }

    public class CoreGameOrchestrator: IInitializable, IStartable, IDisposable, IGameStateNotifier, ICoreGameOrchestrator
    {
        private readonly IGameOrchestrator _gameOrchestrator;
        private readonly CharacterFactory _characterFactory;
        private readonly EnemyFactory _enemyFactory;
        public GameState CurrentGameState { get; private set; }
        
        private readonly List<IGameStateObserver> _observers = new();

        public CoreGameOrchestrator(
            IGameOrchestrator gameOrchestrator,
            CharacterFactory characterFactory,
            EnemyFactory enemyFactory)
        {
            _gameOrchestrator = gameOrchestrator;
            _characterFactory = characterFactory;
            _enemyFactory = enemyFactory;
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
            UnityEngine.Object.FindFirstObjectByType<EnemyEncounterSpawner>().Spawn(_enemyFactory);
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

        public void OpenOptions()
        {
            // TODO: Implement options
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
            foreach (var observer in _observers)
            {
                observer.OnGameStateChanged(CurrentGameState);
            }
        }
    }
}
