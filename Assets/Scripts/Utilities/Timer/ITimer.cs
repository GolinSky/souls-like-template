namespace Prospector.Utility.Timer
{
    public interface ITimer
    {
        bool IsRunning { get; }
        bool IsComplete { get; }
        float TimeLeft { get; }
        void Start();
        void Stop();
        void Reset();
        ITimer ChangeDuration(float newDelay);
    }
}