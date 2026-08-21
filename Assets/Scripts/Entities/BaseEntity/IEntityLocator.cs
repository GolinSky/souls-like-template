using UnityEngine;
using System.Collections.Generic;

namespace SoulsLike.Entities.BaseEntity
{
    public interface IEntityLocator
    {
        void AddEntity(IEntity entity);
        void RemoveEntity(IEntity entity);
        IEntity GetEntity(long entityId);
        bool TryGetEntity(long entityId, out IEntity entity);
        void GetEntities(EntityType entityType, List<IEntity> results);
        
        bool TryGetEntity(RaycastHit raycastHit, out IEntity entity);
        bool TryGetEntity(Collider collider, out IEntity entity);
    }
}
