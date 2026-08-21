using System.Collections.Generic;
using NUnit.Framework;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Combat.AI;

namespace SoulsLike.Tests.EnemyAI
{
    public sealed class EnemyActionSelectionTests
    {
        [Test]
        public void SameSeedProducesTheSameActionSequence()
        {
            IReadOnlyList<EnemyActionCandidate> candidates = new[]
            {
                Candidate(CharacterActionId.LightAttack1, 1f),
                Candidate(CharacterActionId.LightAttack2, 2f),
                Candidate(CharacterActionId.HeavyAttack, 0.5f)
            };
            EnemyActionSelectionContext context = new EnemyActionSelectionContext(
                2f,
                10f,
                true,
                false);
            DeterministicEnemyActionSelector first =
                new DeterministicEnemyActionSelector(347);
            DeterministicEnemyActionSelector second =
                new DeterministicEnemyActionSelector(347);

            for (int index = 0; index < 20; index++)
            {
                Assert.That(
                    first.Select(candidates, context, index),
                    Is.EqualTo(second.Select(candidates, context, index)));
            }
        }

        [Test]
        public void FiltersDistanceAngleAndLineOfSight()
        {
            IReadOnlyList<EnemyActionCandidate> candidates = new[]
            {
                new EnemyActionCandidate(
                    CharacterActionId.LightAttack1,
                    1f,
                    3f,
                    45f,
                    true,
                    1f,
                    0f,
                    1f)
            };
            DeterministicEnemyActionSelector selector =
                new DeterministicEnemyActionSelector(1);

            Assert.That(selector.Select(
                candidates,
                new EnemyActionSelectionContext(0.5f, 10f, true, false),
                0f), Is.Null);
            Assert.That(selector.Select(
                candidates,
                new EnemyActionSelectionContext(2f, 60f, true, false),
                0f), Is.Null);
            Assert.That(selector.Select(
                candidates,
                new EnemyActionSelectionContext(2f, 10f, false, false),
                0f), Is.Null);
            Assert.That(selector.Select(
                candidates,
                new EnemyActionSelectionContext(2f, 10f, true, false),
                0f), Is.EqualTo(CharacterActionId.LightAttack1));
        }

        [Test]
        public void CooldownPreventsImmediateRepetition()
        {
            IReadOnlyList<EnemyActionCandidate> candidates = new[]
            {
                new EnemyActionCandidate(
                    CharacterActionId.LightAttack1,
                    0f,
                    3f,
                    180f,
                    false,
                    1f,
                    10f,
                    1f)
            };
            EnemyActionSelectionContext context = new EnemyActionSelectionContext(
                2f,
                0f,
                true,
                false);
            DeterministicEnemyActionSelector selector =
                new DeterministicEnemyActionSelector(1);

            Assert.That(selector.Select(candidates, context, 0f),
                Is.EqualTo(CharacterActionId.LightAttack1));
            Assert.That(selector.Select(candidates, context, 1f), Is.Null);
            Assert.That(selector.Select(candidates, context, 10f),
                Is.EqualTo(CharacterActionId.LightAttack1));
        }

        [Test]
        public void ComboRuleRequiresWindowAndPreviousAction()
        {
            IReadOnlyList<EnemyActionCandidate> opener = new[]
            {
                Candidate(CharacterActionId.Combo1, 1f)
            };
            IReadOnlyList<EnemyActionCandidate> followUp = new[]
            {
                new EnemyActionCandidate(
                    CharacterActionId.Combo2,
                    0f,
                    4f,
                    180f,
                    true,
                    1f,
                    0f,
                    1f,
                    true,
                    CharacterActionId.Combo1)
            };
            DeterministicEnemyActionSelector selector =
                new DeterministicEnemyActionSelector(1);

            selector.Select(
                opener,
                new EnemyActionSelectionContext(2f, 0f, true, false),
                0f);
            Assert.That(selector.Select(
                followUp,
                new EnemyActionSelectionContext(2f, 0f, true, false),
                0f), Is.Null);
            Assert.That(selector.Select(
                followUp,
                new EnemyActionSelectionContext(2f, 0f, true, true),
                0f), Is.EqualTo(CharacterActionId.Combo2));
        }

        private static EnemyActionCandidate Candidate(
            CharacterActionId actionId,
            float weight) =>
            new EnemyActionCandidate(
                actionId,
                0f,
                4f,
                180f,
                false,
                weight,
                0f,
                0.5f);
    }
}
