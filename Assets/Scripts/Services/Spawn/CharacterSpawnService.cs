using System;
using SoulsLike.Services.Save;
using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using SoulsLike.Services.Travel.Data;
using UnityEngine;

namespace SoulsLike.Services.Spawn
{
    public sealed class CharacterSpawnService
    {
        private enum PendingSpawnKind
        {
            None,
            SavedPosition,
            UnresolvedGrace,
            ResolvedGracePosition
        }

        private readonly SaveStore<CharacterSpawnData> _store;
        private readonly LocationData _locationData;
        private readonly ISceneService _sceneService;

        private PendingSpawnKind _pendingSpawnKind;
        private GraceId _pendingGraceId;
        private Vector3 _pendingPosition;

        public CharacterSpawnService(
            ISaveService saveService,
            LocationData locationData,
            ISceneService sceneService)
        {
            _store = new SaveStore<CharacterSpawnData>(saveService, nameof(SaveKeys.CharacterSpawn));
            _locationData = locationData;
            _sceneService = sceneService;
        }

        public SceneType PrepareResume()
        {
            ClearPendingSpawn();
            if (!_store.Exists)
            {
                return SceneType.DefaultLocation;
            }

            CharacterSpawnData data = _store.LoadOrCreate();
            if (data.HasCurrentPosition)
            {
                _pendingSpawnKind = PendingSpawnKind.SavedPosition;
                _pendingPosition = data.CurrentPosition;
                return data.CurrentScene;
            }

            return PrepareGraceSpawn(data.LastGraceId);
        }

        public SceneType PrepareGraceSpawn(GraceId graceId)
        {
            _pendingSpawnKind = PendingSpawnKind.UnresolvedGrace;
            _pendingGraceId = graceId;
            return _locationData.GetLocation(graceId).Id;
        }

        public SceneType PrepareRespawn()
        {
            CharacterSpawnData data = _store.LoadOrCreate();
            return PrepareGraceSpawn(data.LastGraceId);
        }

        public bool TryGetPendingGrace(out GraceId graceId)
        {
            graceId = _pendingGraceId;
            return _pendingSpawnKind == PendingSpawnKind.UnresolvedGrace;
        }

        public void ResolvePendingGrace(Vector3 position)
        {
            if (_pendingSpawnKind != PendingSpawnKind.UnresolvedGrace)
            {
                throw new InvalidOperationException("A grace spawn can only be resolved while a grace request is pending.");
            }

            CharacterSpawnData data = _store.LoadOrCreate();
            data.HasCurrentPosition = true;
            data.CurrentScene = _locationData.GetLocation(_pendingGraceId).Id;
            data.CurrentPosition = position;
            data.LastGraceId = _pendingGraceId;
            _store.Save(data);

            _pendingPosition = position;
            _pendingSpawnKind = PendingSpawnKind.ResolvedGracePosition;
        }

        public bool TryConsumeSpawn(out Vector3 position, out bool startsOnGrace)
        {
            if (_pendingSpawnKind == PendingSpawnKind.UnresolvedGrace)
            {
                throw new InvalidOperationException("The pending grace spawn must be resolved by GraceSystem before creating the character.");
            }

            if (_pendingSpawnKind == PendingSpawnKind.None)
            {
                position = default;
                startsOnGrace = false;
                return false;
            }

            position = _pendingPosition;
            startsOnGrace = _pendingSpawnKind == PendingSpawnKind.ResolvedGracePosition;
            ClearPendingSpawn();
            return true;
        }

        public void SaveCurrentPosition(Vector3 position)
        {
            CharacterSpawnData data = _store.LoadOrCreate();
            data.HasCurrentPosition = true;
            data.CurrentScene = _sceneService.CurrentScene;
            data.CurrentPosition = position;
            _store.Save(data);
        }

        public void SaveLastGrace(GraceId graceId)
        {
            CharacterSpawnData data = _store.LoadOrCreate();
            data.LastGraceId = graceId;
            _store.Save(data);
        }

        private void ClearPendingSpawn()
        {
            _pendingSpawnKind = PendingSpawnKind.None;
            _pendingGraceId = default;
            _pendingPosition = default;
        }
    }
}
