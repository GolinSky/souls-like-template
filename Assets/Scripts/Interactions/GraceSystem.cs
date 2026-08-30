using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Services;
using SoulsLike.Services.Storage;
using SoulsLike.Services.Travel.Data;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Interactions
{
    public sealed class GraceSystem : MonoBehaviour, IGracePresenter, IInitializable
    {
        private const string OPEN_GRACES_KEY = "OpenGraces";
        private const string SIT_ON_GRACE_PROMPT = "Sit on grace";
        private const string OPEN_GRACE_PROMPT = "Open grace";
        private const string CANNOT_SIT_AT_GRACE_PROMPT = "Cannot sit at grace";

        [SerializeField] private GraceView[] graceViews;

        private ICoreGameOrchestrator _coreGameOrchestrator;
        private IStorageRegistry _storageRegistry;
        private LocationData _locationData;
        private HashSet<string> _openGraceIds;

        [Inject]
        public void Construct(
            ICoreGameOrchestrator coreGameOrchestrator,
            IStorageRegistry storageRegistry,
            LocationData locationData)
        {
            _coreGameOrchestrator = coreGameOrchestrator;
            _storageRegistry = storageRegistry;
            _locationData = locationData;
            _openGraceIds = storageRegistry.GetData(
                OPEN_GRACES_KEY,
                new HashSet<string>());
        }
        
        public void Initialize()
        {
            foreach (GraceView graceView in graceViews)
            {
                graceView.AssignPresenter(this);
            }
        }

        public bool CanInteract() => true;

        public InteractionPrompt GetPrompt(GraceView graceView) =>
            new(IsGraceOpen(graceView.GraceId)
                ? SIT_ON_GRACE_PROMPT
                : OPEN_GRACE_PROMPT);

        public InteractionPrompt GetFailurePrompt() => new(CANNOT_SIT_AT_GRACE_PROMPT);

        public UniTask InteractAsync(GraceView graceView, CancellationToken token)
        {
            if (!IsGraceOpen(graceView.GraceId))
            {
                OpenGrace(graceView);
                return UniTask.CompletedTask;
            }

            return SitOnGrace();
        }

        public void ExitGraceState() => _coreGameOrchestrator.ResumeGame();

        private void OpenGrace(GraceView graceView)
        {
            _openGraceIds.Add(_locationData.GetGrace(graceView.GraceId).Name);
            _storageRegistry.SaveData(OPEN_GRACES_KEY, _openGraceIds);
        }

        private bool IsGraceOpen(GraceId graceId) =>
            _openGraceIds.Contains(_locationData.GetGrace(graceId).Name);

        private UniTask SitOnGrace() => _coreGameOrchestrator.OnGraceSit();
    }
}
