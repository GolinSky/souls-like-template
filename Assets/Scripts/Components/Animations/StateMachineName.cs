namespace SoulsLike.Entities.Character.Components.Animations
{
    public enum StateMachineName
    {
        Idle = 0,
        LightAttack = 1,
        LightAttackAlt = 2,
        HeavyAttack = 3,
        HeavyAttackAlt = 4,
        RollAttack = 5,
        BackStepAttack = 6,
        RunAttack = 7,
        SpecialAttack = 8,
        Roll = 9,
        BackStep = 10,
        Spawn = 11, // appear animation on spawn
        HandModeSwitch = 12,
        EquipmentSwapOut = 13,
        EquipmentSwapIn = 14,
        ShieldBlock = 15,
        Parry = 16,
        Death = 17,
        None = -1
    }
}
