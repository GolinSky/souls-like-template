using System;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Interactions;
using SoulsLike.Items;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Targeting;
using VContainer.Unity;

namespace SoulsLike.Services.PlayerSession
{
    /// <summary>Owns local input, targeting, interaction, death, and rest binding for one actor.</summary>
    public sealed class PlayerSessionCoordinator : IPlayerSession, IInitializable, IGameStateObserver, IDisposable
    {
        private readonly Character _character;
        private readonly HealthComponent _healthComponent;
        private readonly CharacterActionCoordinator _actionCoordinator;
        private readonly CharacterActorTick _actorTick;
        private readonly IInputService _inputService;
        private readonly ITargetingService _targetingService;
        private readonly ICameraService _cameraService;
        private readonly IGameStateNotifier _gameStateNotifier;
        private readonly InteractionController _interactionController;
        private readonly ICoreGameOrchestrator _coreGameOrchestrator;

        private GameState _currentGameState;

        public PlayerSessionCoordinator(
            Character character,
            HealthComponent healthComponent,
            CharacterActionCoordinator actionCoordinator,
            CharacterActorTick actorTick,
            IInputService inputService,
            ITargetingService targetingService,
            ICameraService cameraService,
            IGameStateNotifier gameStateNotifier,
            InteractionController interactionController,
            ICoreGameOrchestrator coreGameOrchestrator)
        {
            _character = character;
            _healthComponent = healthComponent;
            _actionCoordinator = actionCoordinator;
            _actorTick = actorTick;
            _inputService = inputService;
            _targetingService = targetingService;
            _cameraService = cameraService;
            _gameStateNotifier = gameStateNotifier;
            _interactionController = interactionController;
            _coreGameOrchestrator = coreGameOrchestrator;
        }

        public PlayerSessionSnapshot Snapshot => new PlayerSessionSnapshot(
            _character.CameraTarget,
            _character.IsGrounded,
            _character.VerticalVelocity,
            CanReadGameplayInput(),
            _currentGameState == GameState.Paused,
            _currentGameState == GameState.Idle,
            _currentGameState == GameState.Idle
            && _healthComponent.Stats.IsAlive
            && !_character.IsInputBlocked);

        public void Initialize()
        {
            _currentGameState = _gameStateNotifier.CurrentGameState;
            _gameStateNotifier.RegisterObserver(this);
            _healthComponent.Model.OnDied += HandleDied;
            _character.OnDeathAnimationCompleted += HandleDeathAnimationCompleted;
        }

        public void Dispose()
        {
            ClearInput();
            _gameStateNotifier.UnregisterObserver(this);
            _healthComponent.Model.OnDied -= HandleDied;
            _character.OnDeathAnimationCompleted -= HandleDeathAnimationCompleted;
        }

        public void OnGameStateChanged(GameState newState)
        {
            _currentGameState = newState;
            if (newState == GameState.Ended) ClearLockOn();
            if (newState is GameState.OnGraceSit or GameState.Ended)
            {
                _character.RestoreAtGrace();
            }
        }

        public void Submit(in PlayerSessionInput input)
        {
            if (!CanReadGameplayInput())
            {
                ClearInput();
                return;
            }

            CharacterInputFrame frame = new CharacterInputFrame(
                input.MoveInput, input.CameraYaw, input.SprintHeld, input.RollRequested,
                input.CrouchHeld, input.GuardPressed, input.GuardHeld, input.AttackPressed,
                input.AttackHeld, input.StrongAttackPressed, input.StrongAttackHeld,
                input.SpecialAttackPressed, input.SwitchWeaponPressed, input.SwitchShieldPressed,
                input.SwitchFlaskPressed, input.UseItemPressed, input.TwoHandedPressed, input.JumpPressed);

            if (_currentGameState == GameState.Paused)
            {
                _interactionController.ClearTarget();
                _actorTick.Submit(_actionCoordinator.BuildMovementOnly(frame));
                return;
            }

            if (!_character.IsInLadderOperation)
            {
                HandleLockOnInput();
                _interactionController.Tick();
            }
            else
            {
                _interactionController.ClearTarget();
            }

            _actorTick.Submit(_actionCoordinator.BuildInput(frame));
        }

        public void ClearInput()
        {
            _interactionController.ClearTarget();
            _actionCoordinator.Clear();
            _actorTick.Clear();
        }

        private bool CanReadGameplayInput()
        {
            return _healthComponent.Stats.IsAlive
                && (!_character.IsInputBlocked || _character.IsInLadderOperation)
                && (_currentGameState == GameState.Idle || _currentGameState == GameState.Paused);
        }

        private void HandleLockOnInput()
        {
            if (_inputService.WasUiBackConsumedThisFrame) return;
            if (_targetingService.IsLockedOn
                && !_targetingService.IsCurrentTargetValid(_character.transform.position))
            {
                ClearLockOn();
            }

            if (!_inputService.CharacterActions.LockOn.WasPressedThisFrame()) return;
            if (_targetingService.IsLockedOn)
            {
                ClearLockOn();
                return;
            }

            if (_targetingService.TryAcquireTarget(_character.transform.position)
                && _targetingService.TryGetCurrentTarget(out TargetingSnapshot snapshot))
            {
                _character.SetLockOnTarget(true, snapshot.EntityId);
                _cameraService.SetLockOnTarget(snapshot.EntityId);
            }
            else
            {
                _cameraService.RecenterCamera();
            }
        }

        private void ClearLockOn()
        {
            _targetingService.ClearTarget();
            _character.SetLockOnTarget(false, null);
            _cameraService.ClearLockOnTarget();
        }

        private void HandleDied(long sourceEntityId)
        {
            _character.PlayDeath();
        }

        private void HandleDeathAnimationCompleted()
        {
            _coreGameOrchestrator.RespawnAtLastGrace().Forget();
        }
    }
}
