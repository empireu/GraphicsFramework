using System.Drawing;

namespace GameFramework.Layers;

public abstract class Layer
{
    private readonly Dictionary<Type, Delegate> _handlers = new();
    
    protected internal virtual void OnAdded()
    {

    }

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