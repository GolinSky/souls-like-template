namespace MultiPlayerTemplate
{
    public interface IKeyValue<TKey, TValue>
    {
        TKey Key { get; }
        TValue Value { get; }
    }
}