using GameFramework.Utilities.Extensions;
using Veldrid;

namespace GameFramework.Renderer.Batch;

/// <summary>
///     Buffer for Textured Quads with texture sorting. Internally, a <see cref="DeviceBufferList{TData}"/> is used to hold the vertex data.
/// </summary>
/// <typeparam name="TQuad"></typeparam>
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

    /// <summary>
    ///     Clears all buffered quads.
    /// </summary>
    public void Clear()
    {
        DestroyCachedReferences();

        foreach (var (_, buffer) in _texturesVertices)
        {
            buffer.Dispose();
        }

        _texturesVertices.Clear();
    }

    /// <summary>
    ///     Destroys the cached references to the device objects to prevent them from lingering around (not being GC collectable)
    /// </summary>
    private void DestroyCachedReferences()
    {
        _lastSet = null;
        _lastBuffer = null;
    }

    /// <summary>
    ///     Adds a textured quad that uses the specified texture.
    /// </summary>
    /// <param name="instance">The quad instance to add.</param>
    /// <param name="ts">The texture used by the quad.</param>
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

    /// <summary>
    ///     Renders the buffered quads. This results in one draw call per texture used, excluding large geometry batches splitting into multiple draw calls.
    /// </summary>
    /// <param name="list">The command list to submit to. It is assumed that framebuffers and such were set up before calling this method.</param>
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

    /// <summary>
    ///     True, if no quads are buffered. Otherwise, false.
    /// </summary>
    public bool IsEmpty => _texturesVertices.Count == 0;
}