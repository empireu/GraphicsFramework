using System.Numerics;
using Veldrid;

namespace GameFramework.Renderer.Batch;

/// <summary>
///     Versatile rendering batch with support for colored, textured and SDF text quads.
/// </summary>
public sealed unsafe class QuadBatch
{
    private static readonly uint[] Indices =
    {
        0, 1, 2, 0, 2, 3
    };

    public GraphicsDevice Device { get; }

    private readonly DeviceBufferList<ColoredQuad> _coloredQuadVertices;
    private readonly TexturedQuadBuffer<TexturedQuad> _texturedQuadBuffer;
    private readonly TexturedQuadBuffer<SdfQuad> _sdfQuadBuffer;

    private readonly DeviceBuffer _indices;
    private readonly DeviceBuffer _effectsBuffer;

    private readonly ShaderProgram _coloredQuadProgram;
    private readonly ShaderProgram _texturedQuadProgram;
    private readonly ShaderProgram _sdfQuadProgram;

    private readonly ResourceSet _commonResourceSet;

    private readonly ResourceLayout _texturedQuadLayout;
    private readonly ResourceLayout _sdfQuadLayout;
    private readonly ResourceLayout _commonResourceLayout;

    private readonly CommandList _list;

    private Pipeline? _coloredQuadPipeline;
    private Pipeline? _texturedQuadPipeline;
    private Pipeline? _sdfQuadPipeline;

    private QuadBatchEffects _effects;

    public QuadBatchEffects Effects
    {
        get => _effects;
        set
        {
            _effects = value;
            Device.UpdateBuffer(_effectsBuffer, 0, value);
        }
    }

