namespace SoulsLike.Services
{
    public interface IObserver<in T>
    {
        void UpdateState(T arg);
    }
}
