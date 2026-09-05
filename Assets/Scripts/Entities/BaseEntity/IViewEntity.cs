namespace SoulsLike.Entities.BaseEntity
{
    public interface IViewEntity : IEntityComponent
    {
        long Id { get; }
        EntityType EntityType { get; }
    }
}
