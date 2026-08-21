using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity
{
    public class EntityLocator : IEntityLocator
    {
        private readonly Dictionary<long, IEntity> _entities = new();
        private readonly Dictionary<EntityType, List<IEntity>> _entitiesByType = new();
        
        public void AddEntity(IEntity entity)
        {
            _entities.Add(entity.Id, entity);
            if (!_entitiesByType.TryGetValue(entity.EntityType, out List<IEntity> entities))
            {
                entities = new List<IEntity>();
                _entitiesByType.Add(entity.EntityType, entities);
            }
            entities.Add(entity);
        }

        public void RemoveEntity(IEntity entity)
        {
            _entities.Remove(entity.Id);
            if (_entitiesByType.TryGetValue(entity.EntityType, out List<IEntity> entities))
            {
                entities.Remove(entity);
                if (entities.Count == 0)
                {
                    _entitiesByType.Remove(entity.EntityType);
                }
            }
        }

        public IEntity GetEntity(long entityId)
        {
            if (_entities.TryGetValue(entityId, out IEntity entity))
            {
                return entity;
            }
            throw new Exception("No entity found with id: " + entityId);
        }

        public bool TryGetEntity(long entityId, out IEntity entity) =>
            _entities.TryGetValue(entityId, out entity);

        public void GetEntities(EntityType entityType, List<IEntity> results)
        {
            results.Clear();
            if (_entitiesByType.TryGetValue(entityType, out List<IEntity> entities))
            {
                results.AddRange(entities);
            }
        }

        public bool TryGetEntity(RaycastHit raycastHit, out IEntity entity)
        {
            entity = null;
            IViewEntity viewEntity = raycastHit.collider.GetComponentInParent<IViewEntity>();
            if (viewEntity != null)
            {
                return TryGetEntity(viewEntity.Id, out entity);
            }
            return false;
        }
        
        public bool TryGetEntity(Collider collider, out IEntity entity)
        {
            entity = null;
            IViewEntity viewEntity = collider.GetComponentInParent<IViewEntity>();
            if (viewEntity != null)
            {
                return TryGetEntity(viewEntity.Id, out entity);
            }
            return false;
        }
    }
}
