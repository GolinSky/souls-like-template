namespace SoulsLike.Entities.Character.Components.Equipment
{
    public class EquipmentModel
    {
        public HandMode ActiveHandMode { get; private set; } = HandMode.OneHanded;

        public void SetHandMode(HandMode handMode)
        {
            ActiveHandMode = handMode;
        }
    }
}
