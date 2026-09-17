using System;
using VContainer.Unity;

namespace SoulsLike.Services.VContainer
{
    public abstract class EntityLifetimeScope : LifetimeScope
    {
        private bool _isBuilt;

        public void BuildOnce()
        {
            if (_isBuilt)
            {
                throw new InvalidOperationException(
                    $"{nameof(EntityLifetimeScope)} can only be built once.");
            }

            _isBuilt = true;
            Build();
        }
    }
}
