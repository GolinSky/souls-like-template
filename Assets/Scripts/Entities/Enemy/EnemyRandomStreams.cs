using System;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyRandomStreams
    {
        private const int MOVEMENT_STREAM = 0x31A5;
        private const int TIMING_STREAM = 0x59D7;
        private const int ACTION_STREAM = 0x7B1F;

        private readonly Random _movement;
        private readonly Random _timing;

        public EnemyRandomStreams(EnemyActor actor)
        {
            _movement = new Random(DeriveSeed(
                actor.BehaviourProfile.RandomSeed,
                actor.RandomSeedOffset,
                MOVEMENT_STREAM));
            _timing = new Random(DeriveSeed(
                actor.BehaviourProfile.RandomSeed,
                actor.RandomSeedOffset,
                TIMING_STREAM));
        }

        public bool NextMovementBool() => _movement.Next(0, 2) == 0;

        public float TimingRange(float minimum, float maximum) =>
            minimum + ((float)_timing.NextDouble() * (maximum - minimum));

        public static int GetActionSelectionSeed(EnemyActor actor) =>
            DeriveSeed(
                actor.BehaviourProfile.RandomSeed,
                actor.RandomSeedOffset,
                ACTION_STREAM);

        private static int DeriveSeed(int profileSeed, int spawnOffset, int stream)
        {
            uint value = unchecked((uint)profileSeed) ^ unchecked((uint)spawnOffset * 0x9E3779B9u);
            value ^= (uint)stream;
            value ^= value >> 16;
            value *= 0x85EBCA6Bu;
            value ^= value >> 13;
            return unchecked((int)value);
        }
    }
}
