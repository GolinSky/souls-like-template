using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Services.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Ladder
{
    public sealed class LadderSystem : MonoBehaviour, IInitializable, IDisposable
    {
        private const string UNLOCKED_LADDERS_KEY = "UnlockedLadders";

        private readonly Dictionary<LadderView, (Entity Entity, LadderInteractCommand Command)> _entities = new();
        private HashSet<string> _unlockedLadderIds;
        private IEntityLocator _entityLocator;
        private IUniqueIdGenerator _idGenerator;
        private IStorageRegistry _storageRegistry;

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            IUniqueIdGenerator idGenerator,
            IStorageRegistry storageRegistry)
        {
            _entityLocator = entityLocator;
            _idGenerator = idGenerator;
            _storageRegistry = storageRegistry;
        }

        public void Initialize()
        {
            _unlockedLadderIds = _storageRegistry.GetData(
                UNLOCKED_LADDERS_KEY,
                new HashSet<string>());
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                RegisterScene(SceneManager.GetSceneAt(index));
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            foreach (LadderView ladder in new List<LadderView>(_entities.Keys))
            {
                Unregister(ladder);
            }
        }

        public void Register(LadderView ladder)
        {
            if (_entities.ContainsKey(ladder))
            {
                return;
            }

            if (ladder.StartsLocked)
            {
                if (string.IsNullOrWhiteSpace(ladder.SaveIdentifier))
                {
                    throw new InvalidOperationException(
                        $"Locked ladder '{ladder.name}' requires a stable {nameof(LadderView.SaveIdentifier)}.");
                }

                foreach (LadderView registered in _entities.Keys)
                {
                    if (registered.StartsLocked
                        && registered.SaveIdentifier == ladder.SaveIdentifier)
                    {
                        throw new InvalidOperationException(
                            $"Locked ladder id '{ladder.SaveIdentifier}' is duplicated by "
                            + $"'{registered.name}' and '{ladder.name}'.");
                    }
                }
            }

            ViewEntity viewEntity = ladder.GetComponent<ViewEntity>();
            if (viewEntity == null)
            {
                throw new InvalidOperationException(
                    $"Ladder '{ladder.name}' requires {nameof(ViewEntity)} on its root.");
            }

            long id = _idGenerator.GenerateUniqueId();
            viewEntity.Construct(id, EntityType.Ladder);
            Entity entity = new(id, _entityLocator, EntityType.Ladder);
            LadderInteractCommand command = new(entity, ladder);
            ladder.AssignSystem(this);
            ladder.Construct(entity);
            command.Initialize();
            entity.Initialize();
            ladder.ApplyPersistedUnlock(
                ladder.StartsLocked && _unlockedLadderIds.Contains(ladder.SaveIdentifier));
            _entities.Add(ladder, (entity, command));
        }

        public void Unregister(LadderView ladder)
        {
            if (!_entities.TryGetValue(ladder, out var tuple))
            {
                return;
            }

            _entities.Remove(ladder);

            tuple.Command.Dispose();
            ladder.DisposeEntity();
            tuple.Entity.Dispose();
        }

        public async UniTask UnlockAsync(LadderView ladder, CancellationToken token)
        {
            await ladder.DeployAsync(token);
            token.ThrowIfCancellationRequested();
            if (!ladder.StartsLocked)
            {
                return;
            }

            _unlockedLadderIds.Add(ladder.SaveIdentifier);
            _storageRegistry.SaveData(UNLOCKED_LADDERS_KEY, _unlockedLadderIds);
        }

        public bool TryFindRoute(
            Vector3 actorPosition,
            Vector3 targetPosition,
            NavMeshQueryFilter filter,
            out LadderView ladder,
            out LadderEnd entryEnd)
        {
            ladder = null;
            entryEnd = LadderEnd.Bottom;
            if (Mathf.Abs(targetPosition.y - actorPosition.y) < 1f)
            {
                return false;
            }

            float bestCost = float.MaxValue;
            foreach (LadderView candidate in _entities.Keys)
            {
                if (!candidate.IsUnlocked || candidate.IsDeploying)
                {
                    continue;
                }

                bool targetAbove = targetPosition.y > actorPosition.y;
                LadderEnd candidateEntry = targetAbove ? LadderEnd.Bottom : LadderEnd.Top;
                Vector3 entry = candidate.GetExit(candidateEntry).position;
                Vector3 destination = candidate.GetExit(
                    candidateEntry == LadderEnd.Bottom ? LadderEnd.Top : LadderEnd.Bottom).position;
                if (Mathf.Abs(targetPosition.y - destination.y)
                    >= Mathf.Abs(targetPosition.y - actorPosition.y))
                {
                    continue;
                }

                NavMeshPath approachPath = new();
                NavMeshPath departurePath = new();
                if (!NavMesh.CalculatePath(actorPosition, entry, filter, approachPath)
                    || approachPath.status != NavMeshPathStatus.PathComplete
                    || !NavMesh.CalculatePath(destination, targetPosition, filter, departurePath)
                    || departurePath.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }
                float cost = HorizontalDistance(actorPosition, entry)
                    + HorizontalDistance(targetPosition, destination);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    ladder = candidate;
                    entryEnd = candidateEntry;
                }
            }

            return ladder != null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RegisterScene(scene);

        private void OnSceneUnloaded(Scene scene)
        {
            foreach (LadderView ladder in new List<LadderView>(_entities.Keys))
            {
                if (ladder.gameObject.scene == scene)
                {
                    Unregister(ladder);
                }
            }
        }

        private void RegisterScene(Scene scene)
        {
            if (!scene.isLoaded)
            {
                return;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (LadderView ladder in root.GetComponentsInChildren<LadderView>(true))
                {
                    Register(ladder);
                }
            }
        }

        private static float HorizontalDistance(Vector3 first, Vector3 second)
        {
            first.y = 0f;
            second.y = 0f;
            return Vector3.Distance(first, second);
        }
    }
}
