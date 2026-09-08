public sealed class TestContext : SynchronizationContext
{
    private readonly Queue<Action> _callbacks = new();
    public override void Post(SendOrPostCallback callback, object state) => _callbacks.Enqueue(() => callback(state));
    public void Tick()
    {
        UnityEngine.AddressableAssets.Addressables.CompletePending();
        int count = _callbacks.Count;
        while (count-- > 0) _callbacks.Dequeue()();
    }
    public void Finish(Task task)
    {
        for (int i = 0; i < 1000 && !task.IsCompleted; i++) Tick();
        if (!task.IsCompleted) throw new Exception("Test exceeded 1000 scheduler ticks");
    }
}
