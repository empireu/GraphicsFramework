using System.Drawing;

namespace GameFramework.Layers;

/// <summary>
///     A layer has input and rendering capabilities. It is managed by a <see cref="LayerCollection"/>
/// </summary>
public abstract class Layer
{
    private readonly Dictionary<Type, Delegate> _handlers = new();

    /// <summary>
    ///     If true, this layer will be included in the update loop and will be eligible to receive events.
    ///     Otherwise, this layer will not receive any update calls or events from the framework.
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    ///     Called after the layer was added to the LayerCollection.
    /// </summary>
    protected internal virtual void OnAdded()
    {

    }

    /// <summary>
    ///     Called when the layer is removed from the collection 
    /// </summary>
    protected internal virtual void OnRemoved()
    {

    }

    /// <summary>
    ///     Registers an event handler for the specified event class.
    /// </summary>
    /// <typeparam name="T">The event class to handle.</typeparam>
    /// <param name="handler">A function that shall receive the event as parameter.</param>
    protected void RegisterHandler<T>(Func<T, bool> handler)
    {
        _handlers.Add(typeof(T), handler);
    }

    public bool Handle<T>(T @event)
    {
        if (!_handlers.TryGetValue(typeof(T), out var @delegate))
        {
            return false;
        }

        if (@delegate is not Func<T, bool> handler)
        {
            // This really shouldn't happen:
            throw new Exception($"Invalid handler for {typeof(T)}: {@delegate}");
        }

        return handler(@event);
    }

    protected internal virtual void BeforeStart() { }
    protected internal virtual void BeforeUpdate(FrameInfo frameInfo) { }
    protected internal virtual void Update(FrameInfo frameInfo) { }
    protected internal virtual void Render(FrameInfo frameInfo) { }
    protected internal virtual void AfterRender(FrameInfo frameInfo) { }
    protected internal virtual void Resize(Size size) { }
    protected internal virtual void Destroy() { }
}