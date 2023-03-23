using System.Diagnostics;
using System.Numerics;
using GameFramework.Assets;
using GameFramework.Renderer;
using GameFramework.Renderer.Batch;
using Veldrid;

namespace GameFramework.PostProcessing;

public class ColorStage : PostProcessingStage
{
    private static readonly Guid VertexBufferCacheKey = Guid.NewGuid();

    public Sampler Sampler { get; set; }

    private readonly GameApplication _application;
    private readonly CommandList _commandList;

    private readonly ResourceLayout[] _layouts;
    private readonly ResourceSet[] _resourceSets;
    
    private readonly ResourceLayout _commonLayout;
    private Pipeline? _pipeline;

    private readonly ShaderProgram _program;

    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _vertexBuffer;

    public ColorStage(GameApplication application, IResourceKey spirvFragmentKey, ResourceLayout[] layouts, ResourceSet[] resourceSets, Sampler? sampler = null)
    {
        Sampler = sampler ?? application.Device.LinearSampler;
        _application = application;
        _layouts = layouts;
        _resourceSets = resourceSets;

        _commandList = application.Resources.Factory.CreateCommandList();

        _commonLayout = application.Resources.ResourceCache.GetResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _program = application.Resources.AssetManager.GetOrAddSpirvProgram(
            new EmbeddedResourceKey(typeof(ColorStage).Assembly, 
                "GameFramework.PostProcessing.Shaders.ColorStageVertex.spirv"), spirvFragmentKey);

        _indexBuffer = application.Resources.StaticBufferCache.GetOrAdd(new QuadIndexBufferCacheKey(1),
            _ => QuadBatch.CreateIndexBuffer(application.Device, 6));

        _vertexBuffer = application.Resources.StaticBufferCache.GetOrAdd(VertexBufferCacheKey, CreateVertexBuffer);
    }

    private unsafe DeviceBuffer CreateVertexBuffer(GameApplication app)
    {
        var quad = TexturedQuad.Create(Matrix4x4.CreateScale(2, 2, 0), TextureCoordinate4.Full);
        var buffer = app.Resources.Factory.CreateBuffer(new BufferDescription((uint)sizeof(TexturedQuad), BufferUsage.VertexBuffer));
        app.Device.UpdateBuffer(buffer, 0, quad);

        return buffer;
    }

    public override void UpdateOutput(OutputDescription outputDescription)
    {
        _pipeline?.Dispose();

        var pipelineDescription = new GraphicsPipelineDescription();

        pipelineDescription.BlendState = BlendStateDescription.Empty;
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.Disabled;

        pipelineDescription.RasterizerState = RasterizerStateDescription.CullNone;
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;

        pipelineDescription.ResourceLayouts = new[]
        {
            _commonLayout,
        }.Concat(_layouts).ToArray();

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TextureCoordinate", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

        pipelineDescription.Outputs = outputDescription;
        
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            vertexLayouts: new[] { vertexLayout },
            shaders: _program.ToArray());

        _pipeline = _application.Resources.Factory.CreateGraphicsPipeline(pipelineDescription);
    }

    public override void Process(PostProcessingFramebuffer inputFramebuffer, PostProcessingFramebuffer outputBuffer)
    {
        Debug.Assert(_pipeline != null);
        Debug.Assert(inputFramebuffer.View != null);

        using var textureResourceSet = _application.Resources.Factory.CreateResourceSet(new ResourceSetDescription(
            _commonLayout,
                inputFramebuffer.View, 
                Sampler));

        _commandList.Begin();

        _commandList.SetPipeline(_pipeline);
        _commandList.SetFramebuffer(outputBuffer.Framebuffer);
        _commandList.SetGraphicsResourceSet(0, textureResourceSet);

        for (var i = 0; i < _resourceSets.Length; i++)
        {
            _commandList.SetGraphicsResourceSet((uint)(i + 1), _resourceSets[i]);
        }

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);

        _commandList.DrawIndexed(6);

        _commandList.End();
        _application.Device.SubmitCommands(_commandList);
    }

    public override void Dispose()
    {
        _commandList.Dispose();
        _pipeline?.Dispose();

        GC.SuppressFinalize(this);
    }
}