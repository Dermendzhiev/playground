public interface IHashTable
{
    int Count { get; }

    bool ContainsKey(string key);

    object Get(string key);

    /// <summary>
    /// Insert or update
    /// </summary>
    void Upsert(string key, object value);

    void Remove(string key);
}