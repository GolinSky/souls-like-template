using System;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public interface IEnemyHealthUiSource
    {
        event Action<float, float> HealthChanged;

        bool IsAvailable { get; }
        bool ShouldShow { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        Vector3 WorldPosition { get; }
    }

    public interface IEnemyHealthUiService
    {
        void Track(IEnemyHealthUiSource source);
    }
}
