using System.Collections.Concurrent;
using Veldrid;

namespace GameFramework;

/// <summary>
///     This is a cache for resources that share the same description. It will give the same object for subsequent calls.
/// </summary>
public sealed class GraphicsResourceCache
{
    private readonly ResourceFactory _factory;

    private readonly ConcurrentDictionary<ResourceLayoutDescription, ResourceLayout> _resourceLayouts = new();
    
    public GraphicsResourceCache(ResourceFactory factory)
    {
        _factory = factory;
    }

    public ResourceLayout GetResourceLayout(ResourceLayoutDescription description)
    {
        return _resourceLayouts.GetOrAdd(description, _ => _factory.CreateResourceLayout(description));
    }

    public int CachedResourceLayouts => _resourceLayouts.Count;
}