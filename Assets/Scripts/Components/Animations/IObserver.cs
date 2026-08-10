namespace SoulsLike.Entities.Character.Components.Animations
{
    public interface IObserver<in T>
    {
        void UpdateState(T arg);
    }
}
