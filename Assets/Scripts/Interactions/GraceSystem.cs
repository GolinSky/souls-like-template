using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Services.Storage;
using SoulsLike.Services.Spawn;
using SoulsLike.Services.Travel.Data;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Interactions
{
    public sealed class GraceSystem : MonoBehaviour, IGracePresenter, IInitializable, IDisposable
    {
        private const string OPEN_GRACES_KEY = "OpenGraces";
        private const string SIT_ON_GRACE_PROMPT = "Sit on grace";
        private const string OPEN_GRACE_PROMPT = "Open grace";
        private const string CANNOT_SIT_AT_GRACE_PROMPT = "Cannot sit at grace";

        [SerializeField] private GraceView[] graceViews;

        private readonly Dictionary<GraceView, (Entity Entity, GraceInteractCommand Command)> _entities = new();
        private ICoreGameOrchestrator _coreGameOrchestrator;
        private IStorageRegistry _storageRegistry;
        private LocationData _locationData;
        private CharacterSpawnService _characterSpawnService;
        private IEntityLocator _entityLocator;
        private IUniqueIdGenerator _idGenerator;
        private HashSet<string> _openGraceIds;

        [Inject]
        public void Construct(
            ICoreGameOrchestrator coreGameOrchestrator,
            IStorageRegistry storageRegistry,
            LocationData locationData,
            CharacterSpawnService characterSpawnService,
            IEntityLocator entityLocator,
            IUniqueIdGenerator idGenerator)
        {
            _coreGameOrchestrator = coreGameOrchestrator;
            _storageRegistry = storageRegistry;
            _locationData = locationData;
            _characterSpawnService = characterSpawnService;
            _entityLocator = entityLocator;
            _idGenerator = idGenerator;
            _openGraceIds = storageRegistry.GetData(
                OPEN_GRACES_KEY,
                new HashSet<string>());

            foreach (GraceView graceView in graceViews)
            {
                _characterSpawnService.RegisterGracePosition(
                    graceView.GraceId,
                    graceView.InteractionAnchor.position);
            }

            if (_characterSpawnService.TryGetPendingGrace(out GraceId graceId))
            {
                GraceView graceView = graceViews.Single(view => view.GraceId == graceId);
                _characterSpawnService.ResolvePendingGrace(graceView.InteractionAnchor.position);
            }
        }
        
        public void Initialize()
        {
            foreach (GraceView graceView in graceViews)
            {
                graceView.AssignPresenter(this);

                ViewEntity viewEntity = graceView.GetComponent<ViewEntity>();
                if (viewEntity == null)
                {
                    viewEntity = graceView.gameObject.AddComponent<ViewEntity>();
                }

                long id = _idGenerator.GenerateUniqueId();
                viewEntity.Construct(id, EntityType.Grace);
                Entity entity = new(id, _entityLocator, EntityType.Grace);
                GraceInteractCommand command = new(entity, graceView, this);
                command.Initialize();
                entity.Initialize();
                _entities.Add(graceView, (entity, command));
            }
        }

        public void Dispose()
        {
            foreach (var tuple in _entities.Values)
            {
                tuple.Command.Dispose();
                tuple.Entity.Dispose();
            }
            _entities.Clear();
        }

        public bool CanInteract() => true;

        public InteractionPrompt GetPrompt(GraceView graceView) =>
            new(IsGraceOpen(graceView.GraceId)
                ? SIT_ON_GRACE_PROMPT
                : OPEN_GRACE_PROMPT);

        public InteractionPrompt GetFailurePrompt() => new(CANNOT_SIT_AT_GRACE_PROMPT);

        public async UniTask InteractAsync(GraceView graceView, CancellationToken token)
        {
            if (!IsGraceOpen(graceView.GraceId))
            {
                await _coreGameOrchestrator.PlayGraceUnblock(token);
                token.ThrowIfCancellationRequested();
                OpenGrace(graceView);
                return;
            }

            await SitOnGrace(graceView, token);
        }

        public void ExitGraceState() =>
            _coreGameOrchestrator.ExitGraceState(destroyCancellationToken).Forget();

        public void ResetOpenGraces()
        {
            _openGraceIds.Clear();
            _storageRegistry.DeleteData(OPEN_GRACES_KEY);
        }

        private void OpenGrace(GraceView graceView)
        {
            _openGraceIds.Add(_locationData.GetGrace(graceView.GraceId).Name);
            _storageRegistry.SaveData(OPEN_GRACES_KEY, _openGraceIds);
        }

        private bool IsGraceOpen(GraceId graceId) =>
            _openGraceIds.Contains(_locationData.GetGrace(graceId).Name);

        private UniTask SitOnGrace(GraceView graceView, CancellationToken token) =>
            _coreGameOrchestrator.OnGraceSit(graceView.GraceId, token);
    }
}
