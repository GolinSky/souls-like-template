namespace SoulsLike.Entities.BaseEntity
{
    public interface IEntity
    {
        long Id { get; }
        bool TryGetComponent<TEntityComponent>(out TEntityComponent targetComponent) where TEntityComponent : IEntityComponent;
        EntityType EntityType { get; }
    }
}
