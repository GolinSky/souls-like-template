using SoulsLike.Entities.Character.Runtime;
using UnityEngine;

namespace SoulsLike.Entities.Character.Adapters
{
    public sealed class UnityCharacterClock : ICharacterClock
    {
        public float Now => Time.time;
    }
}
