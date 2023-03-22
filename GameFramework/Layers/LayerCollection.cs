using Microsoft.Extensions.DependencyInjection;

namespace GameFramework.Layers;

/// <summary>
///     The LayerCollection manages <see cref="Layer"/>s, and provides ways to raise events and traverse the layers.
/// </summary>
public sealed class LayerCollection
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Layer> _layers = new();

    public LayerCollection(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Used to add a layer to this collection. A layer cannot be added twice; this will result in an exception.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if this layer was added prior to this call.</exception>
    public void AddLayer(Layer layer)
    {
        if (_layers.Contains(layer))
        {
            throw new InvalidOperationException($"Layer {layer} already exists within the collection.");
        }

        _layers.Add(layer);

        layer.OnAdded();
    }

    /// <summary>
    ///     Instantiates the layer, assuming it is registered in the dependency injection container.
    ///     The layer is then added to the collection by a call to <see cref="AddLayer"/>
    /// </summary>
    /// <typeparam name="T">The layer service type to construct.</typeparam>
    /// <returns>The constructed instance of the layer.</returns>
    public T ConstructLayer<T>() where T : Layer
    {
        var instance = _serviceProvider.GetRequiredService<T>();

        AddLayer(instance);

        return instance;
    }

    ///<inheritdoc cref="ConstructLayer{T}()"/>
    public void ConstructLayer<T>(Action<T> configure) where T : Layer
    {
        var instance = _serviceProvider.GetRequiredService<T>();

        configure(instance);

        AddLayer(instance);
    }

    /// <summary>
    ///     Returns an enumerable that will traverse the layers in a front-to-back order. This is useful for input events.
    /// </summary>
    public IEnumerable<Layer> FrontToBack => _layers;

    /// <summary>
    ///     Returns an enumerable that will traverse the layers in a back-to-front order. This is useful for rendering.
    /// </summary>
    public IEnumerable<Layer> BackToFront => Enumerable.Reverse(_layers);

    /// <summary>
    ///     Sends an event down the layer stack, stopping once the event gets handled.
    /// </summary>
    /// <typeparam name="T">The type of event to send.</typeparam>
    /// <param name="event">The event instance to send.</param>
    /// <returns>The layer that handled the event, if any.</returns>
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