#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Services.IdGeneration;
using VContainer;

namespace SoulsLike.Editor.Tests.DependencyInjection
{
    public sealed class EntityRegistrationExtTests
    {
        [Test]
        public void RegisterEntitySystemExt_UsesOneScopedIdAcrossResolves()
        {
            CountingIdGenerator idGenerator = new();
            ContainerBuilder builder = new();
            builder.RegisterInstance<IUniqueIdGenerator>(idGenerator);
            builder.RegisterEntitySystemExt(EntityType.Player);

            using IObjectResolver container = builder.Build();

            long firstId = container.Resolve<long>();
            long secondId = container.Resolve<long>();

            Assert.That(secondId, Is.EqualTo(firstId));
            Assert.That(idGenerator.CallCount, Is.EqualTo(1));
        }

        private sealed class CountingIdGenerator : IUniqueIdGenerator
        {
            public int CallCount { get; private set; }

            public long GenerateUniqueId()
            {
                CallCount++;
                return CallCount;
            }
        }
    }
}
#endif
