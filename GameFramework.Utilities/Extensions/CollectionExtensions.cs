namespace GameFramework.Utilities.Extensions;

public static class CollectionExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory) where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = valueFactory(key);
            dictionary.Add(key, value);
        }

        return value;
    }

    public static void AddOrUpdate<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary, 
        TKey key,
        Func<TKey, TValue> addValueFactory,
        Func<TKey, TValue> updateValueFactory) where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = updateValueFactory(key);
        }
        else
        {
            dictionary.Add(key, addValueFactory(key));
        }
    }

    public static void AddOrUpdate<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value) where TKey : notnull
    {
        if (!dictionary.TryAdd(key, value))
        {
            dictionary[key] = value;
        }
    }
}