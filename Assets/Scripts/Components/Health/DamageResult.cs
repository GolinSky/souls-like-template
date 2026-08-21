using System;

namespace SoulsLike.Entities.Character.Components.Health
{
    [Serializable]
    public struct DamageResult
    {
        public long SourceEntityId;
        public float IncomingAmount;
        public float HealthDamageAmount;
        public HealthStats NewStats;
        public bool Killed;
    }
}
