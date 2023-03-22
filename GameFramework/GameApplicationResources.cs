using GameFramework.Assets;
using GameFramework.Renderer;
using GameFramework.Renderer.Batch;
using Veldrid;
using Veldrid.Sdl2;

namespace GameFramework;

public sealed class GameApplicationResources
{
    public GameApplication Application { get; }

    public GraphicsDevice Device => Application.Device;

    public ResourceFactory Factory => Device.ResourceFactory;

    public Sdl2Window Window => Application.Window;

    public ShaderProgram ColoredQuadShader { get; }

    public ShaderProgram TexturedQuadShader { get; }

    public ShaderProgram SdfQuadShader { get; }

    public AssetManager AssetManager { get; }

    public DeviceBufferPool BufferPool { get; }

    public QuadBatchPool BatchPool { get; }

    public GraphicsResourceCache ResourceCache { get; }

    public StaticDeviceBufferCache StaticBufferCache { get; }

    public GameApplicationResources(GameApplication application)
    {
        Application = application;
        AssetManager = new AssetManager(application);

        var assembly = typeof(GameApplication).Assembly;

        ColoredQuadShader = AssetManager.GetOrAddSpirvProgram(
            new EmbeddedResourceKey(assembly, "GameFramework.Renderer.Shaders.ColoredQuadVertex.spirv"),
            new EmbeddedResourceKey(assembly, "GameFramework.Renderer.Shaders.ColoredQuadPixel.spirv"));

        TexturedQuadShader = AssetManager.GetOrAddSpirvProgram(
            new EmbeddedResourceKey(assembly, "GameFramework.Renderer.Shaders.TexturedQuadVertex.spirv"),
            new EmbeddedResourceKey(assembly, "GameFramework.Renderer.Shaders.TexturedQuadPixel.spirv"));


        SdfQuadShader = AssetManager.GetOrAddSpirvProgram(
            new EmbeddedResourceKey(assembly, "GameFramework.Renderer.Shaders.SdfQuadVertex.spirv"),
            new EmbeddedResourceKey(assembly, "GameFramework.Renderer.Shaders.SdfQuadPixel.spirv"));


        BufferPool = new DeviceBufferPool(Application.Device);
        BatchPool = new QuadBatchPool(Application);
        ResourceCache = new GraphicsResourceCache(Application.Device.ResourceFactory);
        StaticBufferCache = new StaticDeviceBufferCache(Application);
    }
}