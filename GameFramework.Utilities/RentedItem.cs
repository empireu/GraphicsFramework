namespace GameFramework.Utilities;

/// <summary>
///     Wrapper for pooled items.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct RentedItem<T> : IDisposable
{
    private readonly Action<T> _dispose;
    
    /// <summary>
    ///     Creates a new instance of the <see cref="RentedItem{T}"/> class.
    /// </summary>
    /// <param name="instance">The rented object.</param>
    /// <param name="dispose">A dispose callback.</param>
    public RentedItem(T instance, Action<T> dispose)
    {
        _dispose = dispose;
        Instance = instance;
    }
    
    public T Instance { get; }

    /// <summary>
    ///     Calls the dispose callback for this wrapper.
    /// </summary>
    public void Dispose()
    {
        _dispose.Invoke(Instance);
    }
}