    public QuadBatch(GameApplication application, int quadsPerBatch = 1024 * 16, bool cacheIndexBuffer = true)
    {
        Application = application;
        QuadsPerBatch = quadsPerBatch;
        Device = application.Resources.Device;
        
        _coloredQuadProgram = application.Resources.ColoredQuadShader;
        _texturedQuadProgram = application.Resources.TexturedQuadShader;
        _sdfQuadProgram = application.Resources.SdfQuadShader;

        _list = Device.ResourceFactory.CreateCommandList();

        IndicesPerBatch = Indices.Length * quadsPerBatch;

        _coloredQuadVertices = new DeviceBufferList<ColoredQuad>(
            Device, application.Resources.BufferPool, quadsPerBatch, BufferUsage.VertexBuffer | BufferUsage.Dynamic);

        _indices = cacheIndexBuffer
            ? application.Resources.StaticBufferCache.GetOrAdd(new QuadIndexBufferCacheKey(quadsPerBatch),
                _ => CreateIndexBuffer(application.Device, (uint)IndicesPerBatch))
            : CreateIndexBuffer(application.Device, (uint)IndicesPerBatch);

        _effectsBuffer = Device.ResourceFactory.CreateBuffer(
            new BufferDescription((uint)sizeof(QuadBatchEffects), BufferUsage.UniformBuffer));
      
        Effects = QuadBatchEffects.None;

        _texturedQuadLayout = Application.Resources.ResourceCache.GetResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _sdfQuadLayout = Application.Resources.ResourceCache.GetResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _commonResourceLayout = Application.Resources.ResourceCache.GetResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Effects", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

        _texturedQuadBuffer = new TexturedQuadBuffer<TexturedQuad>(application, _texturedQuadLayout, quadsPerBatch);
     
        _sdfQuadBuffer = new TexturedQuadBuffer<SdfQuad>(application, _sdfQuadLayout, quadsPerBatch);

        _commonResourceSet =
            Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_commonResourceLayout, _effectsBuffer));

        UpdatePipelines();
    }

    public GameApplication Application { get; }

    public int IndicesPerBatch { get; }

    public int QuadsPerBatch { get; }

    /// <summary>
    ///     Creates a static index buffer for quads.
    /// </summary>
    /// <param name="device">The graphics device that owns the buffer.</param>
    /// <param name="indicesPerBatch">The number of indices per batch. Ideally, it should be a multiple of 6, since one quad uses 6 indices.</param>
    /// <returns></returns>
    public static DeviceBuffer CreateIndexBuffer(GraphicsDevice device, uint indicesPerBatch)
    {
        var result = device.ResourceFactory.CreateBuffer(
            new BufferDescription(sizeof(uint) * indicesPerBatch,
                BufferUsage.IndexBuffer));

        var indexArray = new uint[indicesPerBatch];

        var offset = 0u;
        for (var i = 0; i < indexArray.Length; i += 6)
        {
            indexArray[i + 0] = offset + Indices[0];
            indexArray[i + 1] = offset + Indices[1];
            indexArray[i + 2] = offset + Indices[2];
            indexArray[i + 3] = offset + Indices[3];
            indexArray[i + 4] = offset + Indices[4];
            indexArray[i + 5] = offset + Indices[5];

            offset += 4;
        }

        device.UpdateBuffer(result, 0, indexArray);

        return result;
    }

    public void UpdatePipelines(BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null, OutputDescription? outputDescription = null)
    {
        blendState ??= BlendStateDescription.SingleAlphaBlend;
        depthStencilState ??= new DepthStencilStateDescription(false, false, ComparisonKind.Always);
        outputDescription ??= Application.Device.SwapchainFramebuffer.OutputDescription;

        UpdateColoredQuadPipeline(blendState.Value, depthStencilState.Value, outputDescription.Value);
        UpdateTexturedQuadPipeline(blendState.Value, depthStencilState.Value, outputDescription.Value);
        UpdateSdfQuadPipeline(blendState.Value, depthStencilState.Value, outputDescription.Value);
    }

    public void UpdateColoredQuadPipeline(BlendStateDescription blendStateDescription, DepthStencilStateDescription depthStencilStateDescription, OutputDescription outputDescription)
    {
        _coloredQuadPipeline?.Dispose();

        var description = new GraphicsPipelineDescription
        {
            BlendState = blendStateDescription,
            DepthStencilState = depthStencilStateDescription,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = new[]
            {
                _commonResourceLayout
            }
        };

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        description.Outputs = outputDescription;

        description.ShaderSet = new ShaderSetDescription(
            vertexLayouts: new[] { vertexLayout },
            shaders: _coloredQuadProgram.ToArray());

        _coloredQuadPipeline = Device.ResourceFactory.CreateGraphicsPipeline(description);
    }

    public void UpdateTexturedQuadPipeline(BlendStateDescription blendStateDescription, DepthStencilStateDescription depthStencilStateDescription, OutputDescription outputDescription)
    {
        _texturedQuadPipeline?.Dispose();

        var description = new GraphicsPipelineDescription
        {
            BlendState = blendStateDescription,
            DepthStencilState = depthStencilStateDescription,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = new[]
            {
                _texturedQuadLayout,
                _commonResourceLayout
            }
        };

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TextureCoordinate", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

        description.Outputs = outputDescription;

        description.ShaderSet = new ShaderSetDescription(
            vertexLayouts: new[] { vertexLayout },
            shaders: _texturedQuadProgram.ToArray());
        _texturedQuadPipeline = Device.ResourceFactory.CreateGraphicsPipeline(description);
    }

    public void UpdateSdfQuadPipeline(BlendStateDescription blendStateDescription, DepthStencilStateDescription depthStencilStateDescription, OutputDescription outputDescription)
    {
        _sdfQuadPipeline?.Dispose();

        var description = new GraphicsPipelineDescription
        {
            BlendState = blendStateDescription,
            DepthStencilState = depthStencilStateDescription,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = new[]
            {
                _sdfQuadLayout,
                _commonResourceLayout
            }
        };

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TextureCoordinate", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Weight", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("Smoothing", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("Alpha", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));

        description.Outputs = outputDescription;

        description.ShaderSet = new ShaderSetDescription(
            vertexLayouts: new[] { vertexLayout },
            shaders: _sdfQuadProgram.ToArray());
        _sdfQuadPipeline = Device.ResourceFactory.CreateGraphicsPipeline(description);
    }

    private void SubmitColoredQuads()
    {
        if (_coloredQuadVertices.Buffers.Count == 0)
        {
            return;
        }

        _list.SetPipeline(_coloredQuadPipeline);
        _list.SetGraphicsResourceSet(0, _commonResourceSet);
        _list.SetIndexBuffer(_indices, IndexFormat.UInt32);

        foreach (var coloredQuadBuffer in _coloredQuadVertices.Buffers)
        {
            coloredQuadBuffer.PrepareDraw();
            
            _list.SetVertexBuffer(0, coloredQuadBuffer.InstanceDataBuffer);
    
            _list.DrawIndexed(
                indexCount: (uint)(Indices.Length * coloredQuadBuffer.InstanceCount),
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }
    }

    private void SubmitTexturedQuads()
    {
        if (_texturedQuadBuffer.IsEmpty)
        {
            return;
        }

        _list.SetPipeline(_texturedQuadPipeline);
        _list.SetGraphicsResourceSet(1, _commonResourceSet);
        _list.SetIndexBuffer(_indices, IndexFormat.UInt32);

        _texturedQuadBuffer.Submit(_list);
    }

    private void SubmitSdfQuads()
    {
        if (_sdfQuadBuffer.IsEmpty)
        {
            return;
        }

        _list.SetPipeline(_sdfQuadPipeline);
        _list.SetGraphicsResourceSet(1, _commonResourceSet);
        _list.SetIndexBuffer(_indices, IndexFormat.UInt32);

        _sdfQuadBuffer.Submit(_list);
    }

    /// <summary>
    ///     Renders the buffered quads. The buffers are not cleared here. If clearing is desired, use <see cref="Clear"/>!
    /// </summary>
    /// <param name="wait">
    ///     If true, the thread will wait for the submitted graphics commands to finish. Only effectual when <code>flush</code> is set to true!
    ///     Otherwise, the function will return right after the graphics commands have been submitted.</param>
    /// <param name="flush">
    ///     If true, the graphics command will be submitted instantly.
    /// </param>
    /// <param name="framebuffer">If null, the swapchain framebuffer is used. Otherwise, the <code>framebuffer</code> will be used for rendering.</param>
    public void Submit(bool wait = true, bool flush = true, Framebuffer? framebuffer = null)
    {
        framebuffer ??= Device.SwapchainFramebuffer;

        _list.Begin();

        _list.SetFramebuffer(framebuffer);

        SubmitColoredQuads();
        SubmitTexturedQuads();
        SubmitSdfQuads();

        _list.End();

        if (flush)
        {
            Flush(wait);
        }
    }

    /// <summary>
    ///     Submits the graphics commands to the GraphicsDevice.
    /// </summary>
    /// <param name="wait">If true, the thread will wait for the commands to complete. Otherwise, the caller will return as soon as submission is completed.</param>
    public void Flush(bool wait)
    {
        Device.SubmitCommands(_list);

        if (wait)
        {
            Device.WaitForIdle();
        }
    }

    private void ClearColoredQuads()
    {
        _coloredQuadVertices.Clear();
    }

    private void ClearTexturedQuads()
    {
        _texturedQuadBuffer.Clear();
    }

    private void ClearSdfQuads()
    {
        _sdfQuadBuffer.Clear();
    }

    /// <summary>
    ///     Clears all quad buffers.
    /// </summary>
    public void Clear()
    {
        ClearColoredQuads();
        ClearTexturedQuads();
        ClearSdfQuads();
    }

    public void AddColoredQuad(ref ColoredQuad instance)
    {
        _coloredQuadVertices.Add(ref instance);
    }

    public void AddColoredQuad(Matrix4x4 transform, RgbaFloat c0, RgbaFloat c1, RgbaFloat c2, RgbaFloat c3)
    {
        var instance = ColoredQuad.Create(transform, c0, c1, c2, c3);

        AddColoredQuad(ref instance);
    }

    public void AddTexturedQuad(ref TexturedQuad instance, TextureSampler ts)
    {
        _texturedQuadBuffer.AddTexturedQuad(ref instance, ts);
    }

    public void AddTexturedQuad(TextureSampler ts, Matrix4x4 transform, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        var instance = TexturedQuad.Create(transform, uv0, uv1, uv2, uv3);

        AddTexturedQuad(ref instance, ts);
    }

    public void AddTexturedQuad(TextureSampler ts, Matrix4x4 transform)
    {
        AddTexturedQuad(ts, transform, 
            TextureCoordinate4.Uv0Full,
            TextureCoordinate4.Uv1Full,
            TextureCoordinate4.Uv2Full,
            TextureCoordinate4.Uv3Full);
    }

    public void AddSdfQuad(ref SdfQuad instance, TextureSampler ts)
    {
        _sdfQuadBuffer.AddTexturedQuad(ref instance, ts);
    }
   
    public void AddSdfQuad(TextureSampler ts, Matrix4x4 transform, TextureCoordinate4 textureCoordinates, SdfOptions4 sdfOptions)
    {
        var instance = SdfQuad.Create(transform, textureCoordinates, sdfOptions);

        AddSdfQuad(ref instance, ts);
    }
}