using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    [CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Enemy/Catalog")]
    public sealed class EnemyCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private SerializedDictionary<EnemyId, Definition> definitions = new();

        [NonSerialized] private Dictionary<EnemyId, Definition> _definitionsById;
        [NonSerialized] private IReadOnlyList<string> _validationErrors;

        public IReadOnlyList<KeyValue<EnemyId, Definition>> Definitions => definitions.KeyValueList;

        public Definition GetDefinition(EnemyId enemyId)
        {
            EnsureCache();
            if (_validationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyCatalog)} '{name}' has invalid definitions: {string.Join(" ", _validationErrors)}");
            }

            if (_definitionsById.TryGetValue(enemyId, out Definition definition))
            {
                return definition;
            }

            throw new InvalidOperationException(
                $"{nameof(EnemyCatalog)} '{name}' does not define {nameof(EnemyId)} '{enemyId}'.");
        }

        public IReadOnlyList<string> GetValidationErrors()
        {
            EnsureCache();
            return _validationErrors;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            InvalidateCache();
        }

        private void OnEnable()
        {
            InvalidateCache();
        }

        private void OnValidate()
        {
            InvalidateCache();
        }

        private void EnsureCache()
        {
            if (_definitionsById != null)
            {
                return;
            }

            _definitionsById = new Dictionary<EnemyId, Definition>();
            List<string> errors = new();
            if (definitions == null || definitions.KeyValueList == null)
            {
                errors.Add("Definitions are missing.");
                _validationErrors = errors.ToArray();
                return;
            }

            if (definitions.KeyValueList.Count == 0)
            {
                errors.Add("Definitions are empty.");
            }

            HashSet<EnemyId> assignedIds = new();
            foreach (KeyValue<EnemyId, Definition> entry in definitions.KeyValueList)
            {
                if (entry == null)
                {
                    errors.Add("Definitions contain an empty entry.");
                    continue;
                }

                EnemyId enemyId = entry.Key;
                if (enemyId == EnemyId.Unassigned)
                {
                    errors.Add("An entry uses the unassigned enemy ID.");
                }
                else if (!Enum.IsDefined(typeof(EnemyId), enemyId))
                {
                    errors.Add($"An entry uses unknown enemy ID '{enemyId}'.");
                }
                else if (!assignedIds.Add(enemyId))
                {
                    errors.Add($"Enemy ID '{enemyId}' is assigned more than once.");
                }
                else
                {
                    _definitionsById.Add(enemyId, entry.Value);
                }

                ValidateDefinition(enemyId, entry.Value, errors);
            }

            _validationErrors = errors.ToArray();
        }

        private void InvalidateCache()
        {
            _definitionsById = null;
            _validationErrors = null;
        }

        private static void ValidateDefinition(
            EnemyId enemyId,
            Definition definition,
            List<string> errors)
        {
            if (definition == null)
            {
                errors.Add($"Enemy ID '{enemyId}' has no definition.");
                return;
            }

            if (definition.EnemyPrefab == null)
            {
                errors.Add($"Enemy ID '{enemyId}' is missing an enemy prefab.");
            }

            if (definition.BehaviourProfile == null)
            {
                errors.Add($"Enemy ID '{enemyId}' is missing a behaviour profile.");
            }

            if (definition.Moveset == null)
            {
                errors.Add($"Enemy ID '{enemyId}' is missing a moveset.");
            }

            if (definition.HealthData == null)
            {
                errors.Add($"Enemy ID '{enemyId}' is missing health data.");
            }
        }

        [Serializable]
        public sealed class Definition
        {
            [SerializeField] private EnemyActor enemyPrefab;
            [SerializeField] private EnemyBehaviourProfile behaviourProfile;
            [SerializeField] private WeaponMovesetDefinition moveset;
            [SerializeField] private HealthData healthData;

            public EnemyActor EnemyPrefab => enemyPrefab;
            public EnemyBehaviourProfile BehaviourProfile => behaviourProfile;
            public WeaponMovesetDefinition Moveset => moveset;
            public HealthData HealthData => healthData;
        }
    }
}
