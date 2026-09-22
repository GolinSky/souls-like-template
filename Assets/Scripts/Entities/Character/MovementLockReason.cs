using System;

namespace SoulsLike.Entities.Character
{
    [Flags]
    public enum MovementLockReason { None = 0, Manual = 1, Animation = 2, Spawn = 4, Parry = 8, Critical = 16, Ladder = 32 }
}