using System;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Health
{
    [Serializable]
    public struct DamageRequest
    {
        public long SourceEntityId;
        public float Amount;
        public Vector3 HitPoint;
        public int HitZone;
    }
}
