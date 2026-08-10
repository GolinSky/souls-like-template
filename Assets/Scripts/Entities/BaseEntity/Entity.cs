using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.BaseEntity
{
    public interface IEntity
    {
        long Id { get; }
        bool TryGetComponent<TEntityComponent>(out TEntityComponent targetComponent) where TEntityComponent : IEntityComponent;
        EntityType EntityType { get; }
    }

    public class Entity : IEntity, IInitializable, IDisposable
    {
        private readonly List<IEntityComponent> _components = new();
        private readonly IEntityLocator _entityLocator;
        public long Id { get; }
        public EntityType EntityType { get; }

        public Entity(long id, IEntityLocator entityLocator, EntityType entityType)
        {
            _entityLocator = entityLocator;
            EntityType = entityType;
            Id = id;
        }
        
        public void Initialize()
        {
            _entityLocator.AddEntity(this);
        }

        public void Dispose()
        {
            _entityLocator.RemoveEntity(this);
        }

        public void RegisterComponent(IEntityComponent component)
        {
            if (_components.Contains(component))
            {
                Debug.LogError($"{component.GetType().Name} has been already registered for entity:{Id},{EntityType}");
                return;
            }
            _components.Add(component);
        }
        
        public void UnRegisterComponent(IEntityComponent component)
        {
            if (!_components.Contains(component))
            {
                Debug.LogError($"{component.GetType().Name} has been already removed for entity:{Id},{EntityType}");
                return;
            }
            _components.Remove(component);
        }
        
        public bool TryGetComponent<TEntityComponent>(out TEntityComponent targetComponent) where TEntityComponent : IEntityComponent
        {
            targetComponent = default;
            foreach (IEntityComponent entityComponent in _components)
            {
                if (entityComponent is TEntityComponent foundComponent)
                {
                    targetComponent = foundComponent;
                    return true;
                }
            }
            
            return false;
        }
    }
}
