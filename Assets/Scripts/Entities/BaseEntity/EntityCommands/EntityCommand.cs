using System;
using SoulsLike.Entities.BaseEntity;
using VContainer.Unity;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public abstract class EntityCommand : IEntityComponent, IInitializable, IDisposable
    {
        private Entity _entity;

        protected EntityCommand(Entity entity)
        {
            _entity = entity;
        }

        public void Initialize()
        {
            _entity.RegisterComponent(this);
        }
        
        public void Dispose()
        {
            _entity.UnRegisterComponent(this);
        }
    }
}
