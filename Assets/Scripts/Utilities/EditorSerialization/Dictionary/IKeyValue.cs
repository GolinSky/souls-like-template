namespace SoulsLike
{
    public interface IKeyValue<TKey, TValue>
    {
        TKey Key { get; }
        TValue Value { get; }
    }
}