using System.Collections.Concurrent;
using Veldrid;

namespace GameFramework;

/// <summary>
///     Used to cache static buffers. Mutation access is not enforced.
/// </summary>
public sealed class StaticDeviceBufferCache
{
    private readonly GameApplication _application;
    private readonly ConcurrentDictionary<object, DeviceBuffer> _cache = new();

    public StaticDeviceBufferCache(GameApplication application)
    {
        _application = application;
    }

    public DeviceBuffer GetOrAdd(object key, Func<GameApplication, DeviceBuffer> create)
    {
        return _cache.GetOrAdd(key, _ => create(_application));
    }
}