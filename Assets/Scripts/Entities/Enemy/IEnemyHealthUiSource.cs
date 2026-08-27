using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public interface IEnemyHealthUiSource
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        Vector3 WorldPosition { get; }
    }
}
