namespace GameFramework.Utilities;

public readonly struct RentedItem<T> : IDisposable
{
    private readonly Action<T> _dispose;
    
    public RentedItem(T instance, Action<T> dispose)
    {
        _dispose = dispose;
        Instance = instance;
    }
    
    public T Instance { get; }

    public void Dispose()
    {
        _dispose.Invoke(Instance);
    }
}