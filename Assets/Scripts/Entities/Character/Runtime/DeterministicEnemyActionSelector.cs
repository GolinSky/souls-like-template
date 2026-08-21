using System;
using System.Collections.Generic;

namespace SoulsLike.Entities.Combat.AI
{
    public sealed class DeterministicEnemyActionSelector
    {
        private readonly Random _random;
        private readonly Dictionary<CharacterActionId, float> _cooldowns = new();
        private readonly List<EnemyActionCandidate> _validCandidates = new();
        private readonly List<float> _validWeights = new();

        public DeterministicEnemyActionSelector(int seed)
        {
            _random = new Random(seed);
        }

        public CharacterActionId PreviousAction { get; private set; }

        public CharacterActionId? Select(
            IReadOnlyList<EnemyActionCandidate> candidates,
            in EnemyActionSelectionContext context,
            float now)
        {
            _validCandidates.Clear();
            _validWeights.Clear();
            float totalWeight = 0f;

            foreach (EnemyActionCandidate candidate in candidates)
            {
                if (!IsValid(candidate, context, now))
                {
                    continue;
                }

                float weight = Math.Max(
                    0f,
                    candidate.BaseWeight * (candidate.ActionId == PreviousAction
                        ? candidate.RepetitionPenalty
                        : 1f));
                if (weight <= 0f)
                {
                    continue;
                }

                _validCandidates.Add(candidate);
                _validWeights.Add(weight);
                totalWeight += weight;
            }

            if (_validCandidates.Count == 0)
            {
                return null;
            }

            double roll = _random.NextDouble() * totalWeight;
            for (int index = 0; index < _validCandidates.Count; index++)
            {
                roll -= _validWeights[index];
                if (roll <= 0d)
                {
                    return SelectCandidate(_validCandidates[index], now);
                }
            }

            return SelectCandidate(_validCandidates[^1], now);
        }

        public float Range(float minimum, float maximum) =>
            minimum + (maximum - minimum) * (float)_random.NextDouble();

        public bool NextBool() => _random.Next(0, 2) == 0;

        private bool IsValid(
            in EnemyActionCandidate candidate,
            in EnemyActionSelectionContext context,
            float now)
        {
            if (candidate.ActionId == CharacterActionId.None
                || candidate.RequiresComboWindow != context.ComboWindowOpen
                || context.Distance < candidate.MinimumDistance
                || context.Distance > candidate.MaximumDistance
                || context.Angle > candidate.MaximumAngle
                || (candidate.RequiresLineOfSight && !context.HasLineOfSight))
            {
                return false;
            }

            if (candidate.RequiredPreviousAction != CharacterActionId.None
                && candidate.RequiredPreviousAction != PreviousAction)
            {
                return false;
            }

            return !_cooldowns.TryGetValue(candidate.ActionId, out float cooldownUntil)
                || cooldownUntil <= now;
        }

        private CharacterActionId SelectCandidate(
            in EnemyActionCandidate candidate,
            float now)
        {
            PreviousAction = candidate.ActionId;
            _cooldowns[PreviousAction] = now + candidate.Cooldown;
            return PreviousAction;
        }
    }
}
