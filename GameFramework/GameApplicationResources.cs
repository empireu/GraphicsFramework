using GameFramework.Assets;
using GameFramework.Renderer;
using GameFramework.Renderer.Batch;
using Veldrid;
using Veldrid.Sdl2;

namespace GameFramework;

/// <summary>
///     Holds the resources used by the framework and an <see cref="AssetManager"/>.
/// </summary>
public sealed class GameApplicationResources
{
    public GameApplication Application { get; }

    public GraphicsDevice Device => Application.Device;

    public ResourceFactory Factory => Device.ResourceFactory;

    public Sdl2Window Window => Application.Window;

    /// <summary>
    ///     The colored quad shader used by <see cref="QuadBatch"/>
    /// </summary>
    public ShaderProgram ColoredQuadShader { get; }

    /// <summary>
    ///     The textured quad shader used by <see cref="QuadBatch"/>
    /// </summary>
    public ShaderProgram TexturedQuadShader { get; }

    /// <summary>
    ///     The SDF quad shader used by <see cref="QuadBatch"/>
    /// </summary>
    public ShaderProgram SdfQuadShader { get; }

    public AssetManager AssetManager { get; }

    /// <summary>
    ///     A global <see cref="DeviceBufferPool"/>. It is also used by framework code.
    /// </summary>
    public DeviceBufferPool BufferPool { get; }

    /// <summary>
    ///     A global <see cref="QuadBatchPool"/>. It is also used by framework code.
    /// </summary>
    public QuadBatchPool BatchPool { get; }

    /// <summary>
    ///     A global <see cref="GraphicsResourceCache"/>. It is also used by framework code.
    /// </summary>
    public GraphicsResourceCache ResourceCache { get; }

    /// <summary>
    ///     A global <see cref="StaticDeviceBufferCache"/>. It is also used by framework code.
    /// </summary>
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