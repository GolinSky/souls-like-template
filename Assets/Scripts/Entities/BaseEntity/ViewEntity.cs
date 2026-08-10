using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.BaseEntity
{
    public interface IViewEntity
    {
        long Id { get; }
        EntityType EntityType { get; }
    }

    /// <summary>
    /// will be added dynamically via installer to every entity unit GO
    /// </summary>
    public class ViewEntity : MonoBehaviour, IViewEntity
    {
        public long Id { get; private set; }
        public EntityType EntityType { get; private set; }
        

        [Inject]
        public void Construct(long id, EntityType entityType)
        {
            Id = id;
            EntityType = entityType;
        }
    }
}
