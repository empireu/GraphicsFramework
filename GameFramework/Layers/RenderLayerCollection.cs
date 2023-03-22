namespace GameFramework.Layers;

internal class RenderLayerCollection
{
    private readonly List<RenderLayer> _layers = new();

    public void AddLayer(RenderLayer layer)
    {
        if (_layers.Contains(layer))
        {
            throw new ArgumentException("Duplicate add for render layer");
        }

        _layers.Add(layer);
    }

    public void RemoveLayer(RenderLayer layer)
    {
        if (!_layers.Remove(layer))
        {
            throw new ArgumentException("Layer not found");
        }
    }

    public void Render(FrameInfo info)
    {
        foreach (var renderLayer in _layers)
        {
            renderLayer.Render(info);
        }
    }
}