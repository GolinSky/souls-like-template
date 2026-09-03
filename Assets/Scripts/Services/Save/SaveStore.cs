namespace SoulsLike.Services.Save
{
    /// <summary>
    /// Thin per-key wrapper over <see cref="ISaveService"/>. Centralises the
    /// "load, or create a fresh instance if absent" pattern that every service
    /// otherwise re-implements. No in-memory caching — every call reads from disk.
    /// </summary>
    public sealed class SaveStore<T> where T : class, new()
    {
        private readonly ISaveService _save;
        private readonly string _key;

        public SaveStore(ISaveService save, string key)
        {
            _save = save;
            _key = key;
        }

        public bool Exists => _save.Exists(_key);

        /// <summary>Loads the saved data, or null when absent/corrupt.</summary>
        public T Load() => _save.Load<T>(_key);

        /// <summary>Loads the saved data, or a fresh instance when absent/corrupt.</summary>
        public T LoadOrCreate() => _save.Load<T>(_key) ?? new T();

        public void Save(T data) => _save.Save(_key, data);
        public void Delete() => _save.Delete(_key);
    }
}
