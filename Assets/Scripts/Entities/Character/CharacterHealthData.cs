using SoulsLike.Entities.Character.Components.Health;

namespace SoulsLike.Entities.Character
{
    //TODO: REMOVE THIS USELESS CLASS. USE DECORATOR 
    public sealed class CharacterHealthData : IHealthData
    {
        private const float HEALTH_PER_VIGOR = 20f;

        private readonly HealthData _healthData;
        private readonly CharacterData _characterData;

        public float MaxHealth =>
            _healthData.MaxHealth + _characterData.Attributes.Vigor * HEALTH_PER_VIGOR;

        public float StartingHealth =>
            MaxHealth * _healthData.StartingHealth / _healthData.MaxHealth;

        public float MaxFocus => _healthData.MaxFocus;
        public float StartingFocus => _healthData.StartingFocus;
        public float MaxStamina => _healthData.MaxStamina;
        public float StartingStamina => _healthData.StartingStamina;
        public float StaminaRecoveryPerSecond => _healthData.StaminaRecoveryPerSecond;
        public float StaminaRecoveryDelaySeconds => _healthData.StaminaRecoveryDelaySeconds;
        public float GuardStaminaRecoveryMultiplier => _healthData.GuardStaminaRecoveryMultiplier;
        public bool CanDie => _healthData.CanDie;
        public float InvulnerableOnSpawnSeconds => _healthData.InvulnerableOnSpawnSeconds;

        public CharacterHealthData(HealthData healthData, CharacterData characterData)
        {
            _healthData = healthData;
            _characterData = characterData;
        }
    }
}
