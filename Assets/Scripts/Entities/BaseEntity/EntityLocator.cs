using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity
{
    public class EntityLocator : IEntityLocator
    {
        private Dictionary<long, IEntity> _entities = new Dictionary<long, IEntity>();
        
        public void AddEntity(IEntity entity)
        {
            _entities.Add(entity.Id, entity);
        }

        public void RemoveEntity(IEntity entity)
        {
            _entities.Remove(entity.Id);
        }

        public IEntity GetEntity(long entityId)
        {
            if (_entities.TryGetValue(entityId, out var entity))
            {
                return entity;
            }
            throw new Exception("No entity found with id: " + entityId);
        }

        public bool TryGetEntity(RaycastHit raycastHit, out IEntity entity)
        {
            entity = null;
            IViewEntity viewEntity = raycastHit.collider.GetComponentInParent<IViewEntity>();
            if (viewEntity != null)
            {
                entity = GetEntity(viewEntity.Id);
                return entity != null;
            }
            return false;
        }
        
        public bool TryGetEntity(Collider collider, out IEntity entity)
        {
            entity = null;
            IViewEntity viewEntity = collider.GetComponentInParent<IViewEntity>();
            if (viewEntity != null)
            {
                entity = GetEntity(viewEntity.Id);
                return entity != null;
            }
            return false;
        }
    }
}
