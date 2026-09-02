using System.Collections.Generic;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyActionSelector
    {
        private readonly int _randomSeed;
        private readonly Dictionary<EnemyMove, float> _cooldownEnds = new();
        private readonly List<EnemyMove> _candidates = new();
        private readonly List<float> _weights = new();
        private int _committedSelectionCount;

        public EnemyActionSelector(EnemyActor actor)
        {
            _randomSeed = EnemyRandomStreams.GetActionSelectionSeed(actor);
        }

        public EnemyMove PreviousMove { get; private set; }

        public EnemyMove Choose(
            IReadOnlyList<EnemyMove> moves,
            float distance,
            float angle,
            bool hasLineOfSight,
            EnemyMove currentMove,
            bool isFollowUp,
            float now)
        {
            _candidates.Clear();
            _weights.Clear();
            float totalWeight = 0f;
            foreach (EnemyMove move in moves)
            {
                if (!IsEligible(move, distance, angle, hasLineOfSight, currentMove, isFollowUp, now))
                {
                    continue;
                }

                float weight = move.Weight;
                if (move == PreviousMove)
                {
                    weight *= move.RepetitionPenalty;
                }

                if (weight <= 0f)
                {
                    continue;
                }

                _candidates.Add(move);
                _weights.Add(weight);
                totalWeight += weight;
            }

            if (_candidates.Count == 0)
            {
                return null;
            }

            float roll = GetSelectionRoll() * totalWeight;
            for (int index = 0; index < _candidates.Count; index++)
            {
                roll -= _weights[index];
                if (roll <= 0f)
                {
                    return _candidates[index];
                }
            }

            return _candidates[^1];
        }

        public void CommitStarted(EnemyMove move, float now)
        {
            PreviousMove = move;
            _cooldownEnds[move] = now + move.Cooldown;
            _committedSelectionCount++;
        }

        public bool IsWithinAnyMoveRange(IReadOnlyList<EnemyMove> moves, float distance)
        {
            foreach (EnemyMove move in moves)
            {
                if (move != null
                    && move.Action != null
                    && move.MoveUsage != EnemyMove.Usage.FollowUp
                    && distance >= move.MinimumDistance
                    && distance <= move.MaximumDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private float GetSelectionRoll()
        {
            uint value = unchecked((uint)_randomSeed)
                + (uint)_committedSelectionCount * 0x9E3779B9u;
            value ^= value >> 16;
            value *= 0x85EBCA6Bu;
            value ^= value >> 13;
            value *= 0xC2B2AE35u;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777216f;
        }

        private bool IsEligible(
            EnemyMove move,
            float distance,
            float angle,
            bool hasLineOfSight,
            EnemyMove currentMove,
            bool isFollowUp,
            float now)
        {
            if (move == null
                || move.Action == null
                || distance < move.MinimumDistance
                || distance > move.MaximumDistance
                || angle > move.MaximumAngle
                || move.RequiresLineOfSight && !hasLineOfSight
                || _cooldownEnds.TryGetValue(move, out float cooldownEnd) && now < cooldownEnd)
            {
                return false;
            }

            if (isFollowUp)
            {
                return move.MoveUsage != EnemyMove.Usage.Opener
                    && IsLegalFollowUp(currentMove, move);
            }

            return move.MoveUsage != EnemyMove.Usage.FollowUp;
        }

        public static bool IsLegalFollowUp(EnemyMove currentMove, EnemyMove nextMove)
        {
            if (currentMove == null || currentMove.Action == null || nextMove == null)
            {
                return false;
            }

            foreach (var followUp in currentMove.Action.FollowUps)
            {
                if (followUp == nextMove.Action)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
