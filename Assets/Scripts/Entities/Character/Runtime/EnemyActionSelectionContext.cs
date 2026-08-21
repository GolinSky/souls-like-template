namespace SoulsLike.Entities.Combat.AI
{
    public readonly struct EnemyActionSelectionContext
    {
        public float Distance { get; }
        public float Angle { get; }
        public bool HasLineOfSight { get; }
        public bool ComboWindowOpen { get; }

        public EnemyActionSelectionContext(
            float distance,
            float angle,
            bool hasLineOfSight,
            bool comboWindowOpen)
        {
            Distance = distance;
            Angle = angle;
            HasLineOfSight = hasLineOfSight;
            ComboWindowOpen = comboWindowOpen;
        }
    }
}
