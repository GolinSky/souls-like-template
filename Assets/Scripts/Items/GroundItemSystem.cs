using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services.IdGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Items
{
    public sealed class GroundItemSystem : MonoBehaviour, IInitializable, IDisposable
    {
        private readonly Dictionary<GroundItem, (Entity Entity, GroundItemInteractCommand Command)> _entities = new();
        private IEntityLocator _entityLocator;
        private IUniqueIdGenerator _idGenerator;

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            IUniqueIdGenerator idGenerator)
        {
            _entityLocator = entityLocator;
            _idGenerator = idGenerator;
        }

        public void Initialize()
        {
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
            foreach (GroundItem item in new List<GroundItem>(_entities.Keys))
            {
                Unregister(item);
            }
        }

        public void Register(GroundItem item)
        {
            if (item == null || _entities.ContainsKey(item))
            {
                return;
            }

            ViewEntity viewEntity = item.GetComponent<ViewEntity>();
            if (viewEntity == null)
            {
                viewEntity = item.gameObject.AddComponent<ViewEntity>();
            }

            long id = _idGenerator.GenerateUniqueId();
            viewEntity.Construct(id, EntityType.GroundItem);
            Entity entity = new(id, _entityLocator, EntityType.GroundItem);
            GroundItemInteractCommand command = new(entity, item);
            item.AssignSystem(this);
            command.Initialize();
            entity.Initialize();
            _entities.Add(item, (entity, command));
        }

        public void Unregister(GroundItem item)
        {
            if (item == null || !_entities.TryGetValue(item, out var tuple))
            {
                return;
            }

            _entities.Remove(item);
            tuple.Command.Dispose();
            tuple.Entity.Dispose();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RegisterScene(scene);

        private void OnSceneUnloaded(Scene scene)
        {
            foreach (GroundItem item in new List<GroundItem>(_entities.Keys))
            {
                if (item == null || item.gameObject.scene == scene)
                {
                    Unregister(item);
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
                foreach (GroundItem item in root.GetComponentsInChildren<GroundItem>(true))
                {
                    Register(item);
                }
            }
        }
    }
}
