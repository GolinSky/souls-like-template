namespace SoulsLike.Ui.LevelUp
{
    public interface ILevelUpPresenter
    {
        void SelectAttribute(int index);
        void IncrementAttribute(int index);
        void DecrementAttribute(int index);
        void Confirm();
        void Back();
    }
}
