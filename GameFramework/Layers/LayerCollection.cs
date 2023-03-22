using Microsoft.Extensions.DependencyInjection;

namespace GameFramework.Layers;

public sealed class LayerCollection
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Layer> _layers = new();

    public LayerCollection(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void AddLayer(Layer layer)
    {
        if (_layers.Contains(layer))
        {
            throw new InvalidOperationException($"Layer {layer} already exists within the collection.");
        }

        _layers.Add(layer);
        layer.OnAdded();
    }

    public T ConstructLayer<T>(params object[] arguments) where T : Layer
    {
        var instance = _serviceProvider.GetRequiredService<T>();

        AddLayer(instance);

        return instance;
    }

    public void ConstructLayer<T>(Action<T> configure, params object[] arguments) where T : Layer
    {
        var instance = _serviceProvider.GetRequiredService<T>();

        configure(instance);

        AddLayer(instance);
    }

    public IEnumerable<Layer> FrontToBack => _layers;

    public IEnumerable<Layer> BackToFront => Enumerable.Reverse(_layers);

    public Layer? SendEvent<T>(in T @event)
    {
        foreach (var layer in FrontToBack)
        {
            if (layer.Handle(@event))
            {
                return layer;
            }
        }

        return null;
    }
}