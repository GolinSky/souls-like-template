using System.Collections.Generic;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Combat.AI;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyActionSelector
    {
        private readonly DeterministicEnemyActionSelector _selector;
        private readonly List<EnemyActionCandidate> _candidates = new();

        public EnemyActionSelector(EnemyBehaviourProfile profile)
        {
            _selector = new DeterministicEnemyActionSelector(profile.RandomSeed);
        }

        public CharacterActionId PreviousAction => _selector.PreviousAction;

        public CharacterActionDefinition Select(
            IReadOnlyList<AiActionRule> rules,
            float distance,
            float angle,
            bool hasLineOfSight,
            bool comboWindowOpen,
            float now)
        {
            _candidates.Clear();
            foreach (AiActionRule rule in rules)
            {
                if (rule.Action == null)
                {
                    continue;
                }

                _candidates.Add(new EnemyActionCandidate(
                    rule.Action.ActionId,
                    rule.MinimumDistance,
                    rule.MaximumDistance,
                    rule.MaximumAngle,
                    rule.RequiresLineOfSight,
                    rule.BaseWeight,
                    rule.Cooldown,
                    rule.RepetitionPenalty,
                    rule.RequiresComboWindow,
                    rule.RequiredPreviousAction));
            }

            EnemyActionSelectionContext context = new EnemyActionSelectionContext(
                distance,
                angle,
                hasLineOfSight,
                comboWindowOpen);
            CharacterActionId? selected = _selector.Select(_candidates, context, now);
            if (!selected.HasValue)
            {
                return null;
            }

            foreach (AiActionRule rule in rules)
            {
                if (rule.Action != null && rule.Action.ActionId == selected.Value)
                {
                    return rule.Action;
                }
            }

            return null;
        }

        public float Range(float minimum, float maximum) =>
            _selector.Range(minimum, maximum);

        public bool NextBool() => _selector.NextBool();
    }
}
