using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.Character;
using SoulsLike.Services.Fade;
using SoulsLike.Services.Spawn;
using SoulsLike.Services.Travel.Data;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services
{
    public interface ICoreGameOrchestrator
    {
        GameState CurrentGameState { get; }
        void ResumeGame();
        void PauseGame();
        UniTask OnGraceSit(GraceId graceId);
        UniTask RespawnAtLastGrace();
        void QuitGame();
    }

    public class CoreGameOrchestrator: IInitializable, IStartable, IGameStateNotifier, ICoreGameOrchestrator
    {
        private const float RESPAWN_FADE_DURATION = 0.5f;

        private readonly IGameOrchestrator _gameOrchestrator;
        private readonly CharacterFactory _characterFactory;
        private readonly CharacterSpawnService _characterSpawnService;
        private readonly IFadeService _fadeService;
        private Character _character;
        private bool _startsOnGrace;
        private bool _isRespawning;
        public GameState CurrentGameState { get; private set; }
        
        private readonly List<IGameStateObserver> _observers = new();

        public CoreGameOrchestrator(
            IGameOrchestrator gameOrchestrator,
            CharacterFactory characterFactory,
            CharacterSpawnService characterSpawnService,
            IFadeService fadeService)
        {
            _gameOrchestrator = gameOrchestrator;
            _characterFactory = characterFactory;
            _characterSpawnService = characterSpawnService;
            _fadeService = fadeService;
        }
        
        public void Initialize()
        {
            SetGameState(GameState.Initialized);
            if (_characterSpawnService.TryConsumeSpawn(out Vector3 spawnPosition, out _startsOnGrace))
            {
                _character = _characterFactory.CreateCharacter(spawnPosition);
                return;
            }

            _character = _characterFactory.CreateCharacter();
        }
        
        public void Start()
        {
            SetGameState(_startsOnGrace ? GameState.OnGraceSit : GameState.Idle);
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

        public UniTask OnGraceSit(GraceId graceId)
        {
            _characterSpawnService.SaveLastGrace(graceId);
            SetGameState(GameState.OnGraceSit);
            return UniTask.CompletedTask;
        }

        public async UniTask RespawnAtLastGrace()
        {
            if (_isRespawning)
            {
                return;
            }

            _isRespawning = true;
            try
            {
                var fadeInCompleted = new UniTaskCompletionSource<bool>();
                _fadeService.FadeIn(RESPAWN_FADE_DURATION, () => fadeInCompleted.TrySetResult(true));
                await fadeInCompleted.Task;

                _character.CompleteDeathAnimation();
                SetGameState(GameState.Ended);
                Vector3 lastGracePosition = _characterSpawnService.GetLastGracePosition();
                _character.SetPosition(lastGracePosition);
                _characterSpawnService.SaveCurrentPosition(lastGracePosition);

                await UniTask.NextFrame(PlayerLoopTiming.LastUpdate);

                var fadeOutCompleted = new UniTaskCompletionSource<bool>();
                _fadeService.FadeOut(RESPAWN_FADE_DURATION, () => fadeOutCompleted.TrySetResult(true));
                await fadeOutCompleted.Task;

                SetGameState(GameState.Idle);
            }
            finally
            {
                _isRespawning = false;
            }
        }

        public void QuitGame()
        {
            _characterSpawnService.SaveCurrentPosition(_character.transform.position);
            _gameOrchestrator.LoadMenu().Forget();
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
