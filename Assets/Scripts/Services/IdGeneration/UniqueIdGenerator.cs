using System;

namespace SoulsLike.Services.IdGeneration
{
    public class UniqueIdGenerator : IUniqueIdGenerator
    {
        public long GenerateUniqueId()
        {
            return BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
        }
    }
}
