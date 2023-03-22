using GameFramework.Utilities.Extensions;
using Veldrid;

namespace GameFramework.Renderer.Batch;

internal sealed class TexturedQuadBuffer<TQuad> where TQuad : unmanaged
{
    public const int Indices = 6;

    private readonly GameApplication _application;
    private readonly ResourceLayout _layout;
    private readonly int _quadsPerBatch;
  
    private readonly Dictionary<ResourceSet, DeviceBufferList<TQuad>> _texturesVertices = new();
    private readonly Dictionary<TextureSampler, ResourceSet> _textureResourceSetCache = new();
    
    private (TextureSampler ts, ResourceSet set)? _lastSet;
    private (ResourceSet set, DeviceBufferList<TQuad> buffer)? _lastBuffer;

    public TexturedQuadBuffer(GameApplication application, ResourceLayout layout, int quadsPerBatch)
    {
        _application = application;
        _layout = layout;
        _quadsPerBatch = quadsPerBatch;
    }

    public void Clear()
    {
        DestroyCachedReferences();

        foreach (var (_, buffer) in _texturesVertices)
        {
            buffer.Dispose();
        }

        _texturesVertices.Clear();
    }

    private void DestroyCachedReferences()
    {
        _lastSet = null;
        _lastBuffer = null;
    }

    public void AddTexturedQuad(ref TQuad instance, TextureSampler ts)
    {
        ResourceSet resourceSet;

        if (_lastSet == null || _lastSet.Value.ts != ts)
        {
            resourceSet = _textureResourceSetCache.GetOrAdd(ts, _ => _application.Resources.Factory.CreateResourceSet(
                new ResourceSetDescription(
                    _layout,
                    ts.Texture,
                    ts.Sampler)));

            _lastSet = (ts, resourceSet);
        }
        else
        {
            resourceSet = _lastSet.Value.set;
        }

        DeviceBufferList<TQuad> buffer;

        if (_lastBuffer == null || _lastBuffer.Value.set != resourceSet)
        {
            buffer = _texturesVertices.GetOrAdd(resourceSet,
                _ => new DeviceBufferList<TQuad>(_application.Device, _application.Resources.BufferPool, _quadsPerBatch,
                    BufferUsage.VertexBuffer | BufferUsage.Dynamic));

            _lastBuffer = (resourceSet, buffer);
        }
        else
        {
            buffer = _lastBuffer.Value.buffer;
        }

        
        buffer.Add(ref instance);
    }

    public void Submit(CommandList list)
    {
        DestroyCachedReferences();

        foreach (var (resourceSet, bufferList) in _texturesVertices)
        {
            list.SetGraphicsResourceSet(0, resourceSet);

            foreach (var texturedQuadBuffer in bufferList.Buffers)
            {
                texturedQuadBuffer.PrepareDraw();

                list.SetVertexBuffer(0, texturedQuadBuffer.InstanceDataBuffer);

                list.DrawIndexed(
                    indexCount: (uint)(Indices * texturedQuadBuffer.InstanceCount),
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0);
            }
        }
    }

    public bool IsEmpty => _texturesVertices.Count == 0;
}