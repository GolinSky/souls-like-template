using UnityEngine;

namespace SoulsLike.Entities.BaseEntity
{
    public interface IEntityLocator
    {
        void AddEntity(IEntity entity);
        void RemoveEntity(IEntity entity);
        IEntity GetEntity(long entityId);
        
        bool TryGetEntity(RaycastHit raycastHit, out IEntity entity);
        bool TryGetEntity(Collider collider, out IEntity entity);
    }
}
